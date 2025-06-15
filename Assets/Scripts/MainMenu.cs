using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void Quit()
    {
        Application.Quit();
    }

    public void Play()
    {
        SceneManager.LoadSceneAsync(1);
    }

}
