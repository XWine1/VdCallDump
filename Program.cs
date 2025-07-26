using VdCallDump;
using System.Runtime.CompilerServices;

[assembly: DisableRuntimeMarshalling]

if (args.Length == 0)
{
    Console.WriteLine($"usage: {Path.GetFileNameWithoutExtension(Environment.ProcessPath)} <dll path> [pdb path]");
    return;
}

var syscalls = new Dictionary<EraVdSystemCall, uint>();
EraSystemCalls.GetSystemCalls(syscalls, args[0], args.Length > 1 ? args[1] : null);

foreach (var (syscall, id) in syscalls)
{
    Console.WriteLine($"{id:X4}: {syscall}");
}
