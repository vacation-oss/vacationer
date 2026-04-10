# Unity 총 시스템 통합 가이드 (기존 FPSPlayerController와 충돌 없이)

이 문서는 **기존 플레이어 이동 코드(FPSPlayerController)** 위에 총 시스템을 추가하는 방법을 설명합니다.

---

## 1) 설계 목표와 구조

### 목표
- 무기 2종을 고려한 확장 가능한 구조
- 우선 기본 소총 1종 구현
- 마우스 클릭/홀드 발사
- 레이캐스트 히트 판정
- 데미지 인터페이스 기반 적용
- 탄약/재장전
- 간단한 반동
- 총구와 카메라 시점 정렬

### 구조 요약
- `WeaponLoadout`: 최대 2개 슬롯 관리 (`weaponSlots[0]`, `weaponSlots[1]`)
- `WeaponBase`: 공통 로직 (발사/재장전/탄약/레이캐스트)
- `RifleWeapon`: 기본 소총 1종
- `PlayerShooter`: 입력 처리 (Fire1, R)
- `IDamageable`: 데미지 대상 인터페이스
- `DamageableHealth`: 테스트용 체력 컴포넌트

> 핵심 통합 포인트: 반동을 무기에서 카메라에 직접 쓰지 않고,
> `ILookRecoilReceiver`를 통해 `FPSPlayerController`에 전달하여 충돌을 줄였습니다.

---

## 2) 오브젝트/스크립트 부착 위치

### Player 오브젝트
- 기존: `CharacterController`, `FPSPlayerController`
- 추가: `PlayerShooter`, `WeaponLoadout`

### WeaponRoot (Player 자식 빈 오브젝트 권장)
- 자식으로 `Rifle` 오브젝트 배치

### Rifle 오브젝트
- `RifleWeapon` 부착
- 자식으로 `Muzzle` 빈 오브젝트 생성 (총구 위치)

### Target 오브젝트(적/박스)
- Collider 추가
- `DamageableHealth` 부착

---

## 3) Inspector 연결 순서

1. `PlayerShooter.loadout` → Player의 `WeaponLoadout`
2. `WeaponLoadout.weaponSlots[0]` → `RifleWeapon`
3. `WeaponLoadout.weaponSlots[1]` → 비워둬도 됨(2종 확장 구조 유지)
4. `RifleWeapon` 연결
   - `Muzzle` = Rifle 자식 총구 Transform
   - `Aim Camera` = Player의 Main Camera
   - `Recoil Receiver Behaviour` = Player의 `FPSPlayerController`
   - `Hit Mask` = 명중 가능한 레이어

---

## 4) 입력 설정

기본 Input Manager 기준:
- `Fire1` = 마우스 왼쪽 버튼
- `R` = 재장전
- `1`, `2` = 슬롯 전환

### 발사 방식
- `Is Automatic = true`: 클릭 유지 시 연사
- `Is Automatic = false`: 클릭 1회당 1발

---

## 5) 구현된 핵심 동작

- **레이캐스트 히트 판정**
  1) 카메라 중앙에서 조준점 계산
  2) 총구에서 조준점을 향해 실제 발사
  3) 총구/카메라 정렬 오차를 줄여 자연스러운 탄착

- **데미지 적용 인터페이스**
  - 명중한 콜라이더의 부모에서 `IDamageable` 탐색
  - `TakeDamage(amount, hitPoint, hitNormal, instigator)` 호출

- **탄약/재장전**
  - 탄창(`ammoInMagazine`) + 예비탄(`reserveAmmo`)
  - 탄창이 0이면 자동 재장전 시도
  - `R`로 수동 재장전

- **반동(충돌 최소화)**
  - 무기가 직접 카메라 회전을 건드리지 않음
  - `FPSPlayerController.AddPitchRecoil`로 반동을 전달
  - 플레이어 카메라 회전 로직 내부에서 부드럽게 복귀 처리

---

## 6) Inspector에서 조정 가능한 값 정리

### RifleWeapon(WeaponBase)
- Common
  - `Weapon Name`
  - `Muzzle`
  - `Aim Camera`
  - `Recoil Receiver Behaviour`
  - `Range`
  - `Hit Mask`
- Damage
  - `Damage`
- Ammo
  - `Magazine Size`
  - `Reserve Ammo`
- Fire / Reload
  - `Fire Rate`
  - `Is Automatic`
  - `Reload Duration`
- Recoil
  - `Recoil Kick`

### FPSPlayerController
- Mouse Look
  - `Mouse Sensitivity`
  - `Max Look Angle`
- Recoil
  - `Recoil Recover Speed`

---

## 7) 테스트 시나리오 체크리스트

### A. 기존 이동 충돌 확인
- [ ] 기존 WASD/점프/달리기가 이전과 동일하게 동작한다.
- [ ] 총 시스템 추가 후 카메라 떨림이 심해지지 않는다.

### B. 발사/명중/데미지
- [ ] `Fire1` 입력 시 소총이 발사된다.
- [ ] `Is Automatic=true`에서 클릭 유지 시 연사된다.
- [ ] `Is Automatic=false`에서 클릭 1회당 1발만 나간다.
- [ ] 조준점 근처 타겟을 쐈을 때 명중한다.
- [ ] `DamageableHealth`가 적용된 타겟의 HP 로그가 감소한다.

### C. 탄약/재장전
- [ ] 발사 시 탄창 수가 감소한다.
- [ ] 탄창이 0이면 발사가 멈추고 재장전이 동작한다.
- [ ] `R` 키로 수동 재장전이 동작한다.
- [ ] 예비탄이 0이면 재장전이 되지 않는다.

### D. 슬롯 확장성
- [ ] 키 `1`로 슬롯1 무기 전환이 가능하다.
- [ ] 슬롯2가 비어 있으면 키 `2` 입력 시 전환되지 않는다.
- [ ] 슬롯2에 다른 무기를 추가하면 같은 인터페이스로 동작하도록 확장 가능하다.

## 8) 품질(타격감/HUD) 개선 포인트

- `WeaponBase`
  - `Spread Angle`: 너무 정확한 레이저 느낌 완화
  - `Recoil Kick`, `Recoil Randomness`: 반복 발사 시 자연스러운 반동
  - `Muzzle Flash`, `Fire Clip`, `Reload Clip`: 시청각 발사감 강화
- `FPSPlayerController`
  - `Recoil Recover Speed`, `Recoil Kick Multiplier`: 부드러운 반동 복귀 튜닝
- `SimpleFPSHUD` (Player에 추가)
  - 크로스헤어 / 탄약 / 히트마커 / 피격 오버레이 표시

