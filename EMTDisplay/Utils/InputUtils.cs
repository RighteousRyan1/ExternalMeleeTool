using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;

namespace EMTDisplay.Utils;

#pragma warning disable IDE0079 // lol
#pragma warning disable SYSLIB1054
public static class InputUtils {
    static readonly Buttons[] _buttonBuffer = new Buttons[11];
    // static readonly MouseInput[] _mouseBuffer = new MouseInput[5];

    public struct GamePadSnapshot {
        /// <summary>The previous frame of input.</summary>
        public GamePadState Previous;
        /// <summary>The current frame of input.</summary>
        public GamePadState Current;
        /// <summary>The capabilties of this controller.</summary>
        public GamePadCapabilities Capabilities;
    }

    public struct KBMSnapshot {
        public KeyboardState CurrentKey;
        public KeyboardState PreviousKey;

        public MouseState CurrentMouse;
        public MouseState PreviousMouse;
    }

    public delegate void OnGamePadConnect(int player);
    public delegate void OnGamePadDisconnect(int player);
    /// <summary>0 = player 1, 3 = player 4</summary>
    public static event OnGamePadConnect OnGamePadConnected;
    /// <summary>0 = player 1, 3 = player 4</summary>
    public static event OnGamePadDisconnect OnGamePadDisconnected;

    public const int MAX_GAMEPADS = 4;
    public static GamePadSnapshot[] GamePads { get; } = new GamePadSnapshot[MAX_GAMEPADS];
    public static KBMSnapshot KeyboardMouse = new();

    readonly static bool[] _previousConnected = new bool[MAX_GAMEPADS];

    // they *must* have a keyboard + mouse plugged in, right?
    public static int NumConnectedInputs { get; private set; } = 1;
    public static void PollKBM() {
        KeyboardMouse.PreviousKey = KeyboardMouse.CurrentKey;
        KeyboardMouse.PreviousMouse = KeyboardMouse.CurrentMouse;

        KeyboardMouse.CurrentKey = Keyboard.GetState();
        KeyboardMouse.CurrentMouse = Mouse.GetState();
    }
    public static void PollGamepad() {
        for (int i = 0; i < MAX_GAMEPADS; i++) {
            GamePads[i].Previous = GamePads[i].Current;

            GamePads[i].Capabilities = GamePad.GetCapabilities(i);

            GamePads[i].Current = GamePad.GetState(i);
        }
    }

    /// <summary>
    /// Monitors controller ports and fires events when controllers are plugged or unplugged.
    /// </summary>
    public static void Watch() {
        for (int i = 0; i < MAX_GAMEPADS; i++) {
            var index = i;
            var nowConnected = GamePads[i].Current.IsConnected;
            bool wasConnected = _previousConnected[i];

            if (nowConnected && !wasConnected) {
                OnGamePadConnected?.Invoke(index);
                NumConnectedInputs++;
            }
            else if (!nowConnected && wasConnected) {
                OnGamePadDisconnected?.Invoke(index);
                NumConnectedInputs--;
            }

            _previousConnected[i] = nowConnected;
        }
    }
    public static bool KeyJustPressed(Keys key) => KeyboardMouse.CurrentKey.IsKeyDown(key) && KeyboardMouse.PreviousKey.IsKeyUp(key);
    public static bool AreKeysDown(params Keys[] keys) => keys.All(key => KeyboardMouse.CurrentKey.IsKeyDown(key));
    public static bool AreKeysJustPressed(params Keys[] keys) {
        bool allAreDown = keys.All(key => KeyboardMouse.CurrentKey.IsKeyDown(key));
        bool notAllUp = keys.Any(key => KeyboardMouse.PreviousKey.IsKeyUp(key));

        return allAreDown && notAllUp;
    }
    public static bool MouseLeft => KeyboardMouse.CurrentMouse.LeftButton == ButtonState.Pressed;
    public static bool MouseMiddle => KeyboardMouse.CurrentMouse.MiddleButton == ButtonState.Pressed;
    public static bool MouseRight => KeyboardMouse.CurrentMouse.RightButton == ButtonState.Pressed;
    public static bool Mouse3 => KeyboardMouse.CurrentMouse.XButton1 == ButtonState.Pressed;
    public static bool Mouse4 => KeyboardMouse.CurrentMouse.XButton2 == ButtonState.Pressed;

