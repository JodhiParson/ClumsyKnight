using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

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
    public float moveSpeed = 3.5f; // keep this below the player's walk speed (and well below sprint) so it can't outrun/flee from them
    public float loseInterestTime = 3f;
    public float leashRange = 20f; // how far from spawn before giving up the chase entirely
    public float predictionTime = 0.4f; // how far ahead to lead the player's position while chasing

    [Header("Engagement / Circle-Strafe")]
    public float closeDistanceThreshold = 7f;      // beyond this, just close the gap directly instead of circling
    public float preferredEngageDistanceMin = 2.5f; // once engaged, orbit no closer than this
    public float preferredEngageDistanceMax = 4.5f; // ...and no farther than this
    public float circleStrafeAngularSpeed = 35f;    // degrees/sec the enemy orbits the player at - keep this modest, see note in Start()
    public float strafeDirectionChangeInterval = 2.5f; // how often it might flip orbit direction, for unpredictability
    public float engageDistanceRerollInterval = 4f;    // how often it re-picks a target distance within the band

    float strafeDirection = 1f;
    float nextStrafeDirectionChangeTime;
    float engageDistance;
    float nextEngageDistanceRerollTime;

    [Header("Memory / Alertness")]
    public float alertMemoryDuration = 12f;      // how long the enemy stays "on alert" after losing sight of the player
    public float alertedViewRadiusMultiplier = 1.5f; // easier to reacquire while alert, like it's actively listening
    public float hearingRadius = 4f;             // within this range while alert, detects the player regardless of angle/LOS
    const float detectionInterval = 0.15f;       // must match the InvokeRepeating interval below

    float alertedUntil = -999f;
    Vector3 playerVelocity;
    Vector3 lastPlayerPos;

    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    public float patrolWaitTime = 2f;

    [Header("Attacks")]
    public AttackMove[] attacks;

    [Header("Poise / Stagger")]
    public float maxPoise = 50f;
    public float poiseRegenPerSecond = 10f;
    public float staggerDuration = 1.2f;
    public float poiseRegenDelay = 2.5f;       // must go this long without taking poise damage before it starts regenerating
    public float criticalStaggerMultiplier = 1.8f; // longer stagger, riposte-eligible, when poise is overkilled
    public float knockbackDistance = 0.4f;
    public float knockbackDuration = 0.15f;

    float lastPoiseDamageTime = -999f;
    public bool isCriticallyStaggered;

    [Header("Group Behavior")]
    public static int maxConcurrentAttackers = 1; // stops the whole pack from attacking at once
    static readonly HashSet<EnemyAI> attackingEnemies = new HashSet<EnemyAI>();

    [System.Serializable]
    public class AttackMove
    {
        public string name = "Attack";
        public float range = 2f;         // must be within this to use the move
        public float minWindup = 0.4f;   // telegraph time before the hit lands
        public float maxWindup = 1.1f;   // randomized so players can't rhythm-dodge it
        public float recoveryTime = 0.6f;// vulnerable window after the attack resolves
        public float poiseDamage = 20f;  // damage this move deals to the player's poise (if you have one)
        public int damage = 10;
    }

    Transform player;
    NavMeshAgent agent;

    enum State { Patrol, Chase, Windup, Attack, Recovery, Search, Staggered }
    State currentState = State.Patrol;

    Vector3 lastKnownPosition;
    Vector3 spawnPosition;
    float searchTimer;
    int currentPatrolIndex;
    float patrolTimer;
    bool waitingAtPatrolPoint;

    AttackMove currentAttack;
    float actionTimer; // generic countdown used for windup / recovery / stagger
    float currentPoise;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        spawnPosition = transform.position;
        currentPoise = maxPoise;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            lastPlayerPos = player.position;
        }
        else Debug.LogWarning($"{name}: No GameObject tagged 'Player' found in the scene.");

        if (eyePoint == null)
        {
            Debug.LogWarning($"{name}: Eye Point not assigned, using transform instead.");
            eyePoint = transform;
        }

        float startDelay = Random.Range(0f, 0.15f);
        InvokeRepeating(nameof(DetectionCheck), startDelay, 0.15f);

        strafeDirection = Random.value < 0.5f ? 1f : -1f;
        engageDistance = Random.Range(preferredEngageDistanceMin, preferredEngageDistanceMax);
        nextStrafeDirectionChangeTime = Time.time + strafeDirectionChangeInterval * Random.Range(0.7f, 1.3f);
        nextEngageDistanceRerollTime = Time.time + engageDistanceRerollInterval * Random.Range(0.7f, 1.3f);
    }

    void DetectionCheck()
    {
        if (player == null) return;

        // Track player velocity so we can lead our pathing instead of always trailing them
        playerVelocity = (player.position - lastPlayerPos) / detectionInterval;
        lastPlayerPos = player.position;

        // Don't let detection yank us out of an attack we've already committed to
        if (currentState == State.Windup || currentState == State.Attack || currentState == State.Staggered) return;

        bool sees = CanDetectPlayer();
        if (sees)
        {
            lastKnownPosition = player.position;
            alertedUntil = Time.time + alertMemoryDuration; // refresh how long we "remember" the player
        }

        switch (currentState)
        {
            case State.Patrol:
                if (sees) currentState = State.Chase;
                break;

            case State.Chase:
                if (!sees)
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
        // Poise regenerates only after a delay since the last poise-damaging hit, and never mid-stagger
        if (currentState != State.Staggered
            && currentPoise < maxPoise
            && Time.time - lastPoiseDamageTime >= poiseRegenDelay)
        {
            currentPoise = Mathf.Min(maxPoise, currentPoise + poiseRegenPerSecond * Time.deltaTime);
        }

        if (player == null || !agent.isOnNavMesh) return;

        float distToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Patrol:
                agent.updateRotation = true; // let the agent face its own travel direction while roaming
                Patrol();
                break;

            case State.Chase:
                agent.isStopped = false;
                agent.updateRotation = false; // we control facing manually so it always looks at the player, not its strafe path
                FacePlayer();

                // Give up entirely if the player fled far past our leash range
                if (Vector3.Distance(spawnPosition, transform.position) > leashRange)
                {
                    currentState = State.Search;
                    lastKnownPosition = spawnPosition;
                    searchTimer = 0.1f; // return-to-spawn, don't linger
                    break;
                }

                if (distToPlayer > closeDistanceThreshold)
                {
                    // Too far to meaningfully circle - lead the player's position based on their
                    // current velocity, rather than always pathing to where they currently are
                    Vector3 predictedPos = player.position + playerVelocity * predictionTime;
                    agent.SetDestination(predictedPos);
                }
                else
                {
                    // Close enough to engage - orbit at a preferred range instead of beelining,
                    // like it's circling for an opening rather than just walking into you
                    UpdateStrafeTarget();
                    agent.SetDestination(ComputeStrafeDestination());
                }

                AttackMove readyMove = PickAttack(distToPlayer);
                if (readyMove != null && CanStartAttack())
                {
                    BeginWindup(readyMove);
                }
                break;

            case State.Windup:
                agent.isStopped = true; // fully committed, no strafing out of it
                FacePlayer();
                actionTimer -= Time.deltaTime;
                if (actionTimer <= 0f)
                {
                    currentState = State.Attack;
                    ExecuteAttack();
                }
                break;

            case State.Attack:
                // Single-frame resolve; hop straight into recovery
                currentState = State.Recovery;
                actionTimer = currentAttack.recoveryTime;
                break;

            case State.Recovery:
                agent.isStopped = true; // vulnerable window - player gets a punish here
                actionTimer -= Time.deltaTime;
                if (actionTimer <= 0f)
                {
                    ReleaseAttackSlot();
                    currentState = State.Chase;
                }
                break;

            case State.Search:
                agent.isStopped = false;
                agent.updateRotation = true; // back to normal movement-facing while wandering to last known position
                agent.SetDestination(lastKnownPosition);
                searchTimer -= Time.deltaTime;
                bool reachedLastKnown = !agent.pathPending && agent.remainingDistance < 0.5f;
                if (searchTimer <= 0f && reachedLastKnown) currentState = State.Patrol;
                break;

            case State.Staggered:
                agent.isStopped = true;
                actionTimer -= Time.deltaTime;
                if (actionTimer <= 0f)
                {
                    currentPoise = maxPoise;
                    isCriticallyStaggered = false;
                    ReleaseAttackSlot();
                    currentState = State.Chase;
                }
                break;
        }
    }

    // Call this from whatever deals damage to the enemy (player weapon hitbox, etc.)
    // hitOrigin is the world position the hit came from (e.g. the attacker's position or the weapon hit point),
    // used to push the enemy back away from the hit on stagger.
    public void TakeDamage(float damage, float poiseDamage, Vector3 hitOrigin)
    {
        currentPoise -= poiseDamage;
        lastPoiseDamageTime = Time.time;

        if (currentPoise <= 0f && currentState != State.Staggered)
        {
            ReleaseAttackSlot();
            currentAttack = null;
            // TODO: cancel windup animation / VFX here

            // Overkill on poise damage = a harder stagger, opens up a riposte window
            isCriticallyStaggered = currentPoise < -maxPoise * 0.5f;
            actionTimer = staggerDuration * (isCriticallyStaggered ? criticalStaggerMultiplier : 1f);
            currentState = State.Staggered;

            Vector3 dir = transform.position - hitOrigin;
            StartCoroutine(StaggerKnockback(dir));
        }

        // TODO: apply `damage` to health here
    }

    IEnumerator StaggerKnockback(Vector3 dir)
    {
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) yield break;
        dir.Normalize();

        float t = 0f;
        while (t < knockbackDuration)
        {
            // The agent can end up off-mesh mid-knockback (death destroying the object,
            // getting shoved off a ledge, being disabled elsewhere) - bail out instead of
            // calling Move() on an agent that isn't currently placed on the NavMesh.
            if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh)
                yield break;

            agent.Move(dir * (knockbackDistance / knockbackDuration) * Time.deltaTime);
            t += Time.deltaTime;
            yield return null;
        }
    }

    void UpdateStrafeTarget()
    {
        // Occasionally reverse orbit direction so the circling doesn't read as a predictable loop
        if (Time.time >= nextStrafeDirectionChangeTime)
        {
            if (Random.value < 0.5f) strafeDirection *= -1f;
            nextStrafeDirectionChangeTime = Time.time + strafeDirectionChangeInterval * Random.Range(0.7f, 1.3f);
        }

        // Periodically re-pick where in the band it wants to sit, so it drifts in/out a bit
        // rather than locking to one exact radius
        if (Time.time >= nextEngageDistanceRerollTime)
        {
            engageDistance = Random.Range(preferredEngageDistanceMin, preferredEngageDistanceMax);
            nextEngageDistanceRerollTime = Time.time + engageDistanceRerollInterval * Random.Range(0.7f, 1.3f);
        }
    }

    Vector3 ComputeStrafeDestination()
    {
        Vector3 offset = transform.position - player.position;
        offset.y = 0f;
        if (offset.sqrMagnitude < 0.01f) offset = -transform.forward; // fallback if standing on top of the player

        // Rotate the current offset around the player to orbit, snapping distance toward the target engage distance
        float angleStep = strafeDirection * circleStrafeAngularSpeed * Time.deltaTime;
        Vector3 rotatedOffset = Quaternion.Euler(0f, angleStep, 0f) * offset;
        Vector3 desiredOffset = rotatedOffset.normalized * engageDistance;

        return player.position + desiredOffset;
    }

    AttackMove PickAttack(float distance)
    {
        if (attacks == null || attacks.Length == 0) return null;
        // Prefer the shortest-range valid attack so it doesn't always throw its "biggest" move
        AttackMove best = null;
        foreach (var move in attacks)
        {
            if (distance <= move.range && (best == null || move.range < best.range))
                best = move;
        }
        return best;
    }

    bool CanStartAttack()
    {
        // Caps how many enemies in the pack can be mid-attack simultaneously,
        // so groups take turns instead of all piling on at once.
        return attackingEnemies.Count < maxConcurrentAttackers || attackingEnemies.Contains(this);
    }

    void BeginWindup(AttackMove move)
    {
        attackingEnemies.Add(this);
        currentAttack = move;
        actionTimer = Random.Range(move.minWindup, move.maxWindup);
        currentState = State.Windup;
    }

    void ExecuteAttack()
    {
        // TODO: trigger attack animation here, and do the actual hit detection
        // (e.g. OverlapSphere / weapon collider) at the correct animation frame.
        Debug.Log($"{name} used {currentAttack.name} for {currentAttack.damage} damage");
    }

    void ReleaseAttackSlot()
    {
        attackingEnemies.Remove(this);
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
        bool alerted = Time.time < alertedUntil;

        Vector3 dirToPlayer = player.position - eyePoint.position;
        float dist = dirToPlayer.magnitude;

        // While alert (has seen the player recently), it "knows" someone's nearby -
        // close range detection doesn't need a clean sightline, like hearing footsteps.
        if (alerted && dist <= hearingRadius) return true;

        float effectiveRadius = alerted ? viewRadius * alertedViewRadiusMultiplier : viewRadius;
        if (dist > effectiveRadius) return false;

        dirToPlayer.Normalize();
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

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, leashRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, closeDistanceThreshold);
        Gizmos.color = new Color(0f, 1f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, preferredEngageDistanceMin);
        Gizmos.DrawWireSphere(transform.position, preferredEngageDistanceMax);

        bool alertedGizmo = Application.isPlaying && Time.time < alertedUntil;
        Gizmos.color = new Color(1f, 0.5f, 0f); // orange
        Gizmos.DrawWireSphere(transform.position, alertedGizmo ? viewRadius * alertedViewRadiusMultiplier : viewRadius);
        Gizmos.DrawWireSphere(transform.position, hearingRadius);

        if (attacks != null)
        {
            Gizmos.color = Color.magenta;
            foreach (var move in attacks)
                Gizmos.DrawWireSphere(transform.position, move.range);
        }

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