using UnityEngine;

/// <summary>
/// 데미지를 받을 수 있는 대상 인터페이스.
/// 적, 파괴 가능한 오브젝트 등에서 구현.
/// </summary>
public interface IDamageable
{
    void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal, GameObject instigator);
}
