using System.Collections.Generic;
using src.Canvas;
using src.MetaBlocks;
using src.MetaBlocks.TdObjectBlock;
using src.Model;
using src.Service;
using src.Utils;
using UnityEngine;

namespace src
{
    public class Player : MonoBehaviour
    {
        public static readonly Vector3Int viewDistance = new Vector3Int(5, 5, 5);

        private bool sprinting;

        public Transform cam;
        public World world;

        public float walkSpeed = 6f;
        public float sprintSpeed = 12f;
        public float jumpHeight = 5;

        private float horizontal;
        private float vertical;
        private Vector3 velocity;
        private Land highlightLand;
        private Land placeLand;
        private bool jumpRequest;
        private bool floating = false;

        private Vector3Int lastChunk;

        private List<Land> ownedLands = new List<Land>();

        public Transform highlightBlock;
        public Transform placeBlock;
        private MetaBlock focusedMetaBlock;
        private Voxels.Face focusedMetaFace;
        private Rigidbody rb;
        private RaycastHit raycastHit;
        private TdObjectBlockObject hitTdObjectBlock;
        private Collider hitCollider;

        public float castStep = 0.1f;
        public float reach = 8f;

        public byte selectedBlockId = 1;


        private void Start()
        {
            gameObject.AddComponent<BoxCollider>();
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            // rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.useGravity = false;
            rb.drag = 0;
            rb.angularDrag = 0;
            Snack.INSTANCE.ShowObject("Owner", null);
        }

        public List<Land> GetOwnedLands()
        {
            return ownedLands;
        }

        public void ResetLands()
        {
            List<Land> lands = null;
            string wallet = Settings.WalletId();
            if (wallet != null)
            {
                var service = VoxelService.INSTANCE;
                lands = service.GetLandsFor(wallet);
                service.RefreshChangedLands(lands);
            }

            this.ownedLands = lands != null ? lands : new List<Land>();
        }

        private void FixedUpdate()
        {
            if (GameManager.INSTANCE.GetState() != GameManager.State.PLAYING) return;
            UpdatePlayerPosition();
            DetectObjectSelection();
        }

        private void UpdatePlayerPosition()
        {
            if (floating && !jumpRequest)
            {
                var rbVelocity = rb.velocity;
                rbVelocity.y = 0;
                rb.velocity = rbVelocity;
            }

            if (sprinting)
                velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime *
                           sprintSpeed;
            else
                velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime *
                           walkSpeed;

            var nextPosition = rb.position + velocity;
            if (jumpRequest)
            {
                if (!floating)
                {
                    rb.AddForce(Vector3.up * Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y),
                        ForceMode.VelocityChange);
                    jumpRequest = false;
                }
                else
                    nextPosition += Vector3.up * jumpHeight * Time.fixedDeltaTime;
            }

            rb.MovePosition(nextPosition);
        }

        private void DetectObjectSelection()
        {
            if (Physics.Raycast(cam.position, cam.forward, out raycastHit))
            {
                if (hitCollider == raycastHit.collider) return;

                hitCollider = raycastHit.collider;
                var tdObjectBlock = hitCollider.transform.parent?.parent?.GetComponent<TdObjectBlockObject>();
                if (tdObjectBlock != null)
                {
                    if (hitTdObjectBlock != null)
                        hitTdObjectBlock.UnFocus();

                    tdObjectBlock.Focus(null);
                    hitTdObjectBlock = tdObjectBlock;
                    return;
                }
            }

            if (hitTdObjectBlock != null)
            {
                hitTdObjectBlock.UnFocus();
                hitTdObjectBlock = null;
                hitCollider = null;
            }
        }

        private void Update()
        {
            if (GameManager.INSTANCE.GetState() != GameManager.State.PLAYING) return;
            GetPlayerInputs();
            PlaceCursorBlocks();

            rb.useGravity = !floating;

            if (lastChunk == null)
            {
                lastChunk = ComputePosition().chunk;
                world.OnPlayerChunkChanged(lastChunk);
            }
            else
            {
                var currChunk = ComputePosition().chunk;
                if (!lastChunk.Equals(currChunk))
                {
                    lastChunk = currChunk;
                    world.OnPlayerChunkChanged(currChunk);
                }
            }
        }

        private void GetPlayerInputs()
        {
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");

            if (Input.GetButtonDown("Sprint"))
                sprinting = true;
            if (Input.GetButtonUp("Sprint"))
                sprinting = false;


            if (Input.GetButtonDown("Jump"))
                jumpRequest = true;
            if (Input.GetButtonUp("Jump"))
                jumpRequest = false;

            if (Input.GetButtonDown("Toggle Floating"))
                floating = !floating;

            if (highlightBlock.gameObject.activeSelf && Input.GetMouseButtonDown(0))
            {
                var vp = new VoxelPosition(highlightBlock.position);
                var chunk = world.GetChunkIfInited(vp.chunk);
                if (chunk != null) chunk.DeleteVoxel(vp, highlightLand);
            }

            if (placeBlock.gameObject.activeSelf && Input.GetMouseButtonDown(1))
            {
                var vp = new VoxelPosition(placeBlock.position);
                var chunk = world.GetChunkIfInited(vp.chunk);
                if (chunk != null)
                {
                    var type = VoxelService.INSTANCE.GetBlockType(selectedBlockId);
                    if (typeof(MetaBlockType).IsAssignableFrom(type.GetType()))
                        chunk.PutMeta(vp, type, placeLand);
                    else
                        chunk.PutVoxel(vp, type, placeLand);
                }
            }
        }

