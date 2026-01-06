namespace ExternalMeleeTool;

/// <summary>A static class that contains important pointers to melee's memory.</summary>
public static class MeleeConstants {
    // these are all offsets from GALE01!!!!
    public const long CAM_START = 0x453040;
    public const long CAM_TYPE = 0x452C6F;
    public const long PLAYER_ONE = 0x453080;
    public const long PLAYER_TWO = 0x453F10;
    public const long PLAYER_THREE = 0x454DA0;
    public const long PLAYER_FOUR = 0x455C30;
    public const long PAUSE_BIT = 0x479D68;

    public const long START_MELEE_RULES = 0x46DB68;

    public const long MINOR_SCENE = 0x479D30;
    public const long MAJOR_SCENE = 0x479D33;
}
/// <summary>A static class that contains important pointers to Slippi Netplay memory.</summary>
public static class SlippiConstants {
    // thanks, Altafen!
    public const long ONLINE_DATA_BLOCK = 0x4DB6A0 - 0x49E4;
}

public enum CameraKind : byte {
    Normal = 0x00,
    Develop = 0x08
}
public enum FighterMemorySlot : long {
    IndexOne = MeleeConstants.PLAYER_ONE,
    IndexTwo = MeleeConstants.PLAYER_TWO,
    IndexThree = MeleeConstants.PLAYER_THREE,
    IndexFour = MeleeConstants.PLAYER_FOUR
}

public enum SlotTeam {
    Red    = 0,
    Blue   = 1,
    Green  = 2
}
// unsure of the other kinds...
public enum SlotKind {
    Human  = 0x0,
    CPU    = 0x1,
    Demo   = 0x2,
    None   = 0x3,
    Boss   = 0x4
}
/*public enum FighterKind {
    Mario             = 0,
    Fox               = 1,
    CaptainFalcon     = 2,
    DonkeyKong        = 3,
    Kirby             = 4,
    Bowser            = 5,
    Link              = 6,
    Sheik             = 7,
    Ness              = 8,
    Peach             = 9,
    IceClimbers       = 10, // instead of Popo, use Ice Climbers since it's the name of the character slot
    Nana              = 11,
    Pikachu           = 12,
    Samus             = 13,
    Yoshi             = 14,
    Jigglypuff        = 15,
    Mewtwo            = 16,
    Luigi             = 17,
    Marth             = 18,
    Zelda             = 19,
    YoungLink         = 20,
    DrMario           = 21,
    Falco             = 22,
    Pichu             = 23,
    GameAndWatch      = 24,
    Ganondorf         = 25,
    Roy               = 26,
    CrazyHand         = 27,
    WireFrameMale     = 28,
    WireFrameFemale   = 29,
    GigaBowser        = 30,
    Sandbag           = 31
}*/

// hal smoking crack to make these different as per usual.
public enum CharacterKind
{
    Captain             = 0x00,
    Donkey              = 0x01,
    Fox                 = 0x02,
    GameWatch           = 0x03,
    Kirby               = 0x04,
    Koopa               = 0x05, // bowser
    Link                = 0x06,
    Luigi               = 0x07,
    Mario               = 0x08,
    Marth               = 0x09,
    Mewtwo              = 0x0A,
    Ness                = 0x0B,
    Peach               = 0x0C,
    Pikachu             = 0x0D,
    PopoNana            = 0x0E,
    Jigglypuff          = 0x0F, // but not purin?? hal???
    Samus               = 0x10,
    Yoshi               = 0x11,
    Zelda               = 0x12,
    Sheik               = 0x13,
    Falco               = 0x14,
    YoungLink           = 0x15,
    DrMario             = 0x16,
    Roy                 = 0x17,
    Pichu               = 0x18,
    Ganondorf           = 0x19,

    PlayableCount       = 0x1A,

    MasterHand          = PlayableCount,
    WireframeMale       = 0x1B,
    WireframeFemale     = 0x1C,
    GigaBowser          = 0x1D,
    CrazyHand           = 0x1E,
    Sandbag             = 0x1F,
    Popo                = 0x20,

    None                = 0x21,
    Max                 = None
}

public enum ExternalStageId {
    DUMMY = 0,
    TEST = 1,
    IZUMI = 2, // FoD
    PSTADIUM = 3,
    CASTLE = 4,
    KONGO = 5,
    ZEBES = 6,
    CORNERIA = 7,
    STORY = 8,
    ONETT = 9,
    MUTECITY = 10,
    RCRUISE = 11,
    GARDEN = 12,
    GREATBAY = 13,
    SHRINE = 14, // Temple
    KRAID = 15, // Depths
    YOSTER = 16, // Yoshi's Island
    GREENS = 17,
    FOURSIDE = 18,
    INISHIE1 = 19, // Kingdom 1
    INISHIE2 = 20, // Kingdom 2
    AKANEIA = 21,  // debug only?
    VENOM = 22,
    PURA = 23, // Poke Floats
    BIGBLUE = 24,
    ICEMT = 25, // Ice Mountain
    ICETOP = 26, // debug only?
    FLATZONE = 27,
    OLD_PPP = 28, // Dreamland 64
    OLD_YOSH = 29, //
    OLD_KONG = 30,
    BATTLE = 31,
    LAST = 32,

    // T = Training, plus character name
    TMARIO = 33,
    TCAPTAIN = 34,
    TCLINK = 35,
    TDONKEY = 36,
    TDRMARIO = 37,
    TFALCO = 38,
    TFOX = 39,
    TICECLIM = 40,
    TKIRBY = 41,
    TKOOPA = 42,
    TLINK = 43,
    TLUIGI = 44,
    TMARS = 45,
    TMEWTWO = 46,
    TNESS = 47,
    TPEACH = 48,
    TPICHU = 49,
    TPIKACHU = 50,
    TPURIN = 51,
    TSAMUS = 52,
    TSEAK = 53,
    TYOSHI = 54,
    TZELDA = 55,
    TGAMEWAT = 56,
    TEMBLEM = 57,
    TGANON = 58,

    _1_1KINOKO = 59, // Adventure Kingdom
    _1_2CASTLE = 60,
    _2_1KONGO = 61,
    _2_2GARDEN = 62,
    _3_1MEIKYU = 63, // Underground Maze
    _3_2SHRINE = 64,
    _4_1ZEBES = 65,
    _4_2DASSYUT = 66, // Brinstar Escape
    _5_1GREENS = 67,
    _5_2GREENS = 68,
    _5_3GREENS = 69,
    _6_1CORNERI = 70,
    _6_2CORNERI = 71,
    _7PSTADIUM = 72,
    _8_1BBROUTE = 73, // F-Zero GP
    _8_2MUTECIT = 74,
    _9_1ONETT = 75,
    _10_1ICEMT = 76,
    _10_2 = 77,
    _11_1BATTLE = 78,
    _11_2BATTLE = 79,
    _12_1LAST = 80,
    _12_2LAST = 81,

    TUKISUSUME = 82, // Race to finish
    FIGUREGET = 83, // trophies
    HOMERUN = 84,
    HEAL = 85 // All-Star rest
}


