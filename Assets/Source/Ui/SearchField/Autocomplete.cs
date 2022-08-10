using System;
using System.Collections.Generic;
using Source.Reactive.Producer;
using Source.Ui.Popup;
using UnityEngine;
using UnityEngine.UIElements;
using static Source.Reactive.Producer.Observables;
using static Source.Ui.Utils.Observer.Observers;

namespace Source.Ui.SearchField
{
    public class Autocomplete<T>
    {
        public event Action<T> OptionSelected = a => { };
        private readonly CompositeSubscription subscription = new();
        private readonly TextField textField;
        private readonly Func<T, VisualElement> optionViewFactory;
        private readonly Func<string, Observable<List<T>>> dataLoader;
        private Subscription loadSubscription;
        private PopupController popup;
        private OptionList<T> optionList;

        public Autocomplete(TextField textField, Func<string, Observable<List<T>>> dataLoader)
            : this(textField, (item) => new Label(item.ToString()), dataLoader)
        {
        }

        public Autocomplete(TextField textField, Func<T, VisualElement> optionViewFactory,
            Func<string, Observable<List<T>>> dataLoader)
        {
            this.textField = textField;
            this.optionViewFactory = optionViewFactory;
            this.dataLoader = dataLoader;
            subscription.Add(FromEvent<InputEvent>(textField)
                .Subscribe(Map<InputEvent, string>(e => textField.value)
                    .Pipe(SkipUntil<string>(v => v != null))
                    .Pipe(Debounce<string>(textField, 600))
                    .Pipe(DistinctUntilChanged<string>())
                    .Then(e => Search())));
            subscription.Add(FromEvent<KeyDownEvent>(textField)
                .Subscribe(e =>
                {
                    if (e.ctrlKey && e.keyCode == KeyCode.Space)
                        Search();
                }));

            subscription.Add(FromEvent<FocusOutEvent>(textField)
                .Subscribe(e =>
                {
                    if (optionList?.HasFocus() == true) return;
                    Close();
                }));

            subscription.Add(FromEvent<DetachFromPanelEvent>(textField)
                .Subscribe(e =>
                {
                    Close();
                    loadSubscription?.Unsubscribe();
                }));
        }

        private void Search()
        {
            if (popup == null)
            {
                if (optionList == null)
                    optionList = new OptionList<T>(this);
                optionList.Clear();
                popup = PopupService.INSTANCE.Show(new PopupConfig(optionList, textField, Side.Bottom)
                    .WithBackDropLayer(false));
            }

            optionList.UpdateOptions();
        }

        private void Close()
        {
            popup?.Close();
            optionList?.Clear();
            popup = null;
        }


        public void Select(T item)
        {
            textField.value = null;
            OptionSelected.Invoke(item);
            Close();
        }

        public void Dispose()
        {
            loadSubscription?.Unsubscribe();
            subscription.Unsubscribe();
        }

        private class OptionList<T> : VisualElement
        {
            private readonly Autocomplete<T> autocomplete;
            private List<OptionPlaceHolder<T>> options = new();
            private int highlightIndex;
            private bool hasFocus = false;

            public OptionList(Autocomplete<T> autocomplete)
            {
                focusable = false;
                styleSheets.Add(UxmlElement.LoadStyleSheet(typeof(Autocomplete<T>)));
                AddToClassList("autocomplete-option-list");
                RegisterCallback<FocusOutEvent>(e => hasFocus = false);
                RegisterCallback<FocusInEvent>(e => hasFocus = true);

                this.autocomplete = autocomplete;
                autocomplete.subscription.Add(FromEvent<KeyDownEvent>(autocomplete.textField)
                    .Subscribe(e =>
                    {
                        if (e.keyCode == KeyCode.DownArrow)
                            Highlight((highlightIndex + 1) % options.Count);
                        else if (e.keyCode == KeyCode.UpArrow)
                            Highlight((highlightIndex + options.Count - 1) % options.Count);
                        else if (e.keyCode == KeyCode.Return && highlightIndex >= 0
                                                             && highlightIndex < options.Count)
                            autocomplete.Select(options[highlightIndex].item);
                        else return;
                        e.PreventDefault();
                        e.StopPropagation();
                    }));
                style.width = autocomplete.textField.localBound.width;
            }

            public void SetOptions(List<T> items)
            {
                options.Clear();
                Clear();
                foreach (var item in items)
                {
                    var option = new OptionPlaceHolder<T>(this, item);
                    options.Add(option);
                    Add(option);
                }

                if (options.Count == 0)
                {
                    var l = new Label("No items found!");
                    l.AddToClassList("empty-state-label");
                    Add(l);
                }

                Highlight(0);
            }

            public void Highlight(int index)
            {
                if (highlightIndex >= 0 && highlightIndex < options.Count)
                    options[highlightIndex].SetHighlight(false);
                highlightIndex = index;
                if (highlightIndex >= 0 && highlightIndex < options.Count)
                    options[highlightIndex].SetHighlight(true);
            }

            public new void Clear()
            {
                base.Clear();
                options.Clear();
            }

            public bool HasFocus()
            {
                return hasFocus;
            }

            public void UpdateOptions()
            {
                Clear();
                autocomplete.loadSubscription?.Unsubscribe();

                var loading = LoadingLayer.LoadingLayer.Show(this);
                autocomplete.loadSubscription = new CompositeSubscription()
                    .Add(autocomplete.dataLoader(autocomplete.textField.text)
                        .Subscribe(r =>
                        {
                            SetOptions(r);
                            loading.Close();
                        }, e => loading.Close(), loading.Close))
                    .Add(loading.Close);
            }

            private class OptionPlaceHolder<T> : VisualElement
            {
                private readonly OptionList<T> list;
                internal readonly T item;

                public OptionPlaceHolder(OptionList<T> list, T item)
                {
                    focusable = false;
                    AddToClassList("option-place-holder");
                    this.list = list;
                    var optionView = list.autocomplete.optionViewFactory(item);
                    this.item = item;
                    Add(optionView);
                    RegisterCallback<MouseDownEvent>(e => list.autocomplete.Select(item));
                }

                public void SetHighlight(bool focused)
                {
                    RemoveFromClassList("highlight");
                    if (focused)
                        AddToClassList("highlight");
                }
            }
        }
    }
}