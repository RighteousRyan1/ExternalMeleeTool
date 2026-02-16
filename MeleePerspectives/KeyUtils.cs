using System.Runtime.InteropServices;

namespace MeleePerspectives;

#pragma warning disable IDE0079 // seriously. why?
#pragma warning disable CA2020, SYSLIB1054
public static class KeyUtils {
    [DllImport("user32.dll")]
    static extern short GetAsyncKeyState(int vKey);

    // stores previous key states
    static readonly Dictionary<int, bool> _prevStates = [];

    // check if down *currently*
    public static bool IsKeyDown(int vKey) {
        return (GetAsyncKeyState(vKey) & 0x8000) != 0;
    }

    public static bool IsKeyDown(ConsoleKey key) => IsKeyDown((int)key);

    public static bool WasKeyPressed(int vKey) {
        bool isDown = IsKeyDown(vKey);
        _prevStates.TryGetValue(vKey, out bool wasDown);
        _prevStates[vKey] = isDown;
        return isDown && !wasDown;
    }

    public static bool WasKeyPressed(ConsoleKey key) => WasKeyPressed((int)key);
}
