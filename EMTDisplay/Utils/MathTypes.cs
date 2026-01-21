using ExternalMeleeTool.GameComponents;
using Microsoft.Xna.Framework;

namespace EMTDisplay.Utils; 

public struct LineSegment(Vector2 start, Vector2 end) {
    public Vector2 Start = start;
    public Vector2 End = end;

    public readonly bool Intersects(LineSegment other, out Vector2 position, out Vector2 normal) {
        float x1 = Start.X;
        float y1 = Start.Y;
        float x2 = End.X;
        float y2 = End.Y;
        float x3 = other.Start.X;
        float y3 = other.Start.Y;
        float x4 = other.End.X;
        float y4 = other.End.Y;
        float denom = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
        if (denom == 0) {
            position = Vector2.Zero;
            normal = Vector2.Zero;
            return false; // Parallel lines
        }
        float t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / denom;
        float u = -((x1 - x2) * (y1 - y3) - (y1 - y2) * (x1 - x3)) / denom;
        if (t >= 0 && t <= 1 && u >= 0 && u <= 1) {
            position = new Vector2(x1 + t * (x2 - x1), y1 + t * (y2 - y1));
            var dir = other.End - other.Start;
            normal = new Vector2(-dir.Y, dir.X);
            normal.Normalize();
            return true;
        }
        position = Vector2.Zero;
        normal = Vector2.Zero;
        return false;
    }

    public override readonly string ToString() => $"Start={Start}, End={End}";
}

// will be used upon hit of a fighter at some point
public readonly struct MovementPrediction(ref FighterData fighter) {
    public readonly FighterData Fighter = fighter;


}