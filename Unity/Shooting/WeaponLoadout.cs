using UnityEngine;

/// <summary>
/// 2종 무기 슬롯을 고려한 로드아웃 관리자.
/// 현재는 1종(소총)만 넣어도 동작.
/// </summary>
public class WeaponLoadout : MonoBehaviour
{
    [Header("Slots (최대 2종 구조)")]
    [Tooltip("Slot 0: 주무기, Slot 1: 보조무기")]
    [SerializeField] private WeaponBase[] weaponSlots = new WeaponBase[2];

    [SerializeField] private int activeIndex;

    public WeaponBase ActiveWeapon =>
        (weaponSlots != null && activeIndex >= 0 && activeIndex < weaponSlots.Length) ? weaponSlots[activeIndex] : null;

    private void Start()
    {
        RefreshActiveState();
    }

    private void Update()
    {
        // 확장용: 무기 전환 (1, 2)
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetActiveWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetActiveWeapon(1);

        if (ActiveWeapon != null)
        {
            ActiveWeapon.Tick(Time.deltaTime);
        }
    }

    public void SetActiveWeapon(int index)
    {
        if (weaponSlots == null) return;
        if (index < 0 || index >= weaponSlots.Length) return;
        if (weaponSlots[index] == null) return;

        activeIndex = index;
        RefreshActiveState();
    }

    private void RefreshActiveState()
    {
        if (weaponSlots == null) return;

        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (weaponSlots[i] != null)
            {
                weaponSlots[i].gameObject.SetActive(i == activeIndex);
            }
        }
    }
}
