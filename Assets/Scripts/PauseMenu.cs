using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Linq; // for .Contai

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel; // Assign your panel in Inspector
    public static bool isPaused = false;

    public AudioSource audioSource;      // Drag your AudioSource here
    public AudioClip[] specialClips;   // Drag your special clip here

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            
            if(audioSource.isPlaying && specialClips.Contains(audioSource.clip))
            {
                Debug.Log("d");
                return;
            }
            else
            {
                if (isPaused)
                    ResumeGame();
                else
                    PauseGame();
            }

            
        }
    }

    public void PauseGame()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

        // Reset all button colors so highlight works again
        Button[] buttons = pausePanel.GetComponentsInChildren<Button>();
        foreach (Button btn in buttons)
        {
            // Deselect any currently selected button
            btn.OnDeselect(null);

            // Reset the button's visual state
            var cb = btn.colors;
            btn.targetGraphic.color = cb.normalColor;

            // Force the Button to refresh its state
            btn.OnPointerExit(null);
            btn.OnPointerEnter(null);
        }
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
