/// <summary>
/// 무기 공통 인터페이스.
/// 확장 시(권총, 샷건 등) 동일한 흐름으로 다루기 쉽다.
/// </summary>
public interface IWeapon
{
    string WeaponName { get; }
    int CurrentAmmoInMagazine { get; }
    int CurrentReserveAmmo { get; }
    int MagazineSize { get; }
    bool IsAutomatic { get; }

    bool TryFire();
    void StartReload();
    void Tick(float deltaTime);
}
