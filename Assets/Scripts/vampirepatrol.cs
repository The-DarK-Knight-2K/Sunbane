using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class vampirepatrol : MonoBehaviour
{
    [Header("Path")]
    public Transform[] waypoints;
    public bool loop = true;
    public List<int> stopAtIndices = new List<int>();
    public float waitTime = 2f;

    [Header("Agent Tuning")]
    public float speed = 2.5f;
    public float acceleration = 12f;
    public float angularSpeed = 360f;
    public float stoppingDistance = 0.25f;
    public bool autoBraking = false;

    [Header("Death / Laser")]
    public string laserTag = "LaserBeam"; // must match the tag used in LaserBeam class
    public bool useRootMotionOnDeath = false;
    public float destroyAfterSeconds = 0f;

    NavMeshAgent agent;
    Animator animator;
    int index = 0;
    bool waiting = false;

    bool isDead = false;
    EnemyVision vision;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        vision = GetComponent<EnemyVision>();

        if (animator) animator.applyRootMotion = false;

        if (agent != null)
        {
            agent.speed = speed;
            agent.acceleration = acceleration;
            agent.angularSpeed = angularSpeed;
            agent.stoppingDistance = stoppingDistance;
            agent.autoBraking = autoBraking;
            agent.updatePosition = true;
            agent.updateRotation = true;
        }
    }

    void Start()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning("No waypoints assigned.");
            enabled = false;
            return;
        }
        index = Mathf.Clamp(index, 0, waypoints.Length - 1);
        GoTo(index);
    }

    void Update()
    {
        if (isDead) return;
        if (waiting || agent == null || agent.pathPending) return;

        // Animator walking control
        if (animator)
        {
            bool walking = agent.velocity.magnitude > 0.1f;
            animator.SetBool("isWalking", walking);
        }

        // Arrival check
        bool arrived =
            agent.remainingDistance <= agent.stoppingDistance &&
            (!agent.hasPath || agent.velocity.sqrMagnitude < 0.001f);

        if (arrived)
        {
            if (stopAtIndices != null && stopAtIndices.Contains(index))
                StartCoroutine(WaitThenNext());
            else
                Next();
        }
    }

    IEnumerator WaitThenNext()
    {
        waiting = true;
        if (agent != null) agent.isStopped = true;
        if (agent != null) agent.velocity = Vector3.zero;
        if (animator) animator.SetBool("isWalking", false);

        yield return new WaitForSeconds(waitTime);

        if (agent != null) agent.isStopped = false;
        Next();
        waiting = false;
    }

    void Next()
    {
        index++;
        if (index >= waypoints.Length)
        {
            if (loop) index = 0;
            else { enabled = false; return; }
        }
        GoTo(index);
    }

    void GoTo(int i)
    {
        if (isDead) return;
        if (agent == null) return;
        if (i < 0 || i >= waypoints.Length) return;
        if (!waypoints[i]) return;
        agent.SetDestination(waypoints[i].position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

        if (other.CompareTag(laserTag))
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        // Stop agent immediately
        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
            agent.enabled = false;
        }

        StopAllCoroutines();

        // Animator: trigger death once
        if (animator != null)
        {
            if (useRootMotionOnDeath) animator.applyRootMotion = true;
            animator.SetBool("isWalking", false);
            animator.SetBool("isDead", true);

            // Freeze animator after death animation completes
            float deathLength = 0f;
            if (animator.runtimeAnimatorController != null)
            {
                // Find length of death clip
                foreach (var clip in animator.runtimeAnimatorController.animationClips)
                {
                    if (clip.name.ToLower().Contains("death"))
                    {
                        deathLength = clip.length;
                        break;
                    }
                }
            }
            if (deathLength > 0f)
                StartCoroutine(FreezeAnimatorAfter(deathLength));
        }

        if (vision != null) vision.enabled = false;

        if (destroyAfterSeconds > 0f)
            StartCoroutine(DestroyAfter(destroyAfterSeconds));

        enabled = false;
    }

    IEnumerator FreezeAnimatorAfter(float secs)
    {
        yield return new WaitForSeconds(secs);
        if (animator != null)
            animator.enabled = false; // freeze vampire on last frame of death
    }

    IEnumerator DestroyAfter(float secs)
    {
        yield return new WaitForSeconds(secs);
        Destroy(gameObject);
    }

    void OnDisable()
    {
        if (agent != null && isDead)
            agent.enabled = false;
    }
}