using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using src.Model;
using src.Service;
using UnityEngine;

namespace src
{
    public class VoxStressTest : MonoBehaviour
    {
        [SerializeField] private string voxSampleName;

        private void Update()
        {
            if (voxSampleName == null || !Input.GetKeyDown(KeyCode.T) ||
                (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))) return;
            var textAsset = Resources.Load<TextAsset>("Test/" + voxSampleName);
            if (textAsset == null)
            {
                Debug.Log("Could not load vox sample");
                return;
            }
            StartCoroutine(PlaceBlocksTest(textAsset.text, Input.GetKey(KeyCode.RightShift)));
        }

        private static IEnumerator PlaceBlocksTest(string request, bool stone = false)
        {
            var reqs = JsonConvert.DeserializeObject<List<PlaceBlockRequest>>(request);

            while (reqs.Count > 0)
            {
                var subReqs = reqs.GetRange(0, Mathf.Min(reqs.Count, 500));
                UtopiaApi.PutBlocks(subReqs.ToDictionary(
                    req => new VoxelPosition(req.position),
                    req => WorldService.INSTANCE.GetBlockType(stone ? "stone" : req.type)));

                reqs.RemoveRange(0, Mathf.Min(reqs.Count, 500));
                yield return null;
            }
        }

        private class PlaceBlockRequest
        {
            public string type;
            public SerializableVector3 position;
        }
    }
}