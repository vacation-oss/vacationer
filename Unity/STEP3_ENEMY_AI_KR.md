# Unity 적 AI 시스템 가이드 (Step 3)

## 1) 구조 요약

- 공통 베이스: `EnemyBase`
  - 상태 머신: `Idle / Chase / Attack / Dead`
  - 플레이어 감지, 추적, 공격, 피격, 사망
- 구현 적 1종: `BasicEnemy`
- 2종 이상 확장 대비: `EnemyType` enum(`Basic`, `Ranged`)
- 웨이브 시스템 연동 대비: `EnemyEvents` 이벤트 허브

## 2) 성능/확장 고려 포인트

- 거리 계산은 `sqrMagnitude` 사용(루트 연산 최소화)
- 감지/시야 판단은 `thinkInterval` 주기로만 계산
- 공통 로직을 `EnemyBase`로 집중해 파생 타입 구현 비용 감소
- 사망 이벤트(`OnEnemyDied`)로 웨이브 매니저 연결 가능

## 3) 오브젝트 세팅

### Player
- Tag를 `Player`로 지정
- 피격 테스트를 위해 `DamageableHealth`를 Player에 붙여도 됨

### Enemy 프리팹 예시
- `BasicEnemy` (Capsule 또는 모델)
  - 컴포넌트: `NavMeshAgent`
  - 컴포넌트: `BasicEnemy` 스크립트
  - Collider 포함

### 환경
- 바닥에 NavMesh Bake 필요 (`Window > AI > Navigation`)

## 4) Inspector 주요 값

`BasicEnemy(=EnemyBase)`
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

## 5) 테스트 시나리오 체크리스트

- [ ] 플레이어가 감지 범위 밖이면 적이 멈춘다(Idle).
- [ ] 플레이어가 감지 범위 내로 들어오면 추적한다(Chase).
- [ ] 공격 범위에 들어오면 공격한다(Attack).
- [ ] 총으로 맞추면 체력이 감소하고 반응한다(피격).
- [ ] 체력이 0 이하가 되면 사망 후 제거된다(Dead -> Despawn).
- [ ] `EnemyEvents.OnEnemyDied` 구독 시 사망 이벤트를 받을 수 있다.
- [ ] 여러 적을 배치해도 `thinkInterval` 조정으로 과부하를 완화할 수 있다.

