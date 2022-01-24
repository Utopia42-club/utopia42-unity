using System;
using System.Collections;
using System.Collections.Generic;
using src.Canvas;
using src.Canvas.Map;
using src.MetaBlocks.TdObjectBlock;
using src.Model;
using src.Service;
using src.Service.Ethereum;
using src.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace src
{
    public class GameManager : MonoBehaviour
    {
        private bool worldInited = false;
        public readonly UnityEvent<State> stateChange = new UnityEvent<State>();
        private State state = State.LOADING;
        private List<Dialog> dialogs = new List<Dialog>();


        void Start()
        {
            SetState(State.SETTINGS);
        }

        private void InitPlayerForWallet()
        {
            if (string.IsNullOrWhiteSpace(Settings.WalletId()))
            {
                SetState(State.SETTINGS);
                return;
            }

            var player = Player.INSTANCE;
            player.ResetLands();

            var pos = new Vector3(0, Chunk.CHUNK_HEIGHT + 10, 0);

            var lands = player.GetOwnedLands();
            if (lands.Count > 0)
            {
                var land = lands[0];
                pos = new Vector3(
                    ((float) (land.x1 + land.x2)) / 2,
                    Chunk.CHUNK_HEIGHT + 10,
                    ((float) (land.y1 + land.y2)) / 2);
            }

            pos = FindStartingY(pos);

            StartCoroutine(DoMovePlayerTo(pos, true));
        }

        internal void Help()
        {
            if (GetState() == State.PLAYING || GetState() == State.SETTINGS)
                SetState(State.HELP);
        }

        public void MovePlayerTo(Vector3 pos)
        {
            StartCoroutine(DoMovePlayerTo(pos, false));
        }

        public void MovePlayerTo(string pos)
        {
            var xz = pos.Split('_');
            if (xz.Length == 2 && float.TryParse(xz[0], out var x) && float.TryParse(xz[1], out var z))
            {
                MovePlayerTo(new Vector3(x, 0, z));
            }
        }

        private IEnumerator DoMovePlayerTo(Vector3 pos, bool clean)
        {
            SetState(State.LOADING);
            Loading.INSTANCE.UpdateText("Positioning the player...");
            yield return null;
            pos = FindStartingY(pos);
            Player.INSTANCE.transform.position = pos;
            yield return InitWorld(pos, clean);
        }

        private IEnumerator InitWorld(Vector3 pos, bool clean)
        {
            var player = Player.INSTANCE;
            SetState(State.LOADING);
            Loading.INSTANCE.UpdateText("Creating the world\n0%");
            yield return null;
            var world = World.INSTANCE;
            while (!world.Initialize(new VoxelPosition(pos).chunk, clean)) yield return null;
            float total = world.CountChunksToCreate();
            while (world.CountChunksToCreate() > 0)
            {
                var perc = ((total - world.CountChunksToCreate()) / total) * 100;
                Loading.INSTANCE.UpdateText(string.Format("Creating the world\n{0}%", Mathf.FloorToInt(perc)));
                yield return null;
            }

            worldInited = true;
            SetState(State.PLAYING);
        }

        private Vector3 FindStartingY(Vector3 pos)
        {
            var service = VoxelService.INSTANCE;
            var feet = Vectors.FloorToInt(pos) + new Vector3(.5f, 0f, .5f);
            while (true)
            {
                bool coll = false;
                for (int i = -1; i < 3; i++)
                {
                    if (coll = service.IsSolid(new VoxelPosition(feet + Vector3Int.up * i)))
                    {
                        feet += Vector3Int.up * Math.Max(1, i + 2);
                        break;
                    }
                }

                if (!coll) return feet;
            }
        }

        void Update()
        {
            if (Input.GetButtonDown("Cancel"))
                ReturnToGame();
            else if (Input.GetButtonDown("Menu") && state == State.PLAYING)
                SetState(State.SETTINGS);
            else if (worldInited && Input.GetButtonDown("Menu") && state == State.SETTINGS)
                SetState(State.PLAYING);
            else if (Input.GetButtonDown("Map"))
            {
                if (state == State.MAP && !LandProfileDialog.INSTANCE.gameObject.activeSelf)
                    SetState(State.PLAYING);
                else if (state == State.PLAYING)
                    SetState(State.MAP);
            }
            else if (Input.GetButtonDown("Inventory"))
            {
                if (state == State.INVENTORY)
                    SetState(State.PLAYING);
                else if (state == State.PLAYING)
                    SetState(State.INVENTORY);
            }
        }

        public void ReturnToGame()
        {
            if (worldInited &&
                (state == State.MAP || state == State.SETTINGS || state == State.HELP || state == State.INVENTORY
                 || state == State.PROFILE_DIALOG))
                SetState(State.PLAYING);
            if (state == State.DIALOG && dialogs.Count > 0)
                CloseDialog(dialogs[dialogs.Count - 1]);
        }

        internal void ExitSettings()
        {
            if (worldInited) SetState(State.PLAYING);
            else InitPlayerForWallet();
        }

        internal void SettingsChanged(EthNetwork network)
        {
            if (!EthereumClientService.INSTANCE.IsInited())
            {
                EthereumClientService.INSTANCE.SetNetwork(network);
                SetState(State.LOADING);
                StartCoroutine(VoxelService.INSTANCE.Initialize(Loading.INSTANCE, () => this.InitPlayerForWallet()));
            }
            else
            {
                SetState(State.LOADING);
                InitPlayerForWallet();
            }
        }

        public void SetProfileDialogState(bool open)
        {
            if (open)
            {
                if (GetState() == State.PLAYING || GetState() == State.SETTINGS)
                    SetState(State.PROFILE_DIALOG);
            }
            else if (GetState() == State.PROFILE_DIALOG)
                ReturnToGame();
        }

        public void ShowUserProfile()
        {
            var profileDialog = ProfileDialog.INSTANCE;
            profileDialog.Open(Profile.LOADING_PROFILE);
            ProfileLoader.INSTANCE.load(Settings.WalletId(), profileDialog.Open,
                () => profileDialog.SetProfile(Profile.FAILED_TO_LOAD_PROFILE));
        }

        public void CopyPositionLink()
        {
            var currentPosition = Player.INSTANCE.transform.position;
            GUIUtility.systemCopyBuffer =
                Constants.WebAppBaseURL + $"/game?position={currentPosition.x}_{currentPosition.z}";
        }

        private void SetState(State state)
        {
            this.state = state;
            stateChange.Invoke(state);
        }

        public State GetState()
        {
            return state;
        }

        public void Save()
        {
            var lands = Player.INSTANCE.GetOwnedLands();
            if (lands == null || lands.Count == 0) return;
            var wallet = Settings.WalletId();
            var service = VoxelService.INSTANCE;
            if (!service.HasChange()) return;

            var worldChanges = service.GetLandsChanges(wallet, lands);
            SetState(State.LOADING);
            Loading.INSTANCE.UpdateText("Saving Changes To Files...");
            StartCoroutine(IpfsClient.INSATANCE.Upload(worldChanges, result =>
            {
                SetState(State.BROWSER_CONNECTION);
                //TODO: Reload lands for player and double check saved lands, remove keys from changed lands
                BrowserConnector.INSTANCE.Save(result, () => StartCoroutine(ReloadOwnerLands()),
                    () => SetState(State.PLAYING));
            }));
        }

        public void Buy(List<Land> lands)
        {
            SetState(State.BROWSER_CONNECTION);
            BrowserConnector.INSTANCE.Buy(lands,
                () => StartCoroutine(ReloadOwnerLands()),
                () => SetState(State.PLAYING));
        }

        public void Transfer(long landId)
        {
            SetState(State.BROWSER_CONNECTION);
            BrowserConnector.INSTANCE.Transfer(landId,
                () => StartCoroutine(ReloadLands()),
                () => SetState(State.PLAYING));
        }

        public void SetNFT(Land land, bool convertToNft)
        {
            if (convertToNft)
            {
                StartCoroutine(GameObject.Find("Map").GetComponent<Map>().TakeNftScreenShot(land, screenshot =>
                {
                    StartCoroutine(IpfsClient.INSATANCE.UploadScreenShot(screenshot,
                        ipfsKey => SetLandMetadata(ipfsKey, land.id), () =>
                        {
                            var dialog = INSTANCE.OpenDialog();
                            dialog
                                .WithTitle("Failed to upload screenshot")
                                .WithContent("Dialog/TextContent")
                                .WithAction("OK", () =>
                                {
                                    INSTANCE.CloseDialog(dialog);
                                    SetState(State.PLAYING);
                                });
                            dialog.GetContent().GetComponent<TextMeshProUGUI>().text =
                                "Conversion to NFT cancelled. Click OK to continue.";
                        }));
                }));
            }
            else
            {
                SetState(State.BROWSER_CONNECTION);
                BrowserConnector.INSTANCE.SetNft(land.id, false,
                    () => StartCoroutine(ReloadLands()),
                    () => SetState(State.PLAYING));
            }
        }

        private void SetLandMetadata(string key, long landId)
        {
            StartCoroutine(RestClient.INSATANCE.SetLandMetadata(new LandMetadata(landId, key), () =>
            {
                SetState(State.BROWSER_CONNECTION);
                BrowserConnector.INSTANCE.SetNft(landId, true,
                    () => StartCoroutine(ReloadLands()),
                    () => SetState(State.PLAYING));
            }, () =>
            {
                var dialog = INSTANCE.OpenDialog();
                dialog
                    .WithTitle("Failed to update land metadata")
                    .WithContent("Dialog/TextContent")
                    .WithAction("OK", () =>
                    {
                        INSTANCE.CloseDialog(dialog);
                        SetState(State.PLAYING);
                    });
                dialog.GetContent().GetComponent<TextMeshProUGUI>().text =
                    "Conversion to NFT cancelled. Click OK to continue.";
            }));
        }

        public void ToggleMovingObjectState(TdObjectBlockObject tdObjectBlockObject)
        {
            if (GetState() == State.PLAYING)
            {
                SetState(State.MOVING_OBJECT);
                tdObjectBlockObject.SetToMovingState();
            }
            else if (GetState() == State.MOVING_OBJECT)
            {
                SetState(State.PLAYING);
                tdObjectBlockObject.ExitMovingState();
            }
        }

        public void ShowProfile(Profile profile, Land currentLand)
        {
            if (GetState() == State.PLAYING || GetState() == State.SETTINGS)
            {
                if (currentLand == null)
                {
                    var profileDialog = ProfileDialog.INSTANCE;
                    profileDialog.Open(profile);
                }
                else
                {
                    var landProfileDialog = LandProfileDialog.INSTANCE;
                    landProfileDialog.Open(currentLand, profile);
                }
            }
        }

        public void EditProfile()
        {
            if (LandProfileDialog.INSTANCE.gameObject.activeSelf)
                LandProfileDialog.INSTANCE.Close();
            SetState(State.BROWSER_CONNECTION);
            BrowserConnector.INSTANCE.EditProfile(() =>
            {
                SetState(State.PLAYING);
                Owner.INSTANCE.OnProfileEdited();
            }, () => { SetState(State.PLAYING); });
        }

        private IEnumerator ReloadLands()
        {
            SetState(State.LOADING);
            Loading.INSTANCE.UpdateText("Reloading Lands...");
            yield return VoxelService.INSTANCE.ReloadLands();
            var player = Player.INSTANCE;
            player.ResetLands();
            yield return InitWorld(player.transform.position, true);
        }

        private IEnumerator ReloadOwnerLands()
        {
            SetState(State.LOADING);
            Loading.INSTANCE.UpdateText("Reloading Your Lands...");
            yield return VoxelService.INSTANCE.ReloadLandsFor(Settings.WalletId());
            var player = Player.INSTANCE;
            player.ResetLands();
            yield return InitWorld(player.transform.position, true);
        }

        public Dialog OpenDialog()
        {
            var go = Instantiate(Resources.Load<GameObject>("Dialog/Dialog"), GameObject.Find("Canvas").transform);
            SetState(State.DIALOG);
            var dialog = go.GetComponent<Dialog>();
            dialogs.Add(dialog);
            return dialog;
        }

        public void CloseDialog(Dialog dialog)
        {
            Destroy(dialog.gameObject);
            dialogs.Remove(dialog);
            if (dialogs.Count == 0)
                SetState(State.PLAYING);
        }

        public static GameManager INSTANCE
        {
            get { return GameObject.Find("GameManager").GetComponent<GameManager>(); }
        }

        public enum State
        {
            LOADING,
            SETTINGS,
            PLAYING,
            MAP,
            BROWSER_CONNECTION,
            INVENTORY,
            HELP,
            DIALOG,
            PROFILE_DIALOG,
            MOVING_OBJECT
        }
    }
}