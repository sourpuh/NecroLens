using Dalamud.Plugin;
using ImGuiNET;
using Pictomancy.Core;

namespace Pictomancy;
public static class Canvas
{
    private static AutoClipZones _autoClipZones;
    private static Renderer _renderer;

    public static void Initialize(DalamudPluginInterface pluginInterface)
    {
        Services.Init(pluginInterface);
        _renderer = new();
        _autoClipZones = new(_renderer);
    }

    public static PctDrawList Draw(ImDrawListPtr? drawlist = null)
    {
        return new(drawlist ?? ImGui.GetBackgroundDrawList(), _renderer, _autoClipZones);
    }

    public static void Dispose()
    {
        _renderer.Dispose();
    }
}
