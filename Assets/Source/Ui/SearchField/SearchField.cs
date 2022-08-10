using System;
using System.Collections.Generic;
using Source.Reactive.Producer;
using UnityEngine.UIElements;

namespace Source.Ui.SearchField
{
    public class SearchField : TextField
    {
        private Autocomplete<object> autocomplete;
        private readonly Label valueLabel;
        private object item;
        private Func<object, string> stringifier;

        public SearchField()
        {
            styleSheets.Add(UxmlElement.LoadStyleSheet(typeof(SearchField)));
            valueLabel = new Label();
            valueLabel.AddToClassList("utopia-search-field-value-label");
            Add(valueLabel);
            valueLabel.SendToBack();
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

            autocomplete = new Autocomplete<object>(this, dataLoader);
            autocomplete.OptionSelected += SetItem;

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
            if (string.IsNullOrEmpty(value))
                valueLabel.text = item == null ? null : stringifier?.Invoke(item) ?? item.ToString();
            else
                valueLabel.text = null;
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

        public new class UxmlFactory : UxmlFactory<SearchField, TextField.UxmlTraits>
        {
        }
    }
}