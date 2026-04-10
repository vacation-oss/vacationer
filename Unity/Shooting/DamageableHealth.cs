using UnityEngine;

/// <summary>
/// 테스트용 체력 컴포넌트.
/// IDamageable을 구현하여 총 데미지를 받는다.
/// </summary>
public class DamageableHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [Tooltip("최대 체력")]
    public float maxHealth = 100f;

    [Tooltip("0 이하일 때 오브젝트를 비활성화할지")]
    public bool disableOnDeath = true;

    private float currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal, GameObject instigator)
    {
        currentHealth -= amount;
        Debug.Log($"[{name}] TakeDamage: {amount}, HP: {currentHealth}");

        if (currentHealth <= 0f)
        {
            if (disableOnDeath)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
