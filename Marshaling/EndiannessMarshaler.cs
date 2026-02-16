using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ExternalMeleeTool.Marshaling;

// this is much better... but it still disallows NAOT.
public static class EndiannessMarshaler {
    static readonly Dictionary<Type, (int Offset, Type Type)[]> _structLayoutCache = [];

    public const DynamicallyAccessedMemberTypes naot_safety =
        DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields;

    public static void FixEndianness<[DynamicallyAccessedMembers(naot_safety)] T>(ref T obj) where T : unmanaged {
        // span of bytes the size of the struct
        Span<byte> data = MemoryMarshal.CreateSpan(ref Unsafe.As<T, byte>(ref obj), Unsafe.SizeOf<T>());
        FixBytesRecursive(typeof(T), data);
    }

    static void FixBytesRecursive([DynamicallyAccessedMembers(naot_safety)] Type type, Span<byte> data) {
        if (!_structLayoutCache.TryGetValue(type, out var fields)) {
            var fInfo = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            fields = [.. fInfo.Select(f => (Marshal.OffsetOf(type, f.Name).ToInt32(), f.FieldType))];
            _structLayoutCache[type] = fields;
        }

        foreach (var (offset, fieldType) in fields) {
            int size = GetTypeSize(fieldType);
            Span<byte> fieldSpan = data.Slice(offset, size);

            if (fieldType.IsPrimitive || fieldType.IsEnum) {
                ReverseSpan(fieldSpan);
            }
            else if (fieldType.IsValueType) {
                // nested structs
                FixBytesRecursive(fieldType, fieldSpan);
            }
        }
    }

    // cool and good
    static void ReverseSpan(Span<byte> span) {
        if (span.Length <= 1) return;
        span.Reverse();
    }

    static int GetTypeSize(Type t) {
        if (t.IsEnum) return Marshal.SizeOf(Enum.GetUnderlyingType(t));
        return Marshal.SizeOf(t);
    }
}