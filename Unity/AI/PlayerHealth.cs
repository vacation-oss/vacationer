using UnityEngine;

/// <summary>
/// 플레이어 피격 테스트용 체력 컴포넌트.
/// EnemyBase 공격 타겟으로 IDamageable을 제공한다.
/// </summary>
public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private bool logOnDeath = true;

    [Header("Hit Feedback")]
    [Tooltip("피격 UI 플래시 지속시간")]
    [SerializeField] private float hitFlashDuration = 0.18f;

    private float currentHealth;
    private float hitFlashTimer;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float HealthNormalized => maxHealth <= 0f ? 0f : currentHealth / maxHealth;
    public float HitFlash01 => hitFlashDuration <= 0f ? 0f : Mathf.Clamp01(hitFlashTimer / hitFlashDuration);

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (hitFlashTimer > 0f)
        {
            hitFlashTimer -= Time.deltaTime;
        }
    }

    public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal, GameObject instigator)
    {
        currentHealth -= amount;
        hitFlashTimer = hitFlashDuration;

        Debug.Log($"[PlayerHealth] Hit by {instigator.name}, damage={amount}, hp={currentHealth}");

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            if (logOnDeath)
            {
                Debug.Log("[PlayerHealth] Player Dead (테스트용 처리)");
            }
        }
    }
}
