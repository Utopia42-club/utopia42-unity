using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using src.Model;
using src.Service.Ethereum;
using src.Utils;
using UnityEngine;

namespace src.Service
{
    internal class LandRegistry
    {
        // private readonly Dictionary<long, Land> ignoredLands = new Dictionary<long, Land>();
        private readonly HashSet<long> ignoredLands = new HashSet<long>();
        private readonly Dictionary<string, List<Land>> ownersLands = new Dictionary<string, List<Land>>();
        private readonly Dictionary<long, Land> validLands = new Dictionary<long, Land>();
        private readonly Dictionary<Vector2Int, HashSet<Land>> chunkLands = new Dictionary<Vector2Int, HashSet<Land>>();

        private IEnumerator SetLands(List<Land> lands)
        {
            ownersLands.Clear();
            validLands.Clear();
            chunkLands.Clear();
            var runCount = 50;
            foreach (var land in lands)
            {
                runCount--;
                InsertLand(land);
                if (runCount <= 0)
                {
                    yield return null;
                    runCount = 50;
                }
            }
        }

        internal HashSet<Land> GetLandsForChunk(Vector2Int chunkPosition)
        {
            HashSet<Land> lands;
            chunkLands.TryGetValue(chunkPosition, out lands);
            return lands;
        }

        internal Dictionary<long, Land> GetLands()
        {
            return validLands;
        }

        private void InsertLand(Land land)
        {
            bool ignored = false;
            var rect = land.ToRect();
            var emptyChunks = new List<Vector2Int>();
            var landLists = new List<HashSet<Land>>();
            var toRemove = new HashSet<Land>(); // If lands are sorted by time, this list will remain empty.
            foreach (var chunk in ChunksForLand(land))
            {
                HashSet<Land> currChunkLands;
                if (chunkLands.TryGetValue(chunk, out currChunkLands))
                {
                    foreach (var oLand in currChunkLands)
                    {
                        if (rect.Overlaps(oLand.ToRect()))
                        {
                            if (land.time > oLand.time)
                            {
                                ignored = true;
                                ignoredLands.Add(land.id);
                                break;
                            }

                            toRemove.Add(oLand);
                        }
                    }

                    if (ignored)
                        break;
                    landLists.Add(currChunkLands);
                }
                else emptyChunks.Add(chunk);
            }

            if (!ignored)
            {
                foreach (var cl in landLists)
                    cl.Add(land);

                foreach (var chunk in emptyChunks)
                {
                    var cl = new HashSet<Land>();
                    chunkLands[chunk] = cl;
                    cl.Add(land);
                }

                List<Land> ownerLands;
                if (!ownersLands.TryGetValue(land.owner, out ownerLands))
                    ownersLands[land.owner] = ownerLands = new List<Land>();
                ownerLands.Add(land);

                validLands[land.id] = land;
                foreach (var l in toRemove)
                {
                    ignoredLands.Add(l.id);
                    RemoveLand(l);
                }
            }
        }

        private void RemoveLand(Land land)
        {
            validLands.Remove(land.id);
            foreach (var chunk in ChunksForLand(land))
            {
                if (chunkLands.TryGetValue(chunk, out var lands))
                {
                    lands.Remove(land);
                    if (lands.Count == 0)
                        chunkLands.Remove(chunk);
                }
            }

            if (ownersLands.TryGetValue(land.owner, out var ownerLands))
                ownerLands.Remove(land);
        }

        private IEnumerable<Vector2Int> ChunksForLand(Land land)
        {
            var startPos = new VoxelPosition(land.startCoordinate);
            var endPos = new VoxelPosition(land.endCoordinate);

            for (int cx = startPos.chunk.x; cx <= endPos.chunk.x; cx++)
            {
                for (int cy = startPos.chunk.z; cy <= endPos.chunk.z; cy++)
                {
                    yield return new Vector2Int(cx, cy);
                }
            }
        }

        public List<Land> GetLandsForOwner(string walletId)
        {
            ownersLands.TryGetValue(walletId, out var res);
            return res;
        }

        public Dictionary<string, List<Land>> GetOwnersLands()
        {
            return ownersLands;
        }

        public IEnumerator ReloadLands(Action onFailed)
        {
            var lands = new List<Land>();

            bool failed = false;
            yield return LoadLandsPaginated(Constants.ApiURL + "/world/lands", lands, () =>
            {
                onFailed();
                failed = true;
            });
            if (failed) yield break;

            yield return SetLands(lands);
        }

        public IEnumerator ReloadLandsForOwner(string wallet, Action onFailed)
        {
            var lands = new List<Land>();

            bool failed = false;
            yield return LoadLandsPaginated($"{Constants.ApiURL}/world/owner/{wallet}/lands", lands, () =>
            {
                onFailed();
                failed = true;
            });
            if (failed) yield break;

            yield return ReSetOwnerLands(wallet, lands, onFailed);
        }

        private static IEnumerator LoadLandsPaginated(string url, List<Land> lands, Action onFailed)
        {
            const int pageSize = 200;
            url = url + "?pageSize=" + pageSize;
            var hasNext = true;
            while (hasNext)
            {
                var failed = false;
                yield return RestClient.Post<Land, List<Land>>(url, lands.Count == 0 ? new Land() : lands.Last(),
                    response =>
                    {
                        if (response != null)
                            lands.AddRange(response);
                        hasNext = response != null && response.Count == pageSize;
                    }, () =>
                    {
                        failed = true;
                        onFailed();
                    });
                if (failed) yield break;
            }
        }

        private IEnumerator ReSetOwnerLands(string wallet, List<Land> lands, Action onFailed)
        {
            var iterations = 50;
            var currentLands = new List<Land>();
            if (ownersLands.TryGetValue(wallet, out var ownerLands))
                currentLands.AddRange(ownerLands);

            foreach (var toRemove in currentLands)
            {
                RemoveLand(toRemove);
                iterations--;
                if (iterations <= 0)
                {
                    yield return null;
                    iterations = 50;
                }
            }

            var transferredLandIds = currentLands.ConvertAll(l => (BigInteger) l.id);
            foreach (var land in lands)
            {
                transferredLandIds.Remove(land.id);
                if (ignoredLands.Contains(land.id)) continue;
                InsertLand(land);
                iterations--;
                if (iterations <= 0)
                {
                    yield return null;
                    iterations = 50;
                }
            }


            var toInsert = new List<Land>();

            var failed = false;
            yield return EthereumClientService.INSTANCE.GetLandsByIds(transferredLandIds,
                loaded => toInsert.AddRange(loaded), () =>
                {
                    failed = true;
                    onFailed();
                });
            if (failed) yield break;

            foreach (var land in toInsert)
            {
                InsertLand(land);
                iterations--;
                if (iterations <= 0)
                {
                    yield return null;
                    iterations = 50;
                }
            }
        }

        private HashSet<long> GetIds(List<Land> lands)
        {
            var ids = new HashSet<long>();
            foreach (var land in lands)
                ids.Add(land.id);
            return ids;
        }

        public Land Get(long id)
        {
            return validLands[id];
        }
    }
}