using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class EnemyPatrolNavMesh : MonoBehaviour
{
    [Header("Path")]
    public Transform[] waypoints;
    public bool loop = true;

    [Tooltip("Indices in 'waypoints' where the agent will stop and wait.")]
    public List<int> stopAtIndices = new List<int>(); // e.g., [0, 3, 5]
    public float waitTime = 7f;

    [Header("Agent Tuning")]
    public float speed = 3f;
    public float acceleration = 8f;
    public float angularSpeed = 180f;
    public float stoppingDistance = 0.5f;
    public bool autoBraking = false;

    NavMeshAgent agent;
    Animator animator;
    int index = 0;
    bool waiting = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (animator) animator.applyRootMotion = false;

        agent.speed = speed;
        agent.acceleration = acceleration;
        agent.angularSpeed = angularSpeed;
        agent.stoppingDistance = stoppingDistance;
        agent.autoBraking = autoBraking;

        agent.updatePosition = true;
        agent.updateRotation = true;
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
        if (waiting || agent.pathPending) return;

        // --- Animator control with isWalking ---
        if (animator)
        {
            bool walking = agent.velocity.magnitude > 0.1f;
            animator.SetBool("isWalking", walking);
        }

        // --- Arrival check ---
        bool arrived =
            agent.remainingDistance <= agent.stoppingDistance &&
            (!agent.hasPath || agent.velocity.sqrMagnitude < 0.001f);

        if (arrived)
        {
            if (stopAtIndices != null && stopAtIndices.Contains(index))
            {
                StartCoroutine(WaitThenNext());
            }
            else
            {
                Next();
            }
        }
    }

    IEnumerator WaitThenNext()
    {
        waiting = true;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        if (animator) animator.SetBool("isWalking", false); // switch to idle

        yield return new WaitForSeconds(waitTime);

        agent.isStopped = false;
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
        if (!waypoints[i]) return;
        agent.SetDestination(waypoints[i].position);
    }
}