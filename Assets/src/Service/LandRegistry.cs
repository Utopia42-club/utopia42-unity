﻿using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using src.Model;
using src.Service.Ethereum;
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
                                break;
                            }

                            toRemove.Add(oLand);
                        }
                    }

                    if (ignored)
                    {
                        ignoredLands.Add(land.id);
                        break;
                    }

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
                var lands = chunkLands[chunk];
                lands.Remove(land);
                if (lands.Count == 0)
                    chunkLands.Remove(chunk);
            }

            ownersLands[land.owner].Remove(land);
        }

        private IEnumerable<Vector2Int> ChunksForLand(Land land)
        {
            var startPos = new VoxelPosition(land.x1, 0, land.y1);
            var endPos = new VoxelPosition(land.x1, 0, land.y1);

            for (int cx = startPos.chunk.x; cx <= endPos.chunk.x; cx++)
            {
                for (int cy = startPos.chunk.y; cy <= endPos.chunk.y; cy++)
                {
                    yield return new Vector2Int(cx, cy);
                }
            }
        }

        public List<Land> GetLandsFor(string walletId)
        {
            List<Land> res = null;
            ownersLands.TryGetValue(walletId, out res);
            return res;
        }

        public Dictionary<string, List<Land>> GetOwnersLands()
        {
            return ownersLands;
        }

        public IEnumerator ReloadLands()
        {
            var lands = new List<Land>();
            yield return EthereumClientService.INSTANCE.GetLands(lands);
            yield return SetLands(lands);
        }

        public IEnumerator ReloadLandsFor(string wallet)
        {
            List<Land> loaded = new List<Land>();
            yield return EthereumClientService.INSTANCE.GetLandsForOwner(wallet,
                lands => loaded.AddRange(lands));
            yield return ReSetOwnerLands(wallet, loaded);
        }

        private IEnumerator ReSetOwnerLands(string wallet, List<Land> lands)
        {
            var iterations = 50;
            var toLoad = ownersLands[wallet];
            ownersLands[wallet] = new List<Land>();
            foreach (var land in lands)
            {
                toLoad.Remove(land);
                if (ignoredLands.Contains(land.id)) continue;
                Land oldValue;
                if (validLands.TryGetValue(land.id, out oldValue))
                    RemoveLand(oldValue);
                InsertLand(land);
                iterations--;
                if (iterations <= 0)
                {
                    yield return null;
                    iterations = 50;
                }
            }

            var toLoadIds = new List<BigInteger>();
            foreach (var land in toLoad)
            {
                RemoveLand(land);
                toLoadIds.Add(land.id);
                iterations--;
                if (iterations <= 0)
                {
                    yield return null;
                    iterations = 50;
                }
            }

            var toInsert = new List<Land>();
            yield return EthereumClientService.INSTANCE.GetLandsByIds(toLoadIds, lands => toInsert.AddRange(lands));


            foreach (var land in lands)
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
    }
}