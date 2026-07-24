using System.Text;

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

    public static unsafe string CharptrToStr(byte* chars, int len = 31) {
        var bytes = new ReadOnlySpan<byte>(chars, len);

        int length = bytes.IndexOf((byte)0);
        if (length < 0)
            length = bytes.Length;

        return Encoding.UTF8.GetString(bytes[..length]);
    }

    public delegate void RefAction<T>(ref T param);
}
