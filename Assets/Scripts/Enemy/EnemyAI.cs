using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public Transform eyePoint;
    public LayerMask obstacleMask;
    public LayerMask playerMask;

    [Header("FOV Settings")]
    public float viewRadius = 15f;
    [Range(0, 360)] public float viewAngle = 90f;

    [Header("Chase Settings")]
    public float loseInterestTime = 3f;
    public float attackRange = 2f;      // distance at which enemy stops and attacks
    public float attackRangeBuffer = 0.3f; // prevents jittering in/out of attack range

    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    public float patrolWaitTime = 2f;

    Transform player;
    NavMeshAgent agent;
    enum State { Patrol, Chase, Attack, Search }
    State currentState = State.Patrol;

    Vector3 lastKnownPosition;
    float searchTimer;
    int currentPatrolIndex;
    float patrolTimer;
    bool waitingAtPatrolPoint;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
        else Debug.LogWarning($"{name}: No GameObject tagged 'Player' found in the scene.");

        if (eyePoint == null)
        {
            Debug.LogWarning($"{name}: Eye Point not assigned, using transform instead.");
            eyePoint = transform;
        }

        float startDelay = Random.Range(0f, 0.15f);
        InvokeRepeating(nameof(DetectionCheck), startDelay, 0.15f);
    }

    void DetectionCheck()
    {
        if (player == null) return;
        bool sees = CanDetectPlayer();

        switch (currentState)
        {
            case State.Patrol:
                if (sees) currentState = State.Chase;
                break;

            case State.Chase:
            case State.Attack:
                if (sees) lastKnownPosition = player.position;
                else
                {
                    currentState = State.Search;
                    searchTimer = loseInterestTime;
                }
                break;

            case State.Search:
                if (sees) currentState = State.Chase;
                break;
        }
    }

    void Update()
    {
        if (player == null || !agent.isOnNavMesh) return;

        float distToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Patrol:
                Patrol();
                break;

            case State.Chase:
                agent.isStopped = false;
                agent.SetDestination(player.position);

                if (distToPlayer <= attackRange)
                {
                    currentState = State.Attack;
                }
                break;

            case State.Attack:
                agent.isStopped = true; // fully stop, no more pushing into player
                FacePlayer();

                // TODO: trigger attack animation / damage here

                // step back out of Attack if player moves away past a buffer,
                // so it doesn't flicker Chase/Attack every frame at the boundary
                if (distToPlayer > attackRange + attackRangeBuffer)
                {
                    currentState = State.Chase;
                }
                break;

            case State.Search:
                agent.isStopped = false;
                agent.SetDestination(lastKnownPosition);
                searchTimer -= Time.deltaTime;
                bool reachedLastKnown = !agent.pathPending && agent.remainingDistance < 0.5f;
                if (searchTimer <= 0f && reachedLastKnown) currentState = State.Patrol;
                break;
        }
    }

    void FacePlayer()
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 8f);
    }

    bool CanDetectPlayer()
    {
        Vector3 dirToPlayer = player.position - eyePoint.position;
        float dist = dirToPlayer.magnitude;
        dirToPlayer.Normalize();

        if (dist > viewRadius) return false;
        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        if (angle > viewAngle / 2f) return false;

        if (Physics.Raycast(eyePoint.position, dirToPlayer, out RaycastHit hit, dist, obstacleMask | playerMask))
            return hit.transform.CompareTag("Player");
        return false;
    }

    void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0 || !agent.isOnNavMesh) return;
        agent.isStopped = false;

        if (waitingAtPatrolPoint)
        {
            patrolTimer -= Time.deltaTime;
            if (patrolTimer <= 0f)
            {
                waitingAtPatrolPoint = false;
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            }
            return;
        }

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            waitingAtPatrolPoint = true;
            patrolTimer = patrolWaitTime;
        }
        else if (!agent.hasPath)
        {
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
    }

    void OnDrawGizmosSelected()
    {
        Transform eye = eyePoint != null ? eyePoint : transform;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Vector3 leftBoundary = DirFromAngle(-viewAngle / 2f);
        Vector3 rightBoundary = DirFromAngle(viewAngle / 2f);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(eye.position, eye.position + leftBoundary * viewRadius);
        Gizmos.DrawLine(eye.position, eye.position + rightBoundary * viewRadius);

        if (Application.isPlaying && player != null)
        {
            Gizmos.color = CanDetectPlayer() ? Color.green : Color.red;
            Gizmos.DrawLine(eye.position, player.position);
        }
    }

    Vector3 DirFromAngle(float angleDeg)
    {
        angleDeg += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angleDeg * Mathf.Deg2Rad), 0, Mathf.Cos(angleDeg * Mathf.Deg2Rad));
    }
}