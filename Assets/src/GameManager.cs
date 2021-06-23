using System;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    private bool worldInited = false;
    public readonly UnityEvent<State> stateChange = new UnityEvent<State>();
    private State state = State.LOADING;

    void Start()
    {
        SetState(State.LOADING);
        StartCoroutine(VoxelService.INSTANCE.Initialize(Loading.INSTANCE, () => this.SetPlayerPosition()));
    }

    private void SetPlayerPosition()
    {
        if (string.IsNullOrWhiteSpace(Settings.WalletId()))
        {
            SetState(State.SETTINGS);
            return;
        }
        SetState(State.LOADING);
        Loading.INSTANCE.UpdateText("Initializing the world...");



        var player = Player.INSTANCE;
        player.OnWalletChanged();

        var pos = new Vector3(10, Chunk.CHUNK_HEIGHT + 10, 10);

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

        player.transform.position = pos;
        World.INSTANCE.Initialize(new VoxelPosition(pos).chunk);
        worldInited = true;
        SetState(State.PLAYING);
    }

    private Vector3 FindStartingY(Vector3 pos)
    {
        var service = VoxelService.INSTANCE;
        var feet = Vectors.FloorToInt(pos);
        while (true)
        {
            bool coll = false;
            for (int i = -1; i < 3; i++)
            {
                if (coll = service.IsSolid(new VoxelPosition(feet + Vector3Int.up * 1)))
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
        bool escape = Input.GetKeyDown(KeyCode.Escape);
        if (escape && state != State.LOADING)
            SetState((state == State.SETTINGS && worldInited) ? State.PLAYING : State.SETTINGS);
    }

    internal void ExitSettings()
    {
        if (worldInited) SetState(State.PLAYING);
        else SetPlayerPosition();
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

    public void WalletChanged()
    {
        SetState(State.LOADING);
        SetPlayerPosition();
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
        LOADING, SETTINGS, PLAYING
    }
}
