using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using TerraFX.Interop.Windows;

namespace VdCallDump;

[SupportedOSPlatform("windows")]
internal static unsafe partial class Dia2
{
    [LibraryImport("msdia140")]
    public static partial HRESULT DllGetClassObject(Guid* rclsid, Guid* riid, void** ppv);
}
