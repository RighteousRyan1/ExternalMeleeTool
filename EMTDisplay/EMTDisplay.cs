using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ExternalMeleeTool;
using System;
using System.Collections.Generic;
using FontStashSharp;
using System.IO;
using ExternalMeleeTool.MeleeTypes;
using EMTDisplay.Utils;

namespace EMTDisplay;

public class EMTDisplay : Game {
    public static GraphicsDeviceManager Graphics;
    public static SpriteBatch SpriteBatch;

    public static MatchData Match;
    public static GlobalMeleeData GlDat;
    public static SlippiOnlineData OnDat;
    public static StageData StDat;

    public static Texture2D WhitePixel;

    public Matrix CameraMatrix;

    public static SpriteFontBase MeleeFont;
    FontSystem _fs;

    public EMTDisplay() {
        Graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        Graphics.PreferredBackBufferHeight = 800;
        Graphics.PreferredBackBufferWidth = 1280;
        Window.AllowUserResizing = true;
    }

    protected override void Initialize() {
        // TODO: Add your initialization logic here
        IsFixedTimeStep = false;
        _fs = new();

        base.Initialize();
    }

    protected override void LoadContent() {
        SpriteBatch = new SpriteBatch(GraphicsDevice);

        WhitePixel = new Texture2D(GraphicsDevice, 1, 1);
        WhitePixel.SetData([Color.White]);

        _fs.AddFont(File.Open("Content/melee_font.ttf", FileMode.Open));
        MeleeFont = _fs.GetFont(30);

        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime) {

        if (!Slippinterop.IsConnected) {
            if (!Slippinterop.Connect("GALE01", "GTME01")) {
                // Optional: Add a "Searching for Dolphin..." log or UI state here
                return; // Skip the rest of the frame if we can't connect
            }
        }
        Match = Slippinterop.GetMatchData();
        GlDat = Slippinterop.GetGlobalData();
        OnDat = Slippinterop.GetOnlineData(GlDat);
        StDat = Slippinterop.GetStageData(GlDat);

        if (InputUtils.KeyJustPressed(Keys.F))
            _writeToGameCam = !_writeToGameCam;
        Slippinterop.SetCameraType(_writeToGameCam ? CameraKind.Develop : CameraKind.Normal);
        if (_writeToGameCam) {
            float baseDistance = 400f;
            float zoomSpeed = 0.2f;

            float zoomDepth = -baseDistance * MathF.Exp(-zoom * zoomSpeed);
            var sysVec = new System.Numerics.Vector3(_translation.X, -_translation.Y, zoomDepth);
            Slippinterop.SetMeleeCamera(
                sysVec,
                sysVec + new System.Numerics.Vector3(0, 0, 20), 
                60
            );
        }

        var ms = Mouse.GetState();
        zoom = MathF.Max(ms.ScrollWheelValue / 120 + 1, 1);

        if (ms.LeftButton == ButtonState.Pressed && IsActive) {
            _translation.X += (_oldMs.Position.X - ms.Position.X) / zoom;
            _translation.Y += (_oldMs.Position.Y - ms.Position.Y) / zoom;

            /*if (ms.X > Window.ClientBounds.Width) {
                Mouse.SetPosition(0, ms.Y);
                _translation.X -= Window.ClientBounds.Width / zoom;
            }
            if (ms.X < 0) {
                Mouse.SetPosition(Window.ClientBounds.Width, ms.Y);
                _translation.X += Window.ClientBounds.Width / zoom;
            }*/

            /*if (ms.Y > Window.ClientBounds.Height) {
                Mouse.SetPosition(ms.X, 0);
                _translation.Y -= Window.ClientBounds.Width / zoom;
            }
            if (ms.Y < 0) {
                Mouse.SetPosition(ms.X, Window.ClientBounds.Height);
                _translation.Y += Window.ClientBounds.Width / zoom;
            }*/
        }

        // will never work i guess
        _ftHover = -1;
        // var transMouse = Vector2.Transform(new Vector2(ms.X, ms.Y), CameraMatrix);
        for (int i = 0; i < Match.Fighters.Length; i++) {
            var fd = Match.Fighters[i];
            if (fd.SlotKind == SlotKind.None) continue;

            //Slippinterop.WriteVec3(fd.Fighter + 0x8C,
            //    new Vector3(0, 2.5f, 0).ToNumerics());

            /*if (fd.ECB.Contains(transMouse.X, transMouse.Y)) {
                _ftHover = i; // change to i?
            }*/
        }

        _oldMs = ms;

        if (IsActive)
            InputUtils.PollKBM();
    }

