using Source;
using Source.MetaBlocks.TeleportBlock;
using Source.Ui;
using UnityEngine;
using UnityEngine.UIElements;

public class TeleportPortal : MonoBehaviour
{
    private CountDownTimer timer;
    private VisualElement root;

    private void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
    }

    private void OnDisable()
    {
        Clear();
    }

    private void Clear()
    {
        root?.Clear();
        timer?.Stop();
        timer = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Player.INSTANCE.IsPlayerCollider(other))
        {
            Clear();
            timer = new CountDownTimer(5);
            root.Add(timer);
            if (IsValid(GetProps()))
                timer.Start(() => DoTeleport());
            else timer.ShowMessage("Teleport data is not valid!");
        }
    }

    private void DoTeleport()
    {
        Clear();
        var props = GetProps();
        if (!IsValid(props))
            return;
        GameManager.INSTANCE.Teleport(props.networkId, props.contractAddress, new Vector3(props.destination[0],
            props.destination[1],
            props.destination[2]));
    }

    private TeleportBlockProperties GetProps()
    {
        return GetComponent<MetaFocusable>().GetBlock().GetProps() as TeleportBlockProperties;
    }

    private bool IsValid(TeleportBlockProperties props)
    {
        return props != null && props.destination != null && props.destination.Length == 3 &&
               !string.IsNullOrWhiteSpace(props.contractAddress) && props.networkId > 0;
    }

    private void OnTriggerExit(Collider other)
    {
        if (Player.INSTANCE.IsPlayerCollider(other))
            timer.Stop();
    }
}