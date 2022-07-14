using Source.MetaBlocks.TeleportBlock;
using Source.Service.Auth;
using Source.Utils;
using UnityEngine.UIElements;

namespace Source.Ui.Menu
{
    public class MetaverseMenu : UxmlElement
    {
        public MetaverseMenu() : base(typeof(MetaverseMenu), true)
        {
            var currentCard = this.Q("current");
            var currentPositionProps = new TeleportPropertiesEditor();
            currentPositionProps.SetEditable(false);
            currentPositionProps.SetDestinationLabel("Position");
            currentCard.Q("content")
                .Add(currentPositionProps);
            currentCard.Q<Button>("action").clicked += () => GameManager.INSTANCE.CopyPositionLink();
            var currentContract = AuthService.Instance.CurrentContract;
            var playerPos = Vectors.FloorToInt(Player.INSTANCE.GetPosition());
            currentPositionProps.SetValue(new TeleportBlockProperties()
            {
                contractAddress = currentContract.address,
                networkId = currentContract.networkId,
                destination = new[] {playerPos.x, playerPos.y, playerPos.z}
            });


            var teleportPropertiesEditor = new TeleportPropertiesEditor();
            teleportPropertiesEditor.SetValue(null);
            var exploreCard = this.Q("explore");
            exploreCard.Q("content")
                .Add(teleportPropertiesEditor);
            exploreCard.Q<Button>("action").clicked +=
                () => TeleportPortal.TeleportIfValid(teleportPropertiesEditor.GetValue());
        }
    }
}