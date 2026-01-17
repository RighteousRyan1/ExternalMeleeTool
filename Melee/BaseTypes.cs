using System.Numerics;
using System.Runtime.InteropServices;

namespace ExternalMeleeTool.Melee;


// holy shit
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public unsafe struct HSD_JObj {
    // /* +0 */ HSD_Obj obj;
    public fixed byte obj[8];
    /* +8 */ public Ptr32 next; // Jobj, jobj, jobj
    /* +C */ public Ptr32 parent; // linked parent
    /* +10 */ public Ptr32 child; // linked child
    /* +14 */ public u32 flags;
    /* +18 */
    /*union {
        HSD_SList* ptcl;
        struct HSD_DObj* dobj;
        HSD_Spline* spline;
    } u;*/
    public fixed byte union_unk_x18[4];
    /* +1C */ public Quaternion rotate;
    /* +2C */ public Vector3 scale;
    /* +38 */ public Vector3 translate;
    /* +44 */ public Matrix3x4 mtx;
    /* +74 */ public Ptr32 scl; // Vector3*
    // /* +78 */ MtxPtr envelopemtx;
    public Ptr32 envelopemtx; // wtf is even MtxPtr???
    // /* +7C */ HSD_AObj* aobj;
    public Ptr32 aobj;
    // /* +80 */ HSD_RObj* robj;
    public Ptr32 robj;
    /* +84 */ 
    public u32 id;
};
