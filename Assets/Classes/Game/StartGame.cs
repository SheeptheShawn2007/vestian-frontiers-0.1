using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGameButton : MonoBehaviour
{
    public string SceneToLoad;
    public void LoadScene()
    {
        if (!string.IsNullOrEmpty(SceneToLoad))
        {
            Debug.Log("Loading scene: " + SceneToLoad);
            SceneManager.LoadScene(SceneToLoad);
        }
        else
        {
            Debug.LogWarning("SceneToLoad is not set!");
        }
    }
        

}
