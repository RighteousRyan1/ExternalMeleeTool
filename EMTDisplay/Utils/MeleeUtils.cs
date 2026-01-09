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
        [CollisionType.Bottom] = Color.PaleVioletRed,
        [CollisionType.Right] = Color.LimeGreen,
        [CollisionType.Left] = Color.LightBlue
    };

    public static readonly Dictionary<InteractType, Color> InteractTypeToColor = new() {
        [InteractType.None] = Color.Gray,
        [InteractType.DropThrough] = Color.DarkRed,
        [InteractType.LedgeGrab] = Color.ForestGreen,
        [InteractType.Unknown] = Color.Purple
    };
}
