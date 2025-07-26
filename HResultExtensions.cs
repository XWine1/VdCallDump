using System.Runtime.InteropServices;
using TerraFX.Interop.Windows;

namespace VdCallDump;

internal static class HResultExtensions
{
    public static HRESULT Assert(this HRESULT hr)
    {
        Marshal.ThrowExceptionForHR(hr);
        return hr;
    }
}
