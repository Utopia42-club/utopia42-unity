using Source.Model;
using Source.Service.Auth;
using Source.Utils;
using UnityEngine.UIElements;

namespace Source.Ui.Menu
{
    public class NewMetaverseMenu : UxmlElement
    {
        private readonly VisualElement searchResultContainer;

        public NewMetaverseMenu() : base(typeof(NewMetaverseMenu), true)
        {
            var currentContract = AuthService.Instance.CurrentContract;
            var currentContractView = this.Q("CurrentContract");
            currentContractView.Q<Label>("ContractName")
                .text = currentContract?.name;
            currentContractView.Q<Label>("ContractId")
                .text = currentContract?.id;
            if (currentContract?.createdAt != null)
            {
                currentContractView.Q<Label>("Date")
                    .text = Temporals.FromEpochSeconds(currentContract.createdAt)
                    .ToString("dd MMM yyy");
            }

            searchResultContainer = this.Q("SearchResult");
            var searchField = this.Q<TextField>();
            // Autocomplete<object>.CreateSearchRequestObservable(searchField)
                // .Subscribe(o => );
        }

        private class MetaverseCard : VisualElement
        {
            private readonly MetaverseContract contract;

            public MetaverseCard(MetaverseContract contract)
            {
                this.contract = contract;
                AddToClassList("metaverse-card");
                var hl = new VisualElement();
                hl.AddToClassList("hover-layer");
                Add(hl);
                var nl = new Label(contract.name);
                nl.AddToClassList("metaverse-name");
                Add(nl);
                var il = new Label(contract.id);
                il.AddToClassList("metaverse-id");
                Add(il);
            }
        }
    }
}