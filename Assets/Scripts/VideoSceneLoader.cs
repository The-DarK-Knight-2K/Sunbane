using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;  // for new Input System

public class VideoSceneLoader : MonoBehaviour
{
    public VideoPlayer videoPlayer;   // Assign in Inspector
    public string sceneToLoad;        // Name of the next scene

    void Start()
    {
        if (videoPlayer != null)
        {
            // When video finishes, call LoadNextScene
            videoPlayer.loopPointReached += OnVideoFinished;
        }
    }

    void Update()
    {
        // If player presses SPACE -> skip video and load scene immediately
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            LoadNextScene();
        }
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        LoadNextScene();
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
