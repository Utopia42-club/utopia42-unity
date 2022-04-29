using Siccity.GLTFUtility;
using UnityEngine;

namespace src.TestUtils
{
    public class TestGlbImport: MonoBehaviour
    {
        [SerializeField] private string glbSampleName;
        private void Update()
        {
            if (glbSampleName == null || !Input.GetKeyDown(KeyCode.T) || !Input.GetKey(KeyCode.LeftShift)) return;
            
            // var result = Importer.LoadFromFile("Assets/Resources/Test/" + glbSampleName);
            // result.name = "Sample GLB";
            
            ImportGltfAsync("Assets/Resources/Test/" + glbSampleName);
        }

        private static void ImportGltfAsync(string filepath)
        {
            Debug.Log("Load async");
            Importer.ImportGLBAsync(filepath, new ImportSettings(), OnFinishAsync);
        }
        
        private static void OnFinishAsync(GameObject result, AnimationClip[] animationClips)
        {
            result.name = "Sample GLB";
            Debug.Log("Finished importing " + result.name);
        }
    }
}