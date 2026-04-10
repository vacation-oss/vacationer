# Unity C# 1단계 구현 가이드 (3D FPS 플레이어)

## 1) 오브젝트 구성

### Hierarchy 예시
- `Player` (Capsule 또는 Empty)
  - 컴포넌트: `CharacterController`
  - 컴포넌트: `FPSPlayerController` (이번에 만든 스크립트)
  - 자식: `Main Camera`
  - 자식: `GroundCheck` (빈 GameObject, 발 근처)

### 배치 팁
- `Player`의 위치는 바닥 위(예: y=1)로 두세요.
- `Main Camera`는 플레이어 머리 높이(예: local y=0.8~1.6)에 둡니다.
- `GroundCheck`는 플레이어 발 아래쪽(예: local y=-0.9 근처)에 둡니다.

## 2) 스크립트 연결

1. `Player` 오브젝트에 `FPSPlayerController.cs`를 붙입니다.
2. Inspector에서 아래를 연결합니다.
   - `Player Camera` → 자식 `Main Camera`
   - `Ground Check` → 자식 `GroundCheck`
   - `Ground Mask` → `Ground` 레이어 선택
3. `Player` 오브젝트에 `CharacterController`가 없다면 추가합니다.

## 3) Input 설정 (초보자용)

이 스크립트는 Unity 기본 Input Manager 축 이름을 사용합니다.
- `Horizontal` (A/D)
- `Vertical` (W/S)
- `Mouse X`, `Mouse Y`
- `Jump` (Space)

### 확인 방법 (구 Input Manager)
- `Edit > Project Settings > Input Manager`
- 위 축/버튼 이름이 기본값으로 있는지 확인

### 참고 (신 Input System 사용하는 경우)
- 프로젝트가 New Input System만 쓰도록 되어 있으면 `Input.GetAxis`가 동작하지 않을 수 있습니다.
- 이 경우 `Project Settings > Player > Active Input Handling`을 `Both`로 바꾸면 학습 단계에서 가장 쉽게 동작합니다.

## 4) 바닥 오브젝트 설정

1. Plane(또는 지형) 생성
2. 해당 오브젝트의 Layer를 `Ground`로 지정
3. `FPSPlayerController`의 `Ground Mask`에 `Ground` 레이어 체크

## 5) 카메라 흔들림 최소화 포인트

- `CharacterController` 기반 이동이라 Rigidbody 물리 떨림을 줄이기 쉽습니다.
- 착지 시 `verticalVelocity = -2`로 고정해 미세 튐을 줄였습니다.
- 카메라는 플레이어 자식으로 두고, 로컬 회전만 제어합니다.

## 6) 1단계 테스트 체크리스트

- [ ] Play 버튼을 누르면 마우스 커서가 잠긴다.
- [ ] 마우스 이동으로 시점이 자연스럽게 회전한다.
- [ ] `W/A/S/D`로 앞/좌/뒤/우 이동이 된다.
- [ ] `Left Shift`를 누르면 이동 속도가 빨라진다(달리기).
- [ ] `Space`를 누르면 점프한다.
- [ ] 공중에서는 연속 점프가 되지 않는다.
- [ ] 바닥에 착지하면 자연스럽게 멈춘다.
- [ ] 경사/바닥에서 카메라가 심하게 떨리지 않는다.

