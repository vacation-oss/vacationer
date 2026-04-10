using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 적 공통 베이스 클래스.
/// 목표:
/// - 플레이어 감지
/// - 추적
/// - 공격
/// - 피격
/// - 사망
/// - 상태 머신 기반 동작
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
    [Tooltip("플레이어 Transform. 비워두면 Tag=Player를 자동 탐색")]
    [SerializeField] protected Transform target;

    [Header("Detection")]
    [Tooltip("플레이어를 감지하는 최대 거리")]
    [SerializeField] protected float detectionRange = 18f;

    [Tooltip("공격 시작 거리")]
    [SerializeField] protected float attackRange = 2f;

    [Tooltip("시야 체크용 레이어(벽 가림 검사)")]
    [SerializeField] protected LayerMask obstacleMask;

    [Tooltip("시야/거리 체크 주기(성능 최적화)")]
    [SerializeField] protected float thinkInterval = 0.1f;

    [Header("Combat")]
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected float attackDamage = 10f;
    [SerializeField] protected float attackCooldown = 1.2f;

    [Header("Death")]
    [Tooltip("사망 후 삭제까지 대기 시간")]
    [SerializeField] protected float destroyDelay = 3f;

    protected NavMeshAgent agent;
    protected EnemyState state;
    protected float currentHealth;
    protected float nextAttackTime;
    protected float nextThinkTime;

    // 거리 계산 최적화용
    protected float detectionRangeSqr;
    protected float attackRangeSqr;

    public EnemyType Type => enemyType;
    public bool IsDead => state == EnemyState.Dead;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        currentHealth = maxHealth;

        detectionRangeSqr = detectionRange * detectionRange;
        attackRangeSqr = attackRange * attackRange;

        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
        }
    }

    protected virtual void OnEnable()
    {
        state = EnemyState.Idle;
        EnemyEvents.RaiseSpawned(this);
    }

    protected virtual void Update()
    {
        if (state == EnemyState.Dead || target == null) return;

        // 매 프레임 전체 계산을 하지 않고 주기적으로 판단
        if (Time.time >= nextThinkTime)
        {
            nextThinkTime = Time.time + thinkInterval;
            EvaluateState();
        }

        TickState();
    }

    /// <summary>
    /// 상태 전이 판단.
    /// </summary>
    protected virtual void EvaluateState()
    {
        Vector3 toTarget = target.position - transform.position;
        float distSqr = toTarget.sqrMagnitude;

        bool canDetect = distSqr <= detectionRangeSqr && HasLineOfSight(target.position);

        if (!canDetect)
        {
            SetState(EnemyState.Idle);
            return;
        }

        if (distSqr <= attackRangeSqr)
        {
            SetState(EnemyState.Attack);
        }
        else
        {
            SetState(EnemyState.Chase);
        }
    }

    /// <summary>
    /// 상태별 동작 처리.
    /// </summary>
    protected virtual void TickState()
    {
        switch (state)
        {
            case EnemyState.Idle:
                agent.isStopped = true;
                break;

            case EnemyState.Chase:
                agent.isStopped = false;
                agent.SetDestination(target.position);
                break;

            case EnemyState.Attack:
                agent.isStopped = true;
                transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
                TryAttack();
                break;
        }
    }

    protected virtual void SetState(EnemyState newState)
    {
        if (state == newState) return;
        state = newState;
    }

    protected virtual void TryAttack()
    {
        if (Time.time < nextAttackTime) return;

        nextAttackTime = Time.time + attackCooldown;

        IDamageable damageable = target.GetComponentInParent<IDamageable>();
        if (damageable != null)
        {
            Vector3 hitPoint = target.position;
            Vector3 hitNormal = (target.position - transform.position).normalized;
            damageable.TakeDamage(attackDamage, hitPoint, hitNormal, gameObject);
        }
    }

    public virtual void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal, GameObject instigator)
    {
        if (state == EnemyState.Dead) return;

        currentHealth -= amount;
        if (currentHealth <= 0f)
        {
            Die();
        }
        else
        {
            // 피격 시 잠깐 추적 상태로 전환해 즉각 반응
            SetState(EnemyState.Chase);
        }
    }

    protected virtual void Die()
    {
        if (state == EnemyState.Dead) return;

        state = EnemyState.Dead;
        agent.isStopped = true;
        agent.enabled = false;

        EnemyEvents.RaiseDied(this);

        // 웨이브 시스템이 이벤트 처리 후 정리할 수 있도록 약간의 유예를 둔다.
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
        Vector3 dir = targetPos - origin;
        float distance = dir.magnitude;

        if (distance <= 0.01f) return true;

        return !Physics.Raycast(origin, dir.normalized, distance, obstacleMask, QueryTriggerInteraction.Ignore);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
