using UnityEngine;
using UnityEngine.SceneManagement; // Sahneler arasý geçiþ için
public class MainMenuManager : MonoBehaviour
{


    public void PlayGame()
    {
        SceneManager.LoadScene(1);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
