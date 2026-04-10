using System;

/// <summary>
/// 웨이브 시스템 연동을 위한 전역 이벤트 허브.
/// - 스폰
/// - 사망
/// - 제거
/// 웨이브 매니저는 이 이벤트를 구독해 현재 생존 수를 관리하면 된다.
/// </summary>
public static class EnemyEvents
{
    public static event Action<EnemyBase> OnEnemySpawned;
    public static event Action<EnemyBase> OnEnemyDied;
    public static event Action<EnemyBase> OnEnemyDespawned;

    public static void RaiseSpawned(EnemyBase enemy) => OnEnemySpawned?.Invoke(enemy);
    public static void RaiseDied(EnemyBase enemy) => OnEnemyDied?.Invoke(enemy);
    public static void RaiseDespawned(EnemyBase enemy) => OnEnemyDespawned?.Invoke(enemy);
}
