namespace ExternalMeleeTool.Utilities; 

// currently stupid and doesnt work
public struct StructHint<T>(Ptr32 ptr) where T : unmanaged {
    public Ptr32 Ptr = ptr;
    public readonly T Value {
        get => Dolphinterop.Read<T>(Ptr);
        set => Dolphinterop.Write(Ptr, value);
    }

    public static implicit operator T(StructHint<T> fu) => fu.Value;
    public static implicit operator Ptr32(StructHint<T> fu) => fu.Ptr;
}