        private void PlaceCursorBlocks()
        {
            float distance = castStep;
            Vector3Int lastPos = Vectors.FloorToInt(cam.position);

            MetaBlock metaToFocus = null;
            bool foundSolid = false;
            while (distance < reach && !foundSolid)
            {
                Vector3 pos = cam.position + (cam.forward * distance);
                distance += castStep;
                Vector3Int posint = Vectors.FloorToInt(pos);
                var vp = new VoxelPosition(posint);

                var chunk = world.GetChunkIfInited(vp.chunk);
                if (chunk == null) break;

                if (metaToFocus == null)
                    metaToFocus = chunk.GetMetaAt(vp);

                if (foundSolid = chunk.GetBlock(vp.local).isSolid)
                {
                    highlightBlock.position = posint;
                    highlightBlock.gameObject.SetActive(CanEdit(posint, out highlightLand));

                    if (typeof(MetaBlockType).IsAssignableFrom(
                            VoxelService.INSTANCE.GetBlockType(selectedBlockId).GetType()))
                    {
                        if (chunk.GetMetaAt(vp) == null)
                        {
                            placeBlock.position = posint;
                            placeBlock.gameObject.SetActive(CanEdit(posint, out placeLand));
                        }
                        else
                            placeBlock.gameObject.SetActive(false);
                    }
                    else
                    {
                        var currVox = Vectors.FloorToInt(transform.position);
                        if (lastPos != currVox && lastPos != currVox + Vector3Int.up)
                        {
                            placeBlock.position = lastPos;
                            placeBlock.gameObject.SetActive(CanEdit(lastPos, out placeLand));
                        }
                        else
                            placeBlock.gameObject.SetActive(false);
                    }
                }

                lastPos = posint;
            }

            Voxels.Face faceToFocus = null;
            if (metaToFocus != null)
            {
                if (!metaToFocus.IsPositioned()) metaToFocus = null;
                else
                {
                    faceToFocus = FindFocusedFace(metaToFocus.GetPosition());
                    if (faceToFocus == null) metaToFocus = null;
                }
            }

            if (focusedMetaBlock != metaToFocus || faceToFocus != focusedMetaFace)
            {
                if (focusedMetaBlock != null)
                    focusedMetaBlock.UnFocus();
                focusedMetaBlock = metaToFocus;
                focusedMetaFace = faceToFocus;
                if (focusedMetaBlock != null)
                {
                    if (!focusedMetaBlock.Focus(focusedMetaFace))
                    {
                        focusedMetaBlock = null;
                        focusedMetaFace = null;
                    }
                }
            }

            if (!foundSolid)
            {
                highlightBlock.gameObject.SetActive(false);
                placeBlock.gameObject.SetActive(false);
            }
        }


        private Voxels.Face FindFocusedFace(Vector3 pos)
        {
            var localPos = cam.position - pos;

            if (IsAimedAt(localPos.x, localPos.z, cam.forward.x, cam.forward.z) &&
                IsAimedAt(localPos.x, localPos.y, cam.forward.x, cam.forward.y))
                return Voxels.Face.LEFT;

            if (IsAimedAt(localPos.z, localPos.x, cam.forward.z, cam.forward.x) &&
                IsAimedAt(localPos.z, localPos.y, cam.forward.z, cam.forward.y))
                return Voxels.Face.BACK;

            if (IsAimedAt(localPos.y, localPos.z, cam.forward.y, cam.forward.z) &&
                IsAimedAt(localPos.y, localPos.x, cam.forward.y, cam.forward.x))
                return Voxels.Face.BOTTOM;


            localPos -= Vector3.one;
            if (IsAimedAt(-localPos.y, -localPos.z, -cam.forward.y, -cam.forward.z) &&
                IsAimedAt(-localPos.y, -localPos.x, -cam.forward.y, -cam.forward.x))
                return Voxels.Face.TOP;


            if (IsAimedAt(-localPos.x, -localPos.z, -cam.forward.x, -cam.forward.z) &&
                IsAimedAt(-localPos.x, -localPos.y, -cam.forward.x, -cam.forward.y))
                return Voxels.Face.RIGHT;

            if (IsAimedAt(-localPos.z, -localPos.x, -cam.forward.z, -cam.forward.x) &&
                IsAimedAt(-localPos.z, -localPos.y, -cam.forward.z, -cam.forward.y))
                return Voxels.Face.FRONT;
            return null;
        }

        private bool IsAimedAt(float posX, float posZ, float forwardX, float forwardZ)
        {
            var pos2d = new Vector2(posX, posZ);
            var lower = Vector2.SignedAngle(Vector2.right, -pos2d);
            var upper = Vector2.SignedAngle(Vector2.right, Vector2.up - pos2d);
            var actual = Vector2.SignedAngle(Vector2.right, new Vector2(forwardX, forwardZ));
            return lower < upper && lower < actual && upper > actual;
        }

        public bool CanEdit(Vector3Int position, out Land land)
        {
            if (Settings.IsGuest())
            {
                land = null;
                return true;
            }

            land = FindOwnedLand(position);
            return land != null && !land.isNft;
        }

        private Land FindOwnedLand(Vector3Int position)
        {
            if (highlightLand != null && highlightLand.Contains(ref position))
                return highlightLand;
            if (placeLand != null && placeLand.Contains(ref position))
                return placeLand;
            foreach (var land in ownedLands)
                if (land.Contains(ref position))
                    return land;
            return null;
        }

        private VoxelPosition ComputePosition()
        {
            return new VoxelPosition(transform.position);
        }


        public static Player INSTANCE
        {
            get { return GameObject.Find("Player").GetComponent<Player>(); }
        }
    }
}