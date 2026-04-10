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

        // 발사(기본: 마우스 왼쪽 클릭)
        if (Input.GetButton("Fire1"))
        {
            loadout.ActiveWeapon.TryFire();
        }

        // 재장전(기본: R)
        if (Input.GetKeyDown(KeyCode.R))
        {
            loadout.ActiveWeapon.StartReload();
        }
    }
}
