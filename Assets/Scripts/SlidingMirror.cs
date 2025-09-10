using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class SlidingMirror : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI textObject;

    [Header("Slide Settings")]
    public float moveSpeed = 5f;
    public float stopDistance = 0.5f; // distance before mark to stop
    public Transform playerRef;

    [Header("Slider Objects")]
    public Transform sliderParent; // Parent holding the slider parts

    [Header("Marks / Stop Points")]
    public Transform markL;  // Left stop
    public Transform markR;  // Right stop

    private bool playerInRange = false;
    private bool canMoveLeft = true;
    private bool canMoveRight = true;

    private void Start()
    {
        if (textObject != null)
            textObject.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!playerInRange) return;

        UpdateInstructionUI();

        float moveInput = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.qKey.isPressed) moveInput = -1f;
            else if (Keyboard.current.eKey.isPressed) moveInput = 1f;
        }

        // Calculate distance to left and right marks
        float distLeft = Vector3.Distance(sliderParent.position, markL.position);
        float distRight = Vector3.Distance(sliderParent.position, markR.position);

        Debug.Log(distLeft + "  "+distRight);

        // Prevent moving too close to marks
        canMoveLeft = distRight > stopDistance;
        canMoveRight = distLeft > stopDistance;

        if ((moveInput < 0 && canMoveLeft) || (moveInput > 0 && canMoveRight))
        {
            // Move along slider's local right axis
            Vector3 moveDir = sliderParent.right * moveInput * moveSpeed * Time.deltaTime;
            sliderParent.position += moveDir;
        }
    }

    private void UpdateInstructionUI()
    {
        if (textObject == null) return;

        string msg = "";
        if (canMoveLeft) msg += "Press Q to Left\n";
        if (canMoveRight) msg += "Press E to Right";

        textObject.text = msg;
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerRef = other.transform;
            if (textObject != null)
                textObject.gameObject.SetActive(true);
            UpdateInstructionUI();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerRef = null;
            if (textObject != null)
                textObject.gameObject.SetActive(false);
        }
    }
}
