using Dalamud.Plugin;
using Pictomancy.Core;

namespace Pictomancy;
public class Registrar : IDisposable
{
    public const string StrokeElementTag = "stroke";
    public const string FanElementTag = "fan";
    public const string TriangleElementTag = "triangle";
    public const string LineElementTag = "line";
    public const string QuadElementTag = "quad";

    private const string registrationTag = "SplatoonApi";
    private const int majorVersion = 0;
    private const int minorVersion = 0;
    private readonly DalamudPluginInterface plugin;
    private Registration registration;

    public Registrar(DalamudPluginInterface plugin)
    {
        this.plugin = plugin;
        Services.Init(plugin);
    }

    public void Broadcast()
    {
        registration = plugin.GetOrCreateData<List<Registration>>(registrationTag, () => [])[0];
        registration.majorVersion = majorVersion;
        registration.minorVersion = minorVersion;
    }

    public void Dispose()
    {
        plugin.RelinquishData(registrationTag);
    }

    public enum RegistrationStatus
    {
        NOT_CONNECTED,
        // No Splatoon API detected
        NOT_DETECTED,
        // Splatoon API is too new to function. Dependent Plogon Update required.
        SPLATOON_TOO_NEW,
        // Splatoon API is too old to function. Splatoon Update required.
        SPLATOON_TOO_OLD,
        // Splatoon API is old; functionality may be degraded. Splatoon Update recommended.
        SPLATOON_OLD,
        // Splatoon API is compatible.
        SPLATOON_OK,
    }

    public RegistrationStatus RegisterUsage(string plogon)
    {
        List<Registration> registrationList;
        bool detected = plugin.TryGetData<List<Registration>>(registrationTag, out registrationList);
        if (!detected)
        {
            return RegistrationStatus.NOT_DETECTED;
        }

        if (registration.majorVersion > majorVersion)
        {
            return RegistrationStatus.SPLATOON_TOO_NEW;
        }

        if (registration.majorVersion < majorVersion)
        {
            return RegistrationStatus.SPLATOON_TOO_OLD;
        }

        if (registration.minorVersion < minorVersion)
        {
            return RegistrationStatus.SPLATOON_OLD;
        }

        //registration.plugins.Add(plogon);

        return RegistrationStatus.SPLATOON_OK;
    }

    public bool IsRegistered(string plogon)
    {
        return false;
        // return registration.plugins.Contains(plogon);
    }
}
