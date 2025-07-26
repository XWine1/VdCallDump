using System.Runtime.Versioning;
using TerraFX.Interop.Windows;

namespace VdCallDump;

[SupportedOSPlatform("windows")]
internal static unsafe class DiaExtensions
{
    public static HRESULT loadDataFromPdb(ref this IDiaDataSource @this, string pdbPath)
    {
        fixed (char* pPdbPath = pdbPath)
        {
            return @this.loadDataFromPdb(pPdbPath);
        }
    }

    public static HRESULT loadDataForExe(ref this IDiaDataSource @this, string exePath, string? searchPath, IUnknown* pCallback)
    {
        fixed (char* pExePath = exePath)
        fixed (char* pSearchPath = searchPath)
        {
            return @this.loadDataForExe(pExePath, pSearchPath, pCallback);
        }
    }

    public static HRESULT findChildren(ref this IDiaSymbol @this, SymTagEnum symtag, string name, uint compareFlags, IDiaEnumSymbols** ppResult)
    {
        fixed (char* pName = name)
        {
            return @this.findChildren(symtag, pName, compareFlags, ppResult);
        }
    }
}
