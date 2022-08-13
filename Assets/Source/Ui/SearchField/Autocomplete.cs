using System;
using System.Collections.Generic;
using Source.Reactive.Producer;
using Source.Ui.Popup;
using UnityEngine;
using UnityEngine.UIElements;
using static Source.Reactive.Producer.Observables;
using static Source.Reactive.Consumer.Operators;

namespace Source.Ui.SearchField
{
    public partial class Autocomplete<T> //TODO convert to manipulator
    {
        public event Action<T> OptionSelected = a => { };
        private readonly CompositeSubscription subscription = new();
        private readonly TextField textField;
        private Func<T, VisualElement> optionViewFactory;
        private readonly Func<string, Observable<List<T>>> dataLoader;
        private Subscription loadSubscription;
        private PopupController popup;
        private OptionList<T> optionList;

        public Autocomplete(TextField textField, Func<string, Observable<List<T>>> dataLoader)
            : this(textField, dataLoader, (item) => new Label(item.ToString()))
        {
        }

        public Autocomplete(TextField textField, Func<string, Observable<List<T>>> dataLoader,
            Func<T, VisualElement> optionViewFactory)
        {
            this.textField = textField;
            this.optionViewFactory = optionViewFactory;
            this.dataLoader = dataLoader;
            subscription.Add(CreateSearchRequestObservable(textField)
                .Subscribe(o => Search()));

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

        public void SetOptionViewFactory(Func<T, VisualElement> optionViewFactory)
        {
            this.optionViewFactory = optionViewFactory;
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

        public static Observable<object> CreateSearchRequestObservable(TextField textField)
        {
            var o1 = FromEvent<InputEvent>(textField)
                .Pipe(Map<InputEvent, string>(e => textField.value))
                .Pipe(SkipUntil<string>(v => v != null))
                .Pipe(Debounce<string>(textField, 600))
                .Pipe(DistinctUntilChanged<string>());
            var o = FromEvent<KeyDownEvent>(textField)
                .Pipe(Filter<KeyDownEvent>(e => e.ctrlKey && e.keyCode == KeyCode.Space));
            return Merge<object>(o1, o);
        }
    }
}