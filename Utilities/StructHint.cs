namespace ExternalMeleeTool.Utilities; 

// currently stupid and doesnt work
public struct StructHint<T>(Ptr32 ptr) where T : unmanaged {
    public Ptr32 Ptr = ptr;
    T cache = Dolphinterop.Read<T>(ptr);
    public T Value {
        readonly get => cache;
        set {
            cache = value;
            Dolphinterop.Write(Ptr, value);
        }
    }

    public static implicit operator T(StructHint<T> fu) => fu.Value;
    public static implicit operator Ptr32(StructHint<T> fu) => fu.Ptr;
}
