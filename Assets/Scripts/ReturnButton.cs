using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnButton : MonoBehaviour
{
    // Ta metoda pojawi się w Unity UI przy przypisaniu do przycisku
    public void Menu()
    {
        SceneManager.LoadScene(0); // lub index sceny np. 0
    }
}