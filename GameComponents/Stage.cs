using ExternalMeleeTool.Melee;
using ExternalMeleeTool.Melee.Collision;
using ExternalMeleeTool.Melee.HSD;
using ExternalMeleeTool.Utilities;
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

    // move back to this later
    public static StageData GetStageData() {
        const uint stinfo = MeleeGlobals.STAGE_INFO;
        Ptr32 coll_data_ptr = Dolphinterop.ReadPtr(stinfo + 0x6AC);
        // read as a sanity check first and foremost to prevent bad data reads
        int vertCount = Dolphinterop.ReadS32(coll_data_ptr + 0x4);

        // i feel like in most cases if data isnt initialized and it reads garbage
        // it won't be near 0 or below 1000, so check it
        if (vertCount <= 0 || vertCount >= 1000)
            return default;

        var coll = Dolphinterop.Read<MapCollData>(coll_data_ptr); // joint_count being vertex joints? so one line = 2 joints?

        // NAOT sanity check?
        // update: NAOT always makes vert_count garbo data. why tf?
        if (coll.vert_count > 1000)
            return default;

        Ptr32 grParam_ptr = Dolphinterop.ReadPtr(stinfo + 0x6B0);
        var grParams = Dolphinterop.Read<GrParam>(grParam_ptr);

        var verts = new Vector2[coll.vert_count];
        var lines = new MapLine[coll.line_count];
        var coll_groups = new CollLineGroup[coll.coll_group_count];

        // vert_count is seemingly garbage data when using NAOT.... what?
        for (int i = 0; i < coll.vert_count; i++) {
            // subtract rom size cuz the pointers are in location respecting the entire system memory
            verts[i] = Dolphinterop.Read<Vector2>(coll.verts + (i * 8)); // 8 bytes per Vector2 (x, y)
        }
        for (int i = 0; i < coll.line_count; i++) {
            lines[i] = Dolphinterop.Read<MapLine>(coll.lines + (i * MapLine.SIZE));
        }

        // why is this just giving me a struct full of zeros?
        /* for reference, in GrSt:
         * coll_groups length = 2
         * coll_groups[0] = randall's CollLineGroup
         * coll_groups[1] = the regular stage's CollLineGroup
         */
        for (int i = 0; i < coll.coll_group_count; i++) {
            coll_groups[i] = Dolphinterop.Read<CollLineGroup>(coll.joints + (i * CollLineGroup.SIZE));
        }

        var collJointHeadPtr = Dolphinterop.ReadPtr(MeleeGlobals.MAP_COLL_JOINT_HEAD);

        List<CollJoint> collJoints = [];
        var curCollJointPtr = collJointHeadPtr;

        // linked list traversal...
        // does the head have zero important data???
        // TODO: get working. fails

        do {
            var curCollJoint = Dolphinterop.Read<CollJoint>(curCollJointPtr);

            // this jobj describes the joint that moves the coll group
            var jobj = Dolphinterop.Read<JObj>(curCollJoint.jobj);
            var coll_group = Dolphinterop.Read<CollLineGroup>(curCollJoint.inner);

            // var coll_group = coll_groups[collJoints.Count];

            // note to self: stages like PS use some garbage value for this translation when certain coll groups are de-loaded?
            var trans = jobj.mtx.Translation;

            collJoints.Add(curCollJoint);
            curCollJointPtr = curCollJoint.next;

            if (jobj.flags.HasFlag(JObjFlags.Hidden) /*|| jobj.flags.HasFlag(JObjFlags.NullObj)*/) {
                for (int i = coll_group.vtx_start; i < coll_group.vtx_start + coll_group.vtx_count; i++) {
                    // bootleg hiding lol
                    verts[i] = new Vec2(1000000, 1000000);
                }
                continue;
            }

            // something is wrong with stagedata if this hits
            if (coll_group.vtx_start < 0 || coll_group.vtx_start >= verts.Length) continue;

            // note: rotations are done around the bone (translation?)
            for (int i = coll_group.vtx_start; i < coll_group.vtx_start + coll_group.vtx_count; i++) {
                if (jobj.scale.Length() > 0)
                    verts[i] *= new Vec2(jobj.scale.X, jobj.scale.Y);
                verts[i] = new Vec2(verts[i].X + trans.X / grParams.StageScale, verts[i].Y + trans.Y / grParams.StageScale);
                // verts[i] += new Vec2(jobj.translate.X, jobj.translate.Y);
                // var scl = Read<Vec3>(jobj.scl);
                // * new Vec2(scl.X, scl.Y);

                float angle = 2f * (float)Math.Acos(jobj.mtx.Rotation.W); // full rotation angle

                if (float.IsNaN(angle)) continue;

                // determine the sign based on Z component of quaternion
                if (jobj.mtx.Rotation.Z < 0) angle = -angle;

                verts[i] = verts[i].Rotate(-angle, new Vector2(trans.X, trans.Y));
            }
        }
        while (curCollJointPtr != 0);

        var data = new StageData {
            StageId = (ExternalStageId)Dolphinterop.ReadU16(MeleeGlobals.START_MELEE_RULES + 0xE),
            // Scale = stageScale,
            GroundParams = grParams,
            BlastZone = Dolphinterop.Read<BoundingRect>(stinfo + 0x74), //ReadBoundingRect(stinfo + 0x74),
            // 0x0 = camerainfo
            CameraInfo = Dolphinterop.Read<StageCameraInfo>(stinfo),
            MapLines = lines,
            Vertices = verts,
            Collision = coll,
            CollJoints = collJoints,
            CollGroups = coll_groups
            // MapJoints = joints
        };

        return data;
    }
}