using System;
using System.Collections;
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

        private readonly Dictionary<Vector3Int, HashSet<Vector3Int>> selectedPositions =
            new Dictionary<Vector3Int, HashSet<Vector3Int>>(); // chunk coordinate -> selected blocks local coordinates, SelectableBlockProps

        private readonly HashSet<Chunk> chunksToUpdateHighlights = new HashSet<Chunk>();
        public Dictionary<VoxelPosition, Vector3Int> highlightOffsets = new Dictionary<VoxelPosition, Vector3Int>();


        private bool creatingChunks = false;
        private List<Chunk> chunkRequests = new List<Chunk>();

        public GameObject debugScreen;
        public GameObject inventory;
        public GameObject cursorSlot;
        public GameObject help;
        public Player player;
        private VoxelPosition firstSelectedPosition;
        public VoxelPosition lastSelectedPosition { get; private set; }

        public Material Material => material;
        public Material HighlightBlock => highlightBlock;
        public Material SelectedBlock => selectedBlock;

        public int TotalBlocksSelected =>
            selectedPositions.Values.Where(items => items != null).Sum(items => items.Count);

        public bool SelectionActive => TotalBlocksSelected > 0; // TODO: enhance performance

        private IEnumerable<VoxelPosition> SelectedVoxelPositions =>
            from chunkCoordinate in selectedPositions.Keys
            from localPosition in selectedPositions[chunkCoordinate]
            select new VoxelPosition(chunkCoordinate, localPosition);

        private void Start()
        {
            GameManager.INSTANCE.stateChange.AddListener(state =>
            {
                inventory.SetActive(state == GameManager.State.INVENTORY);
                cursorSlot.SetActive(state == GameManager.State.INVENTORY);
                help.SetActive(state == GameManager.State.HELP);
            });
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F3))
                debugScreen.SetActive(!debugScreen.activeSelf);
        }

        public Vector3Int GetHighlightOffset(VoxelPosition vp)
        {
            return highlightOffsets.TryGetValue(vp, out var offset) ? offset : Vector3Int.zero;
        }

        public HashSet<Vector3Int> GetChunkSelectedPositions(Vector3Int chunkCoordinate)
        {
            if (selectedPositions.TryGetValue(chunkCoordinate, out var chunkSelectedPositions) &&
                chunkSelectedPositions != null && chunkSelectedPositions.Count > 0)
                return chunkSelectedPositions;
            return null;
        }

        private bool AddSelectedPosition(VoxelPosition vp, bool delayedUpdate)
        {
            if (!selectedPositions.TryGetValue(vp.chunk, out var chunkSelectedBlocks))
            {
                chunkSelectedBlocks = new HashSet<Vector3Int>();
                selectedPositions[vp.chunk] = chunkSelectedBlocks;
            }

            if (chunkSelectedBlocks.Contains(vp.local))
            {
                RemoveHighlight(vp, delayedUpdate);
                return false;
            }

            chunkSelectedBlocks.Add(vp.local);
            if (TotalBlocksSelected == 1)
                firstSelectedPosition = vp;
            lastSelectedPosition = vp;

            return true;
        }

        public bool AddHighlight(VoxelPosition vp, bool delayedUpdate = false)
        {
            var result = AddSelectedPosition(vp, delayedUpdate);
            var chunk = GetChunkIfInited(vp.chunk);
            if (chunk != null) // not considering garbage chunks here, since their highlights would be rebuilt OnEnabled
                chunksToUpdateHighlights.Add(chunk);
            if (!delayedUpdate)
                UpdateHighlights();
            return result;
        }

        private void RemoveHighlight(VoxelPosition vp, bool delayedUpdate = false)
        {
            if (selectedPositions.TryGetValue(vp.chunk, out var chunkSelectedBlocks) &&
                chunkSelectedBlocks.Remove(vp.local))
            {
                var chunk = GetChunkIfInited(vp.chunk);
                if (chunk != null)
                    chunksToUpdateHighlights.Add(chunk);
            }

            if (!delayedUpdate)
                UpdateHighlights();
        }

        public void UpdateHighlights() // TODO: better name? render highlights?
        {
            foreach (var chunk in chunksToUpdateHighlights.Where(chunk => chunk.IsActive() && chunk.IsInitialized()))
                chunk.UpdateHighlight();
            chunksToUpdateHighlights.Clear();
        }

        private List<Chunk> ActiveChunksContainingSelections =>
            selectedPositions.Keys.Select(chunkCoordinate => chunks[chunkCoordinate])
                .Where(chunk => chunk.IsActive() && chunk.IsInitialized()).ToList();

        public void ClearHighlights()
        {
            var chunksToUpdate = ActiveChunksContainingSelections;
            selectedPositions.Clear();
            foreach (var chunk in chunksToUpdate)
                chunk.UpdateHighlight();
            chunksToUpdateHighlights.Clear();
            highlightOffsets.Clear();
        }

        public void RemoveSelectedBlocks(bool ignoreUnmovedBlocks = false)
        {
            var blocks = new Dictionary<VoxelPosition, Land>();

            foreach (var vp in SelectedVoxelPositions)
            {
                if (!player.CanEdit(vp.ToWorld(), out var land) ||
                    ignoreUnmovedBlocks && GetHighlightOffset(vp) == Vector3Int.zero) continue;
                var chunk = GetChunkIfInited(vp.chunk);
                if (chunk == null) continue;
                blocks.Add(vp, land);
                if (chunk.GetMetaAt(vp) != null)
                    chunk.DeleteMeta(vp);
            }

            DeleteBlocks(blocks);
        }

        public void DuplicateSelectedBlocks()
        {
            var selectedBlocks = GetSelectedBlocksProperties();
            var blocks = new Dictionary<VoxelPosition, Tuple<BlockType, Land>>();
            var metas = new Dictionary<VoxelPosition, Tuple<SelectedBlockProperties, Land>>();
            foreach (var pos in selectedBlocks.Keys)
            {
                var offset = GetHighlightOffset(pos);
                if (offset == Vector3Int.zero) continue;

                var newPos = pos.ToWorld() + offset;
                if (!player.CanEdit(newPos, out var land)) continue;

                var newPosVp = new VoxelPosition(newPos);
                var selectedBlockProperties = selectedBlocks[pos];
                blocks.Add(newPosVp,
                    new Tuple<BlockType, Land>(Blocks.GetBlockType(selectedBlockProperties.blockTypeId), land));
                if (selectedBlockProperties.metaAttached)
                    metas.Add(newPosVp, new Tuple<SelectedBlockProperties, Land>(selectedBlockProperties, land));
            }

            PutBlocks(blocks);
            foreach (var vp in metas.Keys)
            {
                var (selectedBlockProperties, land) = metas[vp];
                PutMetaWithProps(vp,
                    (MetaBlockType) Blocks.GetBlockType(selectedBlockProperties.metaBlockTypeId),
                    selectedBlockProperties.metaProperties, land);
            }
        }

        private Dictionary<VoxelPosition, SelectedBlockProperties> GetSelectedBlocksProperties()
        {
            var result = new Dictionary<VoxelPosition, SelectedBlockProperties>();
            foreach (var vp in SelectedVoxelPositions)
            {
                var props = GetSelectedBlockProperties(vp);
                if (props != null)
                    result.Add(vp, props);
            }

            return result;
        }

        private SelectedBlockProperties GetSelectedBlockProperties(VoxelPosition vp)
        {
            if (!player.CanEdit(vp.ToWorld(), out var land)) return null;

            var chunk = GetChunkIfInited(vp.chunk);
            if (chunk == null)
                return
                    null; // TODO: we need a mechanism to be able to retrieve the props independent of the related chunk

            var blockType = chunk.GetBlock(vp.local);
            if (!blockType.isSolid) return null; // air block is ignored

            var blockTypeId = blockType.id;

            var meta = chunk.GetMetaAt(vp);
            if (meta == null) return new SelectedBlockProperties(blockTypeId);
            return new SelectedBlockProperties(blockTypeId, meta.type.id, ((ICloneable) meta.GetProps())?.Clone());
        }

        public Vector3Int CalculateSelectionDisplacement(Vector3 moveDirection)
        {
            var offset = GetHighlightOffset(firstSelectedPosition);
            return Vectors.FloorToInt(offset + 0.5f * Vector3.one + moveDirection) - offset;
        }

        public void MoveSelection(Vector3Int delta)
        {
            foreach (var vp in SelectedVoxelPositions)
                highlightOffsets[vp] = GetHighlightOffset(vp) + delta;

            foreach (var chunk in ActiveChunksContainingSelections)
                chunk.UpdateHighlight();
            chunksToUpdateHighlights.Clear();
        }

        private Vector3? GetSelectionRotationCenter()
        {
            if (!SelectionActive) return null;
            return firstSelectedPosition.ToWorld() + GetHighlightOffset(firstSelectedPosition) + 0.5f * Vector3.one;
        }

        public void RotateSelection(Vector3 axis)
        {
            var center = GetSelectionRotationCenter();
            if (!center.HasValue) return;

            foreach (var vp in SelectedVoxelPositions.Where(vp => !vp.Equals(firstSelectedPosition)))
            {
                var offset = GetHighlightOffset(vp);
                var highlightPosition = vp.ToWorld() + offset;
                var rotatedVector = Quaternion.AngleAxis(90, axis) * (highlightPosition + 0.5f * Vector3.one - center.Value);
                var newHighlightPosition = Vectors.TruncateFloor(center.Value + rotatedVector - 0.5f * Vector3.one);
                highlightOffsets[vp] = offset + newHighlightPosition - highlightPosition;
            }

            foreach (var chunk in ActiveChunksContainingSelections)
                chunk.UpdateHighlight();
            chunksToUpdateHighlights.Clear();
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

        public void PutBlock(VoxelPosition vp, BlockType type)
        {
            var chunk = GetChunkIfInited(vp.chunk);
            if (chunk == null) return;
            if (type is MetaBlockType blockType)
                chunk.PutMeta(vp, blockType, player.placeLand);
            else
                chunk.PutVoxel(vp, type, player.placeLand);
        }

        public bool PutMetaWithProps(VoxelPosition vp, MetaBlockType type, object props, Land ownerLand = null)
        {
            var pos = vp.ToWorld();
            if (ownerLand == null && !player.CanEdit(pos, out ownerLand, true) || !IsSolidIfLoaded(vp))
                return false;

            var chunk = GetChunkIfInited(vp.chunk);
            if (chunk != null)
            {
                chunk.PutMeta(vp, type, ownerLand);
                chunk.GetMetaAt(vp).SetProps(props, ownerLand);
            }
            else
            {
                DestroyGarbageChunkIfExists(vp.chunk);
                WorldService.INSTANCE.AddMetaBlock(vp, type, ownerLand).SetProps(props, ownerLand);
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