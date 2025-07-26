using AsmResolver.IO;
using AsmResolver.PE.File;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Runtime.Versioning;
using TerraFX.Interop.Windows;
using static TerraFX.Interop.Windows.Windows;

namespace VdCallDump;

internal static class EraSystemCalls
{
    public static ImmutableArray<string> VdSystemCallBinaries { get; } =
    [
        "d3d11_x", "d3d12_x",
        "umd", "umd_d", "umd_i", "umd_v",
        "umd12", "umd12_d", "umd12_i",
        "xhit", "xhit_s"
    ];

    public static void GetSystemCalls(Dictionary<EraVdSystemCall, uint> values, string dllPath, string? pdbPath = null)
    {
        var rvas = new List<uint>();
        var file = PEFile.FromFile(dllPath);
        var code = file.CreateReaderAtRva(file.OptionalHeader.BaseOfCode);
        GetSystemCalls(code, rvas);

        if (!ValidSystemCallCounts.Contains(rvas.Count))
        {
            Console.WriteLine($"Error: bad number of system calls ({rvas.Count})");
            return;
        }

        if (OperatingSystem.IsWindows() && !string.IsNullOrEmpty(pdbPath))
        {
            GetSystemCallsWithPdb(values, pdbPath, rvas, code);
            return;
        }

        int index = 0;

        foreach (EraVdSystemCall syscall in Enum.GetValues<EraVdSystemCall>().Order())
        {
            if (VdSystemCalls[syscall] > rvas.Count)
                continue;

            code.Rva = rvas[index++] - 4;
            values[syscall] = code.ReadUInt32();
        }
    }

    [SupportedOSPlatform("windows")]
    private static unsafe void GetSystemCallsWithPdb(Dictionary<EraVdSystemCall, uint> values, string pdbPath, List<uint> rvas, BinaryStreamReader code)
    {
        using ComPtr<IClassFactory> factory = new();
        Dia2.DllGetClassObject(__uuidof<DiaSource>(), __uuidof<IClassFactory>(), (void**)factory.GetAddressOf()).Assert();

        using ComPtr<IDiaDataSource> source = new();
        factory.Get()->CreateInstance(null, __uuidof<IDiaDataSource>(), (void**)source.GetAddressOf()).Assert();
        source.Get()->loadDataFromPdb(pdbPath).Assert();

        using ComPtr<IDiaSession> session = new();
        source.Get()->openSession(session.GetAddressOf()).Assert();

        foreach (var rva in rvas)
        {
            using ComPtr<IDiaSymbol> symbol = new();
            session.Get()->findSymbolByRVA(rva, SymTagEnum.SymTagPublicSymbol, symbol.GetAddressOf());

            using BSTR name = new();
            symbol.Get()->get_name(name.GetAddressOf()).Assert();

            if (TryGetVdSystemCallFromThunkName(name.AsSpan(), out EraVdSystemCall type))
            {
                code.Rva = rva - 4;
                values[type] = code.ReadUInt32();
            }
        }
    }

    private static bool TryGetVdSystemCallFromThunkName(ReadOnlySpan<char> thunkName, out EraVdSystemCall syscall)
    {
        return Enum.TryParse(thunkName["VdEra".Length..], out syscall);
    }

    private static void GetSystemCalls(BinaryStreamReader code, List<uint> rvas)
    {
        // Scan for the following pattern:
        //
        // B8 XX XX XX XX | mov eax, imm32
        // 0F 05          | syscall
        // C3             | ret
        while (code.CanRead(8))
        {
            if (code.ReadByte() != 0xB8)
                continue;

            uint rva = code.Rva;
            code.ReadUInt32();

            if (code.ReadByte() != 0x0F ||
                code.ReadByte() != 0x05 ||
                code.ReadByte() != 0xC3)
            {
                code.Rva = rva;
                continue;
            }

            rvas.Add(rva + 4);
        }
    }

    // For each system call, the total number of system calls that existed at the time they were introduced.
    private static readonly FrozenDictionary<EraVdSystemCall, int> VdSystemCalls = new Dictionary<EraVdSystemCall, int>
    {
        [EraVdSystemCall.GetFrameStatistics] = 6,
        [EraVdSystemCall.GetGpuPageTable] = 6,
        [EraVdSystemCall.KickoffGraphics] = 6,
        [EraVdSystemCall.KickoffGraphicsSegmentQueue] = 6,
        [EraVdSystemCall.KickoffSdma] = 6,
        [EraVdSystemCall.Swap] = 6,
        [EraVdSystemCall.GetTimestamps] = 7,
        [EraVdSystemCall.KickoffCompute] = 9,
        [EraVdSystemCall.RequestComputeQueue] = 9,
        [EraVdSystemCall.GetVmid] = 15,
        [EraVdSystemCall.KickoffTileMappingCopies] = 15,
        [EraVdSystemCall.KickoffTileMappings] = 15,
        [EraVdSystemCall.PixReplaceEsram] = 15,
        [EraVdSystemCall.ProtectReservedRegion] = 15,
        [EraVdSystemCall.RegisterTilePool] = 15,
        [EraVdSystemCall.PixQueryModifiedAddressRanges] = 16,
        [EraVdSystemCall.ConfigureVirtualMemory] = 18,
        [EraVdSystemCall.SetVLineNotification] = 18,
        [EraVdSystemCall.TerminateDevice] = 19,
        [EraVdSystemCall.Create32BitMapping] = 21,
        [EraVdSystemCall.SetFrameNotification] = 21,
        [EraVdSystemCall.InitializeDevice] = 22,
        [EraVdSystemCall.CanCreateContext] = 24,
        [EraVdSystemCall.MapToEsram] = 24,
        [EraVdSystemCall.UploadCustomMicrocode] = 25,
        [EraVdSystemCall.QueueTileMappingCopies] = 28,
        [EraVdSystemCall.QueueTileMappings] = 28,
        [EraVdSystemCall.StreamingCountersControl] = 28,
        [EraVdSystemCall.FlushEntireCpuCache] = 29,
        [EraVdSystemCall.GetLastKmdError] = 31,
        [EraVdSystemCall.QueryTilePool] = 31,
        [EraVdSystemCall.MapPhysicalToVirtual] = 33,
        [EraVdSystemCall.SetHaltOnAccess] = 33,
        [EraVdSystemCall.GetSEQCounters] = 34,
        [EraVdSystemCall.QueueBatchMappings] = 35,
        [EraVdSystemCall.MarkSwapChainMemory] = 36,
        [EraVdSystemCall.KPixNotifyShaderCreation] = 40,
        [EraVdSystemCall.ScheduleFrameEvents] = 40,
        [EraVdSystemCall.SetFrameInterval] = 40,
        [EraVdSystemCall.Swap2] = 40,
        [EraVdSystemCall.GetFrameEventScheduleStatistics] = 42,
        [EraVdSystemCall.GetFrameEventSignalStatistics] = 42

    }.ToFrozenDictionary();

    private static readonly FrozenSet<int> ValidSystemCallCounts = VdSystemCalls.Values.ToFrozenSet();
}
