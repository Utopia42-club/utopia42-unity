using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using src.MetaBlocks;
using src.Model;
using src.Service;
using src.Utils;
using UnityEngine;

namespace src
{
    public class World : MonoBehaviour
    {
        [SerializeField] private Material material;
        [SerializeField] private Material highlightBlock;
        [SerializeField] private Material selectedBlock;

        //TODO take care of size/time limit
        //Deactivated chunks are added to this collection for possible future reuse
        private Dictionary<Vector3Int, Chunk> garbageChunks = new Dictionary<Vector3Int, Chunk>();
        private readonly Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();

        private readonly ConcurrentDictionary<Vector3Int, HighlightChunk> highlightChunks =
            new ConcurrentDictionary<Vector3Int, HighlightChunk>(); // chunk coordinate -> highlight chunk

        private readonly HashSet<VoxelPosition> clipboard = new HashSet<VoxelPosition>();
        private readonly HashSet<MetaPosition> metaClipboard = new HashSet<MetaPosition>();
        private ConcurrentQueue<HighlightChunk> highlightChunksToRedraw = new ConcurrentQueue<HighlightChunk>();

        public Vector3Int HighlightOffset { private set; get; } = Vector3Int.zero;

        private Vector3 metaOffset = Vector3.zero; // used when the selection is meta only
        public Vector3 MetaHighlightOffset => OnlyMetaSelectionActive ? metaOffset : HighlightOffset;
        private GameObject highlight;


        private bool creatingChunks = false;
        private List<Chunk> chunkRequests = new List<Chunk>();

        public GameObject debugScreen;
        public GameObject inventory;
        public GameObject cursorSlot;
        public Player player;
        private VoxelPosition firstSelectedPosition;
        public VoxelPosition LastSelectedPosition { get; private set; }
        public MetaPosition LastSelectedMetaPosition { get; private set; }

        public Material Material => material;
        public Material HighlightBlock => highlightBlock;
        public Material SelectedBlock => selectedBlock;

        public int TotalBlocksSelected =>
            highlightChunks.Values.Where(chunk => chunk != null).Sum(chunk => chunk.TotalBlocksHighlighted);

        public int TotalNonMetaBlocksSelected =>
            highlightChunks.Values.Where(chunk => chunk != null).Sum(chunk => chunk.TotalNonMetaBlocksHighlighted);

        public bool SelectionActive => TotalBlocksSelected > 0;

        public bool OnlyMetaSelectionActive =>
            metaOffset != Vector3.zero || SelectionActive && TotalNonMetaBlocksSelected == 0;

        public bool MetaSelectionActive => TotalBlocksSelected - TotalNonMetaBlocksSelected > 0;

        public bool ClipboardEmpty => clipboard.Count + metaClipboard.Count == 0;

        public List<Vector3Int> GetClipboardWorldPositions()
        {
            var positions = clipboard.Select(vp => vp.ToWorld()).ToList();
            positions.AddRange(metaClipboard.Select(mp => mp.ToVoxelPosition().ToWorld()).ToList());
            return positions;
        }

        public float? GetMetaClipboardMinY()
        {
            if (metaClipboard == null || metaClipboard.Count == 0) return null;
            var minY = float.MaxValue;
            foreach (var mp in metaClipboard)
            {
                var chunk = GetChunkIfInited(mp.chunk);
                if (chunk == null) return null;
                var metaBlock = chunk.GetMetaAt(mp)?.blockObject;
                if (metaBlock == null || !metaBlock.MinGlobalY.HasValue) return null;
                if (minY > metaBlock.MinGlobalY) minY = metaBlock.MinGlobalY.Value;
            }

            return minY;
        }

        public float? GetMetaSelectionMinY()
        {
            var minY = float.MaxValue;
            foreach (var (key, value) in highlightChunks)
            foreach (var mp in value.HighlightedMetaLocalPositions)
            {
                var chunk = GetChunkIfInited(key);
                if (chunk == null) return null;
                var metaBlock = chunk.GetMetaAt(new MetaPosition(key, mp))?.blockObject;
                if (metaBlock == null || !metaBlock.MinGlobalY.HasValue) return null;
                if (minY > metaBlock.MinGlobalY) minY = metaBlock.MinGlobalY.Value;
            }

            return minY;
        }

