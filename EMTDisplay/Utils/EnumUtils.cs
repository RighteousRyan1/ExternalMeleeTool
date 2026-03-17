using System;

namespace EMTDisplay.Utils; 
public static class EnumUtils {
    public static bool TryGetEnumValue<TEnum>(string name, out TEnum value, bool ignoreCase = true) where TEnum : struct, Enum {
        if (Enum.TryParse(name, ignoreCase, out TEnum result)) {
            value = result;
            return true;
        }

        value = default;
        return false;
    }

    public static void PrintAll<TEnum>(string separator = null) where TEnum : struct, Enum {
        var names = Enum.GetNames<TEnum>();

        for (int i = 0; i < names.Length; i++) {
            Console.Write(names[i] + separator);
        }
    }
}
