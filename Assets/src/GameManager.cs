using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using src.Canvas;
using src.Canvas.Map;
using src.MetaBlocks;
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
        public readonly List<Func<State, State, bool>> stateGuards = new List<Func<State, State, bool>>();

        private State state = State.LOADING;
        private State? previousState;

        private List<Dialog> dialogs = new List<Dialog>();
        private bool captureAllKeyboardInputOrig;

        public GameObject helpDialog;
        public Map map;

        private bool doubleCtrlTap = false;
        private double doubleCtrlTapTime;

        void Start()
        {
            SetState(State.SETTINGS);
            stateChange.AddListener(newState => { BrowserConnector.INSTANCE.ReportGameState(newState); });
        }

        void Update()
        {
            if (Input.GetButtonDown("Cancel"))
            {
                if (state == State.PLAYING && MouseLook.INSTANCE.cursorLocked)
                    MouseLook.INSTANCE.UnlockCursor();
                else
                    ReturnToGame();
            }
            else if (state == State.PLAYING)
            {
                if (IsControlKeyDown() && doubleCtrlTap)
                {
                    if (Time.time - doubleCtrlTapTime < 0.4f)
                    {
                        OpenPluginsDialog();
                        doubleCtrlTapTime = 0f;
                    }

                    doubleCtrlTap = false;
                }
                else if (IsControlKeyDown() && !doubleCtrlTap)
                {
                    doubleCtrlTap = true;
                    doubleCtrlTapTime = Time.time;
                }
                else if (Input.GetButtonDown("Menu"))
                    SetState(State.SETTINGS);
                else if (Input.GetButtonDown("Map"))
                    SetState(State.MAP);
                else if (Input.GetButtonDown("Inventory"))
                    SetState(State.INVENTORY);
            }else if (worldInited && Input.GetButtonDown("Menu") && state == State.SETTINGS)
                SetState(State.PLAYING);
            else if (Input.GetButtonDown("Map") && state == State.MAP)
                SetState(State.PLAYING);
            else if (Input.GetButtonDown("Inventory") && state == State.INVENTORY)
                SetState(State.PLAYING);
        }

        public void OpenPluginsDialog()
        {
            if (state != State.PLAYING)
                SetState(State.PLAYING);
            if (WebBridge.IsPresent())
            {
                WebBridge.Call<object>("openPluginsDialog", "menu");
            }
        }

        private bool IsControlKeyDown()
        {
            return Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl)
                                                         || Input.GetKeyDown(KeyCode.LeftCommand) ||
                                                         Input.GetKeyDown(KeyCode.RightCommand);
        }

        public bool IsWorldInited()
        {
            return worldInited;
        }

        private void InitPlayerForWallet(Vector3? startingPosition)
        {
            if (string.IsNullOrWhiteSpace(Settings.WalletId()))
            {
                SetState(State.SETTINGS);
                return;
            }

            var player = Player.INSTANCE;
            player.ResetLands();

            Vector3 pos;

            if (startingPosition == null)
            {
                startingPosition = Player.GetSavedPosition();
                if (startingPosition.HasValue)
                    pos = startingPosition.Value;
                else
                {
                    var chunkSize = Chunk.CHUNK_SIZE;
                    pos = new Vector3(0, chunkSize.y + 10, 0);
                    var lands = player.GetOwnedLands();
                    if (lands.Count > 0)
                    {
                        var land = lands[0];
                        pos = land.startCoordinate.ToVector3() + land.endCoordinate.ToVector3();
                        pos /= 2;
                        pos.y = chunkSize.y + 10;
                    }
                }
            }
            else pos = startingPosition.Value;

            StartCoroutine(DoMovePlayerTo(pos, true));
        }

        private IEnumerator InitWorld(Vector3 pos, bool clean)
        {
            SetState(State.LOADING);
            Loading.INSTANCE.UpdateText("Creating the world\n0%");
            yield return null;
            var world = World.INSTANCE;
            while (!world.Initialize(new VoxelPosition(pos).chunk, clean)) yield return null;
            float total = world.CountChunksToCreate();
            while (world.CountChunksToCreate() > 0)
            {
                var perc = (total - world.CountChunksToCreate()) / total * 100;
                Loading.INSTANCE.UpdateText(string.Format("Creating the world\n{0}%", Mathf.FloorToInt(perc)));
                yield return null;
            }

            // If not stated, player might go through the ground (like there is no collider for the ground)
            yield return null;

            worldInited = true;
            SetState(State.PLAYING);
        }

        internal void OpenHelpDialog()
        {
            if (GetState() == State.PLAYING || GetState() == State.SETTINGS)
            {
                helpDialog.SetActive(true);
                SetState(State.HELP);
            }
        }

        public void MovePlayerTo(Vector3 pos)
        {
            StartCoroutine(DoMovePlayerTo(pos, false));
        }

        private IEnumerator DoMovePlayerTo(Vector3 pos, bool clean)
        {
            SetState(State.LOADING);
            Loading.INSTANCE.UpdateText("Positioning the player...");
            yield return FindStartingY(pos, result => pos = result);

            Player.INSTANCE.SetPosition(pos);
            yield return InitWorld(pos, clean);
        }

        private IEnumerator FindStartingY(Vector3 pos, Action<Vector3> consumer)
        {
            var service = WorldService.INSTANCE;
            var checksPerFrame = 100;
            var todo = checksPerFrame;

            var feet = Vectors.FloorToInt(pos) + new Vector3(.5f, .5f, .5f);
            while (true)
            {
                bool coll = false;
                for (int i = -1; i < 3; i++)
                {
                    var loaded = false;
                    service.IsSolid(new VoxelPosition(feet + Vector3Int.up * i), s =>
                    {
                        coll = s;
                        loaded = true;
                    });
                    if (!loaded)
                        yield return new WaitUntil(() => loaded);
                    if (coll)
                    {
                        feet += Vector3Int.up * Math.Max(1, i + 2);
                        break;
                    }
                }

                if (!coll)
                {
                    consumer.Invoke(feet);
                    yield break;
                }

                todo--;
                if (todo == 0)
                {
                    yield return null;
                    todo = checksPerFrame;
                }
            }
        }

        public bool OpenMap()
        {
            return SetState(State.MAP);
        }

        public bool OpenSettings()
        {
            return SetState(State.SETTINGS);
        }

        public void ReturnToGame()
        {
            if (!worldInited)
                return; //FIXME

            switch (state)
            {
                case State.PROFILE_DIALOG:
                    LandProfileDialog.INSTANCE.CloseIfOpened();
                    ProfileDialog.INSTANCE.CloseIfOpened();
                    break;
                case State.DIALOG when dialogs.Count > 0:
                    CloseDialog(dialogs[dialogs.Count - 1]);
                    break;
                case State.MAP:
                    if (LandProfileDialog.INSTANCE.gameObject.activeSelf)
                        LandProfileDialog.INSTANCE.CloseIfOpened();
                    else if (map.IsLandBuyDialogOpen())
                        map.CloseLandBuyDialogState();
                    else
                        SetState(State.PLAYING);
                    break;
                case State.HELP:
                    helpDialog.SetActive(false);
                    SetState(State.PLAYING);
                    break;
                case State.SETTINGS:
                case State.INVENTORY:
                case State.FREEZE:
                    SetState(State.PLAYING);
                    break;
            }
        }

        internal void ExitSettings(Vector3? startingPosition)
        {
            if (worldInited) SetState(State.PLAYING);
            else InitPlayerForWallet(startingPosition);
        }

        internal void SettingsChanged(EthNetwork network, Vector3? startingPosition)
        {
            if (!EthereumClientService.INSTANCE.IsInited())
            {
                EthereumClientService.INSTANCE.SetNetwork(network);
                SetState(State.LOADING);
                StartCoroutine(WorldService.INSTANCE.Initialize(Loading.INSTANCE,
                    () => InitPlayerForWallet(startingPosition), () => { Loading.INSTANCE.ShowConnectionError(); }));
            }
            else
            {
                SetState(State.LOADING);
                InitPlayerForWallet(startingPosition);
            }
        }

        public void SetProfileDialogState(bool open)
        {
            if (open && GetState() == State.PLAYING || GetState() == State.SETTINGS)
                SetState(State.PROFILE_DIALOG);
            else if (GetState() == State.PROFILE_DIALOG)
                SetState(State.PLAYING);
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
            var currentPosition = Player.INSTANCE.GetPosition();
            var url = Constants.WebAppBaseURL +
                      $"/game?position={currentPosition.x}_{currentPosition.y}_{currentPosition.z}";

            if (WebBridge.IsPresent())
                WebBridge.Call<object>("copyToClipboard", url);
            else
                GUIUtility.systemCopyBuffer = url;
        }

        private bool SetState(State state)
        {
            if (state == this.state)
                return false; //Or should we throw an exception?

            if (stateGuards.Any(guard => !guard.Invoke(this.state, state)))
            {
                Debug.Log("State change prevented by guard : " + this.state + " -> " + state);
                return false;
            }

            previousState = this.state;
            this.state = state;
            stateChange.Invoke(state);

            return true;
        }

        public State GetState()
        {
            return state;
        }

        public void Save()
        {
            StartCoroutine(DoSave());
        }

        private IEnumerator DoSave()
        {
            var openFailureDialog = new Action(() =>
            {
                var dialog = OpenDialog();
                dialog
                    .WithTitle("Failed to save your lands!")
                    .WithAction("Retry", () => Save())
                    .WithAction("OK", () => CloseDialog(dialog));
            });


            var lands = Player.INSTANCE.GetOwnedLands();
            if (lands == null || lands.Count == 0) yield break;
            var wallet = Settings.WalletId();
            var service = WorldService.INSTANCE;
            if (!service.HasChange()) yield break;
            SetState(State.LOADING);
            Loading.INSTANCE.UpdateText("Preparing your changes...");

            Dictionary<long, LandDetails> worldChanges = null;
            yield return service.GetLandsChanges(wallet, lands, changes => worldChanges = changes,
                () => worldChanges = null);

            if (worldChanges == null)
            {
                openFailureDialog();
                yield break;
            }

            Loading.INSTANCE.UpdateText("Saving data on IPFS...");

            var done = 0;
            var hashes = new Dictionary<long, string>();
            var failed = false;
            foreach (var changeEntry in worldChanges)
            {
                yield return LandDetailsService.INSTANCE.Save(changeEntry.Value, h => hashes[changeEntry.Key] = h,
                    () => failed = true);
                if (failed)
                {
                    openFailureDialog();
                    yield break;
                }

                done++;
                Loading.INSTANCE.UpdateText($"Saving data on IPFS...\n {done}/{worldChanges.Count}");
            }

            //TODO: Reload lands for player and double check saved lands, remove keys from changed lands
            BrowserConnector.INSTANCE.Save(hashes, () => StartCoroutine(ReloadOwnerLands()),
                () => SetState(State.PLAYING));
        }

        public void Buy(List<Land> lands)
        {
            BrowserConnector.INSTANCE.Buy(lands,
                () => StartCoroutine(ReloadOwnerLands()),
                () => SetState(State.PLAYING));
        }

        public void Transfer(long landId)
        {
            BrowserConnector.INSTANCE.Transfer(landId,
                () => StartCoroutine(ReloadLandOwnerAndNft(landId, true)),
                () => SetState(State.PLAYING));
        }

        public void SetNFT(Land land, bool convertToNft)
        {
            if (convertToNft)
            {
                StartCoroutine(GameObject.Find("Map").GetComponent<Map>().TakeNftScreenShot(land, screenshot =>
                {
                    // using(var ms = new MemoryStream(screenshot)) {
                    //     using(var fs = new FileStream("nftImg", FileMode.Create)) {
                    //         ms.WriteTo(fs);
                    //     }
                    // }
                    StartCoroutine(IpfsClient.INSATANCE.UploadImage(screenshot,
                        ipfsKey => SetLandNftImage(ipfsKey, land), () =>
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
                BrowserConnector.INSTANCE.SetNft(land.id, false,
                    () => StartCoroutine(ReloadLandOwnerAndNft(land.id, false)), () => SetState(State.PLAYING));
            }
        }

        private void SetLandNftImage(string key, Land land)
        {
            StartCoroutine(WorldRestClient.INSTANCE.SetLandMetadata(new LandMetadata(land.id, key), () =>
            {
                BrowserConnector.INSTANCE.SetNft(land.id, true,
                    () => StartCoroutine(ReloadLandOwnerAndNft(land.id, false)),
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

        public void ToggleMovingObjectState(MetaBlockObject metaBlockObject)
        {
            if (GetState() == State.PLAYING)
            {
                SetState(State.MOVING_OBJECT);
                metaBlockObject.SetToMovingState();
            }
            else if (GetState() == State.MOVING_OBJECT)
            {
                SetState(State.PLAYING);
                metaBlockObject.ExitMovingState();
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
                    LandProfileDialog.INSTANCE.Open(currentLand, profile);
                }
            }
        }

        public void EditProfile()
        {
            LandProfileDialog.INSTANCE.CloseIfOpened();
            BrowserConnector.INSTANCE.EditProfile(() =>
            {
                SetState(State.PLAYING);
                Owner.INSTANCE.OnProfileEdited();
            }, () => { SetState(State.PLAYING); });
        }

        private IEnumerator ReloadLandOwnerAndNft(long id, bool reCreateWorld)
        {
            SetState(State.LOADING);
            Loading.INSTANCE.UpdateText($"Reloading Land {id}...");

            var failed = false;
            yield return WorldService.INSTANCE.ReloadLandOwnerAndNft(id, () => { }, () =>
            {
                failed = true;
                Loading.INSTANCE.ShowConnectionError();
            });
            if (failed) yield break;

            var player = Player.INSTANCE;
            player.ResetLands();
            if (reCreateWorld)
                yield return InitWorld(player.GetPosition(), true);
            else SetState(State.PLAYING);
        }

        private IEnumerator ReloadOwnerLands()
        {
            SetState(State.LOADING);
            Loading.INSTANCE.UpdateText("Reloading Your Lands...");

            var failed = false;
            yield return WorldService.INSTANCE.ReloadPlayerLands(() =>
            {
                failed = true;
                Loading.INSTANCE.ShowConnectionError();
            });
            if (failed) yield break;

            var player = Player.INSTANCE;
            player.ResetLands();
            yield return InitWorld(player.GetPosition(), true);
        }

        public Dialog OpenDialog(State targetState = State.DIALOG)
        {
            var go = Instantiate(Resources.Load<GameObject>("Dialog/Dialog"), GameObject.Find("Canvas").transform);
            SetState(targetState);
            var dialog = go.GetComponent<Dialog>();
            dialogs.Add(dialog);
            return dialog;
        }

        public void CloseDialog(Dialog dialog, State? targetState = null)
        {
            Destroy(dialog.gameObject);
            dialogs.Remove(dialog);
            if (dialogs.Count == 0)
            {
                SetState(targetState ?? (previousState ?? State.PLAYING));
            }
        }

        public void FreezeGame()
        {
            SetState(State.FREEZE);
#if UNITY_WEBGL
            captureAllKeyboardInputOrig = WebGLInput.captureAllKeyboardInput;
            WebGLInput.captureAllKeyboardInput = false;
#endif
        }

        public void UnFreezeGame()
        {
            ReturnToGame();
#if UNITY_WEBGL
            WebGLInput.captureAllKeyboardInput = captureAllKeyboardInputOrig == null || captureAllKeyboardInputOrig;
#endif
        }

        public void LockCursor()
        {
#if UNITY_WEBGL
            MouseLook.INSTANCE.LockCursor();
#endif
        }

        public void UnlockCursor()
        {
#if UNITY_WEBGL
            MouseLook.INSTANCE.UnlockCursor();
#endif
        }

        public void NavigateInMap(Land land)
        {
            var mapInputManager = GameObject.Find("InputManager").GetComponent<MapInputManager>();
            mapInputManager.NavigateInMap(land);
        }

        public static GameManager INSTANCE => GameObject.Find("GameManager").GetComponent<GameManager>();

        public enum State
        {
            LOADING,
            SETTINGS,
            PLAYING,
            MAP,
            INVENTORY,
            HELP,
            DIALOG,
            PROFILE_DIALOG,
            MOVING_OBJECT,
            FREEZE
        }

        public void ShowConnectionError()
        {
            SetState(State.LOADING);
            Loading.INSTANCE.ShowConnectionError();
        }

        public void OpenInventory()
        {
            if (state == State.PLAYING)
                SetState(State.INVENTORY);
        }
    }
}