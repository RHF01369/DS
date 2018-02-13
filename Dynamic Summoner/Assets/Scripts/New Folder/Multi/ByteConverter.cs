using System;
using System.Text;

public static class ByteConverter
{
    static ByteConverter()
    {
    }

    public static void FromString(string value, byte[] bytes, int startIndex)
    {
        byte[] strBytes = Encoding.UTF8.GetBytes(value);

        CopyToBytes(strBytes, bytes, startIndex);
    }

    public static void FromString(string value, byte[] bytes, ref int startIndex)
    {
        byte[] strBytes = Encoding.UTF8.GetBytes(value);

        CopyToBytes(strBytes, bytes, startIndex);

        startIndex = startIndex + 10;
    }

    public static void FromInt(int value, byte[] bytes, int startIndex)
    {
        byte[] intBytes = BitConverter.GetBytes(value);

        CopyToBytes(intBytes, bytes, startIndex);
    }

    public static void FromInt(int value, byte[] bytes, ref int startIndex)
    {
        byte[] intBytes = BitConverter.GetBytes(value);

        CopyToBytes(intBytes, bytes, ref startIndex);
    }

    public static void FromFloat(float value, byte[] bytes, int startIndex)
    {
        byte[] floatBytes = BitConverter.GetBytes(value);

        CopyToBytes(floatBytes, bytes, startIndex);
    }

    public static void FromFloat(float value, byte[] bytes, ref int startIndex)
    {
        byte[] floatBytes = BitConverter.GetBytes(value);

        CopyToBytes(floatBytes, bytes, ref startIndex);
    }

    public static void FromBool(bool value, byte[] bytes, int startIndex)
    {
        byte[] boolByte = BitConverter.GetBytes(value);

        CopyToBytes(boolByte, bytes, startIndex);
    }

    public static void FromBool(bool value, byte[] bytes, ref int startIndex)
    {
        byte[] boolByte = BitConverter.GetBytes(value);

        CopyToBytes(boolByte, bytes, ref startIndex);
    }

    private static void CopyToBytes(byte[] fromBytes, byte[] toBytes, int startIndex)
    {
        for (int index = 0; index < fromBytes.Length; index++)
            toBytes[startIndex + index] = fromBytes[index];
    }

    private static void CopyToBytes(byte[] fromBytes, byte[] toBytes, ref int startIndex)
    {
        for (int index = 0; index < fromBytes.Length; index++)
            toBytes[startIndex + index] = fromBytes[index];

        startIndex += fromBytes.Length;
    }


    public static int ToInt(byte[] intBytes, int startIndex)
    {
        return BitConverter.ToInt32(intBytes, startIndex);
    }

    public static int ToInt(byte[] intBytes, ref int startIndex)
    {
        int result = BitConverter.ToInt32(intBytes, startIndex);
        startIndex = startIndex + 4;
        return result;
    }

    public static string ToString(byte[] strBytes, int startIndex, int count)
    {
        return Encoding.Default.GetString(strBytes, startIndex, count);
    }

    public static string ToString(byte[] strBytes, ref int startIndex, int count)
    {
        string result = Encoding.Default.GetString(strBytes, startIndex, count);
        startIndex = startIndex + count;
        return result;
    }

    public static float ToFloat(byte[] floatBytes, int startIndex)
    {
        return BitConverter.ToSingle(floatBytes, startIndex);
    }

    public static float ToFloat(byte[] floatBytes, ref int startIndex)
    {
        float result = BitConverter.ToSingle(floatBytes, startIndex);
        startIndex = startIndex + 4;
        return result;
    }

    public static bool ToBool(byte[] boolByte, int startIndex)
    {
        return BitConverter.ToBoolean(boolByte, startIndex);
    }

    public static bool ToBool(byte[] boolByte, ref int startIndex)
    {
        bool result = BitConverter.ToBoolean(boolByte, startIndex);
        startIndex++;
        return result;
    }
}