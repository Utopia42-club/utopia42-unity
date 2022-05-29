using System.Collections;
using src;
using src.Model;
using UnityEngine;
using SystemRandom = System.Random;

public class RandomPlayers : MonoBehaviour
{
    public int numberOfPlayers = 10;
    public Players players;

    private SystemRandom _random = new SystemRandom();

    void Start()
    {
        var done = false;
        GameManager.INSTANCE.stateChange.AddListener(state =>
        {
            if (state == GameManager.State.PLAYING && !done)
            {
                for (int i = 0; i < numberOfPlayers; i++)
                {
                    players.ReportOtherPlayersState(new AvatarController.PlayerState(
                        i + "",
                        new SerializableVector3(Player.INSTANCE.GetPosition() + (Vector3.one * i * 2)),
                        new SerializableVector3(Vector3.one),
                        false, false
                    ), false);
                }

                StartCoroutine(MovePlayers());
                done = true;
            }
        });
    }

    private IEnumerator MovePlayers()
    {
        while (true)
        {
            if (GameManager.INSTANCE.GetState() == GameManager.State.PLAYING)
            {
                var i = _random.Next(0, numberOfPlayers - 1);
                var wallet = i + "";
                players.playersMap.TryGetValue(wallet, out var player);
                player.SetIsAnotherPlayer(false);
                player.UpdatePlayerState(
                    new AvatarController.PlayerState(
                        wallet,
                        new SerializableVector3(player.GetState().Position() + new Vector3(
                            _random.Next(-1, 1), _random.Next(-1, 1), _random.Next(-1, 1)
                        )),
                        new SerializableVector3(Quaternion.Euler(0, 90, 0) * player.GetState().Forward()),
                        false, false
                    )
                );
                player.ReportToServer();
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
}