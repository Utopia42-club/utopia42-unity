using System.Text;
using Unity.Profiling;
using UnityEngine;

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

        private void OnEnable()
        {
            totalReservedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Reserved Memory");
            gcReservedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory");
            systemUsedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
            textureMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Texture Memory");
            meshMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Mesh Memory");
            
        }

        private void OnDisable()
        {
            totalReservedMemoryRecorder.Dispose();
            gcReservedMemoryRecorder.Dispose();
            systemUsedMemoryRecorder.Dispose();
            textureMemoryRecorder.Dispose();
            meshMemoryRecorder.Dispose();
        }

        private void Update()
        {
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
            statsText = sb.ToString();
        }

        private void OnGUI()
        {
            GUI.TextArea(new Rect(10, 30, 250, 80), statsText);
        }
    }
}