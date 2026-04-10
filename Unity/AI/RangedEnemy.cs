using UnityEngine;

/// <summary>
/// 2종 확장 구조 예시용 적 타입.
/// 현재 프로젝트 목표는 BasicEnemy 1종 구현이므로,
/// RangedEnemy는 추후 원거리 로직 오버라이드용 뼈대만 제공한다.
/// </summary>
public class RangedEnemy : EnemyBase
{
    protected override void Awake()
    {
        base.Awake();
        enemyType = EnemyType.Ranged;
    }

    // 예시) 추후 필요 시 PerformAttack()를 오버라이드하여
    // 투사체 발사/사격 패턴을 구현하면 된다.
}
