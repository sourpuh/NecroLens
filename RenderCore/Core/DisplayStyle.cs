using ImGuiNET;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Pictomancy.Core;
public struct DisplayStyle(uint fillColor, uint strokeColor, float strokeThickness)
{
    public uint fillColor = fillColor;
    public uint strokeColor = strokeColor;
    public float strokeThickness = strokeThickness;

    public bool filled => fillColor != 0;
}

public static class ColorUtils
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ToUint(this Vector4 color)
    {
        return ImGui.ColorConvertFloat4ToU32(color);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 ToVector4(this uint color)
    {
        return ImGui.ColorConvertU32ToFloat4(color);
    }
}
