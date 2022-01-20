using System;
using System.Collections.Generic;
using src.Canvas;
using src.MetaBlocks;
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
        public float gravity = -9.8f;

        private float horizontal;
        private float vertical;
        private Vector3 velocity = Vector3.zero;
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
        private RaycastHit raycastHit;
        private MetaSelectable selectedMeta;
        private Collider hitCollider;
        private CharacterController controller;

        public float castStep = 0.01f;
        public byte selectedBlockId = 1;

        private void Start()
        {
            controller = GetComponent<CharacterController>();
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
            DetectSelection();
        }

        private void UpdatePlayerPosition()
        {
            var moveDirection = ((transform.forward * vertical) + (transform.right * horizontal));
            controller.Move(moveDirection * (sprinting ? sprintSpeed : walkSpeed) * Time.fixedDeltaTime);


            if (controller.isGrounded && velocity.y < 0 || floating)
                velocity.y = 0f;

            if (jumpRequest)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                if (!floating)
                    jumpRequest = false;
            }

            if (!floating && !controller.isGrounded)
                velocity.y += gravity * Time.fixedDeltaTime;

            controller.Move(velocity * Time.fixedDeltaTime);
        }

        private void DetectSelection()
        {
            if (Physics.Raycast(cam.position, cam.forward, out raycastHit))
            {
                PlaceCursorBlocks(raycastHit.point);
                if (hitCollider == raycastHit.collider) return;
                hitCollider = raycastHit.collider;
                var metaSelectable = hitCollider.gameObject.GetComponent<MetaSelectable>();
                if (metaSelectable != null)
                {
                    focusedMetaFace = null;
                    if (selectedMeta != null)
                    {
                        selectedMeta.UnSelect();
                    }

                    metaSelectable.Select();
                    selectedMeta = metaSelectable;
                    return;
                }
            }

            if (selectedMeta == null) return;
            selectedMeta.UnSelect();
            focusedMetaFace = null;
            selectedMeta = null;
            hitCollider = null;
        }

        private void Update()
        {
            if (GameManager.INSTANCE.GetState() != GameManager.State.PLAYING) return;
            GetPlayerInputs();

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
                if (chunk != null)
                {
                    chunk.DeleteVoxel(vp, highlightLand);
                    if (chunk.GetMetaAt(vp) != null)
                        chunk.DeleteMeta(vp);
                }
            }

            if (placeBlock.gameObject.activeSelf && Input.GetMouseButtonDown(1))
            {
                var vp = new VoxelPosition(placeBlock.position);
                var chunk = world.GetChunkIfInited(vp.chunk);
                if (chunk != null)
                {
                    var type = VoxelService.INSTANCE.GetBlockType(selectedBlockId);
                    if (type is MetaBlockType)
                        chunk.PutMeta(vp, type, placeLand);
                    else
                        chunk.PutVoxel(vp, type, placeLand);
                }
            }
        }

        private void PlaceCursorBlocks(Vector3 blockHitPoint)
        {
            var epsilon = cam.forward * castStep;
            var placeBlockPosInt = Vectors.FloorToInt(blockHitPoint - epsilon);

            var posInt = Vectors.FloorToInt(blockHitPoint + epsilon);
            var vp = new VoxelPosition(posInt);
            var chunk = world.GetChunkIfInited(vp.chunk);
            if (chunk == null) return;
            var metaToFocus = chunk.GetMetaAt(vp);
            var foundSolid = chunk.GetBlock(vp.local).isSolid;

            if (foundSolid)
            {
                highlightBlock.position = posInt;
                highlightBlock.gameObject.SetActive(CanEdit(posInt, out highlightLand));

                if (VoxelService.INSTANCE.GetBlockType(selectedBlockId) is MetaBlockType)
                {
                    if (chunk.GetMetaAt(vp) == null)
                    {
                        placeBlock.position = posInt;
                        placeBlock.gameObject.SetActive(CanEdit(posInt, out placeLand));
                    }
                    else
                        placeBlock.gameObject.SetActive(false);
                }
                else
                {
                    var currVox = Vectors.FloorToInt(transform.position);
                    if (placeBlockPosInt != currVox && placeBlockPosInt != currVox + Vector3Int.up)
                    {
                        placeBlock.position = placeBlockPosInt;
                        placeBlock.gameObject.SetActive(CanEdit(placeBlockPosInt, out placeLand));
                    }
                    else
                        placeBlock.gameObject.SetActive(false);
                }
            }
            else
            {
                highlightBlock.gameObject.SetActive(false);
                placeBlock.gameObject.SetActive(false);
            }

            Voxels.Face faceToFocus = null;
            if (metaToFocus != null)
            {
                if (!metaToFocus.IsPositioned()) metaToFocus = null;
                else
                {
                    faceToFocus = FindFocusedFace(blockHitPoint - posInt);
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
        }


        private Voxels.Face FindFocusedFace(Vector3 blockLocalHitPoint)
        {
            if (blockLocalHitPoint.x < castStep) return Voxels.Face.LEFT;
            if (Math.Abs(blockLocalHitPoint.x - 1) < castStep) return Voxels.Face.RIGHT;

            if (blockLocalHitPoint.z < castStep) return Voxels.Face.BACK;
            if (Math.Abs(blockLocalHitPoint.z - 1) < castStep) return Voxels.Face.FRONT;

            if (blockLocalHitPoint.y < castStep) return Voxels.Face.BOTTOM;
            if (Math.Abs(blockLocalHitPoint.y - 1) < castStep) return Voxels.Face.TOP;

            return null;
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