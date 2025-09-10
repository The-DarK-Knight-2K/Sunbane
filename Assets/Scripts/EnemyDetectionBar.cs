using UnityEngine;
using UnityEngine.UI;

public class EnemyDetectionBar : MonoBehaviour
{
    [Header("References")]
    public Canvas uiCanvas;              // Assign your Detection Canvas
    public GameObject barPrefab;         // Prefab with Slider + Arrow (arrow should be a child RectTransform named "Arrow" or assign in inspector)
    public Transform enemyTransform;     // Enemy GameObject
    public EnemyVision vision;           // Your EnemyVision script

    [Header("Settings")]
    public float screenEdgeBuffer = 50f;   // Distance from screen edges
    public Vector2 barSize = new Vector2(100, 20); // Width/height of bar prefab

    // Runtime
    private GameObject barGO;
    private Slider slider;
    private RectTransform barRect;
    private RectTransform arrowRect;      // the arrow image rect that will rotate
    private Camera mainCamera;
    private RectTransform canvasRect;

    void Start()
    {
        mainCamera = Camera.main;

        if (uiCanvas == null)
            uiCanvas = FindObjectOfType<Canvas>();

        if (uiCanvas == null)
        {
            Debug.LogError("No Canvas found! Assign Detection Canvas in Inspector.");
            return;
        }

        canvasRect = uiCanvas.GetComponent<RectTransform>();

        // Instantiate prefab
        barGO = Instantiate(barPrefab, uiCanvas.transform);
        barRect = barGO.GetComponent<RectTransform>();
        barRect.sizeDelta = barSize;

        // Get slider component
        slider = barGO.GetComponentInChildren<Slider>();

        // Try to find Arrow child automatically (or user can assign via inspector)
        Transform arrowT = barGO.transform.Find("Arrow");
        if (arrowT != null)
            arrowRect = arrowT.GetComponent<RectTransform>();
        else
        {
            // fallback: find first child RectTransform that is not the slider's handle/fill
            foreach (RectTransform rt in barGO.GetComponentsInChildren<RectTransform>(true))
            {
                if (rt == barRect) continue;
                if (slider != null && rt == slider.fillRect) continue;
                if (rt.name.ToLower().Contains("arrow") || rt.name.ToLower().Contains("indicator"))
                {
                    arrowRect = rt;
                    break;
                }
            }
        }

        // Assign vision if not set
        if (vision == null && enemyTransform != null)
            vision = enemyTransform.GetComponent<EnemyVision>();

        // Start hidden until detection > 0
        if (barGO != null)
            barGO.SetActive(false);
    }

    void Update()
    {
        if (enemyTransform == null || barGO == null) return;

        // If vision missing, we still show arrow if enemy exists? follow requirement: only appear when detection > 0
        float detectionValue = (vision != null) ? vision.detection : 0f;
        if (detectionValue <= 0f)
        {
            if (barGO.activeSelf) barGO.SetActive(false);
            return;
        }
        else
        {
            if (!barGO.activeSelf) barGO.SetActive(true);
        }

        UpdateSlider(detectionValue);
        UpdatePositionAndRotation();
    }

    void UpdateSlider(float detectionValue)
    {
        if (slider == null || vision == null) return;

        float normalized = Mathf.Clamp01(detectionValue / Mathf.Max(0.0001f, vision.detectionMax));
        slider.value = normalized;

        // Optional: change fill color (green → red)
        if (slider.fillRect != null)
        {
            Image fillImage = slider.fillRect.GetComponent<Image>();
            if (fillImage != null)
                fillImage.color = Color.Lerp(Color.green, Color.red, normalized);
        }
    }

    void UpdatePositionAndRotation()
    {
        // Get enemy screen position
        Vector3 enemyScreenPos = mainCamera.WorldToScreenPoint(enemyTransform.position);

        // If enemy is behind the camera, invert direction so arrow points sensibly
        bool isBehind = enemyScreenPos.z < 0f;

        // Determine a screen X to place the bar (clamped inside horizontal edges)
        float clampedX = Mathf.Clamp(enemyScreenPos.x, screenEdgeBuffer, Screen.width - screenEdgeBuffer);
        // Top Y (meter will stay at top)
        float topY = Screen.height - screenEdgeBuffer;

        // If behind camera, project the enemy mirrored on screen center horizontally (optional tweak)
        if (isBehind)
        {
            // Mirror horizontally around screen center so arrow indicates roughly opposite direction
            clampedX = Mathf.Clamp(Screen.width - enemyScreenPos.x, screenEdgeBuffer, Screen.width - screenEdgeBuffer);
        }

        Vector2 screenPosTop = new Vector2(clampedX, topY);

        // Convert to canvas local position (respecting canvas render mode)
        Camera camForCanvas = (uiCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : uiCanvas.worldCamera;
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosTop, camForCanvas, out localPoint);
        barRect.anchoredPosition = localPoint;

        // Keep the bar upright (no rotation for the whole bar)
        barRect.localEulerAngles = Vector3.zero;

        // Compute direction from top-center of screen to enemy's screen pos (use center of screen as reference)
        Vector2 screenCenter = new Vector2(Screen.width, Screen.height) * 0.5f;
        Vector2 dir = new Vector2(enemyScreenPos.x, enemyScreenPos.y) - screenCenter;

        if (isBehind)
            dir = -dir; // flip the direction when behind camera

        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector2.up;

        dir.Normalize();

        // Rotate the arrow child to point toward the enemy direction
        if (arrowRect != null)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            // adjust -90 depending on arrow art pointing upwards by default
            arrowRect.localEulerAngles = new Vector3(0f, 0f, angle - 90f);
        }
    }

    void OnDestroy()
    {
        if (barGO != null)
            Destroy(barGO);
    }
}