    public static bool OldMouseLeft => KeyboardMouse.PreviousMouse.LeftButton == ButtonState.Pressed;
    public static bool OldMouseMiddle => KeyboardMouse.PreviousMouse.MiddleButton == ButtonState.Pressed;
    public static bool OldMouseRight => KeyboardMouse.PreviousMouse.LeftButton == ButtonState.Pressed;
    public static bool OldMouse3 => KeyboardMouse.PreviousMouse.XButton1 == ButtonState.Pressed;
    public static bool OldMouse4 => KeyboardMouse.PreviousMouse.XButton2 == ButtonState.Pressed;
    public static bool CanDetectClick(bool rightClick = false) {
        bool clicked = !rightClick ? KeyboardMouse.CurrentMouse.LeftButton == ButtonState.Pressed && KeyboardMouse.PreviousMouse.LeftButton == ButtonState.Released
            : KeyboardMouse.CurrentMouse.RightButton == ButtonState.Pressed && KeyboardMouse.PreviousMouse.RightButton == ButtonState.Released;
        return clicked;
    }
    public static bool CanDetectClickRelease(bool rightClick = false) {
        bool released = !rightClick ? KeyboardMouse.CurrentMouse.LeftButton != ButtonState.Pressed && KeyboardMouse.PreviousMouse.LeftButton != ButtonState.Released
            : KeyboardMouse.CurrentMouse.RightButton != ButtonState.Pressed && KeyboardMouse.PreviousMouse.RightButton != ButtonState.Released;
        return released;
    }
    public static Keys FirstPressedKey {
        get {
            if (KeyboardMouse.CurrentKey.GetPressedKeys().Length > 0)
                return KeyboardMouse.CurrentKey.GetPressedKeys()[^1];
            return Keys.None;
        }
    }
    /// <summary>
    /// Returns true if the user has used the gamepad this frame (i.e: pressed buttons, moved stick, pulled trigger)
    /// </summary>
    public static bool IsGamepadBeingUsed(PlayerIndex player = PlayerIndex.One) {
        var state = GamePad.GetState(player);

        if (!state.IsConnected)
            return false;

        bool isUsed =
            state.Buttons.A == ButtonState.Pressed ||
            state.Buttons.B == ButtonState.Pressed ||
            state.Buttons.X == ButtonState.Pressed ||
            state.Buttons.Y == ButtonState.Pressed ||
            state.Buttons.Start == ButtonState.Pressed ||
            state.Buttons.Back == ButtonState.Pressed ||
            state.Buttons.LeftShoulder == ButtonState.Pressed ||
            state.Buttons.RightShoulder == ButtonState.Pressed ||
            state.DPad.IsPressed() ||
            MathF.Abs(state.ThumbSticks.Left.X) > 0.1f ||
            MathF.Abs(state.ThumbSticks.Left.Y) > 0.1f ||
            MathF.Abs(state.ThumbSticks.Right.X) > 0.1f ||
            MathF.Abs(state.ThumbSticks.Right.Y) > 0.1f ||
            state.Triggers.Left > 0.05f ||
            state.Triggers.Right > 0.05f;

        return isUsed;
    }

    private static bool IsPressed(this GamePadDPad dpad) {
        return dpad.Up == ButtonState.Pressed ||
               dpad.Down == ButtonState.Pressed ||
               dpad.Left == ButtonState.Pressed ||
               dpad.Right == ButtonState.Pressed;
    }

    public static ReadOnlySpan<Buttons> GetPressedButtons(GamePadButtons buttons, bool excludeSystemButtons = false) {
        int count = 0;

        if (buttons.A == ButtonState.Pressed)
            _buttonBuffer[count++] = Buttons.A;
        if (buttons.B == ButtonState.Pressed)
            _buttonBuffer[count++] = Buttons.B;
        if (buttons.Back == ButtonState.Pressed && !excludeSystemButtons)
            _buttonBuffer[count++] = Buttons.Back;
        if (buttons.BigButton == ButtonState.Pressed && !excludeSystemButtons)
            _buttonBuffer[count++] = Buttons.BigButton;
        if (buttons.LeftShoulder == ButtonState.Pressed)
            _buttonBuffer[count++] = Buttons.LeftShoulder;
        if (buttons.LeftStick == ButtonState.Pressed)
            _buttonBuffer[count++] = Buttons.LeftStick;
        if (buttons.RightShoulder == ButtonState.Pressed)
            _buttonBuffer[count++] = Buttons.RightShoulder;
        if (buttons.RightStick == ButtonState.Pressed)
            _buttonBuffer[count++] = Buttons.RightStick;
        if (buttons.Start == ButtonState.Pressed && !excludeSystemButtons)
            _buttonBuffer[count++] = Buttons.Start;
        if (buttons.X == ButtonState.Pressed)
            _buttonBuffer[count++] = Buttons.X;
        if (buttons.Y == ButtonState.Pressed)
            _buttonBuffer[count++] = Buttons.Y;

        return _buttonBuffer.AsSpan(0, count);
    }
    public static bool ButtonJustPressed(int player, Buttons button) 
        => GamePads[player].Current.IsButtonDown(button) && GamePads[player].Previous.IsButtonUp(button);
    public static int DeltaScrollWheel => KeyboardMouse.CurrentMouse.ScrollWheelValue / 120;
    public static int OldDeltaScrollWheel => KeyboardMouse.CurrentMouse.ScrollWheelValue / 120;
    public static int GetScrollWheelChange() => DeltaScrollWheel == OldDeltaScrollWheel ? 0 : DeltaScrollWheel - OldDeltaScrollWheel;
    public static float ApplyDeadzone(float value, float minDeadzone, float maxDeadzone, float minVal, float maxVal) {
        float mid = (minVal + maxVal) * 0.5f;

        float offset = value - mid;
        float magnitude = MathF.Abs(offset);
        float sign = MathF.Sign(offset);

        if (magnitude < minDeadzone)
            return 0f;

        if (magnitude > maxDeadzone)
            return sign;


        // rescale between MinDeadzone and MaxDeadzone to [0, 1]
        float normalized = (magnitude - minDeadzone) / (maxDeadzone - minDeadzone);
        return MathHelper.Clamp(normalized * sign, minVal, maxVal);
    }
}