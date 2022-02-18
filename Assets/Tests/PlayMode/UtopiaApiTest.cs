using System.Collections;
using NUnit.Framework;
using src;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class UtopiaApiTest
{
    [Test]
    public void UtopiaApiTestSimplePasses()
    {
    }

    [UnityTest]
    public IEnumerator TestPlaceBlock()
    {
        SceneManager.LoadScene("UtopiaScene");
        while (SceneManager.GetActiveScene().name != "UtopiaScene")
        {
            yield return null;
        }

        while (GameManager.INSTANCE.GetState() != GameManager.State.PLAYING)
        {
            yield return null;
        }
        yield return null;
    }
}