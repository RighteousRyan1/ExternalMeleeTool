using System.Numerics;

namespace ExternalMeleeTool.Melee; 

public struct GCInput {
    /*  fp+620 */
    public Vector2 LeftStick;
    /*  fp+628 */
    public Vector2 lstick1;
    /*  fp+630 */
    public float x630;
    /*  fp+634 */
    public float x634;
    /*  fp+638 */
    public Vector2 CStick;
    /*  fp+640 */
    public Vector2 cstick1;
    /*  fp+648 */
    public float x648;
    /*  fp+64C */
    public float x64C;
    /*  fp+650 */
    public float x650;
    /*  fp+654 */
    public float x654;
    /*  fp+658 */
    public float x658;
    /*  fp+65C */
    public HSD_Pad ButtonsHeld;
    /*  fp+660 */
    public HSD_Pad ButtonsPrevious; // < previous held inputs
    /*  fp+664 */
    public HSD_Pad x664;
    /*  fp+668 */
    public HSD_Pad ButtonsPressed; //<  pressed inputs
    /*  fp+66C */
    public HSD_Pad ButtonsReleased; // < released inputs
}
