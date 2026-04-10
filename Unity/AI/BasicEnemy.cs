using UnityEngine;

/// <summary>
/// 기본 적 1종.
/// 현재는 EnemyBase 공통 로직을 그대로 사용.
/// </summary>
public class BasicEnemy : EnemyBase
{
    protected override void Awake()
    {
        base.Awake();
        enemyType = EnemyType.Basic;
    }
}
