using UnityEngine;

/// <summary>
/// 경량 HUD(시인성 개선용).
/// - 중앙 크로스헤어
/// - 탄약 정보
/// - 피격 시 화면 가장자리 플래시
/// - 적 명중 시 히트마커
/// </summary>
public class SimpleFPSHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WeaponLoadout loadout;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Crosshair")]
    [SerializeField] private Color crosshairColor = Color.white;
    [SerializeField] private float crosshairSize = 10f;
    [SerializeField] private float crosshairThickness = 2f;

    [Header("Ammo HUD")]
    [SerializeField] private Color ammoTextColor = Color.white;
    [SerializeField] private int ammoFontSize = 22;

    [Header("Hit Marker")]
    [SerializeField] private Color hitMarkerColor = Color.white;
    [SerializeField] private float hitMarkerDuration = 0.08f;
    [SerializeField] private float hitMarkerSize = 9f;

    [Header("Damage Overlay")]
    [SerializeField] private Color damageOverlayColor = new Color(1f, 0f, 0f, 0.25f);

    private Texture2D pixel;
    private float hitMarkerTimer;
    private WeaponBase subscribedWeapon;

    private void Awake()
    {
        pixel = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        pixel.SetPixel(0, 0, Color.white);
        pixel.Apply();
    }

    private void Update()
    {
        HookWeaponEvent();

        if (hitMarkerTimer > 0f)
        {
            hitMarkerTimer -= Time.deltaTime;
        }
    }

    private void OnDestroy()
    {
        if (subscribedWeapon != null)
        {
            subscribedWeapon.OnShotFeedback -= OnShotFeedback;
        }

        if (pixel != null)
        {
            Destroy(pixel);
        }
    }

    private void HookWeaponEvent()
    {
        if (loadout == null) return;

        WeaponBase active = loadout.ActiveWeapon;
        if (active == subscribedWeapon) return;

        if (subscribedWeapon != null)
        {
            subscribedWeapon.OnShotFeedback -= OnShotFeedback;
        }

        subscribedWeapon = active;

        if (subscribedWeapon != null)
        {
            subscribedWeapon.OnShotFeedback += OnShotFeedback;
        }
    }

    private void OnShotFeedback(bool hitDamageable)
    {
        if (hitDamageable)
        {
            hitMarkerTimer = hitMarkerDuration;
        }
    }

    private void OnGUI()
    {
        DrawCrosshair();
        DrawAmmoHud();
        DrawHitMarker();
        DrawDamageOverlay();
    }

    private void DrawCrosshair()
    {
        float cx = Screen.width * 0.5f;
        float cy = Screen.height * 0.5f;
        GUI.color = crosshairColor;

        DrawRect(cx - crosshairSize, cy - crosshairThickness * 0.5f, crosshairSize * 2f, crosshairThickness);
        DrawRect(cx - crosshairThickness * 0.5f, cy - crosshairSize, crosshairThickness, crosshairSize * 2f);
    }

    private void DrawAmmoHud()
    {
        if (loadout == null || loadout.ActiveWeapon == null) return;

        IWeapon weapon = loadout.ActiveWeapon;
        string ammoText = $"{weapon.CurrentAmmoInMagazine} / {weapon.CurrentReserveAmmo}";

        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            fontSize = ammoFontSize,
            fontStyle = FontStyle.Bold,
            normal = { textColor = ammoTextColor },
            alignment = TextAnchor.LowerRight
        };

        Rect rect = new Rect(Screen.width - 220f, Screen.height - 60f, 200f, 40f);
        GUI.Label(rect, ammoText, style);
    }

    private void DrawHitMarker()
    {
        if (hitMarkerTimer <= 0f) return;

        float cx = Screen.width * 0.5f;
        float cy = Screen.height * 0.5f;
        float a = Mathf.Clamp01(hitMarkerTimer / hitMarkerDuration);

        GUI.color = new Color(hitMarkerColor.r, hitMarkerColor.g, hitMarkerColor.b, a);

        // X 모양 히트마커를 직선 4개로 근사
        DrawRect(cx - hitMarkerSize - 1f, cy - hitMarkerSize - 1f, hitMarkerSize, 2f);
        DrawRect(cx + 1f, cy - hitMarkerSize - 1f, hitMarkerSize, 2f);
        DrawRect(cx - hitMarkerSize - 1f, cy + hitMarkerSize - 1f, hitMarkerSize, 2f);
        DrawRect(cx + 1f, cy + hitMarkerSize - 1f, hitMarkerSize, 2f);
    }

    private void DrawDamageOverlay()
    {
        if (playerHealth == null) return;

        float flash = playerHealth.HitFlash01;
        if (flash <= 0f) return;

        Color overlay = damageOverlayColor;
        overlay.a *= flash;

        GUI.color = overlay;
        DrawRect(0f, 0f, Screen.width, Screen.height);
    }

    private void DrawRect(float x, float y, float width, float height)
    {
        GUI.DrawTexture(new Rect(x, y, width, height), pixel);
    }
}
