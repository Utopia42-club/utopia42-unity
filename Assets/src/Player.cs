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

        private bool grounded;
        private bool sprinting;

        public Transform cam;
        public World world;

        public float walkSpeed = 4f;
        public float sprintSpeed = 8f;
        public float jumpForce = 8f;
        public float gravity = -9.8f;

        public float playerWidth = 0.15f;
        public float boundsTolerance = 0.1f;

        private float horizontal;
        private float vertical;
        private Vector3 velocity;
        private float verticalMomentum = 0;
        private bool jumpRequest;
        private bool floating = false;
        private Land highlightLand;
        private Land placeLand;

        private Vector3Int lastChunk;

        private List<Land> ownedLands = new List<Land>();

        public Transform highlightBlock;
        public Transform placeBlock;
        private MetaBlock focusedMetaBlock;
        private Voxels.Face focusedMetaFace;

        public float castStep = 0.1f;
        public float reach = 8f;

        public byte selectedBlockId = 1;


        private void Start()
        {
            gameObject.AddComponent<CapsuleCollider>();
            var rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX |
                             RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationY;
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
            CalculateVelocity();
            if (jumpRequest)
                Jump();

            transform.Translate(velocity, Space.World);
        }

        private void Update()
        {
            if (GameManager.INSTANCE.GetState() != GameManager.State.PLAYING) return;
            GetPlayerInputs();
            PlaceCursorBlocks();

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

        void Jump()
        {
            verticalMomentum = jumpForce;
            grounded = false;
            jumpRequest = false;
        }

        private void CalculateVelocity()
        {
            // Affect vertical momentum with gravity.
            if (!floating && verticalMomentum > gravity)
                verticalMomentum += Time.fixedDeltaTime * gravity;

            // if we're sprinting, use the sprint multiplier.
            if (sprinting)
                velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime *
                           sprintSpeed;
            else
                velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime *
                           walkSpeed;

            // Apply vertical momentum (falling/jumping).
            velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;
            if (floating)
                verticalMomentum = 0;

            if ((velocity.z > 0 && front) || (velocity.z < 0 && back))
                velocity.z = 0;
            if ((velocity.x > 0 && right) || (velocity.x < 0 && left))
                velocity.x = 0;

            if (velocity.y < 0)
                velocity.y = ComputeDownSpeed(velocity.y);
            else if (velocity.y > 0)
                velocity.y = ComputeUpSpeed(velocity.y);
        }

        private void GetPlayerInputs()
        {
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");

            if (Input.GetButtonDown("Sprint"))
                sprinting = true;
            if (Input.GetButtonUp("Sprint"))
                sprinting = false;

            if (Input.GetButtonDown("Toggle Floating"))
                floating = !floating;
            //if (grounded && Input.GetButtonDown("Jump"))
            if (Input.GetButton("Jump"))
                jumpRequest = true;

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

        public Land FindOwnedLand(Vector3Int position)
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

        private float ComputeDownSpeed(float downSpeed)
        {
            if (grounded = CollidesXz(new Vector3(0, downSpeed, 0)))
            {
                return Mathf.Min(Mathf.FloorToInt(transform.position.y + 0.01f) - transform.position.y, 0);
            }

            return downSpeed;
        }

        private float ComputeUpSpeed(float upSpeed)
        {
            if (CollidesXz(new Vector3(0, upSpeed + 0.05f, 0)))
                return 0;
            return upSpeed;
        }

        public bool front
        {
            get { return CollidesXz(new Vector3(0, 0, +playerWidth)); }
        }

        public bool back
        {
            get { return CollidesXz(new Vector3(0, 0, -playerWidth)); }
        }

        public bool left
        {
            get { return CollidesXz(new Vector3(-playerWidth, 0, 0)); }
        }

        public bool right
        {
            get { return CollidesXz(new Vector3(+playerWidth, 0, 0)); }
        }

        private bool CollidesXz(Vector3 offset)
        {
            var center = transform.position + offset;
            float[] dys = new float[] {0.01f, 0.95f, 1.95f};
            int[] coef = new int[] {-1, 0, 1};
            foreach (int xcoef in coef)
            {
                foreach (int zcoef in coef)
                {
                    foreach (float dy in dys)
                    {
                        var delta = new Vector3(xcoef * playerWidth, dy, zcoef * playerWidth); //FIXME ?
                        if (world.IsSolidAt(Vectors.FloorToInt(center + delta)))
                            return true;
                    }
                }
            }

            return false;
        }

        public VoxelPosition ComputePosition()
        {
            return new VoxelPosition(transform.position);
        }


        public static Player INSTANCE
        {
            get { return GameObject.Find("Player").GetComponent<Player>(); }
        }
    }
}