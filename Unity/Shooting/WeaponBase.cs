using UnityEngine;

/// <summary>
/// 히트스캔 기반 무기 기본 클래스.
/// - 발사/재장전/탄약
/// - 카메라 중심 조준 + 총구 방향 보정
/// - 간단한 반동(카메라 킥)
/// </summary>
public abstract class WeaponBase : MonoBehaviour, IWeapon
{
    [Header("Common")]
    [SerializeField] protected string weaponName = "Weapon";

    [Tooltip("총구 위치")]
    [SerializeField] protected Transform muzzle;

    [Tooltip("조준 기준 카메라")]
    [SerializeField] protected Camera aimCamera;

    [Tooltip("사거리")]
    [SerializeField] protected float range = 120f;

    [Tooltip("공격 레이어")]
    [SerializeField] protected LayerMask hitMask = ~0;

    [Header("Damage")]
    [SerializeField] protected float damage = 25f;

    [Header("Ammo")]
    [SerializeField] protected int magazineSize = 30;
    [SerializeField] protected int reserveAmmo = 90;

    [Header("Fire / Reload")]
    [Tooltip("초당 발사 수")]
    [SerializeField] protected float fireRate = 10f;
    [SerializeField] protected float reloadDuration = 1.8f;

    [Header("Recoil")]
    [Tooltip("발사 시 카메라가 위로 들리는 각도")]
    [SerializeField] protected float recoilKick = 1.3f;

    [Tooltip("카메라 반동 복귀 속도")]
    [SerializeField] protected float recoilRecoverSpeed = 10f;

    protected int ammoInMagazine;
    protected bool isReloading;
    protected float reloadTimer;
    protected float nextFireTime;

    // 반동 누적(카메라 local X축)
    protected float recoilOffsetX;

    public string WeaponName => weaponName;
    public int CurrentAmmoInMagazine => ammoInMagazine;
    public int CurrentReserveAmmo => reserveAmmo;
    public int MagazineSize => magazineSize;

    protected virtual void Awake()
    {
        ammoInMagazine = magazineSize;
    }

    public virtual void Tick(float deltaTime)
    {
        // 반동 복귀
        recoilOffsetX = Mathf.Lerp(recoilOffsetX, 0f, recoilRecoverSpeed * deltaTime);

        if (aimCamera != null)
        {
            Vector3 baseEuler = aimCamera.transform.localEulerAngles;
            float x = NormalizeAngle(baseEuler.x);
            aimCamera.transform.localRotation = Quaternion.Euler(x - recoilOffsetX, 0f, 0f);
        }

        if (!isReloading) return;

        reloadTimer -= deltaTime;
        if (reloadTimer <= 0f)
        {
            CompleteReload();
        }
    }

    public virtual bool TryFire()
    {
        if (isReloading) return false;
        if (Time.time < nextFireTime) return false;

        if (ammoInMagazine <= 0)
        {
            StartReload();
            return false;
        }

        nextFireTime = Time.time + 1f / fireRate;
        ammoInMagazine--;

        FireRaycast();

        // 단순 반동 누적
        recoilOffsetX += recoilKick;

        return true;
    }

    public virtual void StartReload()
    {
        if (isReloading) return;
        if (ammoInMagazine >= magazineSize) return;
        if (reserveAmmo <= 0) return;

        isReloading = true;
        reloadTimer = reloadDuration;
    }

    protected virtual void CompleteReload()
    {
        isReloading = false;

        int needed = magazineSize - ammoInMagazine;
        int loaded = Mathf.Min(needed, reserveAmmo);
        ammoInMagazine += loaded;
        reserveAmmo -= loaded;
    }

    protected virtual void FireRaycast()
    {
        if (muzzle == null || aimCamera == null)
        {
            Debug.LogWarning($"[{weaponName}] muzzle/aimCamera 할당 필요");
            return;
        }

        // 1) 카메라 중앙에서 먼저 조준점 획득
        Ray camRay = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        Vector3 targetPoint;
        if (Physics.Raycast(camRay, out RaycastHit camHit, range, hitMask, QueryTriggerInteraction.Ignore))
        {
            targetPoint = camHit.point;
        }
        else
        {
            targetPoint = camRay.origin + camRay.direction * range;
        }

        // 2) 실제 발사는 총구에서 targetPoint 방향으로
        Vector3 fireDir = (targetPoint - muzzle.position).normalized;
        Ray muzzleRay = new Ray(muzzle.position, fireDir);

        if (Physics.Raycast(muzzleRay, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore))
        {
            var damageable = hit.collider.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage, hit.point, hit.normal, gameObject);
            }
        }
    }

    protected float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }
}