    static int _ftHover;
    static bool _writeToGameCam;
    readonly Color[] _portColors = [Color.Red, Color.Blue, Color.Yellow, Color.Green];
    Vector3 _translation;
    MouseState _oldMs;

    static float zoom;
    protected override void Draw(GameTime gameTime) {
        GraphicsDevice.Clear(Color.Transparent);

        if (!Slippinterop.IsConnected) {
            SpriteBatch.Begin();

            SpriteBatch.DrawString(MeleeFont,
                $"Scanning for Melee...",
                Vector2.Zero, Color.White);

            SpriteBatch.End();
            return;
        }

        var stageScale = StDat.GroundParams.StageScale;
        var screenCenter = new Vector3(GraphicsDevice.Viewport.Width / 2f, GraphicsDevice.Viewport.Height / 2f, 0);
        var lineZoom = 1f / zoom;
        CameraMatrix =
            Matrix.CreateTranslation(-_translation.X, -_translation.Y, 0) *
            Matrix.CreateScale(zoom) *
            Matrix.CreateTranslation(screenCenter);
        ;

        SpriteBatch.Begin(transformMatrix: CameraMatrix);

        // the zones already account for stage scale parameter...
        DrawBoundingRect(StDat.GetRealBlastZone(), Color.DarkRed, lineZoom);
        DrawBoundingRect(StDat.GetRealCameraBounds(), Color.CadetBlue, lineZoom);

        for (int i = 0; i < StDat.LineCount; i++) {
            var lineDesc = StDat.MapLines[i];
            var lStart = StDat.Vertices[lineDesc.StartIdx] * stageScale;
            var lEnd = StDat.Vertices[lineDesc.EndIdx] * stageScale;
            DrawLine(lStart, lEnd, Color.Orange, lineZoom, invertY: true);
        }

        for (int i = 0; i < Match.Fighters.Length; i++) {
            var fd = Match.Fighters[i];
            if (fd.SlotKind == SlotKind.None) continue;

            DrawMeleePlayer(fd, _portColors[i], lineZoom);
        }

        SpriteBatch.End();

        // draws regular info
        SpriteBatch.Begin();

        SpriteBatch.DrawString(MeleeFont,
            $"Zoom: {zoom}\n" +
            $"StageScale: {StDat.GroundParams.StageScale}\n" +
            $"IsTeams: {Match.IsTeams}\n" +
            $"GameCamWrite: {_writeToGameCam}\n" +
            $"ftHover: {_ftHover}",
            Vector2.Zero, Color.White);

        SpriteBatch.End();

        base.Draw(gameTime);
    }

