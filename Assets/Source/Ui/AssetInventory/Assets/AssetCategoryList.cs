using System;
using System.Collections.Generic;
using Source.Ui.AssetInventory.Models;
using Source.Ui.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.AssetInventory.Assets
{
    internal class AssetCategoryList : ScrollView
    {
        internal Category SelectedCategory { private set; get; }
        internal event Action<Category> CategorySelected = (c) => { };

        public AssetCategoryList(List<Category> categories)
        {
            AddToClassList("categories");
            Scrolls.IncreaseScrollSpeed(this, 600);
            mode = ScrollViewMode.Vertical;
            verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible;
            foreach (var category in categories)
            {
                var button = new CategoryButton(category);
                button.Selected += () =>
                {
                    SelectedCategory = category;
                    CategorySelected.Invoke(category);
                };
                Add(button);
            }
        }
    }

    internal class CategoryButton : UxmlElement
    {
        public event Action Selected = () => { };

        internal CategoryButton(Category category)
            : base(typeof(CategoryButton))
        {
            var label = this.Q<Label>("label");
            label.text = category.name;

            var image = this.Q("image");

            AssetsInventory.INSTANCE
                .StartCoroutine(UiImageUtils.SetBackGroundImageFromUrl(category.thumbnailUrl,
                    Resources.Load<Sprite>("Icons/loading"), image));

            this.Q<Button>().clickable.clicked += () => Selected.Invoke();
        }
    }
}