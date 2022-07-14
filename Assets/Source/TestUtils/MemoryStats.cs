using System;
using System.Text;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;

namespace Source.TestUtils
{
    public class MemoryStats : MonoBehaviour
    {
        private string statsText;
        private ProfilerRecorder totalReservedMemoryRecorder;
        private ProfilerRecorder gcReservedMemoryRecorder;
        private ProfilerRecorder systemUsedMemoryRecorder;
        private ProfilerRecorder textureMemoryRecorder;
        private ProfilerRecorder meshMemoryRecorder;
        private bool visible = false;

        private void Enable()
        {
            totalReservedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Reserved Memory");
            gcReservedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory");
            systemUsedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
            textureMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Texture Memory");
            meshMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Mesh Memory");
            visible = true;
        }

        private void Disable()
        {
            totalReservedMemoryRecorder.Dispose();
            gcReservedMemoryRecorder.Dispose();
            systemUsedMemoryRecorder.Dispose();
            textureMemoryRecorder.Dispose();
            meshMemoryRecorder.Dispose();
            visible = false;
        }

        private void Start()
        {
            Enable();
            Disable();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                if (totalReservedMemoryRecorder.Valid)
                    Disable();
                else
                    Enable();
            }

            if (!visible) return;

            var sb = new StringBuilder(800);
            if (totalReservedMemoryRecorder.Valid)
                sb.AppendLine($"Total Reserved Memory: {Mathf.Floor(totalReservedMemoryRecorder.LastValue / 1000000)}");
            if (gcReservedMemoryRecorder.Valid)
                sb.AppendLine($"GC Reserved Memory: {Mathf.Floor(gcReservedMemoryRecorder.LastValue / 1000000)}");
            if (systemUsedMemoryRecorder.Valid)
                sb.AppendLine($"System Used Memory: {Mathf.Floor(systemUsedMemoryRecorder.LastValue / 1000000)}");
            if (textureMemoryRecorder.Valid)
                sb.AppendLine($"Texture Memory: {Mathf.Floor(textureMemoryRecorder.LastValue / 1000000)}");
            if (meshMemoryRecorder.Valid)
                sb.AppendLine($"Mesh Memory: {Mathf.Floor(meshMemoryRecorder.LastValue / 1000000)}");

            Players.INSTANCE.GetStatistics(out var playersCount, out var avatarsCount);
            sb.AppendLine($"Other Players Count: {playersCount}");
            sb.AppendLine($"Other Players Avatar Count: {avatarsCount}");

            statsText = sb.ToString();
        }

        private void OnGUI()
        {
            if (visible)
                GUI.TextArea(new Rect(10, 30, 250, 110), statsText);
        }
    }
}