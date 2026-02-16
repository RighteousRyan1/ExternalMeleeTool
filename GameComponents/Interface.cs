using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ExternalMeleeTool.GameComponents;

// located in ifall.c
/// <summary>
/// A particular instance of this struct exists at 0x804A0FD8.
/// </summary>
public struct StockHUD {
    public GObj_t gobj;
    public GObj_t gobj_2;
    public JObj_t jobj;

    public Vec3 xC;

    // weirdly... these are only read from when a fighter respawns
    public Vec3 stock_icon_pos1; // prolly positions of each fighter's hud
    public Vec3 stock_icon_pos2;
    public Vec3 stock_icon_pos3;
    public Vec3 stock_icon_pos4;
    public Vec3 stock_icon_pos5;
    public Vec3 stock_icon_pos6;

    public Vec3 x60_1;
    public Vec3 x60_2;
    public Vec3 x60_3;

    public Vec3 x84_1;
    public Vec3 x84_2;
}

// 0x74
public struct OffscreenBubbleTable {
    public Ptr32 joint_table; // HSD_Joint*. THIS IS NOT THE SAME AS JOBJ
    // these could just be pointers, but i doubt it given the values they are
    public uint x4;
    public uint x8;
    public uint xC;
    public uint x10; // <-- this is certainly a pointer.
    // ^ = 0x14

    // size 10 * 6 = 0x60
    public OffscreenBubbleDataBuffer6 bubbles;

    [InlineArray(6)]
    public struct OffscreenBubbleDataBuffer6 { OffscreenBubbleData _inst; public const uint LENGTH = 0x6; }
}
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public unsafe struct OffscreenBubbleData {
    public GObj_t gobj;
    public Ptr32 jobj; // decomp says TObj*. ghidra db says JObj*. i'm convinced it's a jobj ?????????
    public Ptr32 imagedesc;
    public OffscreenBubbleFlags flags; // decomp says this is split into 1, 1, 6.
    // first bit is "is_offscreen", second is "ignore_offscreen", rest is unknown

    // this entire padding was completely absent in the altimor db
    fixed byte pad[0xf0 - 0x74];

    // fixed byte pad[0x3];

    public const uint SIZE = 0x10;
}

[Flags]
public enum OffscreenBubbleFlags : u8 {
    // unknown!
    None = 1 << 0,
    IsOffscreen = 1 << 1,
    IgnoreOffscreen = 1 << 2,

    // rest are all for one state
    Unk3 = 1 << 3,
    Unk4 = 1 << 4,
    Unk5 = 1 << 5,
    Unk6 = 1 << 6,
    Unk7 = 1 << 7,
}