    // draws using melee coordinates, Y = -Y
    public static void DrawLine(Vector2 start, Vector2 end, Color color, float thickness = 1f, bool invertY = false) {
        if (invertY) {
            start.Y *= -1;
            end.Y *= -1;
        }

        SpriteBatch.Draw(WhitePixel, start, null, color,
            (end - start).ToRotation(),
            new Vector2(0, 0.5f),
            new Vector2(Vector2.Distance(start, end), thickness),
            SpriteEffects.None, 0);
    }
    // eventually: draw ecb
    static string[] _infoArr;
    public static void DrawMeleePlayer(FighterData fd, Color color, float thickness = 1f) {
        var pos = new Vector2(fd.Position.X /*+ fd.Position.X * 0.11f*/, fd.Position.Y /*+ fd.Position.Y * 0.11f*/);
        var ecb = fd.ECB;

        DrawLine(pos + ecb.Bottom, pos + ecb.Right, color, thickness, invertY: true);

        DrawLine(pos + ecb.Right, pos + ecb.Top, color, thickness, invertY: true);

        DrawLine(pos + ecb.Top, pos + ecb.Left, color, thickness, invertY: true);

        DrawLine(pos + ecb.Left, pos + ecb.Bottom, color, thickness, invertY: true);

        float linesLength = 1;
        DrawLine(pos - new Vector2(linesLength, 0), pos + new Vector2(linesLength, 0), Color.White, thickness, true);
        DrawLine(pos - new Vector2(0, linesLength), pos + new Vector2(0, linesLength), Color.White, thickness, true);

        _infoArr = [
            $"<{fd.Position.X:F2}, {fd.Position.Y:F2}>" +
            $"{fd.CharKind}"
        ];

        var scale = 0.1f;
        for (int i = _infoArr.Length - 1; i >= 0; i--) {
            var info = _infoArr[i];
            SpriteBatch.DrawString(MeleeFont, info,
                new Vector2(fd.Position.X, -fd.Position.Y - i * 30 * scale) + new Vector2(fd.ECB.Top.X, -fd.ECB.Top.Y), color, scale: new Vector2(scale),
                origin: RenderUtils.GetAnchor(Anchor.BottomCenter, MeleeFont.MeasureString(info)));
        }

        //if (fd.SlotKind == SlotKind.Human) {
        //    Slippinterop.WriteVec3(fd.Fighter + 0xB0, new System.Numerics.Vector3(0, 10, 0));
        //}
    }

    public static void DrawBoundingRect(BoundingRect rect, Color color, float thickness = 1f) {
        // flip Y because Y = Up in 2d space
        var topLeft = new Vector2(rect.Left, -rect.Top);
        var topRight = new Vector2(rect.Right, -rect.Top);
        var bottomLeft = new Vector2(rect.Left, -rect.Bottom);
        var bottomRight = new Vector2(rect.Right, -rect.Bottom);

        var topStr = $"Top: {-topLeft.Y:F1}";
        var botStr = $"Bottom: {-bottomLeft.Y:F1}";
        var leftStr = $"Left: {topLeft.X:F1}";
        var rightStr = $"Right: {topRight.X:F1}";

        DrawLine(topLeft, topRight, color, thickness); // top Edge
        SpriteBatch.DrawString(MeleeFont, topStr,
            (topLeft + topRight) / 2, color, scale: new Vector2(0.25f), 
            origin: RenderUtils.GetAnchor(Anchor.BottomCenter, MeleeFont.MeasureString(topStr)));

        DrawLine(topRight, bottomRight, color, thickness); // right Edge
        SpriteBatch.DrawString(MeleeFont, rightStr,
            (topRight + bottomRight) / 2, color, scale: new Vector2(0.25f),
            origin: RenderUtils.GetAnchor(Anchor.BottomCenter, MeleeFont.MeasureString(rightStr)),
            rotation: MathHelper.PiOver2);

        DrawLine(bottomRight, bottomLeft, color, thickness); // bottom Edge
        SpriteBatch.DrawString(MeleeFont, botStr,
            (bottomRight + bottomLeft) / 2, color, scale: new Vector2(0.25f),
            origin: RenderUtils.GetAnchor(Anchor.TopCenter, MeleeFont.MeasureString(botStr)));

        DrawLine(bottomLeft, topLeft, color, thickness); // left Edge
        SpriteBatch.DrawString(MeleeFont, leftStr,
            (bottomLeft + topLeft) / 2, color, scale: new Vector2(0.25f),
            origin: RenderUtils.GetAnchor(Anchor.BottomCenter, MeleeFont.MeasureString(leftStr)),
            rotation: -MathHelper.PiOver2);
    }
}
