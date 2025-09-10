using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;   // New Input System

public class BoxPush : MonoBehaviour
{
    [Header("UI")]
    public GameObject textObject;  // Assign UI text in Inspector

    [Header("Push Settings")]
    public float stepDistance = 1f;
    public float moveSpeed = 5f;
    public Transform pushDirectionRef;
    public string stopAreaTag = "StopArea";

    private Rigidbody rb;
    private bool playerInRange = false;
    private bool reachedTarget = false;

    private Vector3 targetStep;
    private bool isMoving = false;

    [Header("Audio")]
    public AudioSource audiosource;
    public AudioClip clip;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;  // manual movement control

        if (textObject != null)
            textObject.SetActive(false);

        targetStep = transform.position;
    }

    private void Update()
    {
        if (reachedTarget) return;

        // Move the box
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetStep,
                moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetStep) < 0.01f)
            {
                isMoving = false;

                // 🔇 Stop sound when finished
                if (audiosource.isPlaying)
                    audiosource.Stop();
            }
        }

        // Press E + W to push forward
        if (playerInRange && !isMoving &&
            Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame &&
            Keyboard.current.wKey.isPressed)
        {
            Vector3 direction = pushDirectionRef != null ? pushDirectionRef.forward : Vector3.forward;
            direction.y = 0;

            targetStep = transform.position + direction.normalized * stepDistance;
            isMoving = true;

            // 🔊 Play sound once per push
            if (audiosource != null && clip != null)
                audiosource.PlayOneShot(clip);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (reachedTarget) return;

        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            pushDirectionRef = other.transform;

            if (textObject != null)
                textObject.SetActive(true);
        }

        // Stop movement if box reaches stop area
        if (other.CompareTag(stopAreaTag))
        {
            reachedTarget = true;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;

            if (textObject != null)
                textObject.SetActive(false);

            // 🔇 Ensure sound stops
            if (audiosource.isPlaying)
                audiosource.Stop();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            pushDirectionRef = null;

            if (!reachedTarget && textObject != null)
                textObject.SetActive(false);
        }
    }
}
