using System.Numerics;
using System;
using System.Globalization;

namespace EMTDisplay.Utils; 
public static class Parsing {
    public static bool TryParse(string text, out Vector3 result) {
        result = default;

        if (string.IsNullOrWhiteSpace(text))
            return false;

        text = text.Trim();

        // common wrappers
        if ((text.StartsWith('(') && text.EndsWith(')')) ||
            (text.StartsWith('<') && text.EndsWith('>'))) {
            text = text[1..^1];
        }

        // common splitters
        var parts = text.Split([',', ' '], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3)
            return false;

        if (!float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x))
            return false;
        if (!float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y))
            return false;
        if (!float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
            return false;

        result = new Vector3(x, y, z);
        return true;
    }
    public static bool TryParse(string text, out Vector2 result) {
        result = default;

        if (string.IsNullOrWhiteSpace(text))
            return false;

        text = text.Trim();

        // common wrappers
        if ((text.StartsWith('(') && text.EndsWith(')')) ||
            (text.StartsWith('<') && text.EndsWith('>'))) {
            text = text[1..^1];
        }

        // common splitters
        var parts = text.Split([',', ' '], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
            return false;

        if (!float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x))
            return false;
        if (!float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y))
            return false;

        result = new Vector2(x, y);
        return true;
    }
}
