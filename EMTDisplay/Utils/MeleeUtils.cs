using ExternalMeleeTool.Melee.Collision;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace EMTDisplay.Utils; 
public static class MeleeDisplayUtils {
    public static readonly Dictionary<CollMaterial, Color> MatTypeToColor = new() {
        [CollMaterial.Basic] = Color.Gray,
        [CollMaterial.Rock] = Color.DarkGray,
        [CollMaterial.Grass] = Color.Green,
        [CollMaterial.Dirt] = new Color(139, 69, 19),
        [CollMaterial.Wood] = new Color(160, 82, 45),
        [CollMaterial.LightMetal] = Color.Silver,
        [CollMaterial.HeavyMetal] = Color.DarkSlateGray,
        [CollMaterial.Cloth] = Color.Beige,
        [CollMaterial.AlienGoop] = Color.Purple,
        [CollMaterial.Felt] = Color.Red,
        [CollMaterial.Water] = Color.Blue,
        [CollMaterial.Unknown11] = Color.Magenta,
        [CollMaterial.Glass] = new Color(135, 206, 235, 100),
        [CollMaterial.TurtleShell] = new Color(34, 139, 34),
        [CollMaterial.Snow] = Color.White,
        [CollMaterial.Ice] = Color.Cyan,
        [CollMaterial.FlatZone] = Color.LightGray,
        [CollMaterial.Swamp] = new Color(0, 100, 0),
        [CollMaterial.Cardboard] = new Color(210, 180, 140)
    };
    public static readonly Dictionary<CollKind, Color> CollKindToColor = new() {
        [CollKind.Top] = Color.Gray,
        [CollKind.Bottom] = Color.Red,
        [CollKind.Right] = Color.LimeGreen,
        [CollKind.Left] = Color.Blue,
        [CollKind.Disabled] = Color.Gold
    };

    public static readonly Dictionary<CollProperty, Color> CollPropertyToColor = new() {
        [CollProperty.None] = Color.Gray,
        [CollProperty.DropThrough] = Color.DarkRed,
        [CollProperty.LedgeGrab] = Color.ForestGreen,
        [CollProperty.Unknown] = Color.Purple
    };

    public static readonly Dictionary<HitElement, Color> HitElementToColor = new() {
        // refer to FighterColl.cs for enum values
        [HitElement.Normal] = Color.IndianRed,
        [HitElement.Fire] = Color.OrangeRed,
        [HitElement.Electric] = Color.Yellow,
        [HitElement.Ice] = Color.Cyan,
        [HitElement.Slash] = Color.LightBlue,
        [HitElement.Coin] = Color.Gold,
        [HitElement.Catch] = Color.LightGreen,
        [HitElement.Dark] = Color.Purple,
        [HitElement.Sleep103] = Color.MediumSlateBlue,
        [HitElement.Sleep412] = Color.CornflowerBlue,
        [HitElement.Inert] = Color.Gray,
        [HitElement.Cape] = Color.White,
        [HitElement.Screw] = Color.DarkOrange,
        [HitElement.Ground] = Color.SaddleBrown,
        [HitElement.Disable] = Color.DarkSeaGreen,
        [HitElement.Lipstick] = Color.HotPink
    };

    public static readonly Dictionary<HurtCapsuleState, Color> HurtCapsuleStateToColor = new() {
        [HurtCapsuleState.Disabled] = Color.DarkGray,
        [HurtCapsuleState.Enabled] = Color.DeepSkyBlue,
        [HurtCapsuleState.Intangible] = Color.LightGreen
    };
}
