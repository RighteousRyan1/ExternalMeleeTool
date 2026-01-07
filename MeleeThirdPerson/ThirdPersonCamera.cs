using ExternalMeleeTool;
using System.Numerics;

namespace MeleeThirdPerson;

public enum ThirdPersonFocusType {
    PlayerDirection, // camera rotates to face player's facing direction
    ClosestEnemy  // camera rotates to face enemy's position
}
public class ThirdPersonCamera {
    public MeleeFreeCamera Camera;

    float _t;
    bool _tActive;
    float _desiredSide;
    float _oldSide;
    float _yFocus;
    float _zFocus;
    float _startPosX;
    float _startFocusX;

    public float FollowDist = 25;
    public float OutwardAngle = 40;
    public float DegOff = 30;
    public float CamOffY = 20;
    public float FovMin = 70;
    public float FovMax = 100;
    public float DistMin = 50;
    public float DistMax = 200;
    public float SwitchDuration = 0.5f;

    public int FocusPort = 0;

    public EasingFunction MovementFunction = EasingFunction.OutQuart;
    public ThirdPersonFocusType FocusType = ThirdPersonFocusType.ClosestEnemy;

    public void Update() {
        var target = MeleeCamManip.Fighters[FocusPort];

        if (FocusType == ThirdPersonFocusType.PlayerDirection) {
            _desiredSide = target.Direction;
            _yFocus = 0;
            _zFocus = 0;
            // _yFocus = MathUtils.Lerp(_yFocus, 0, 0.01f);
        }
        else if (FocusType == ThirdPersonFocusType.ClosestEnemy) {
            int closestIndex = GetClosestEnemyIndex(target);
            HandleClosestEnemyCamera(target, closestIndex);
        }

        // calculates target positions and angles for switching sides
        var targetAngleX = target.Position.X + OutwardAngle * _desiredSide;
        var targetX = target.Position.X + -FollowDist * _desiredSide;

        // if side changed, start a transition from current camera position
        if (_oldSide != _desiredSide) {
            _t = 0f;
            _tActive = true;

            // set start positions to current Eye and Focus
            _startPosX = Camera.Eye.X - target.Position.X;   // relative to target
            _startFocusX = Camera.Focus.X - target.Position.X;
        }

        if (_tActive) {
            if (_t < 1f) _t += 0.00125f;
            else {
                _t = 1f;
                _tActive = false;
            }
        }

        MovementFunction = EasingFunction.OutSine;
        var ease = Easings.GetEasingBehavior(MovementFunction, _t);

        // lerp from the previous start positions to the new target positions
        var posX = MathUtils.Lerp(_startPosX + target.Position.X, targetX, ease);
        var focusX = MathUtils.Lerp(_startFocusX + target.Position.X, targetAngleX, ease);

        _oldSide = _desiredSide;

        // apply to camera
        Camera.Eye = new Vector3(posX, target.Position.Y + CamOffY, -target.Position.Z - 20);
        Camera.Focus = new Vector3(focusX, _yFocus + target.Position.Y + CamOffY, _zFocus);
    }

    public void HandleClosestEnemyCamera(FighterBlock target, int closestIndex) {
        if (closestIndex == -1) return;

        // the closest enemy
        var enemy = MeleeCamManip.Fighters[closestIndex];

        var diff = enemy.Position - target.Position;

        // fanagle with these to change look-at differences
        float variance = MathF.PI / 6;
        float yOff = MathUtils.Clamp(diff.Y / 100f, -variance, variance);

        if (diff.X > 0)
            _desiredSide = 1;
        else // if less?
            _desiredSide = -1;

        var oppDist = diff.Length();
        var clampedDist = 1f - MathUtils.InverseLerp(DistMin, DistMax, oppDist);

        var distFov = MathUtils.Lerp(FovMin, FovMax, clampedDist);
        var heightFov = MathF.Min(yOff * 25, 50);
        var finalFov = distFov + heightFov;

        Camera.Fov = finalFov;

        _yFocus = MathUtils.Lerp(_yFocus, MathF.Abs(diff.Y) * yOff, 0.01f);

        _zFocus = -enemy.Position.Z;

        /*Console.WriteLine(_yFocus);
        Console.WriteLine(diff.Y);
        Console.WriteLine(yOff);
        Console.WriteLine(Camera.Eye);
        Console.WriteLine(Camera.Focus);*/
    }

    // returns -1 if none are found
    int GetClosestEnemyIndex(FighterBlock focusedPlr) {
        int closestIndex = -1;
        float closestDist = float.MaxValue;
        for (int i = 0; i < MeleeCamManip.Fighters.Length; i++) {
            if (i == FocusPort) continue; // strictly loop through other non-focused players

            var ft = MeleeCamManip.Fighters[i];

            if (ft.SlotKind == SlotKind.None) continue;
            if (MeleeCamManip.Match.IsTeams && ft.Team == focusedPlr.Team) continue;

            var dist = Vector3.Distance(focusedPlr.Position, ft.Position);
            if (dist < closestDist) {
                closestIndex = i;
                closestDist = dist;
            }
        }
        return closestIndex;
    }
}
