using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class BarrelPush : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI textObject;
    public string message = "Press E to push the box";

    [Header("Push Settings")]
    public float stepDistance = 1f;
    public float moveSpeed = 5f;
    public GameObject boxToPush;

    private bool playerInRange = false;
    private Transform playerTransform;
    private Vector3 targetPosition;
    private bool isMoving = false;

    [Header("Audio")]
    public AudioSource audiosource;
    public AudioClip clip;

    private void Start()
    {
        if (textObject != null)
            textObject.gameObject.SetActive(false);

        if (boxToPush != null)
            targetPosition = boxToPush.transform.position;
    }

    private void Update()
    {
        // Move box smoothly
        if (isMoving && boxToPush != null)
        {
            boxToPush.transform.position = Vector3.MoveTowards(
                boxToPush.transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime);

            if (Vector3.Distance(boxToPush.transform.position, targetPosition) < 0.01f)
            {
                isMoving = false;

                // 🔇 Stop sound when done
                if (audiosource.isPlaying)
                    audiosource.Stop();
            }
        }

        // Start pushing when in range + press E
        if (playerInRange && !isMoving && boxToPush != null)
        {
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                Vector3 direction = playerTransform.forward;
                direction.y = 0;

                targetPosition = boxToPush.transform.position + direction.normalized * stepDistance;
                isMoving = true;

                // 🔊 Play push sound once
                if (audiosource != null && clip != null)
                    audiosource.PlayOneShot(clip);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerTransform = other.transform;

            if (textObject != null)
            {
                textObject.text = message;
                textObject.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerTransform = null;

            if (textObject != null)
                textObject.gameObject.SetActive(false);
        }
    }
}
