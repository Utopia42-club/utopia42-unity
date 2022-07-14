#if UNITY_EDITOR

using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Source
{
    public class WebglPreBuildProcessing : IPreprocessBuildWithReport
    {
        public int callbackOrder => 1;

        public void OnPreprocessBuild(BuildReport report)
        {
            System.Environment.SetEnvironmentVariable("EMSDK_PYTHON",
                "/opt/homebrew/bin/python3");
        }
    }
}
#endif