        public bool SelectionDisplaced =>
            metaOffset != Vector3.zero ||
            HighlightOffset != Vector3Int.zero ||
            highlightChunks.Values.Any(highlightChunk => highlightChunk.SelectionDisplaced);

        private void Start()
        {
            GameManager.INSTANCE.stateChange.AddListener(state =>
            {
                inventory.SetActive(state == GameManager.State.INVENTORY);
                cursorSlot.SetActive(state == GameManager.State.INVENTORY);
            });
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F3))
                debugScreen.SetActive(!debugScreen.activeSelf);
        }

        public void AddHighlights(List<VoxelPosition> vps, Action<Dictionary<VoxelPosition, bool>> consumer = null)
        {
            StartCoroutine(AddHighlights(vps, Vector3Int.zero, consumer));
        }

        private IEnumerator AddHighlights(List<VoxelPosition> vps, Vector3Int offset,
            Action<Dictionary<VoxelPosition, bool>> consumer)
        {
            var done = new Dictionary<VoxelPosition, bool>();

            foreach (var vp in vps)
                StartCoroutine(AddHighlight(vp, null, true, (result) => { done[vp] = result; }));

            while (done.Count != vps.Count) yield return null;
            RedrawChangedHighlightChunks();
            consumer?.Invoke(done);
            if (offset != Vector3Int.zero)
                MoveSelection(offset, null);
        }

        public IEnumerator AddHighlights(Dictionary<VoxelPosition, uint> highlights,
            Action<Dictionary<VoxelPosition, bool>> consumer = null)
        {
            var done = new Dictionary<VoxelPosition, bool>();

            foreach (var vp in highlights.Keys)
            {
                var blockType = highlights[vp];
                StartCoroutine(AddHighlight(vp, blockType, true,
                    (result) => { done[vp] = result; }));
            }

            while (done.Count != highlights.Count) yield return null;
            RedrawChangedHighlightChunks();
            consumer?.Invoke(done);
        }

        public void AddHighlight(VoxelPosition vp, Action<bool> consumer = null)
        {
            StartCoroutine(AddHighlight(vp, null, false, consumer));
        }

        public void AddHighlight(MetaPosition mp, Action consumer = null)
        {
            StartCoroutine(AddHighlight(mp, null, false, consumer));
        }

        private HighlightChunk GetHighlightChunk(Vector3Int chunk)
        {
            if (highlight == null)
            {
                highlight = new GameObject();
                highlight.name = "World Highlight";
            }

            return highlightChunks.GetOrAdd(chunk, HighlightChunk.Create(highlight, chunk));
        }

        private IEnumerator AddHighlight(VoxelPosition vp, uint? blockType, bool delayedUpdate,
            Action<bool> consumer = null)
        {
            if (!player.CanEdit(vp.ToWorld(), out _))
            {
                consumer?.Invoke(false);
                yield break;
            }

            var highlightChunk = GetHighlightChunk(vp.chunk);
            yield return null;

            if (highlightChunk.Contains(vp.local))
            {
                RemoveHighlight(vp, delayedUpdate);
                consumer?.Invoke(false);
                yield break;
            }

            Action<HighlightedBlock> highlightedBlockProcess = block =>
            {
                if (block == null)
                {
                    consumer?.Invoke(false);
                    return;
                }

                highlightChunk.Add(vp.local, block);
                if (TotalBlocksSelected == 1)
                    firstSelectedPosition = vp;
                LastSelectedPosition = vp;
                highlightChunksToRedraw.Enqueue(highlightChunk);
                if (!delayedUpdate)
                    RedrawChangedHighlightChunks();
                consumer?.Invoke(true);
            };

            if (!blockType.HasValue)
            {
                GetHighlightedBlock(highlightChunk, vp, highlightedBlockProcess);
                yield break;
            }

            highlightedBlockProcess.Invoke(HighlightedBlock.Create(vp.local, highlightChunk, blockType.Value));
        }

        private IEnumerator AddHighlight(MetaPosition mp, MetaBlock metaBlock, bool delayedUpdate,
            Action consumer = null)
        {
            var vp = mp.ToVoxelPosition();
            if (!player.CanEdit(vp.ToWorld(), out _))
            {
                consumer?.Invoke();
                yield break;
            }

            var highlightChunk = GetHighlightChunk(mp.chunk);
            yield return null;

            if (highlightChunk.Contains(mp.local))
            {
                RemoveHighlight(mp, delayedUpdate);
                consumer?.Invoke();
                yield break;
            }

            void HighlightedMetaBlockProcess(HighlightedMetaBlock highlightedMetaBlock)
            {
                if (highlightedMetaBlock == null)
                {
                    consumer?.Invoke();
                    return;
                }

                highlightChunk.Add(mp.local, highlightedMetaBlock);
                highlightChunksToRedraw.Enqueue(highlightChunk);
                if (!delayedUpdate) RedrawChangedHighlightChunks();
                consumer?.Invoke();
            }

            if (metaBlock == null)
            {
                CreateHighlightedMetaBlockAndAddToChunk(highlightChunk, mp, HighlightedMetaBlockProcess);
                yield break;
            }

            HighlightedMetaBlock.CreateAndAddToChunk(mp.local, highlightChunk, metaBlock, HighlightedMetaBlockProcess);
        }

        private void GetHighlightedBlock(HighlightChunk highlightChunk, VoxelPosition vp,
            Action<HighlightedBlock> consumer, bool ignoreAir = true)
        {
            if (!player.CanEdit(vp.ToWorld(), out _))
            {
                consumer.Invoke(null);
                return;
            }

            var chunk = GetChunkIfInited(vp.chunk);
            if (chunk != null)
            {
                var blockType = chunk.GetBlock(vp.local);
                if (ignoreAir && !blockType.isSolid)
                {
                    consumer.Invoke(null);
                    return;
                }

                consumer.Invoke(HighlightedBlock.Create(vp.local, highlightChunk, blockType.id));
                return;
            }

            WorldService.INSTANCE.GetBlockType(vp, blockType =>
            {
                if (blockType == null || ignoreAir && !blockType.isSolid)
                {
                    consumer.Invoke(null);
                    return;
                }

                consumer.Invoke(HighlightedBlock.Create(vp.local, highlightChunk, blockType.id));
            });
        }

        private void CreateHighlightedMetaBlockAndAddToChunk(HighlightChunk highlightChunk, MetaPosition mp,
            Action<HighlightedMetaBlock> consumer)
        {
            var vp = mp.ToVoxelPosition();
            if (!player.CanEdit(vp.ToWorld(), out var land))
            {
                consumer.Invoke(null);
                return;
            }

            var chunk = GetChunkIfInited(vp.chunk);
            if (chunk != null)
            {
                var meta = chunk.GetMetaAt(mp);
                if (meta == null)
                {
                    consumer.Invoke(null);
                    return;
                }

                HighlightedMetaBlock.CreateAndAddToChunk(mp.local, highlightChunk, meta, consumer);
                return;
            }

            WorldService.INSTANCE.GetMetaBlock(mp,
                meta => { HighlightedMetaBlock.CreateAndAddToChunk(mp.local, highlightChunk, meta, consumer); });
        }

        private void RemoveHighlight(VoxelPosition vp, bool delayedUpdate = false)
        {
            if (highlightChunks.TryGetValue(vp.chunk, out var highlightChunk) && highlightChunk != null &&
                highlightChunk.Remove(vp.local))
                highlightChunksToRedraw.Enqueue(highlightChunk);
            if (!delayedUpdate)
                RedrawChangedHighlightChunks();
        }

        private void RemoveHighlight(MetaPosition mp, bool delayedUpdate = false)
        {
            if (highlightChunks.TryGetValue(mp.chunk, out var highlightChunk) && highlightChunk != null &&
                highlightChunk.Remove(mp.local))
                highlightChunksToRedraw.Enqueue(highlightChunk);
            if (!delayedUpdate)
                RedrawChangedHighlightChunks();
        }

        private void RedrawChangedHighlightChunks()
        {
            while (highlightChunksToRedraw.Count > 0)
            {
                if (highlightChunksToRedraw.TryDequeue(out var highlightChunk) && highlightChunk != null)
                    highlightChunk.Redraw();
            }
        }

        public void ClearHighlights()
        {
            if (highlight != null)
            {
                DestroyImmediate(highlight);
                highlight = null;
            }

            highlightChunks.Clear();
            highlightChunksToRedraw = new ConcurrentQueue<HighlightChunk>();
            HighlightOffset = Vector3Int.zero;
            metaOffset = Vector3.zero;
            firstSelectedPosition = null;
            LastSelectedPosition = null;
        }

        public void RemoveSelectedBlocks(bool ignoreUnmovedBlocks = false)
        {
            var blocks = new Dictionary<VoxelPosition, Land>();
            foreach (var highlightChunkCoordinate in highlightChunks.Keys)
            {
                var highlightChunk = highlightChunks[highlightChunkCoordinate];
                if (highlightChunk == null) continue;
                foreach (var localPosition in highlightChunk.HighlightedLocalPositions)
                {
                    var offset = highlightChunk.GetRotationOffset(localPosition);
                    if (!offset.HasValue ||
                        ignoreUnmovedBlocks && HighlightOffset + offset.Value == Vector3Int.zero) continue;
                    var vp = new VoxelPosition(highlightChunkCoordinate, localPosition);
                    if (!player.CanEdit(vp.ToWorld(), out var land)) continue;
                    blocks.Add(vp, land);
                }

                var metaOffset = MetaHighlightOffset;
                foreach (var metaLocalPosition in highlightChunk.HighlightedMetaLocalPositions)
                {
                    var offset = highlightChunk.GetRotationOffset(metaLocalPosition);
                    if (!offset.HasValue ||
                        ignoreUnmovedBlocks && metaOffset + offset.Value == Vector3Int.zero) continue;
                    var mp = new MetaPosition(highlightChunkCoordinate, metaLocalPosition);
                    var vp = mp.ToVoxelPosition();
                    if (!player.CanEdit(vp.ToWorld(), out var land)) continue;

                    var chunk = GetChunkIfInited(mp.chunk);
                    if (chunk != null)
                    {
                        if (chunk.GetMetaAt(mp) != null)
                            chunk.DeleteMeta(mp);
                        continue;
                    }

                    WorldService.INSTANCE.GetMetaBlock(mp,
                        meta => { WorldService.INSTANCE.OnMetaRemoved(meta, mp); });
                }
            }

            DeleteBlocks(blocks);
        }

        public List<VoxelPosition> GetSelectedBlocksPositions()
        {
            var result = new List<VoxelPosition>();
            foreach (var highlightChunk in highlightChunks.Values)
            {
                if (highlightChunk == null) continue;
                foreach (var highlightedBlock in highlightChunk.HighlightedBlocks)
                {
                    result.Add(new VoxelPosition(HighlightOffset + highlightChunk.Position +
                                                 highlightedBlock.CurrentPosition));
                }
            }

            return result;
        }

        public List<MetaPosition> GetSelectedMetaBlocksPositions()
        {
            var result = new List<MetaPosition>();
            foreach (var highlightChunk in highlightChunks.Values)
            {
                if (highlightChunk == null) continue;
                var metaOffset = MetaHighlightOffset;
                foreach (var highlightedMetaBlock in highlightChunk.HighlightedMetaBlocks)
                {
                    result.Add(new MetaPosition(metaOffset + highlightChunk.Position +
                                                highlightedMetaBlock.CurrentPosition.position));
                }
            }

            return result;
        }

        public void DuplicateSelectedBlocks(bool offsetCheck)
        {
            foreach (var highlightChunk in highlightChunks.Values)
            {
                if (highlightChunk == null) continue;
                var blocks = new Dictionary<VoxelPosition, Tuple<BlockType, Land>>();
                foreach (var highlightedBlock in highlightChunk.HighlightedBlocks)
                {
                    if (highlightedBlock == null ||
                        offsetCheck && HighlightOffset + highlightedBlock.Offset == Vector3Int.zero) continue;

                    var newPos = HighlightOffset + highlightChunk.Position + highlightedBlock.CurrentPosition;
                    if (!player.CanEdit(newPos, out var land)) continue;

                    var newPosVp = new VoxelPosition(newPos);
                    blocks.Add(newPosVp,
                        new Tuple<BlockType, Land>(Blocks.GetBlockType(highlightedBlock.BlockTypeId), land));
                }

                PutBlocks(blocks);

                var metaOffset = MetaHighlightOffset;
                foreach (var highlightedMetaBlock in highlightChunk.HighlightedMetaBlocks)
                {
                    if (highlightedMetaBlock == null ||
                        offsetCheck && metaOffset + highlightedMetaBlock.Offset == Vector3Int.zero) continue;
                    var newPos = metaOffset + highlightChunk.Position + highlightedMetaBlock.CurrentPosition.position;
                    if (!player.CanEdit(Vectors.TruncateFloor(newPos), out var land)) continue;
                    PutMetaWithProps(new MetaPosition(newPos),
                        (MetaBlockType) Blocks.GetBlockType(highlightedMetaBlock.MetaBlockTypeId),
                        highlightedMetaBlock.MetaProperties, land);
                }
            }
        }

        public void MoveMetaSelection(Vector3 v, Vector3? referencePosition)
        {
            if (!referencePosition.HasValue)
            {
                metaOffset += v;
                highlight.transform.position += v;
                return;
            }

            var oldOffset = metaOffset;
            metaOffset = v - referencePosition.Value;
            highlight.transform.position = (highlight.transform.position - oldOffset) + metaOffset;
        }

        public void MoveSelection(Vector3Int v, Vector3Int? referencePosition)
        {
            if (!referencePosition.HasValue)
            {
                HighlightOffset += v;
                highlight.transform.position += v;
                return;
            }

            var oldOffset = HighlightOffset;
            HighlightOffset = v - referencePosition.Value;
            highlight.transform.position = (highlight.transform.position - oldOffset) + HighlightOffset;
        }

        private Vector3? GetSelectionRotationCenter()
        {
            if (!SelectionActive || firstSelectedPosition == null ||
                !highlightChunks.TryGetValue(firstSelectedPosition.chunk, out var highlightChunk) ||
                highlightChunk == null) return null;
            var rotationOffset = highlightChunk.GetRotationOffset(firstSelectedPosition.local);
            if (!rotationOffset.HasValue) return null;
            return firstSelectedPosition.ToWorld() + HighlightOffset + rotationOffset.Value + 0.5f * Vector3.one;
        }

        public void RotateSelection(Vector3 axis)
        {
            var center = GetSelectionRotationCenter();
            if (!center.HasValue) return;

            foreach (var highlightChunk in highlightChunks.Values.Where(highlightChunk => highlightChunk != null))
            {
                highlightChunk.Rotate(center.Value, axis);
                highlightChunksToRedraw.Enqueue(highlightChunk);
            }

            RedrawChangedHighlightChunks();
        }

        public void ResetClipboard()
        {
            clipboard.Clear();
            metaClipboard.Clear();
            foreach (var highlightChunkCoordinate in highlightChunks.Keys)
            {
                var highlightChunk = highlightChunks[highlightChunkCoordinate];
                if (highlightChunk == null) continue;
                foreach (var localPosition in highlightChunk.HighlightedLocalPositions)
                {
                    clipboard.Add(new VoxelPosition(highlightChunkCoordinate, localPosition));
                }

                foreach (var localPosition in highlightChunk.HighlightedMetaLocalPositions)
                {
                    metaClipboard.Add(new MetaPosition(highlightChunkCoordinate, localPosition));
                }
            }
        }

        public Vector3Int GetClipboardMinPoint()
        {
            var positions = GetClipboardWorldPositions();
            // if (positions.Count == 0) return null;
            
            var minX = int.MaxValue;
            var minY = int.MaxValue;
            var minZ = int.MaxValue;
            foreach (var pos in positions)
            {
                if (pos.x < minX)
                    minX = pos.x;
                if (pos.y < minY)
                    minY = pos.y;
                if (pos.z < minZ)
                    minZ = pos.z;
            }

            return new Vector3Int(minX, minY, minZ);
        }

        public void PasteClipboard(Vector3Int offset)
        {
            StartCoroutine(PasteClipboardCoroutine(offset));
        }

        private IEnumerator PasteClipboardCoroutine(Vector3Int offset)
        {
            ClearHighlights();
            yield return AddHighlights(clipboard.ToList(), Vector3Int.zero, null);
            foreach (var metaPosition in metaClipboard)
                AddHighlight(metaPosition);
            if (clipboard.Count == 0)
                MoveMetaSelection(offset, null);
            else
                MoveSelection(offset, null);
        }

        private Chunk PopRequest()
        {
            var chunk = chunkRequests[0];
            chunkRequests.RemoveAt(0);
            return chunk;
        }

        public bool Initialize(Vector3Int currChunk, bool clean)
        {
            if (clean) chunkRequests.Clear();
            else
            {
                while (0 != chunkRequests.Count)
                {
                    var chunk = PopRequest();
                    if (chunks.ContainsKey(chunk.coordinate))
                        chunks.Remove(chunk.coordinate); //FIXME Memory leak?
                }
            }

            if (creatingChunks) return false;
            if (clean)
            {
                foreach (var chunk in chunkRequests)
                {
                    if (chunk.chunkObject != null)
                    {
                        chunk.Destroy();
                        chunk.chunkObject = null;
                    }
                }

                chunkRequests.Clear();

                foreach (var chunk in garbageChunks.Values)
                    if (chunk.chunkObject != null)
                        chunk.Destroy();
                garbageChunks.Clear();

                foreach (var chunk in chunks.Values)
                    if (chunk.chunkObject != null)
                        chunk.Destroy();
                chunks.Clear();
            }

            OnPlayerChunkChanged(currChunk, true);
            return true;
        }

        private void OnPlayerChunkChanged(Vector3Int currChunk, bool instantly)
        {
            RequestChunkCreation(currChunk);

            if (!creatingChunks && chunkRequests.Count > 0)
                StartCoroutine(CreateChunks(instantly ? 50 : 1));
        }

        public void OnPlayerChunkChanged(Vector3Int currChunk)
        {
            this.OnPlayerChunkChanged(currChunk, false);
        }

        IEnumerator CreateChunks(int chunksPerFrame)
        {
            // creatingChunks = true;
            int toStart = chunksPerFrame;
            var initing = new List<Chunk>();
            while (chunkRequests.Count != 0)
            {
                var chunk = PopRequest();
                if (chunk.IsActive() && !chunk.IsInitStarted())
                {
                    initing.Add(chunk);
                    if (!chunk.Init(() => initing.Remove(chunk)))
                        initing.Remove(chunk);
                    toStart--;
                    if (toStart <= 0)
                    {
                        toStart = chunksPerFrame;
                        yield return new WaitUntil(() => initing.Count == 0);
                    }
                }
            }

            creatingChunks = false;
        }

        public int CountChunksToCreate()
        {
            return chunkRequests.Count;
        }

        private void RequestChunkCreation(Vector3Int centerChunk)
        {
            var to = centerChunk + Player.ViewDistance;
            var from = centerChunk - Player.ViewDistance;

            var unseens = new HashSet<Vector3Int>(chunks.Keys);
            for (int x = from.x; x <= to.x; x++)
            {
                for (int y = from.y; y <= to.y; y++)
                {
                    for (int z = from.z; z <= to.z; z++)
                    {
                        Vector3Int key = new Vector3Int(x, y, z);
                        Chunk chunk;
                        if (!chunks.TryGetValue(key, out chunk))
                        {
                            if (garbageChunks.TryGetValue(key, out chunk))
                            {
                                chunk.SetActive(true);
                                chunks[key] = chunk;
                                garbageChunks.Remove(key);
                            }
                            else
                            {
                                chunks[key] = chunk = new Chunk(key, this);
                                chunk.SetActive(true);
                            }
                        }

                        if (!chunk.IsInitStarted()) chunkRequests.Add(chunk);
                        unseens.Remove(key);
                    }
                }
            }


            foreach (Vector3Int key in unseens)
            {
                Chunk ch;
                if (chunks.TryGetValue(key, out ch))
                {
                    //Destroy(ch.chunkObject);
                    chunks.Remove(key);
                }

                ch.SetActive(false);
                if (ch.IsInitStarted())
                    garbageChunks[key] = ch;
            }

            if (garbageChunks.Count > 5000)
            {
                while (garbageChunks.Count > 2500)
                {
                    var iter = garbageChunks.Keys.GetEnumerator();
                    iter.MoveNext();
                    var key = iter.Current;
                    garbageChunks[key].Destroy();
                    garbageChunks.Remove(key);
                }
            }
        }

        private void DestroyGarbageChunkIfExists(Vector3Int chunkPos)
        {
            if (!garbageChunks.TryGetValue(chunkPos, out var chunk)) return;
            chunk.Destroy();
            garbageChunks.Remove(chunkPos);
        }

        public Chunk GetChunkIfInited(Vector3Int chunkPos)
        {
            if (chunks.TryGetValue(chunkPos, out var chunk) && chunk.IsInitialized() && chunk.IsActive())
                return chunk;
            return null;
        }

        public void TryPutVoxel(VoxelPosition vp, BlockType type)
        {
            var chunk = GetChunkIfInited(vp.chunk);
            if (chunk == null) return;
            if (type is not MetaBlockType)
                chunk.PutVoxel(vp, type, player.placeLand);
        }

        public void TryDeleteVoxel(VoxelPosition vp)
        {
            var chunk = GetChunkIfInited(vp.chunk);
            if (chunk == null) return;
            chunk.DeleteVoxel(vp, player.HighlightLand);
        }

        public void TryDeleteMeta(MetaPosition mp)
        {
            var chunk = GetChunkIfInited(mp.chunk);
            if (chunk == null) return;
            chunk.DeleteMeta(mp);
        }

        public void TryPutMeta(MetaPosition mp, BlockType type)
        {
            var chunk = GetChunkIfInited(mp.chunk);
            if (chunk == null) return;
            if (type is MetaBlockType blockType)
                chunk.PutMeta(mp, blockType, player.placeLand);
        }

        public bool PutMetaWithProps(MetaPosition mp, MetaBlockType type, object props, Land ownerLand = null)
        {
            var vp = mp.ToVoxelPosition();
            if (ownerLand == null && !player.CanEdit(vp.ToWorld(), out ownerLand, true))
                return false;

            var chunk = GetChunkIfInited(mp.chunk);
            if (chunk != null)
            {
                chunk.PutMeta(mp, type, ownerLand);
                chunk.GetMetaAt(mp).SetProps(props, ownerLand);
            }
            else
            {
                DestroyGarbageChunkIfExists(vp.chunk);
                WorldService.INSTANCE.AddMetaBlock(mp, type, ownerLand).SetProps(props, ownerLand);
            }

            return true;
        }

        public void PutBlocks(Dictionary<VoxelPosition, Tuple<BlockType, Land>> blocks)
        {
            var chunks = new Dictionary<Chunk, Dictionary<VoxelPosition, Tuple<BlockType, Land>>>();
            foreach (var vp in blocks.Keys)
            {
                var chunk = GetChunkIfInited(vp.chunk);
                if (chunk == null)
                {
                    DestroyGarbageChunkIfExists(vp.chunk);
                    WorldService.INSTANCE.AddChange(vp, blocks[vp].Item1,
                        blocks[vp].Item2); // TODO: re-draw neighbors?
                    continue;
                }

                if (!chunks.TryGetValue(chunk, out var chunkData))
                {
                    chunkData = new Dictionary<VoxelPosition, Tuple<BlockType, Land>>();
                    chunks.Add(chunk, chunkData);
                }

                chunkData.Add(vp, blocks[vp]);
            }

            foreach (var chunk in chunks.Keys)
                chunk.PutVoxels(chunks[chunk]);
        }

        private void DeleteBlocks(Dictionary<VoxelPosition, Land> blocks)
        {
            var chunks = new Dictionary<Chunk, Dictionary<VoxelPosition, Land>>();
            // var nullChunkNeighbors = new List<Chunk>();

            foreach (var vp in blocks.Keys)
            {
                var chunk = GetChunkIfInited(vp.chunk);
                if (chunk == null)
                {
                    WorldService.INSTANCE.AddChange(vp, blocks[vp]); // TODO: re-draw neighbors?
                    continue;
                }

                if (!chunks.TryGetValue(chunk, out var chunkData))
                {
                    chunkData = new Dictionary<VoxelPosition, Land>();
                    chunks.Add(chunk, chunkData);
                }

                chunkData.Add(vp, blocks[vp]);
            }

            foreach (var chunk in chunks.Keys)
                chunk.DeleteVoxels(chunks[chunk]);
        }

        public bool IsSolidIfLoaded(Vector3Int pos)
        {
            return IsSolidIfLoaded(new VoxelPosition(pos));
        }

        public bool IsSolidIfLoaded(VoxelPosition vp)
        {
            Chunk chunk;
            if (chunks.TryGetValue(vp.chunk, out chunk) && chunk.IsInitialized())
                return chunk.GetBlock(vp.local).isSolid;
            return WorldService.INSTANCE.IsSolidIfLoaded(vp);
        }

        public static World INSTANCE
        {
            get { return GameObject.Find("World").GetComponent<World>(); }
        }
    }
}