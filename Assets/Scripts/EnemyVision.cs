using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyVision : MonoBehaviour
{
    [Header("References")]
    public Transform eyeTransform; // The "eye" of the enemy (usually head or chest)
    public Transform player;       // Assign your player here

    [Header("Vision Settings")]
    public float viewRadius = 10f;       // How far enemy can see
    [Range(0, 360)]
    public float viewAngle = 90f;        // Pizza slice angle
    public float viewHeight = 2f;        // Vertical height (like cone height)

    [Header("Layers")]
    public LayerMask playerMask;         // Layer = Player
    public LayerMask environmentMask;    // Layer = Environment

    [Header("Detection Settings")]
    public float detection = 0f;         // Current value
    public float detectionMax = 100f;    // Max value
    public float minRate = 5f;           // Fill rate (when far)
    public float maxRate = 25f;          // Fill rate (when close)
    public float decayRate = 10f;        // How fast it decreases

    public bool playerVisible = false;
    public TextMeshProUGUI healthText;

    private Image panelImage;
    private GameObject gameOverTextObj;

    public AudioSource AS;   // assign an AudioSource in Inspector
    public AudioClip clipGameOver;   // assign your AudioClip in Inspector

    void Update()
    {
        if (eyeTransform == null) eyeTransform = transform;

        CheckVision();
        UpdateDetection(Time.deltaTime);
    }

    void CheckVision()
    {
        playerVisible = false;

        Vector3 dirToPlayer = player.position - eyeTransform.position;

        // 1) Check distance
        float distance = dirToPlayer.magnitude;
        if (distance > viewRadius) return;

        // 2) Check vertical
        if (Mathf.Abs(dirToPlayer.y) > viewHeight * 0.5f) return;

        // 3) Check angle
        Vector3 dirFlat = Vector3.ProjectOnPlane(dirToPlayer, Vector3.up).normalized;
        float angle = Vector3.Angle(eyeTransform.forward, dirFlat);
        if (angle > viewAngle * 0.5f) return;

        // 4) Check obstruction (walls in Environment layer)
        if (Physics.Raycast(eyeTransform.position, dirToPlayer.normalized, distance, environmentMask))
            return;

        // ✅ If we reach here → Player is visible
        playerVisible = true;
    }

    void UpdateDetection(float deltaTime)
    {
        if (playerVisible)
        {
            float distance = Vector3.Distance(eyeTransform.position, player.position);
            float t = Mathf.Clamp01(distance / viewRadius);
            float rate = Mathf.Lerp(maxRate, minRate, t);

            detection += rate * deltaTime;
        }
        else
        {
            detection -= decayRate * deltaTime;
        }

        detection = Mathf.Clamp(detection, 0f, detectionMax);

        // Check for Game Over
        if (detection >= detectionMax)
        {
            Debug.Log("DIE");
            Die();
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

    // Gizmos in Scene view (to see the pizza slice)
    void OnDrawGizmosSelected()
    {
        if (eyeTransform == null) eyeTransform = transform;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eyeTransform.position, viewRadius);

        Vector3 left = Quaternion.Euler(0, -viewAngle / 2, 0) * eyeTransform.forward;
        Vector3 right = Quaternion.Euler(0, viewAngle / 2, 0) * eyeTransform.forward;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(eyeTransform.position, eyeTransform.position + left * viewRadius);
        Gizmos.DrawLine(eyeTransform.position, eyeTransform.position + right * viewRadius);
    }
}