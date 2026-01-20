using ExternalMeleeTool.Melee;
using ExternalMeleeTool.Melee.Collision;
using System.Numerics;

namespace ExternalMeleeTool.GameComponents;

public struct StageData {
    /// <summary>The ID of the stage being played on.</summary>
    public ExternalStageId StageId;
    public GrParam GroundParams;

    // holds all collision data!
    public MapCollData Collision;
    // these can't be included in MapCollData because they're managed types
    public Vector2[] Vertices;
    public MapLine[] MapLines;
    public CollLineGroup[] CollGroups;
    public List<CollJoint> CollJoints;

    // bounding areas
    public BoundingRect BlastZone;
    public StageCameraInfo CameraInfo;

    public BoundingRect GetRealBlastZone() {
        return new BoundingRect() {
            Top = BlastZone.Top + CameraInfo.OffsetY,
            Bottom = BlastZone.Bottom + CameraInfo.OffsetY,
            Left = BlastZone.Left + CameraInfo.OffsetX,
            Right = BlastZone.Right + CameraInfo.OffsetX
        };
    }
    public BoundingRect GetRealCameraBounds() {
        return new BoundingRect() {
            Top = CameraInfo.CamBounds.Top + CameraInfo.OffsetY,
            Bottom = CameraInfo.CamBounds.Bottom + CameraInfo.OffsetY,
            Left = CameraInfo.CamBounds.Left + CameraInfo.OffsetX,
            Right = CameraInfo.CamBounds.Right + CameraInfo.OffsetX
        };
    }
}