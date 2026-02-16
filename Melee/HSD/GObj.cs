using System.Runtime.InteropServices;

namespace ExternalMeleeTool.Melee.HSD;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct HSD_Obj {
    public Struct_t parent; // regularly a HSD_Class, which holds a pointer to a pointer toa pointer../. fdskmjnkasfd koljafsd nkjasdf 
    public u16 ref_count;
    public u16 ref_count_individual;

    public override readonly string ToString() => $"parent={parent}, ref={ref_count}, ref_i={ref_count_individual}";
};

/// <summary>
/// Data that describes a general/game object.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct GObj {
    /*  +0 */
    public u16 classifier;
    /*  +2 */
    public PLink p_link;
    /*  +3 */
    public u8 gx_link;
    /*  +4 */
    public u8 p_priority;
    /*  +5 */
    public u8 render_priority;
    /*  +6 */
    public u8 obj_kind;
    /*  +7 */
    public u8 user_data_kind;
    /*  +8 */
    public GObj_t next; // next 4 are gobj*
    /*  +C */
    public GObj_t prev;
    /* +10 */
    public GObj_t next_gx;
    /* +14 */
    public GObj_t prev_gx;
    /* +18 */
    public Ptr32 proc; // HSD_GObjProc*... whatever that is
    /* +1C */
    public Callback32 render_cb; // GObj_RenderFunc... also whatever that is
    /* +20 */
    public u64 gxlink_prios;
    /* +28 */
    public Ptr32 hsd_obj; // void*... what XObj it is?
    /* +2C */
    public Ptr32 user_data;
    /* +30 */
    // void (*user_data_remove_func) (void* data);
    public Func_t user_data_remove_func; // function pointer w/ func pointer func(void* data)
    /* +34 */
    public UNK_T x34_unk; // void*
}

// ENUMS
public enum UserdataKind : u8 {
    // to be filled out later
    Fighter = 4 // ... what's the point of this when plink exists?
}

public enum PLink : u8 {
    SYS,
    PLINK_1,
    PLINK_2,
    LIGHT,
    ZAKO,
    MAP,
    COLL,
    PLINK_7,
    FIGHTER,
    ITEM,
    PLINK_10,
    EFFECT1,
    EFFECT2,
    MAPMISC,
    MISC,
    HUD,
    PLINK_16,
    PLINK_17,
    MATCHCAM,
    MISCCAM,
    HUDCAM,
    COINCAM,
    SCREENFLASHCAM,
    CROWDSFX,
    DEVTEXT,
}