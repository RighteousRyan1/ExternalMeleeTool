using System.Buffers.Binary;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ExternalMeleeTool.Marshaling;
public static class EndiannessMarshaler {
    // Cache fields to avoid Reflection overhead every frame
    static readonly Dictionary<Type, FieldInfo[]> _fieldCache = [];

    public static void FixEndianness<T>(ref T obj) where T : struct {
        // Box the struct so we can modify it in place via Reflection
        object boxed = obj;
        FixEndiannessRecursive(boxed.GetType(), boxed);
        obj = (T)boxed;
    }

    static void FixEndiannessRecursive(Type type, object obj) {
        if (!_fieldCache.TryGetValue(type, out var fields)) {
            fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            _fieldCache[type] = fields;
        }

        foreach (var field in fields) {
            Type fieldType = field.FieldType;
            object value = field.GetValue(obj);

            // 1. Handle Enums (unwrap to underlying type)
            if (fieldType.IsEnum) {
                // We can't swap enums directly, we have to unbox, swap, rebox
                Type underlying = Enum.GetUnderlyingType(fieldType);
                object underlyingVal = Convert.ChangeType(value, underlying);
                SwapPrimitive(ref underlyingVal, underlying);
                field.SetValue(obj, Enum.ToObject(fieldType, underlyingVal));
                continue;
            }

            // 2. Handle Primitives
            if (fieldType.IsPrimitive) {
                SwapPrimitive(ref value, fieldType);
                field.SetValue(obj, value);
                continue;
            }

            // 3. Handle Structs (Recursive) -- e.g. Vector3, BoundingRect
            if (fieldType.IsValueType && !fieldType.IsPrimitive) {
                // Recursively fix the nested struct
                FixEndiannessRecursive(fieldType, value);
                field.SetValue(obj, value);
                continue;
            }

            // 4. Handle Arrays (Fixed Buffers or standard arrays)
            if (fieldType.IsArray) {
                Array arr = (Array)value;
                if (arr == null) continue;
                Type elemType = fieldType.GetElementType();

                // If array of primitives, swap them
                if (elemType.IsPrimitive) {
                    for (int i = 0; i < arr.Length; i++) {
                        object elem = arr.GetValue(i);
                        SwapPrimitive(ref elem, elemType);
                        arr.SetValue(elem, i);
                    }
                }
                // If array of structs, recurse
                else if (elemType.IsValueType) {
                    for (int i = 0; i < arr.Length; i++) {
                        object elem = arr.GetValue(i);
                        FixEndiannessRecursive(elemType, elem);
                        arr.SetValue(elem, i);
                    }
                }
            }
        }
    }

    static void SwapPrimitive(ref object value, Type type) {
        if (type == typeof(byte) || type == typeof(sbyte) || type == typeof(bool)) return;

        if (type == typeof(ushort)) { value = BinaryPrimitives.ReverseEndianness((ushort)value); return; }
        if (type == typeof(short)) { value = BinaryPrimitives.ReverseEndianness((short)value); return; }
        if (type == typeof(uint)) { value = BinaryPrimitives.ReverseEndianness((uint)value); return; }
        if (type == typeof(int)) { value = BinaryPrimitives.ReverseEndianness((int)value); return; }
        if (type == typeof(ulong)) { value = BinaryPrimitives.ReverseEndianness((ulong)value); return; }
        if (type == typeof(long)) { value = BinaryPrimitives.ReverseEndianness((long)value); return; }

        // Floats need bit-casting to int/uint to swap
        if (type == typeof(float)) {
            int asInt = BitConverter.SingleToInt32Bits((float)value);
            asInt = BinaryPrimitives.ReverseEndianness(asInt);
            value = BitConverter.Int32BitsToSingle(asInt);
            return;
        }
        if (type == typeof(double)) {
            long asLong = BitConverter.DoubleToInt64Bits((double)value);
            asLong = BinaryPrimitives.ReverseEndianness(asLong);
            value = BitConverter.Int64BitsToDouble(asLong);
            return;
        }
    }
}