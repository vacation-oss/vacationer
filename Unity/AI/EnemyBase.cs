using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 적 공통 베이스 클래스.
/// 목표:
/// - 플레이어 감지 / 추적 / 공격
/// - 피격 / 사망
/// - 간단한 상태 머신
/// - 웨이브 시스템 확장에 유리한 구조
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public abstract class EnemyBase : MonoBehaviour, IDamageable
{
    protected enum EnemyState
    {
        Idle,
        Chase,
        Attack,
        Dead
    }

    [Header("Identity")]
    [SerializeField] protected EnemyType enemyType = EnemyType.Basic;

    [Header("Target")]
    [SerializeField] protected Transform target;
    [SerializeField] protected LayerMask targetMask = ~0;

    [Header("Detection")]
    [SerializeField] protected float detectionRange = 18f;
    [SerializeField] protected float attackRange = 2f;
    [SerializeField] protected LayerMask obstacleMask;
    [SerializeField] protected float thinkInterval = 0.12f;

    [Header("Combat")]
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected float attackDamage = 10f;
    [SerializeField] protected float attackCooldown = 1.2f;

    [Header("Hit Reaction")]
    [Tooltip("피격 시 이동/공격이 잠깐 멈추는 시간")]
    [SerializeField] protected float hitStunDuration = 0.07f;
    [Tooltip("피격 시 몸이 뒤로 밀리는 거리")]
    [SerializeField] protected float hitPushDistance = 0.12f;

    [Header("Death")]
    [SerializeField] protected float destroyDelay = 3f;

    protected NavMeshAgent agent;
    protected EnemyState state;
    protected float currentHealth;
    protected float nextAttackTime;
    protected float nextThinkTime;

    protected float detectionRangeSqr;
    protected float attackRangeSqr;
    protected IDamageable cachedTargetDamageable;

    protected float hitStunTimer;

    public EnemyType Type => enemyType;
    public bool IsDead => state == EnemyState.Dead;
    public float HealthNormalized => maxHealth <= 0f ? 0f : currentHealth / maxHealth;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        detectionRangeSqr = detectionRange * detectionRange;
        attackRangeSqr = attackRange * attackRange;

        currentHealth = maxHealth;
        ResolveTarget();
    }

    protected virtual void OnEnable()
    {
        ResetRuntimeState();
        EnemyEvents.RaiseSpawned(this);
    }

    public virtual void InitializeForSpawn(Transform forcedTarget)
    {
        target = forcedTarget;
        cachedTargetDamageable = target != null ? target.GetComponentInParent<IDamageable>() : null;

        currentHealth = maxHealth;
        ResetRuntimeState();
    }

    protected virtual void Update()
    {
        if (state == EnemyState.Dead || target == null) return;

        if (hitStunTimer > 0f)
        {
            hitStunTimer -= Time.deltaTime;
            return;
        }

        if (Time.time >= nextThinkTime)
        {
            nextThinkTime = Time.time + thinkInterval;
            EvaluateState();
        }

        TickState();
    }

    protected virtual void EvaluateState()
    {
        Vector3 toTarget = target.position - transform.position;
        float distSqr = toTarget.sqrMagnitude;

        bool isTargetInLayer = (targetMask.value & (1 << target.gameObject.layer)) != 0;
        bool canDetect = isTargetInLayer && distSqr <= detectionRangeSqr && HasLineOfSight(target.position);

        if (!canDetect)
        {
            SetState(EnemyState.Idle);
            return;
        }

        SetState(distSqr <= attackRangeSqr ? EnemyState.Attack : EnemyState.Chase);
    }

    protected virtual void TickState()
    {
        switch (state)
        {
            case EnemyState.Idle:
                break;

            case EnemyState.Chase:
                if (agent.enabled)
                {
                    agent.SetDestination(target.position);
                }
                break;

            case EnemyState.Attack:
                FaceTargetOnY();
                TryAttack();
                break;
        }
    }

    protected virtual void SetState(EnemyState newState)
    {
        if (state == newState) return;

        EnemyState previous = state;
        state = newState;
        OnStateChanged(previous, newState);
    }

    protected virtual void OnStateChanged(EnemyState previous, EnemyState next)
    {
        if (!agent.enabled) return;

        switch (next)
        {
            case EnemyState.Idle:
                agent.isStopped = true;
                break;
            case EnemyState.Chase:
                agent.isStopped = false;
                break;
            case EnemyState.Attack:
                agent.isStopped = true;
                break;
            case EnemyState.Dead:
                agent.isStopped = true;
                break;
        }
    }

    protected virtual void TryAttack()
    {
        if (Time.time < nextAttackTime) return;

        nextAttackTime = Time.time + attackCooldown;
        PerformAttack();
    }

    protected virtual void PerformAttack()
    {
        if (cachedTargetDamageable == null) return;

        Vector3 hitPoint = target.position;
        Vector3 hitNormal = (target.position - transform.position).normalized;
        cachedTargetDamageable.TakeDamage(attackDamage, hitPoint, hitNormal, gameObject);
    }

    public virtual void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal, GameObject instigator)
    {
        if (state == EnemyState.Dead) return;

        currentHealth -= amount;

        // 피격 반응: 잠깐 경직 + 가벼운 넉백
        hitStunTimer = hitStunDuration;
        if (agent.enabled)
        {
            agent.ResetPath();
        }
        transform.position += -hitNormal.normalized * hitPushDistance;

        if (currentHealth <= 0f)
        {
            Die();
            return;
        }

        SetState(EnemyState.Chase);
    }

    protected virtual void Die()
    {
        if (state == EnemyState.Dead) return;

        SetState(EnemyState.Dead);

        if (agent.enabled)
        {
            agent.enabled = false;
        }

        EnemyEvents.RaiseDied(this);
        Invoke(nameof(Despawn), destroyDelay);
    }

    protected virtual void Despawn()
    {
        EnemyEvents.RaiseDespawned(this);
        Destroy(gameObject);
    }

    protected virtual bool HasLineOfSight(Vector3 targetPos)
    {
        Vector3 origin = transform.position + Vector3.up * 1.2f;
        Vector3 direction = targetPos - origin;
        float distance = direction.magnitude;

        if (distance <= 0.01f) return true;

        return !Physics.Raycast(origin, direction / distance, distance, obstacleMask, QueryTriggerInteraction.Ignore);
    }

    protected virtual void ResolveTarget()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }

        cachedTargetDamageable = target != null ? target.GetComponentInParent<IDamageable>() : null;
    }

    protected virtual void FaceTargetOnY()
    {
        Vector3 lookPoint = new Vector3(target.position.x, transform.position.y, target.position.z);
        transform.LookAt(lookPoint);
    }

    protected virtual void ResetRuntimeState()
    {
        CancelInvoke();
        state = EnemyState.Idle;
        hitStunTimer = 0f;
        nextAttackTime = 0f;
        nextThinkTime = Time.time + Random.Range(0f, thinkInterval);

        if (agent.enabled)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
