using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Source.Canvas;
using Source.Configuration;
using Source.Model;
using Source.Service;
using Source.Service.Auth;
using Source.Ui.Dialog;
using Source.Ui.FocusLayer;
using Source.Ui.Map;
using Source.Ui.Profile;
using Source.Ui.Snack;
using Source.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Source
{
    public class GameManager : MonoBehaviour
    {
        public enum State
        {
            LOADING,
            PLAYING,
            MENU,
            LOGIN,
            INITIAL
        }
        public readonly UnityEvent<State> stateChange = new();
        public readonly List<Func<State, State, bool>> stateGuards = new();

        private string avatarLoadingMsg;

        private bool captureAllKeyboardInputOrig;

        private bool doubleCtrlTap;
        private double doubleCtrlTapTime;

        private State state = State.INITIAL;
        private int uiId;
        private bool worldInited;

        public static GameManager INSTANCE => GameObject.Find("GameManager").GetComponent<GameManager>();

        private void Start()
        {
            ResetAvatarMsg();
            SetState(State.LOGIN);
            var checkedForProfile = false;
            stateChange.AddListener(newState =>
            {
                BrowserConnector.INSTANCE.ReportGameState(newState);
                if (!checkedForProfile && state == State.PLAYING)
                {
                    checkedForProfile = true;
                    var authService = AuthService.Instance;
                    if (!authService.IsGuest())
                        ProfileLoader.INSTANCE.load(authService.WalletId(), profile =>
                        {
                            if (profile == null)
                                BrowserConnector.INSTANCE.EditProfile(() =>
                                {
                                    ProfileLoader.INSTANCE.InvalidateProfile(authService.WalletId());
                                    ProfileLoader.INSTANCE.load(authService.WalletId(),
                                        p =>
                                        {
                                            if (p != null) Player.INSTANCE.DoReloadAvatar(p.avatarUrl);
                                        }, () => { });
                                }, () => { });
                        }, () => { });
                }
            });
        }

        private void Update()
        {
            if (Input.GetButtonDown("Cancel"))
            {
                if (state == State.PLAYING && MouseLook.INSTANCE.cursorLocked)
                    MouseLook.INSTANCE.UnlockCursor();
                else
                    ReturnToGame();
            }
            else if (state == State.PLAYING && !IsTextInputFocused())
            {
                if (IsControlKeyDown())
                {
                    if (doubleCtrlTap)
                    {
                        if (Time.time - doubleCtrlTapTime < Mathf.Max(0.4f, Time.deltaTime + 0.1f))
                        {
                            OpenPluginsDialog();
                            doubleCtrlTapTime = 0f;
                        }

                        doubleCtrlTap = false;
                    }
                    else
                    {
                        doubleCtrlTap = true;
                        doubleCtrlTapTime = Time.time;
                    }
                }
                else if (Input.GetButtonDown("Menu") || Input.GetButtonDown("Map"))
                {
                    SetState(State.MENU);
                }
            }
            else if (worldInited && Input.GetButtonDown("Menu") && state == State.MENU)
            {
                SetState(State.PLAYING);
            }
        }

        public void OpenPluginsDialog()
        {
            if (state != State.PLAYING)
                SetState(State.PLAYING);
            if (WebBridge.IsPresent()) WebBridge.Call<object>("openPluginsDialog", "menu");
        }

        private bool IsControlKeyDown()
        {
            return Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl);
        }

        private void InitPlayerForWallet(Vector3? startingPosition)
        {
            if (!AuthService.Instance.HasSession())
            {
                SetState(State.LOGIN);
                return;
            }

            var player = Player.INSTANCE;
            player.ResetLands();

            Vector3 pos;

            if (startingPosition == null)
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
            else
            {
                pos = startingPosition.Value;
            }

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
        }

        private IEnumerator LoadAvatar()
        {
            SetState(State.LOADING);
            var msg = "";
            while (Player.INSTANCE.AvatarNotLoaded)
            {
                if (!msg.Equals(avatarLoadingMsg))
                {
                    msg = avatarLoadingMsg;
                    Loading.INSTANCE.UpdateText(msg);
                }

                yield return new WaitForSeconds(0.1f);
            }
        }

        public void ResetAvatarMsg()
        {
            avatarLoadingMsg = "Loading the avatar...";
        }

        public void ShowAvatarStateMessage(string msg, bool forceToast)
        {
            avatarLoadingMsg = msg;

            if (worldInited && GetState() == State.LOADING)
            {
                Loading.INSTANCE.UpdateText(msg);
                return;
            }

            if (worldInited || forceToast)
                new Toast(msg, Toast.ToastType.Warning).ShowWithCloseButtonDisabled();
        }

        public void Teleport(int networkId, string contract, Vector3 position)
        {
            var current = AuthService.Instance.CurrentContract;
            if (current.networkId == networkId &&
                string.Equals(current.address, contract, StringComparison.OrdinalIgnoreCase))
            {
                MovePlayerTo(position);
            }
            else
            {
                if (WorldService.INSTANCE.HasChange())
                    DialogService.INSTANCE.Show(new DialogConfig("Unsaved Changes!",
                            new Label("Your changes will be discarded, Are you sure?"))
                        .WithAction(new DialogAction("YES",
                            () => AuthService.Instance.ChangeContract(networkId, contract, position)))
                        .WithAction(new DialogAction("CANCEL", () => { }))
                    );
                else
                    AuthService.Instance.ChangeContract(networkId, contract, position);
            }
        }

        public void MovePlayerTo(Vector3 pos)
        {
            StartCoroutine(DoMovePlayerTo(pos, false));
        }

        private IEnumerator DoMovePlayerTo(Vector3 pos, bool clean)
        {
            SetState(State.LOADING);
            Player.INSTANCE.ResetVelocity();
            Loading.INSTANCE.UpdateText("Positioning the player...");
            yield return FindStartingY(pos, result => pos = result);

            Player.INSTANCE.SetTeleportTarget(pos);
            yield return InitWorld(pos, clean);
            yield return LoadAvatar();
            SetState(State.PLAYING);
        }

        private IEnumerator FindStartingY(Vector3 pos, Action<Vector3> consumer)
        {
            var service = WorldService.INSTANCE;
            var checksPerFrame = 100;
            var todo = checksPerFrame;

            var feet = Vectors.FloorToInt(pos) + new Vector3(.5f, .5f, .5f);
            while (true)
            {
                var coll = false;
                for (var i = -1; i < 3; i++)
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

        public void ReturnToGame()
        {
            if (!worldInited)
                return; //FIXME

            var dialogService = DialogService.INSTANCE;
            if (dialogService.IsAnyDialogOpen())
            {
                dialogService.CloseLastOpenedDialog();
                return;
            }

            switch (state)
            {
                case State.MENU:
                    SetState(State.PLAYING);
                    break;
            }
        }

        internal void SessionChanged(Vector3? startingPosition)
        {
            WorldService.Invalidate();
            Players.INSTANCE.Clear();
            SetState(State.LOADING);
            StartCoroutine(WorldService.INSTANCE.Initialize(Loading.INSTANCE,
                () => InitPlayerForWallet(startingPosition), () => { Loading.INSTANCE.ShowConnectionError(); }));
        }

        public void CopyPositionLink()
        {
            var contract = AuthService.Instance.CurrentContract;
            var currentPosition = Player.INSTANCE.GetPosition();
            var url = Configurations.Instance.webAppBaseURL +
                      $"/game?position={currentPosition.x}_{currentPosition.y}_{currentPosition.z}&network={contract.networkId}&contract={contract.address}";

            if (WebBridge.IsPresent())
                WebBridge.Call<object>("copyToClipboard", url);
            else
                GUIUtility.systemCopyBuffer = url;
            new Toast("Url copied to clipboard!", Toast.ToastType.Info).Show();
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
                var content = new VisualElement();
                DialogService.INSTANCE.Show(
                    new DialogConfig("Failed to save your lands!", content)
                        .WithAction(new DialogAction("Retry", Save, "utopia-button-secondary"))
                        .WithAction(new DialogAction("Ok", () => { SetState(State.PLAYING); }))
                );
            });

            var lands = Player.INSTANCE.GetOwnedLands();
            if (lands == null || lands.Count == 0) yield break;
            var wallet = AuthService.Instance.WalletId();
            var service = WorldService.INSTANCE;
            if (!service.HasChange())
            {
                SnackService.INSTANCE.Show(new SnackConfig(
                        new Toast("Lands are already saved", Toast.ToastType.Info))
                    .WithCloseButtonVisible(false));
                yield break;
            }

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

            Loading.INSTANCE.UpdateText("Issuing transaction...");
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
                () => { });
        }

        public void SetNFT(Map map, Land land, bool convertToNft)
        {
            if (convertToNft)
                StartCoroutine(map.TakeNftScreenShot(land, screenshot =>
                {
                    // using (var ms = new MemoryStream(screenshot))
                    // {
                    //     using (var fs = new FileStream("nftImg.jpg", FileMode.Create))
                    //     {
                    //         ms.WriteTo(fs);
                    //     }
                    // }

                    StartCoroutine(IpfsClient.INSATANCE.UploadImage(screenshot,
                        ipfsKey => SetLandNftImage(ipfsKey, land), () =>
                        {
                            var label = new Label
                            {
                                text = "Conversion to NFT cancelled. Click OK to continue."
                            };
                            DialogService.INSTANCE.Show(
                                new DialogConfig("Failed to upload screenshot", label)
                                    .WithAction(new DialogAction("Ok", () => { }))
                            );
                        }));
                }));
            else
                BrowserConnector.INSTANCE.SetNft(land.id, false,
                    () => StartCoroutine(ReloadLandOwnerAndNft(land.id, false)),
                    () => { });
        }

        private void SetLandNftImage(string key, Land land)
        {
            var contract = AuthService.Instance.CurrentContract;
            StartCoroutine(LandMetadataRestClient.INSTANCE.SetLandMetadata(
                new LandMetadata(contract.networkId, contract.address, land.id, key), () =>
                {
                    BrowserConnector.INSTANCE.SetNft(land.id, true,
                        () => StartCoroutine(ReloadLandOwnerAndNft(land.id, false)),
                        () => SetState(State.PLAYING));
                }, () =>
                {
                    var label = new Label
                    {
                        text = "Conversion to NFT cancelled. Click OK to continue."
                    };
                    DialogService.INSTANCE.Show(
                        new DialogConfig("Failed to update land metadata", label)
                            .WithAction(new DialogAction("Ok", () => { }))
                    );
                }));
        }

        // public void ShowProfile(Profile profile, Land currentLand)
        // {
        //     if (GetState() == State.PLAYING)
        //     {
        //         if (currentLand == null)
        //         {
        //             var userProfile = new UserProfile(profile);
        //             DialogService.INSTANCE.Show(new DialogConfig("User Profile", userProfile));
        //         }
        //         else
        //         {
        //             var landProfile = new LandProfile(currentLand);
        //             landProfile.SetProfile(profile);
        //             DialogService.INSTANCE.Show(
        //                 new DialogConfig("Land Profile", landProfile)
        //                     .WithWidth(new Length(70, LengthUnit.Percent))
        //                     .WithHeight(new Length(60, LengthUnit.Percent))
        //             );
        //         }
        //     }
        // }

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
            SetState(State.PLAYING);
        }

        public void FreezeGame()
        {
#if UNITY_WEBGL
            captureAllKeyboardInputOrig = WebGLInput.captureAllKeyboardInput;
            WebGLInput.captureAllKeyboardInput = false;
#endif
        }

        public void UnFreezeGame()
        {
#if UNITY_WEBGL
            WebGLInput.captureAllKeyboardInput = captureAllKeyboardInputOrig;
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

        public void ShowConnectionError()
        {
            SetState(State.LOADING);
            Loading.INSTANCE.ShowConnectionError();
        }

        public bool IsTextInputFocused()
        {
            return FocusLayer.Instance == null || FocusLayer.Instance.IsTextInputFocused();
        }

        public void Exit()
        {
            if (WebBridge.IsPresent())
                WebBridge.Call<object>("moveToHome", null);
            else
                Application.Quit();
        }

        public void OpenMenu()
        {
            SetState(State.MENU);
        }
    }
}