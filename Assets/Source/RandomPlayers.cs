using System.Collections;
using Source;
using Source.Model;
using UnityEngine;
using SystemRandom = System.Random;

public class RandomPlayers : MonoBehaviour
{
    public int numberOfPlayers = 10;
    private readonly SystemRandom random = new();

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.T) || !Input.GetKey(KeyCode.LeftShift)) return;

        for (var i = 0; i < numberOfPlayers; i++)
        {
            var pos = Player.INSTANCE.GetPosition() + (Vector3.one * i * 2);
            pos.y = 32;
            Players.INSTANCE.ReportOtherPlayersState(new AvatarController.PlayerState(i + "",
                new SerializableVector3(pos), false, false, true
            ));
        }

        StartCoroutine(MovePlayers());
    }

    private IEnumerator MovePlayers()
    {
        yield return 0;
        while (true)
        {
            var i = random.Next(0, numberOfPlayers - 1);
            var wallet = i + "";
            Players.INSTANCE.playersMap.TryGetValue(wallet, out var player);
            Players.INSTANCE.ReportOtherPlayersState(new AvatarController.PlayerState(i + "",
                new SerializableVector3(player.GetState().GetPosition() + RandomNextStep(i)),
                false, RandomJump(), true
            ));

            yield return new WaitForSeconds(0.1f);
        }
    }

    private bool RandomJump()
    {
        return false;
        return random.Next(0, 20) < 3;
    }

    private Vector3 RandomNextStep(int i)
    {
        return i % 2 == 0 ? new Vector3(RandomNext(), 0, 0) : new Vector3(0, 0, RandomNext());
    }

    private float RandomNext()
    {
        return random.Next(0, 2) * 0.5f;
    }
}