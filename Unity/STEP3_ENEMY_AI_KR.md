# Unity 적 AI 시스템 가이드 (Step 3)

## 1) 설계 요약

- 공통 베이스: `EnemyBase`
  - 상태 머신: `Idle / Chase / Attack / Dead`
  - 플레이어 감지, 추적, 공격, 피격, 사망
- 기본 구현 적 1종: `BasicEnemy`
- 확장 구조 2종 대비:
  - `EnemyType` enum: `Basic`, `Ranged`
  - `RangedEnemy` 스캐폴드(뼈대) 제공
- 웨이브 확장 대비:
  - `EnemyEvents` (스폰/사망/제거 이벤트)
  - `InitializeForSpawn()` (스폰 시 target 주입용)

---

## 2) 성능/확장 고려 포인트

- 거리 계산은 `sqrMagnitude` 사용(루트 연산 최소화)
- 상태 판단은 `thinkInterval` 주기로만 실행
- `nextThinkTime` 초기 랜덤 분산으로 다수 적 동시 스파이크 완화
- 상태 전환 1회성 로직은 `OnStateChanged()`로 분리
- 공격 구현은 `PerformAttack()` 오버라이드 지점으로 분리
- target `IDamageable` 캐시로 반복 탐색 최소화

---

## 3) 오브젝트 세팅

### Player
- Tag를 `Player`로 지정
- `PlayerHealth` 부착 (적 공격 피격 테스트용)

### Enemy 프리팹 예시 (기본형)
- `BasicEnemy` (Capsule 또는 모델)
  - `NavMeshAgent`
  - `BasicEnemy` 스크립트
  - Collider

### Enemy 프리팹 예시 (확장형 준비)
- `RangedEnemy`
  - 현재는 뼈대만 제공
  - 추후 `PerformAttack()` 오버라이드로 원거리 공격 구현

### 환경
- 바닥 NavMesh Bake 필요 (`Window > AI > Navigation`)

---

## 4) Inspector 주요 값

`EnemyBase` 공통:
- Identity
  - `Enemy Type`
- Target
  - `Target`
  - `Target Mask`
- Detection
  - `Detection Range`
  - `Attack Range`
  - `Obstacle Mask`
  - `Think Interval`
- Combat
  - `Max Health`
  - `Attack Damage`
  - `Attack Cooldown`
- Death
  - `Destroy Delay`

---

## 5) 테스트 시나리오

### A. 감지/추적/공격
- [ ] 플레이어가 감지 범위 밖이면 Idle 상태 유지
- [ ] 감지 범위 안 + 가시선 확보 시 Chase 전환
- [ ] 공격 범위 안으로 들어오면 Attack 전환
- [ ] 공격 쿨다운(`Attack Cooldown`)에 맞춰 플레이어 HP가 감소

### B. 피격/사망
- [ ] 플레이어 총으로 적을 맞추면 적 HP가 감소
- [ ] 생존 중 피격 시 Chase로 즉시 반응
- [ ] 적 HP가 0 이하가 되면 Dead 상태로 전환
- [ ] `Destroy Delay` 후 적 오브젝트가 제거

### C. 웨이브 확장 준비
- [ ] `EnemyEvents.OnEnemySpawned` 이벤트 수신 가능
- [ ] `EnemyEvents.OnEnemyDied` 이벤트 수신 가능
- [ ] `EnemyEvents.OnEnemyDespawned` 이벤트 수신 가능
- [ ] 스포너에서 `InitializeForSpawn(playerTransform)`로 target 주입 가능

---

## 6) 나중에 2번째 적 타입 추가하는 방법

1. `RangedEnemy : EnemyBase`에서 `PerformAttack()` 오버라이드
2. 투사체 프리팹/발사점/쿨다운 패턴 추가
3. 필요 시 `EvaluateState()`를 오버라이드해 사거리 기반 행동 분리

