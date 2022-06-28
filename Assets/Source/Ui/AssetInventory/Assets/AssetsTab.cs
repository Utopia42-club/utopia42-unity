using System;
using System.Collections.Generic;
using Source.Ui.AssetInventory.Models;
using Source.Ui.TabPane;
using Source.Ui.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.AssetInventory.Assets
{
    internal partial class AssetsTab : UxmlElement, TabOpenListener, TabCloseListener
    {
        private readonly List<IDisposable> disposables = new();
        private readonly VisualElement root;
        private readonly DataLoader dataLoader;
        private readonly TextField searchField;
        private string lastSearchFilter = "";
        private TabPane.TabPane tabPane;
        private VisualElement content;

        public AssetsTab(AssetsInventory inventory, VisualElement loadingTarget)
            : base(typeof(AssetsTab), true)
        {
            dataLoader = new DataLoader(loadingTarget);
            root = this.Q("root");
            searchField = this.Q<TextField>("searchField");
            searchField.multiline = false;
            TextFields.SetPlaceHolderForTextField(searchField, "Search");
            TextFields.RegisterUiEngagementCallbacksForTextField(searchField);
            searchField.RegisterValueChangedCallback(new DebounceEventListener<ChangeEvent<string>>
                (inventory, 0.6f, e => FilterAssets(e.newValue)).Deligate);
        }

        public void OnTabOpen(TabOpenEvent e)
        {
            tabPane = e.TabPane;
            OpenCategoryList();
        }

        private void OpenCategoryList()
        {
            disposables.Add(dataLoader.GetCategories((categories) =>
            {
                var categoriesList = new AssetCategoryList(categories);
                SetContent(categoriesList);
                categoriesList.CategorySelected += OpenPackList;
            }));
        }

        private void OpenPackList(Category category)
        {
            disposables.Add(dataLoader.GetPacks(packs =>
            {
                var packList = new AssetPackList(packs, category);
                SetContent(packList);
                packList.BackRequested += OnBack;
                packList.LoadData(lastSearchFilter);
            }));
        }

        private void OnBack()
        {
            if (string.IsNullOrEmpty(lastSearchFilter))
                OpenCategoryList();
            searchField.value = null;
        }

        private void FilterAssets(string filter)
        {
            filter = filter ?? "";
            if (Equals(filter, lastSearchFilter))
                return;
            lastSearchFilter = filter;

            if (content is AssetPackList list)
                list.LoadData(lastSearchFilter);
            else OpenPackList(null);
        }

        public void OnTabClose(TabCloseEvent e)
        {
            SetContent(null);
            disposables.ForEach(d => d.Dispose());
            disposables.Clear();
        }

        private void SetContent(VisualElement content)
        {
            if (this.content != null)
                root.Remove(this.content);
            this.content = content;
            if (this.content != null)
                root.Add(this.content);
        }
    }
}