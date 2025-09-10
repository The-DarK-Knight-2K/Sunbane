using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;   // new input system

public class RotateMirror : MonoBehaviour
{
    [Header("UI Settings")]
    public GameObject textObject;   // Assign Canvas Text object in Inspector
    public string instructionText = "Press Q or E";

    [Header("Target to Rotate")]
    public Transform targetObject;  // Object to rotate
    public float rotateStep = 10f;  // Degrees per press

    private bool playerInRange = false;
    private TextMeshProUGUI tmpText;

    void Start()
    {
        if (textObject != null)
        {
            textObject.SetActive(false);
            tmpText = textObject.GetComponent<TextMeshProUGUI>();
        }
    }

    void Update()
    {
        if (playerInRange && targetObject != null)
        {
            var kb = Keyboard.current;   // use new InputSystem keyboard
            if (kb != null)
            {
                if (kb.qKey.wasPressedThisFrame)
                {
                    targetObject.Rotate(0f, 0f, -rotateStep);
                }
                else if (kb.eKey.wasPressedThisFrame)
                {
                    targetObject.Rotate(0f, 0f , rotateStep);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (textObject != null)
            {
                textObject.SetActive(true);
                if (tmpText != null)
                    tmpText.text = instructionText;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (textObject != null)
                textObject.SetActive(false);
        }
    }
}
