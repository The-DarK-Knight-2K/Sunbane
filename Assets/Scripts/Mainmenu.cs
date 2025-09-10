using UnityEngine;
using UnityEngine.SceneManagement;

public class Mainmenu : MonoBehaviour
{
    public void Playgame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void Quitgame()
    {
        Application.Quit();
    }

    public void Mainmenu1()
    {
        SceneManager.LoadScene("MainMenu");
    }

}
