using System.ComponentModel;

namespace ExternalMeleeTool.Utilities; 

// currently stupid and doesnt work
public struct FieldUnion<T>(Ptr32 ptr) where T : unmanaged {
    public Ptr32 Ptr = ptr;
    public readonly T Value {
        get => Dolphinterop.Read<T>(Ptr);
        set => Dolphinterop.Write(Ptr, value);
    }

    public static implicit operator T(FieldUnion<T> fu) => fu.Value;
    public static implicit operator Ptr32(FieldUnion<T> fu) => fu.Ptr;
}
