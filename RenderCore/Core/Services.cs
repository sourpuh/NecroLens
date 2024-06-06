using Dalamud.Game;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace Pictomancy.Core;
#nullable disable

internal class Services
{
    [PluginService] public static DalamudPluginInterface PluginInterface { get; private set; }
    [PluginService] public static IGameGui GameGui { get; private set; }
    [PluginService] public static ISigScanner SigScanner { get; private set; }
    [PluginService] public static IPluginLog Log { get; private set; }
    [PluginService] public static IAddonLifecycle AddonLifecycle { get; private set; }

    internal static bool IsInitialized = false;
    public static void Init(DalamudPluginInterface pi)
    {
        try
        {
            pi.Create<Services>();
        }
        catch (Exception ex)
        {
        }
    }
}
