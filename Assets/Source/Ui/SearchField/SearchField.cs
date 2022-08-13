using System;
using System.Collections.Generic;
using Source.Reactive.Producer;
using UnityEngine.UIElements;

namespace Source.Ui.SearchField
{
    public class SearchField : TextField
    {
        private Autocomplete<object> autocomplete;
        private readonly VisualElement valueView;
        private object item;
        private Func<object, string> stringifier;
        private Func<object, VisualElement> viewFactory;

        public SearchField()
        {
            styleSheets.Add(UxmlElement.LoadStyleSheet(typeof(SearchField)));
            valueView = new VisualElement();
            valueView.AddToClassList("utopia-search-field-value-view");
            Add(valueView);
            valueView.SendToBack();
            viewFactory = v => new Label(stringifier?.Invoke(v) ?? v.ToString());
            RegisterCallback<FocusOutEvent>(e => value = null);
            this.RegisterValueChangedCallback(e => UpdateLabel());
        }

        public SearchField WithDataLoader(Func<string, Observable<List<object>>> dataLoader)
        {
            if (autocomplete != null)
            {
                autocomplete.OptionSelected -= SetItem;
                autocomplete?.Dispose();
            }

            autocomplete = new Autocomplete<object>(this, dataLoader, viewFactory);
            autocomplete.OptionSelected += SetItem;

            return this;
        }

        public SearchField WithViewFactory(Func<object, VisualElement> viewFactory)
        {
            autocomplete?.SetOptionViewFactory(viewFactory);
            this.viewFactory = viewFactory;
            UpdateLabel();
            return this;
        }

        public SearchField WithStringifier(Func<object, string> stringifier)
        {
            this.stringifier = stringifier;
            UpdateLabel();
            return this;
        }

        private void UpdateLabel()
        {
            valueView.Clear();
            if (string.IsNullOrEmpty(value) && item != null)
                valueView.Add(viewFactory.Invoke(item));
        }

        public void SetItem(object item)
        {
            this.item = item;
            UpdateLabel();
        }

        public T GetItem<T>()
        {
            return (T) item;
        }

        public new class UxmlFactory : UxmlFactory<SearchField, UxmlTraits>
        {
        }
    }
}