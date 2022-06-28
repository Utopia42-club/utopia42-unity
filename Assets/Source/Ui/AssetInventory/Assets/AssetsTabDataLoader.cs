using System;
using System.Collections.Generic;
using Source.Ui.AssetInventory.Models;
using UnityEngine;
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
            private Dictionary<int, Pack> packs = new();
            private List<Category> categories = new();

            public DataLoader(VisualElement loadingTarget)
            {
                this.loadingTarget = loadingTarget;
            }

            public IDisposable GetCategories(Action<List<Category>> consumer)
            {
                if (loaded)
                {
                    consumer(categories);
                    return DisposableAction.NoOp;
                }
                var action = new DisposableAction(() => consumer(categories), true);
                Load(action.Invoke);
                return action;
            }

            public IDisposable GetPacks(Action<Dictionary<int, Pack>> consumer)
            {
                if (loaded)
                    consumer(packs);
                var action = new DisposableAction(() => consumer(packs), true);
                Load(action.Invoke);
                return action;
            }

            private void Load(Action done)
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
                        done();
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
                            done();
                        }
                        catsLoading.Close();
                    }, () => catsLoading.Close()));
            }

            private class DisposableAction : IDisposable
            {
                internal static DisposableAction NoOp = new DisposableAction(null, true);
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