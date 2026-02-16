namespace ExternalMeleeTool.Melee.HSD;

public unsafe struct MObj {
    fixed byte parent[4];
    public u32 rendermode;
    public Ptr32 tobj;
    public Ptr32 hsd_mat;
    public Ptr32 pe_desc; // rasterization?
    public Ptr32 aobj;
    public Struct_t tevdesc;
    public Struct_t texp;
}