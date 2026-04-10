# Unity C# STEP 1 구현 가이드 (초보자용 FPS 컨트롤러)

아래 내용은 **1단계(플레이어 이동/시점/점프/달리기/바닥판정)**만 다룹니다.

---

## 1) 어떤 오브젝트에 어떤 스크립트를 붙이나요?

### Hierarchy 추천 구조
- `Player` (Capsule 또는 Empty)
  - `CharacterController` 컴포넌트 추가
  - `FPSPlayerController` 스크립트 추가
  - 자식: `Main Camera`
  - 자식: `GroundCheck` (빈 오브젝트)

### 배치 기준
- `Player`는 바닥 위에 위치 (예: y=1)
- `Main Camera`는 머리 높이 (예: local y=1.4)
- `GroundCheck`는 발 근처 (예: local y=-0.9)

---

## 2) Inspector 연결 방법

`Player`를 선택하고 `FPSPlayerController`에서 아래를 연결합니다.

- `Player Camera` → 자식 `Main Camera`
- `Ground Check` → 자식 `GroundCheck`
- `Ground Mask` → `Ground` 레이어 체크

### Ground 레이어 준비
1. 바닥 Plane(또는 지형)을 선택
2. Layer를 `Ground`로 지정 (없으면 새로 생성)
3. 스크립트의 `Ground Mask`에서도 `Ground`를 체크

---

## 3) 필요한 입력 설정 (중요)

이 스크립트는 **기본 Input Manager (`Input.GetAxis`)** 기준입니다.

사용 입력:
- `Horizontal` → A / D
- `Vertical` → W / S
- `Mouse X`, `Mouse Y` → 마우스 시점
- `Jump` → Space
- 달리기 → `Left Shift` (`GetKey`)

### 확인 위치
- `Edit > Project Settings > Input Manager`
- 위 이름들이 기본값으로 존재하는지 확인

### New Input System 프로젝트라면
`Input.GetAxis`가 동작하지 않을 수 있습니다.

- `Edit > Project Settings > Player > Active Input Handling`
- 학습 단계에서는 `Both` 권장

---

## 4) 구현된 기능 설명

- WASD 이동
- 마우스 시점 회전 (좌우: 플레이어, 상하: 카메라)
- 점프 (바닥일 때만)
- 달리기 (Left Shift)
- Sphere 기반 바닥 판정
- 착지 시 y속도 보정(`-2`)으로 떨림 완화
- 카메라 로컬 위치 고정으로 흔들림 최소화

---

## 5) 테스트 방법 체크리스트

아래 순서대로 확인하면 초보자도 쉽게 검증할 수 있습니다.

- [ ] Play 누르면 마우스 커서가 잠기고 숨겨진다.
- [ ] 마우스를 움직이면 시점이 자연스럽게 회전한다.
- [ ] `W/A/S/D`로 이동한다.
- [ ] `Left Shift`를 누르면 이동 속도가 빨라진다.
- [ ] `Space`를 누르면 점프한다.
- [ ] 공중에서 `Space`를 눌러도 연속 점프가 되지 않는다.
- [ ] 바닥에 착지하면 캐릭터가 과하게 튀지 않는다.
- [ ] 경사/바닥 이동 중 카메라가 심하게 흔들리지 않는다.

