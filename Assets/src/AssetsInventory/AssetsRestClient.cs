using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using src.AssetsInventory.Models;
using src.Model;
using src.Service;
using src.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace src.AssetsInventory
{
    public class AssetsRestClient
    {
        public IEnumerator GetCategories(SearchCriteria searchCriteria, Action<List<Category>> consumer, Action failed)
        {
            var url = Constants.ApiURL + "/assets/categories";
            yield return RestClient.Post(url, searchCriteria, consumer, failed);
        }

        public IEnumerator GetPacks(SearchCriteria searchCriteria, Action<List<Pack>> consumer, Action failed)
        {
            var url = Constants.ApiURL + "/assets/packs";
            yield return RestClient.Post(url, searchCriteria, consumer, failed);
        }

        public IEnumerator GetAllAssets(SearchCriteria searchCriteria, Action<List<Asset>> consumer, Action failed,
            MonoBehaviour monoBehaviour)
        {
            searchCriteria.limit = 100;
            var limit = searchCriteria.limit;
            yield return GetAssets(searchCriteria, currentPage =>
            {
                if (currentPage.Count < limit)
                {
                    consumer(currentPage);
                }
                else
                {
                    searchCriteria.lastId = currentPage[^1].id;
                    monoBehaviour.StartCoroutine(GetAllAssets(searchCriteria, nextPages =>
                    {
                        var allAssets = new List<Asset>();
                        allAssets.AddRange(currentPage);
                        allAssets.AddRange(nextPages);
                        consumer(allAssets);
                    }, failed, monoBehaviour));
                }
            }, failed);
        }

        public IEnumerator GetAssets(SearchCriteria searchCriteria, Action<List<Asset>> consumer, Action failed)
        {
            var url = Constants.ApiURL + "/assets";
            yield return RestClient.Post(url, searchCriteria, consumer, failed);
        }

        public IEnumerator GetAllFavoriteItems(SearchCriteria searchCriteria, Action<List<FavoriteItem>> consumer,
            Action failed, MonoBehaviour monoBehaviour)
        {
            searchCriteria.limit = 100;
            var limit = searchCriteria.limit;
            yield return GetFavoriteItems(searchCriteria, currentPage =>
            {
                if (currentPage.Count < limit)
                {
                    consumer(currentPage);
                }
                else
                {
                    searchCriteria.lastId = currentPage[^1].id;
                    monoBehaviour.StartCoroutine(GetAllFavoriteItems(searchCriteria, nextPages =>
                    {
                        var allAssets = new List<FavoriteItem>();
                        allAssets.AddRange(currentPage);
                        allAssets.AddRange(nextPages);
                        consumer(allAssets);
                    }, failed, monoBehaviour));
                }
            }, failed);
        }

        public IEnumerator GetFavoriteItems(SearchCriteria searchCriteria, Action<List<FavoriteItem>> consumer,
            Action failed)
        {
            var url = Constants.ApiURL + "/assets/favorite-items";
            yield return RestClient.Post(url, searchCriteria, consumer, failed);
        }

        public IEnumerator CreateFavoriteItem(FavoriteItem favoriteItem, Action<FavoriteItem> consumer, Action failed)
        {
            var url = Constants.ApiURL + "/assets/favorite-items/create";
            yield return RestClient.Post(url, favoriteItem, consumer, failed);
        }
        
        public IEnumerator UpdateFavoriteItem(FavoriteItem favoriteItem, Action success, Action failed)
        {
            var url = Constants.ApiURL + "/assets/favorite-items/update";
            yield return RestClient.Post(url, favoriteItem, success, failed);
        }

        public IEnumerator DeleteFavoriteItem(FavoriteItem favoriteItem, Action success, Action failed)
        {
            var url = Constants.ApiURL + "/assets/favorite-items/" + favoriteItem.id;
            yield return RestClient.Delete(url, success, failed);
        }
    }
}