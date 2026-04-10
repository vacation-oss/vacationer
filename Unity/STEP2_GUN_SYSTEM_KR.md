# Unity 총 시스템 통합 가이드 (기존 FPSPlayerController와 함께 사용)

## 1) 설계 요약

- `WeaponLoadout`이 **최대 2종 슬롯**(`weaponSlots[0..1]`)을 관리합니다.
- 각 무기는 `WeaponBase`를 상속받아 구현합니다.
  - 현재 구현: `RifleWeapon` 1종
  - 확장 예정: `PistolWeapon`, `ShotgunWeapon` 등
- 데미지는 `IDamageable` 인터페이스를 통해 적용합니다.
- `PlayerShooter`가 입력(발사/재장전)만 담당합니다.

> 핵심: 이동(`FPSPlayerController`)과 사격(`PlayerShooter`)을 분리하여 충돌을 줄였습니다.

## 2) 스크립트 부착 위치

### Player 오브젝트
- 기존: `FPSPlayerController`, `CharacterController`
- 추가: `PlayerShooter`, `WeaponLoadout`

### WeaponRoot (Player 자식 빈 오브젝트 권장)
- 자식으로 무기 모델 배치 (예: `Rifle`)

### Rifle 오브젝트
- `RifleWeapon` 부착
- 자식으로 `Muzzle`(총구) 빈 오브젝트 생성

### 타겟(적/박스)
- `DamageableHealth` 부착 (테스트용)
- Collider 필요

## 3) Inspector 연결

1. `PlayerShooter.loadout`에 Player의 `WeaponLoadout` 연결
2. `WeaponLoadout.weaponSlots[0]`에 `RifleWeapon` 연결
3. `WeaponLoadout.weaponSlots[1]`는 비워둬도 됨(2종 구조 확보)
4. `RifleWeapon`에서 연결
   - `Muzzle` = Rifle의 총구 Transform
   - `Aim Camera` = Player의 Main Camera
   - `Hit Mask` = 맞출 레이어

## 4) 입력 설정

기본 Unity Input Manager 기준:
- `Fire1`: 마우스 왼쪽 버튼
- `R`: 재장전
- `1`, `2`: 슬롯 전환

## 5) 기능 체크 포인트

- 발사: 마우스 클릭 시 연사(`fireRate`) 반영
- 히트 판정: Raycast
  - 카메라 중앙 조준점을 구한 뒤
  - 총구에서 그 지점으로 쏴서 정렬 오차 최소화
- 데미지: 맞은 콜라이더의 부모에서 `IDamageable` 탐색 후 `TakeDamage` 호출
- 탄약: 탄창/예비탄 분리 관리
- 재장전: `reloadDuration` 경과 후 탄 이동
- 반동: 카메라 X축 위로 킥 + 시간에 따라 복귀

## 6) Inspector에서 조정 가능한 주요 값

`RifleWeapon(=WeaponBase)`
- Common: `Range`, `Hit Mask`
- Damage: `Damage`
- Ammo: `Magazine Size`, `Reserve Ammo`
- Fire/Reload: `Fire Rate`, `Reload Duration`
- Recoil: `Recoil Kick`, `Recoil Recover Speed`

## 7) 테스트 시나리오 체크리스트

- [ ] 이동/점프/달리기가 기존과 동일하게 동작한다.
- [ ] 마우스 좌클릭 시 소총이 발사된다.
- [ ] 연사 속도가 `Fire Rate`와 일치한다.
- [ ] 탄창 탄약이 0이 되면 발사가 멈추고 재장전으로 회복된다.
- [ ] `R` 키로 수동 재장전이 된다.
- [ ] 오브젝트에 `DamageableHealth`를 붙이면 데미지가 적용된다.
- [ ] 카메라 조준점과 실제 명중점이 크게 어긋나지 않는다.
- [ ] 슬롯 1(키 `1`)은 정상, 슬롯 2(키 `2`)는 비어있으면 전환되지 않는다.

