using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace VdCallDump;

internal unsafe struct BSTR : IDisposable
{
    private char* _ptr;

    public readonly char** GetAddressOf()
    {
        return (char**)Unsafe.AsPointer(ref Unsafe.AsRef(in this));
    }

    public readonly ReadOnlySpan<char> AsSpan()
    {
        return _ptr != null ? MemoryMarshal.CreateReadOnlySpanFromNullTerminated(_ptr) : [];
    }

    public readonly override string ToString()
    {
        return _ptr != null ? Marshal.PtrToStringBSTR((nint)_ptr) : string.Empty;
    }

    public void Dispose()
    {
        Marshal.FreeBSTR((nint)_ptr);
        _ptr = null;
    }
}
