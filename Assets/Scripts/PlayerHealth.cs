using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using StarterAssets; // import namespace for FirstPersonController


public class PlayerHealth : MonoBehaviour
{
    private FirstPersonController fpsController; // reference to movement script

    public float maxHealth = 100f;
    public float currentHealth;
    public float damagePerSecond = 50f;

    public TextMeshProUGUI healthText;

    private bool inLightArea = false;

    // Checkpoint handling
    private Vector3 lastCheckpointPos;
    private Quaternion lastCheckpointRot;

    private GameObject gameOverTextObj;
    private Image panelImage;

    public AudioSource AS;   // assign an AudioSource in Inspector
    public AudioSource ASVoice;
    public AudioClip clip;   // assign your AudioClip in Inspector
    public AudioClip clipGameOver;   // assign your AudioClip in Inspector

    public AudioClip Intro;
    public AudioClip Phase2;
    public AudioClip Phase3;
    public AudioClip Phase4;

    private bool hasPlayedPhase2 = false;
    private bool hasPlayedPhase3 = false;
    private bool hasPlayedPhase4 = false;

    public TextMeshProUGUI subtitleText; // assign in Inspector



    void Start()
    {
        fpsController = GetComponent<FirstPersonController>();

        currentHealth = maxHealth;

        // Default checkpoint is starting point
        lastCheckpointPos = transform.position;
        lastCheckpointRot = transform.rotation;

        // Initialize health UI
        UpdateHealthText();

        StartCoroutine(LockAndPlayAudio(Intro));

        if (AS != null && clip != null)
        {
            AS.clip = clip;      // set the clip
            AS.volume = 0.1f;
            AS.loop = true;      // make it repeat forever
            AS.playOnAwake = true; // start automatically if enabled
            AS.Play();           // play background music
        }
        else
        {
            Debug.LogWarning("AudioSource or AudioClip not assigned!");
        }
    }

    void Update()
    {
        if (inLightArea)
        {
            TakeDamage(damagePerSecond * Time.deltaTime);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthText();
    }

    private void UpdateHealthText()
    {
        if (healthText != null)
        {
            healthText.text = "Health: " + (int)currentHealth;
        }
    }

    public void Die()
    {
        AS.Pause();
        AS.clip = clipGameOver;
        AS.Play();
        Debug.Log("Player Died!");

        // Prevent multiple death triggers
        if (GameObject.Find("GameOverPanel") != null) return;

        Canvas canvas = healthText.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            // Clear existing UI
            foreach (Transform child in canvas.transform)
            {
                Destroy(child.gameObject);
            }

            // Create Game Over Panel
            GameObject panelObj = new GameObject("GameOverPanel");
            panelObj.transform.SetParent(canvas.transform, false);
            RectTransform rect = panelObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            panelImage = panelObj.AddComponent<Image>();
            panelImage.color = Color.red;

            // Add "GAME OVER" text
            gameOverTextObj = new GameObject("GameOverText");
            gameOverTextObj.transform.SetParent(panelObj.transform, false);
            TextMeshProUGUI gameOverText = gameOverTextObj.AddComponent<TextMeshProUGUI>();
            gameOverText.text = "Keep Zero Contact";
            gameOverText.alignment = TextAlignmentOptions.Center;
            gameOverText.fontSize = 120;
            gameOverText.color = Color.white;

            RectTransform textRect = gameOverTextObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            // Start coroutine to load ReloadGame scene
            StartCoroutine(LoadReloadScene());
        }
    }

    private System.Collections.IEnumerator LoadReloadScene()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("ReloadGame");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("LightArea"))
        {
            inLightArea = true;
        }

        if (other.CompareTag("LaserBeam"))
        {
            currentHealth = 0;
            Die();
        }

        if (other.CompareTag("Final"))
        {
            Won();
        }

        if (other.CompareTag("Phase2") && !hasPlayedPhase2)
        {
            hasPlayedPhase2 = true;  // mark as used
            StartCoroutine(LockAndPlayAudio(Phase2));
        }

        if (other.CompareTag("Phase3") && !hasPlayedPhase3)
        {
            hasPlayedPhase3 = true;  // mark as used
            StartCoroutine(LockAndPlayAudio(Phase3));
        }

        if (other.CompareTag("Phase4") && !hasPlayedPhase4)
        {
            hasPlayedPhase4 = true;  // mark as used
            StartCoroutine(LockAndPlayAudio(Phase4));
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("LightArea"))
        {
            inLightArea = false;
        }
    }

    private void Won()
    {
        Debug.Log("Player Died!");

        // Prevent multiple death triggers
        if (GameObject.Find("GameOverPanel") != null) return;

        Canvas canvas = healthText.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            // Destroy the detection meter (assuming it's named "DetectionMeter")
            Transform detectionMeter = canvas.transform.Find("EnemyIndecatorUI");
            if (detectionMeter != null)
                Destroy(detectionMeter.gameObject);

            // Clear existing UI
            foreach (Transform child in canvas.transform)
            {
                Destroy(child.gameObject);
            }

            // Create Game Over Panel
            GameObject panelObj = new GameObject("GameOverPanel");
            panelObj.transform.SetParent(canvas.transform, false);
            RectTransform rect = panelObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            panelImage = panelObj.AddComponent<Image>();
            panelImage.color = Color.greenYellow;

            // Add "GAME OVER" text
            gameOverTextObj = new GameObject("GameOverText");
            gameOverTextObj.transform.SetParent(panelObj.transform, false);
            TextMeshProUGUI gameOverText = gameOverTextObj.AddComponent<TextMeshProUGUI>();
            gameOverText.text = "You Won";
            gameOverText.alignment = TextAlignmentOptions.Center;
            gameOverText.fontSize = 120;
            gameOverText.color = Color.white;

            RectTransform textRect = gameOverTextObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            // Start coroutine to load ReloadGame scene
            StartCoroutine(LoadMainmenuScene());
        }
    }

    private System.Collections.IEnumerator LoadMainmenuScene()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("MainMenu");
    }

    private System.Collections.IEnumerator LockAndPlayAudio(AudioClip clipToPlay)
    {
        // Pause game (everything except audio)
        //Time.timeScale = 0f;

        // Stop current audio and play the new one
        ASVoice.Stop();
        AS.Pause();
        ASVoice.clip = clipToPlay;
        ASVoice.loop = false;
        ASVoice.Play();

        // Wait using realtime (ignores Time.timeScale)
        yield return new WaitForSecondsRealtime(ASVoice.clip.length);

        // Resume game
        //Time.timeScale = 1f;
        AS.Play();
    }

}

