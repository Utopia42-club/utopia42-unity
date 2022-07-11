using System;
using System.Collections.Generic;
using Source.Model;
using Source.Model.Inventory;
using Source.Service;
using UnityEngine.UIElements;

namespace Source.Ui.AssetInventory.Assets
{
    internal partial class AssetsTab
    {
        //TODO add exception handling
        private class DataLoader
        {
            private readonly AssetsRestClient restClient = new();
            private readonly VisualElement loadingTarget;
            private bool loaded = false;
            private bool loading = false;
            private readonly List<Action> consumers = new();
            private readonly Dictionary<int, Pack> packs = new();
            private readonly List<Category> categories = new();

            public DataLoader(VisualElement loadingTarget)
            {
                this.loadingTarget = loadingTarget;
            }

            public IDisposable GetCategories(Action<List<Category>> consumer)
            {
                return Get(() => consumer(categories));
            }

            public IDisposable GetPacks(Action<Dictionary<int, Pack>> consumer)
            {
                return Get(() => consumer(packs));
            }

            private IDisposable Get(Action consumer)
            {
                if (loaded)
                {
                    consumer();
                    CallConsumers();
                    return DisposableAction.NoOp;
                }

                var action = new DisposableAction(consumer, true);
                consumers.Add(action.Invoke);
                Load();
                return action;
            }

            private void CallConsumers()
            {
                consumers.ForEach(c => c.Invoke());
                consumers.Clear();
            }

            private void Load()
            {
                bool packsLoaded = false;
                bool catsLoaded = false;

                var searchCriteria = new SearchCriteria
                {
                    limit = 100
                };
                var packsLoading = LoadingLayer.LoadingLayer.Show(loadingTarget); //FIXME target was inventory.content
                AssetsInventory.INSTANCE.StartCoroutine(restClient.GetPacks(searchCriteria, packs =>
                {
                    foreach (var pack in packs)
                        this.packs[pack.id] = pack;
                    packsLoading.Close();
                    packsLoaded = true;
                    if (catsLoaded)
                    {
                        loaded = true;
                        CallConsumers();
                    }
                }, () => packsLoading.Close()));

                var catsLoading = LoadingLayer.LoadingLayer.Show(loadingTarget); //FIXME target was inventory.content
                AssetsInventory.INSTANCE.StartCoroutine(restClient.GetCategories(searchCriteria,
                    categories =>
                    {
                        this.categories.AddRange(categories);
                        catsLoaded = true;
                        if (catsLoaded)
                        {
                            loaded = true;
                            CallConsumers();
                        }

                        catsLoading.Close();
                    }, () => catsLoading.Close()));
            }

            private class DisposableAction : IDisposable
            {
                internal static DisposableAction NoOp = new(null, true);
                private readonly bool disposeAfterFirst = false;
                private Action action;

                public DisposableAction(Action action, bool disposeAfterFirst)
                {
                    this.action = action;
                    this.disposeAfterFirst = disposeAfterFirst;
                }

                public void Invoke()
                {
                    if (action != null)
                    {
                        action();
                        if (disposeAfterFirst)
                            Dispose();
                    }
                }

                public void Dispose()
                {
                    action = null;
                }
            }
        }
    }
}