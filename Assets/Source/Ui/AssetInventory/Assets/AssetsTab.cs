﻿using System;
using System.Collections.Generic;
using Source.Model.Inventory;
using Source.Ui.TabPane;
using Source.Ui.Utils;
using UnityEngine.UIElements;
using static Source.Reactive.Consumer.Observers;

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

        public AssetsTab(VisualElement loadingTarget)
            : base(typeof(AssetsTab), true)
        {
            dataLoader = new DataLoader(loadingTarget);
            root = this.Q("root");
            searchField = this.Q<TextField>("searchField");
            searchField.multiline = false;
            TextFields.SetPlaceHolderForTextField(searchField, "Search");
            searchField.RegisterValueChangedCallback(
                Map<ChangeEvent<string>, string>(e => e.newValue)
                    .Pipe(Debounce<string>(searchField, 600))
                    .Pipe(DistinctUntilChanged<string>())
                    .Then(e => FilterAssets()).Observe);
        }

        public void OnTabOpen(TabOpenEvent e)
        {
            tabPane = e.TabPane;
            OpenCategoryList();
        }

        private void OpenCategoryList()
        {
            DisposeAll();
            disposables.Add(dataLoader.GetCategories((categories) =>
            {
                var categoriesList = new AssetCategoryList(categories);
                SetContent(categoriesList);
                categoriesList.CategorySelected += OpenPackList;
            }));
        }

        private void OpenPackList(Category category)
        {
            DisposeAll();
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

        private void FilterAssets()
        {
            var filter = searchField.text ?? "";
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
            DisposeAll();
        }

        private void DisposeAll()
        {
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