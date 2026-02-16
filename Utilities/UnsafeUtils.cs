namespace ExternalMeleeTool.Utilities; 
public static class UnsafeUtils {
    public static IEnumerable<(Ptr32 Ptr, T Value)> IteratePointerList<T>(
        uint headPtr,
        Func<T, uint> getNext
    ) where T : unmanaged {
        uint cur = headPtr;

        while (cur != 0) {
            T value = Dolphinterop.Read<T>(cur);
            yield return (cur, value);
            cur = getNext(value);
        }
    }

    public delegate void RefAction<T>(ref T param);
}
