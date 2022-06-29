using System;
using System.Collections.Generic;
using Source.Ui.AssetInventory.Models;
using Source.Ui.Utils;
using UnityEngine.UIElements;

namespace Source.Ui.AssetInventory.Assets
{
    internal class AssetPackList : VisualElement
    {
        private readonly Category category;
        private readonly BreadCrumb breadCrumb;
        private readonly ScrollView scrollView;
        private readonly Dictionary<int, Pack> packs;
        internal event Action BackRequested = delegate { };

        public AssetPackList(Dictionary<int, Pack> packs, Category category)
        {
            this.packs = packs;
            this.category = category;
            AddToClassList("packs");
            breadCrumb = new BreadCrumb(category);
            breadCrumb.BackRequested += () => BackRequested();
            Add(breadCrumb);
            scrollView = new ScrollView(ScrollViewMode.Vertical)
            {
                scrollDecelerationRate = 0.135f,
                verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible,
                horizontalScrollerVisibility = ScrollerVisibility.Hidden
            };
            Scrolls.IncreaseScrollSpeed(scrollView);
            Add(scrollView);
        }

        internal void LoadData(string filter)
        {
            scrollView.Clear();
            var hasFilter = !string.IsNullOrEmpty(filter);
            if (!hasFilter && category == null)
            {
                BackRequested();
                return;
            }

            breadCrumb.Update(filter);

            var searchCriteria = new SearchCriteria
            {
                limit = 100,
                searchTerms = new Dictionary<string, object>()
            };
            if (hasFilter)
                searchCriteria.searchTerms.Add("generalSearch", filter);
            if (category != null)
                searchCriteria.searchTerms.Add("category", category.id);

            foreach (var packEntry in packs)
            {
                var foldout = new PackFoldout<AssetPackContent>(packEntry.Value.name, true);

                foldout.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue)
                        foldout.SetContent(new AssetPackContent(searchCriteria.Clone(), packEntry.Value));
                });
                if (hasFilter)
                    foldout.schedule.Execute(() => foldout.value = true);

                scrollView.Add(foldout);
            }
        }


        private class BreadCrumb : VisualElement
        {
            private readonly Label searchLabel;
            public event Action BackRequested = delegate { };

            public BreadCrumb(Category category)
            {
                AddToClassList("bread-crumb");
                var backButton = new Button();
                backButton.AddToClassList("back-button");
                backButton.AddToClassList("utopia-basic-button-primary");
                backButton.clicked += () => BackRequested();
                Add(backButton);
                if (category != null)
                    Add(new Label(category.name));
                searchLabel = new Label(category != null ? " - Search" : "Search");
            }

            internal void Update(string filter)
            {
                if (Contains(searchLabel))
                    Remove(searchLabel);
                if (!string.IsNullOrEmpty(filter))
                    Add(searchLabel);
            }
        }
    }
}