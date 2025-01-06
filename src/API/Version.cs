using System;

namespace EvilMask.Elin.ModOptions;

public readonly ref struct Version(uint major, uint minor, uint patch, uint build = 0)
{
    public readonly int Major => (int)(value / 1000000000000UL);
    public readonly int Minor => (int)(value % 1000000000000UL / 100000000UL);
    public readonly int Patch => (int)(value % 100000000UL / 10000UL);
    public readonly int Build => (int)(value % 10000UL);
    public override readonly string ToString() => Build > 0 ? $"{Major}.{Minor}.{Patch}.{Build}" : $"{Major}.{Minor}.{Patch}";

    private readonly UInt64 value = major * 1000000000000UL + minor * 100000000UL + patch * 10000UL + build % 10000UL;

    public static Version Current => new(ModInfo.Minor, ModInfo.Minor, ModInfo.Patch, ModInfo.Build);
}