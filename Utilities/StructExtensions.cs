using ExternalMeleeTool.Marshaling;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace ExternalMeleeTool.Utilities; 
public static class StructExtensions {
    public static unsafe string FieldsToString<T>(this T obj) where T : struct {
        var builder = new StringBuilder();
        var type = typeof(T);

        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        int maxNameLen = fields.Max(f => f.Name.Length);

        foreach (var field in fields) {
            object value = field.GetValue(obj);
            var offset = (int)Marshal.OffsetOf<T>(field.Name);

            if (field.Name.Contains("flags")) {
                switch (value) {
                    case u8 u:
                        value = Convert.ToString(u, 2);
                        break;
                    case u16 s:
                        value = Convert.ToString(s, 2);
                        break;
                    case u32 i:
                        value = Convert.ToString(i, 2);
                        break;

                    case s8 sb:
                        value = Convert.ToString(sb, 2);
                        break;
                    case s16 ss:
                        value = Convert.ToString(ss, 2);
                        break;
                    case s32 si:
                        value = Convert.ToString(si, 2);
                        break;
                }
            }
            string name = field.Name.PadRight(maxNameLen);
            builder.AppendLine($"[0x{offset:X3}] {name}: {value} [{field.FieldType.Name}]");
            //builder.AppendLine($"[0x{offset:X}] {field.Name}: {value}");
        }

        return builder.ToString();
    }
    /// <summary>
    /// Casts a location in Big-Endian GameCube memory to Small-Endian memory and reads it as <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The struct to read from memory</typeparam>
    /// <param name="ptr">The location in GameCube memory.</param>
    /// <returns>The struct read.</returns>
    public static T As<[DynamicallyAccessedMembers(EndiannessMarshaler.naot_safety)] T>(this Ptr32 ptr) where T : unmanaged {
        return Dolphinterop.Read<T>(ptr);
    }
}
