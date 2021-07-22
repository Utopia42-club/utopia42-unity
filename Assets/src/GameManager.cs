using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    private bool worldInited = false;
    public readonly UnityEvent<State> stateChange = new UnityEvent<State>();
    private State state = State.LOADING;

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

        var lands = player.GetLands();
        if (lands.Count > 0)
        {
            var land = lands[0];
            pos = new Vector3(
                 ((float)(land.x1 + land.x2)) / 2,
                 Chunk.CHUNK_HEIGHT + 10,
                 ((float)(land.y1 + land.y2)) / 2);
        }

        pos = FindStartingY(pos);

        StartCoroutine(DoMovePlayerTo(pos, true));
    }

    public void MovePlayerTo(Vector3 pos)
    {
        StartCoroutine(DoMovePlayerTo(pos, false));
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
        SetState(State.LOADING);
        Loading.INSTANCE.UpdateText("Creating the world...");
        yield return null;
        var world = World.INSTANCE;
        while (!world.Initialize(new VoxelPosition(pos).chunk, clean)) yield return null;
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
        if (Input.GetButtonDown("Cancel") && worldInited && (state == State.MAP || state == State.SETTINGS))
            SetState(State.PLAYING);
        else if (Input.GetButtonDown("Menu") && state == State.PLAYING)
            SetState(State.SETTINGS);
        else if (Input.GetButtonDown("Map"))
        {
            if (state == State.MAP)
                SetState(State.PLAYING);
            else if (state == State.PLAYING)
                SetState(State.MAP);
        }
    }

    internal void ExitSettings()
    {
        if (worldInited) SetState(State.PLAYING);
        else InitPlayerForWallet();
    }

    internal void SettingsChanged()
    {
        if (!EthereumClientService.INSTANCE.IsInited())
        {
            EthereumClientService.INSTANCE.SetNetwork(Settings.Network());
            SetState(State.LOADING);
            StartCoroutine(VoxelService.INSTANCE.Initialize(Loading.INSTANCE, () => this.InitPlayerForWallet()));
        }
        else
        {
            SetState(State.LOADING);
            InitPlayerForWallet();
        }
    }

    private void SetState(State state)
    {
        this.state = state;
        stateChange.Invoke(state);
    }

    public State GetSTate()
    {
        return state;
    }

    public void Save()
    {
        var lands = Player.INSTANCE.GetLands();
        if (lands == null || lands.Count == 0) return;
        var wallet = Settings.WalletId();
        List<LandDetails> worldChanges = VoxelService.INSTANCE.GetLandsChanges(wallet, lands);
        SetState(State.LOADING);
        Loading.INSTANCE.UpdateText("Saving Changes To Files...");
        StartCoroutine(IpfsClient.INSATANCE.Upload(worldChanges, ids =>
        {
            SetState(State.BROWSER_CONNECTION);
            Action done = () => SetState(State.PLAYING);
            BrowserConnector.INSTANCE.Save(ids, done, done);
        }));
    }

    public void Buy(List<Land> lands)
    {
        SetState(State.BROWSER_CONNECTION);
        BrowserConnector.INSTANCE.Buy(lands,
            () => StartCoroutine(ReloadOwnerLands()),
            () => SetState(State.PLAYING));
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

    public static GameManager INSTANCE
    {
        get
        {
            return GameObject.Find("GameManager").GetComponent<GameManager>();
        }
    }

    public enum State
    {
        LOADING, SETTINGS, PLAYING, MAP, BROWSER_CONNECTION
    }
}
