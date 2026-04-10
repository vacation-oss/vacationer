using UnityEngine;

/// <summary>
/// 플레이어 입력과 무기 시스템 연결 담당.
/// 기존 FPSPlayerController와 분리하여 충돌을 줄인다.
/// </summary>
public class PlayerShooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WeaponLoadout loadout;

    private void Update()
    {
        if (loadout == null || loadout.ActiveWeapon == null) return;

        IWeapon activeWeapon = loadout.ActiveWeapon;

        // 자동 연사 무기: 누르고 있는 동안 발사
        // 단발 무기: 클릭한 순간만 발사
        bool triggerPressed = activeWeapon.IsAutomatic
            ? Input.GetButton("Fire1")
            : Input.GetButtonDown("Fire1");

        if (triggerPressed)
        {
            activeWeapon.TryFire();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            activeWeapon.StartReload();
        }
    }
}
