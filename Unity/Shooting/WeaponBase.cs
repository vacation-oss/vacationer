using System;
using UnityEngine;

/// <summary>
/// 히트스캔 기반 무기 기본 클래스.
/// - 발사/재장전/탄약
/// - 카메라 중심 조준 + 총구 방향 보정
/// - 반동은 ILookRecoilReceiver로 전달
/// </summary>
public abstract class WeaponBase : MonoBehaviour, IWeapon
{
    [Header("Common")]
    [SerializeField] protected string weaponName = "Weapon";
    [SerializeField] protected Transform muzzle;
    [SerializeField] protected Camera aimCamera;
    [SerializeField] protected MonoBehaviour recoilReceiverBehaviour;
    [SerializeField] protected LayerMask hitMask = ~0;
    [SerializeField] protected float range = 120f;

    [Header("Damage")]
    [SerializeField] protected float damage = 25f;

    [Header("Ammo")]
    [SerializeField] protected int magazineSize = 30;
    [SerializeField] protected int reserveAmmo = 90;

    [Header("Fire / Reload")]
    [SerializeField] protected float fireRate = 10f;
    [SerializeField] protected bool isAutomatic = true;
    [SerializeField] protected float reloadDuration = 1.8f;

    [Header("Gun Feel")]
    [Tooltip("발사 시 탄퍼짐 각도(도). 0이면 직진")]
    [SerializeField] protected float spreadAngle = 0.35f;
    [Tooltip("기본 반동 킥")]
    [SerializeField] protected float recoilKick = 1.3f;
    [Tooltip("반동 랜덤 가중치")]
    [SerializeField] protected float recoilRandomness = 0.2f;
    [SerializeField] protected ParticleSystem muzzleFlash;
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected AudioClip fireClip;
    [SerializeField] protected AudioClip reloadClip;

    protected int ammoInMagazine;
    protected bool isReloading;
    protected float reloadTimer;
    protected float nextFireTime;

    protected ILookRecoilReceiver recoilReceiver;

    // HUD/피드백 시스템 연동: true면 IDamageable을 명중함
    public event Action<bool> OnShotFeedback;

    public string WeaponName => weaponName;
    public int CurrentAmmoInMagazine => ammoInMagazine;
    public int CurrentReserveAmmo => reserveAmmo;
    public int MagazineSize => magazineSize;
    public bool IsAutomatic => isAutomatic;

    protected virtual void Awake()
    {
        ammoInMagazine = magazineSize;

        if (recoilReceiverBehaviour != null)
        {
            recoilReceiver = recoilReceiverBehaviour as ILookRecoilReceiver;
        }
        if (recoilReceiver == null)
        {
            recoilReceiver = GetComponentInParent<ILookRecoilReceiver>();
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public virtual void Tick(float deltaTime)
    {
        if (!isReloading) return;

        reloadTimer -= deltaTime;
        if (reloadTimer <= 0f)
        {
            CompleteReload();
        }
    }

    public virtual bool TryFire()
    {
        if (isReloading || Time.time < nextFireTime) return false;

        if (ammoInMagazine <= 0)
        {
            StartReload();
            return false;
        }

        nextFireTime = Time.time + 1f / fireRate;
        ammoInMagazine--;

        bool hitDamageable = FireRaycast();
        PlayFireFeedback();

        float recoilScale = 1f + UnityEngine.Random.Range(-recoilRandomness, recoilRandomness);
        recoilReceiver?.AddPitchRecoil(recoilKick * recoilScale);

        OnShotFeedback?.Invoke(hitDamageable);
        return true;
    }

    public virtual void StartReload()
    {
        if (isReloading || ammoInMagazine >= magazineSize || reserveAmmo <= 0) return;

        isReloading = true;
        reloadTimer = reloadDuration;

        if (audioSource != null && reloadClip != null)
        {
            audioSource.PlayOneShot(reloadClip);
        }
    }

    protected virtual void CompleteReload()
    {
        isReloading = false;

        int needed = magazineSize - ammoInMagazine;
        int loaded = Mathf.Min(needed, reserveAmmo);
        ammoInMagazine += loaded;
        reserveAmmo -= loaded;
    }

    protected virtual bool FireRaycast()
    {
        if (muzzle == null || aimCamera == null)
        {
            Debug.LogWarning($"[{weaponName}] muzzle/aimCamera 할당 필요");
            return false;
        }

        Ray camRay = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 spreadDir = ApplySpread(camRay.direction, spreadAngle);

        Vector3 targetPoint;
        if (Physics.Raycast(camRay.origin, spreadDir, out RaycastHit camHit, range, hitMask, QueryTriggerInteraction.Ignore))
        {
            targetPoint = camHit.point;
        }
        else
        {
            targetPoint = camRay.origin + spreadDir * range;
        }

        Vector3 fireDir = (targetPoint - muzzle.position).normalized;

        if (Physics.Raycast(muzzle.position, fireDir, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore))
        {
            var damageable = hit.collider.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage, hit.point, hit.normal, gameObject);
                return true;
            }
        }

        return false;
    }

    protected virtual void PlayFireFeedback()
    {
        if (muzzleFlash != null)
        {
            muzzleFlash.Play(true);
        }

        if (audioSource != null && fireClip != null)
        {
            audioSource.PlayOneShot(fireClip);
        }
    }

    private Vector3 ApplySpread(Vector3 forward, float angle)
    {
        if (angle <= 0.001f) return forward;

        Vector2 random = UnityEngine.Random.insideUnitCircle * Mathf.Tan(angle * Mathf.Deg2Rad);
        Vector3 right = aimCamera.transform.right;
        Vector3 up = aimCamera.transform.up;
        return (forward + right * random.x + up * random.y).normalized;
    }
}
