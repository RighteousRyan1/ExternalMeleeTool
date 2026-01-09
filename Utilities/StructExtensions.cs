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

            if (field.Name.Contains("_flags")) {
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
            builder.AppendLine($"[0x{offset:X3}] {name}: {value}");
            //builder.AppendLine($"[0x{offset:X}] {field.Name}: {value}");
        }

        return builder.ToString();
    }
}
