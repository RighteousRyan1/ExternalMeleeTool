using System.Numerics;

namespace ExternalMeleeTool.Melee; 

/// <summary>
/// Input on the game level. Changing these will not modify raw input.
/// </summary>
public struct FighterInput {
    /*  fp+620 */
    public Vector2 LeftStick;
    /*  fp+628 */
    public Vector2 LeftStickPrev;
    /*  fp+630 */
    public float x630;
    /*  fp+634 */
    public float x634;
    /*  fp+638 */
    public Vector2 CStick;
    /*  fp+640 */
    public Vector2 CStickPrev;
    /*  fp+648 */
    public float x648;
    /*  fp+64C */
    public float x64C;
    /*  fp+650 */
    public float Triggers;
    /*  fp+654 */
    public float TriggersPrev;
    /*  fp+658 */
    public float x658;
    /*  fp+65C */
    public HSDPadButton Held;
    /*  fp+660 */
    public HSDPadButton Prev; // < previous held inputs
    /*  fp+664 */
    // preserves inputs from previous frame into pause...? works for me!
    public HSDPadButton Preserved;
    /*  fp+668 */
    // originally HSD_Pad
    public HSDPadButton Pressed; //<  pressed inputs
    /*  fp+66C */
    public HSDPadButton Released; // < released inputs
}