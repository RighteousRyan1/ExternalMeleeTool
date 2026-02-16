using System.Numerics;
using System.Runtime.InteropServices;

namespace ExternalMeleeTool.Melee.HSD;

/// <summary>
/// Data that represents a joint object.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public unsafe struct JObj {
    // /* +0 */ HSD_Obj obj;
    public fixed byte obj[8];
    /* +8 */
    public Ptr32 next; // Jobj, jobj, jobj
    /* +C */
    public Ptr32 parent; // linked parent
    /* +10 */
    public Ptr32 child; // linked child
    /* +14 */
    public JObjFlags flags; // u32
    /* +18 */
    /*union {
        HSD_SList* ptcl;
        struct HSD_DObj* dobj;
        HSD_Spline* spline;
    } u;*/
    // not always DObj! can be HSD_SList* or HSD_Spline. fuggin hell.
    public Ptr32 dobj;
    // public fixed byte union_unk_x18[4];
    /* +1C */
    public Quaternion rotate;
    /* +2C */
    public Vector3 scale;
    /* +38 */
    public Vector3 translate;
    /* +44 */
    public Matrix3x4 mtx;
    /* +74 */
    public Ptr32 scl; // Vector3*, why is this a thing? or a pointer?
    // /* +78 */ MtxPtr envelopemtx;
    public Ptr32 envelope; // wtf is even MtxPtr???
    // /* +7C */ HSD_AObj* aobj;
    public Ptr32 aobj;
    // /* +80 */ HSD_RObj* robj;
    public Ptr32 robj;
    /* +84 */
    public u32 id;
}

// ENUMS

[Flags]
public enum JObjFlags : uint {
    Skeleton = 1u << 0,
    SkeletonRoot = 1u << 1,
    EnvelopeModel = 1u << 2,
    ClassicalScale = 1u << 3,
    Hidden = 1u << 4,
    Ptcl = 1u << 5,
    MtxDirty = 1u << 6,
    Lighting = 1u << 7,
    Texgen = 1u << 8,

    Instance = 1u << 12,
    Spline = 1u << 14,
    FlipIk = 1u << 15,
    Specular = 1u << 16,
    UseQuaternion = 1u << 17,

    UnkB18 = 1u << 18,
    UnkB19 = 1u << 19,
    UnkB20 = 1u << 20,

    NullObj = 0u << 21,
    Joint1 = 1u << 21,
    Joint2 = 2u << 21,
    Joint = 3u << 21,
    Effector = 3u << 21,

    UserDefMtx = 1u << 23,
    MtxIndepParent = 1u << 24,
    MtxIndepSrt = 1u << 25,

    UnkB26 = 1u << 26,
    UnkB27 = 1u << 27,

    RootOpa = 1u << 28,
    RootXlu = 1u << 29,
    RootTexedge = 1u << 30,
}
