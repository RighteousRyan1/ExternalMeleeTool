using ExternalMeleeTool.Melee.Collision;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace EMTDisplay.Utils; 
public static class MeleeDisplayUtils {
    public static readonly Dictionary<MaterialType, Color> MatTypeToColor = new() {
        [MaterialType.Basic] = Color.Gray,
        [MaterialType.Rock] = Color.DarkGray,
        [MaterialType.Grass] = Color.Green,
        [MaterialType.Dirt] = new Color(139, 69, 19),
        [MaterialType.Wood] = new Color(160, 82, 45),
        [MaterialType.LightMetal] = Color.Silver,
        [MaterialType.HeavyMetal] = Color.DarkSlateGray,
        [MaterialType.Cloth] = Color.Beige,
        [MaterialType.AlienGoop] = Color.Purple,
        [MaterialType.Felt] = Color.Red,
        [MaterialType.Water] = Color.Blue,
        [MaterialType.Unknown11] = Color.Magenta,
        [MaterialType.Glass] = new Color(135, 206, 235, 100),
        [MaterialType.TurtleShell] = new Color(34, 139, 34),
        [MaterialType.Snow] = Color.White,
        [MaterialType.Ice] = Color.Cyan,
        [MaterialType.FlatZone] = Color.LightGray,
        [MaterialType.Swamp] = new Color(0, 100, 0),
        [MaterialType.Cardboard] = new Color(210, 180, 140)
    };
    public static readonly Dictionary<CollisionType, Color> CollTypeToColor = new() {
        [CollisionType.Top] = Color.Gray,
        [CollisionType.Bottom] = Color.Red,
        [CollisionType.Right] = Color.LimeGreen,
        [CollisionType.Left] = Color.Blue,
        [CollisionType.Disabled] = Color.Gold
    };

    public static readonly Dictionary<InteractType, Color> InteractTypeToColor = new() {
        [InteractType.None] = Color.Gray,
        [InteractType.DropThrough] = Color.DarkRed,
        [InteractType.LedgeGrab] = Color.ForestGreen,
        [InteractType.Unknown] = Color.Purple
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
