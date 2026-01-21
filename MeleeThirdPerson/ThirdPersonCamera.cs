using ExternalMeleeTool;
using ExternalMeleeTool.GameComponents;
using ExternalMeleeTool.Melee;
using ExternalMeleeTool.Melee.Fighter;
using System.Numerics;

namespace MeleeThirdPerson;

public enum CameraFollowKind {
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
    float _yEyeOffset;
    float _targetZFocus;
    float _startPosX;
    float _startFocusX;
    float _eyeYReal;
    // float _zFocusReal;
    float _targetFov;

    public float FollowDist = 25;
    public float OutwardAngle = 40;
    public float DegOff = 30;
    public float CamOffY = 20;
    public float FovMin = 60;
    public float FovMax = 100;
    public float DistMin = 50;
    public float DistMax = 200;
    public float SwitchDuration = 0.5f;

    public int FocusPort = 0;

    public EasingFunction MovementFunction = EasingFunction.OutQuart;
    public CameraFollowKind FocusType = CameraFollowKind.ClosestEnemy;

    public void Update(float deltaTime = 1f) {
        var target = MeleeCamManip.Match.Fighters[FocusPort];

        _yEyeOffset = 0f;
        if (FocusType == CameraFollowKind.PlayerDirection) {
            _desiredSide = target.Direction;
            _yFocus = 0;
            _targetZFocus = 0;
            _targetFov = 90;
            // _yFocus = MathUtils.Lerp(_yFocus, 0, 0.01f);
        }
        else if (FocusType == CameraFollowKind.ClosestEnemy) {
            int closestIndex = GetClosestEnemyIndex(target);
            HandleClosestEnemyCamera(target, closestIndex, deltaTime);
        }

        // L2ndNa = Head? not every character tho
        // TODO: look at this tomorrow
        // var transform = target.GetBoneTransform(FtPart.FtPart_TransN);
        var xCenter = target.Position.X;

        // calculates target positions and angles for switching sides
        var targetAngleX = xCenter + OutwardAngle * _desiredSide;
        var targetX = xCenter + -FollowDist * _desiredSide;

        // if side changed, start a transition from current camera position
        if (_oldSide != _desiredSide) {
            _t = 0f;
            _tActive = true;

            // set start positions to current Eye and Focus
            _startPosX = Camera.Eye.X - xCenter;   // relative to target
            _startFocusX = Camera.Focus.X - xCenter;
        }

        if (_tActive) {
            // 0.002 originally
            if (_t < 1f) _t += 1.25f * deltaTime;
            else {
                _t = 1f;
                _tActive = false;
            }
        }

        MovementFunction = EasingFunction.OutSine;
        var ease = Easings.GetEasingBehavior(MovementFunction, _t);

        // lerp from the previous start positions to the new target positions
        var posX = MathUtils.Lerp(_startPosX + xCenter, targetX, ease);
        var focusX = MathUtils.Lerp(_startFocusX + xCenter, targetAngleX, ease);

        _oldSide = _desiredSide;

        // apply to camera
        // Camera.Eye = transform.Translation //- new Vector3(0, 0, 10);
        var targetEyeY = target.Position.Y + CamOffY + _yEyeOffset;
        _eyeYReal = MathUtils.Lerp(_eyeYReal, targetEyeY, 15f * deltaTime);

        Camera.Eye = new Vector3(posX, _eyeYReal, -target.Position.Z - 20);
        // Camera.Eye = target.GetBoneTransform(FtPart.FtPart_TransN).Translation;
        Camera.Focus = new Vector3(focusX, _yFocus + target.Position.Y + CamOffY, _targetZFocus);

        // .01 originally
        Camera.Fov = MathUtils.Lerp(Camera.Fov, _targetFov, 10f * deltaTime);
    }
    public void HandleClosestEnemyCamera(FighterData target, int closestIndex, float globalSpeed = 1.0f) {
        if (closestIndex == -1) return;

        // the closest enemy
        var enemy = MeleeCamManip.Match.Fighters[closestIndex];

        // used to be transform... experimenting
        var enemyPosition = enemy.Position; //enemy.GetBoneTransform(FtPart.FtPart_TransN).Translation;

        var diff = enemyPosition - target.Position;
        var oppDist = Vector3.Distance(target.Position, enemyPosition); //diff.Length();

        // fanagle with these to change look-at differences
        float variance = MathF.PI / 6;

        float diminishment = 100f;
        float yOff = MathUtils.Clamp(diff.Y / diminishment, -variance, variance);

        /*float maxHeight = 150f;
        float heightFactor = 1f - MathUtils.Clamp(MathF.Abs(diff.Y) / maxHeight, 0f, 1f);
        yOff *= heightFactor;*/
        //Console.WriteLine(yOff + " " + heightFactor);

        // opponent is above
        if (diff.Y > 0) {
            // again
            
            // creates a 0.2f minimum
            yOff *= 1f - (MathUtils.InverseLerp(0, 300, diff.X) * 0.8f + 0.2f);
        }

        if (diff.X > 0)
            _desiredSide = 1;
        else // if less?
            _desiredSide = -1;

        if (target.IsOnLedge) {
            _yEyeOffset = 10;
        }

        // adjusts the camera up or down a bit to allow for easier viewing of vertically steep opponents
        var yEasyView = MathUtils.Clamp(diff.Y / 5, -15, 15);
        _yEyeOffset -= yEasyView;

        var clampedDist = 1f - MathUtils.InverseLerp(DistMin, DistMax, oppDist);

        var distFov = MathUtils.Lerp(FovMin, FovMax, clampedDist);
        var heightFov = MathF.Min(yOff * 25, 50);
        var finalFov = distFov + heightFov;

        _targetFov = finalFov;

        // maybe hard-focus y position?
        // .01 originally
        _yFocus = MathUtils.Lerp(_yFocus, MathF.Abs(diff.Y) * yOff, 10f * globalSpeed);

        _targetZFocus = -enemy.Position.Z;
    }

    // returns -1 if none are found
    int GetClosestEnemyIndex(FighterData focusedPlr) {
        int closestIndex = -1;
        float closestDist = float.MaxValue;
        for (int i = 0; i < MeleeCamManip.Match.Fighters.Length; i++) {
            if (i == FocusPort) continue; // strictly loop through other non-focused players

            var ft = MeleeCamManip.Match.Fighters[i];

            if (ft.SlotKind == SlotKind.None) continue;
            if (MeleeCamManip.Match.IsTeams && ft.Team == focusedPlr.Team && !MeleeCamManip.ScDat.IsUnclePunch) continue;

            var dist = Vector3.Distance(focusedPlr.Position, ft.Position);
            if (dist < closestDist) {
                closestIndex = i;
                closestDist = dist;
            }
        }
        return closestIndex;
    }
}
