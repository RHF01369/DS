using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class ByteConverter
{
    static ByteConverter()
    {
    }

    public static void FromString(string value, byte[] bytes, int startIndex)
    {
        byte[] strBytes = Encoding.UTF8.GetBytes(value);

        CopyToBytes(bytes, strBytes, ref startIndex);
    }

    public static void FromString(string value, byte[] bytes, ref int startIndex)
    {
        byte[] strBytes = Encoding.UTF8.GetBytes(value);

        CopyToBytes(bytes, strBytes, ref startIndex);
    }

    public static void FromInt(int value, byte[] bytes, int startIndex)
    {
        byte[] intBytes = BitConverter.GetBytes(value);

        CopyToBytes(bytes, intBytes, startIndex);
    }

    public static void FromInt(int value, byte[] bytes, ref int startIndex)
    {
        byte[] intBytes = BitConverter.GetBytes(value);

        CopyToBytes(bytes, intBytes, ref startIndex);
    }

    public static void FromFloat(float value, byte[] bytes, int startIndex)
    {
        byte[] floatBytes = BitConverter.GetBytes(value);

        CopyToBytes(bytes, floatBytes, startIndex);
    }

    public static void FromFloat(float value, byte[] bytes, ref int startIndex)
    {
        byte[] floatBytes = BitConverter.GetBytes(value);

        CopyToBytes(bytes, floatBytes, ref startIndex);
    }

    public static void FromBool(bool value, byte[] bytes, int startIndex)
    {
        byte[] boolByte = BitConverter.GetBytes(value);

        CopyToBytes(bytes, boolByte, startIndex);
    }

    public static void FromBool(bool value, byte[] bytes, ref int startIndex)
    {
        byte[] boolByte = BitConverter.GetBytes(value);

        CopyToBytes(bytes, boolByte, ref startIndex);
    }

    private static void CopyToBytes(byte[] toBytes, byte[] fromBytes, int startIndex)
    {
        for (int index = 0; index < fromBytes.Length; index++)
            toBytes[startIndex + index] = fromBytes[index];
    }

    private static void CopyToBytes(byte[] toBytes, byte[] fromBytes, ref int startIndex)
    {
        for (int index = 0; index < fromBytes.Length; index++)
            toBytes[startIndex + index] = fromBytes[index];

        startIndex += fromBytes.Length;
    }

    public static int ToInt(byte[] intBytes, int startIndex)
    {
        return BitConverter.ToInt32(intBytes, startIndex);
    }

    public static string ToString(byte[] strBytes, int startIndex, int count)
    {
        return Encoding.Default.GetString(strBytes, startIndex, count);
    }

    public static float ToFloat(byte[] floatBytes, int startIndex)
    {
        return BitConverter.ToSingle(floatBytes, startIndex);
    }

    public static bool ToBool(byte[] boolByte, int startIndex)
    {
        return BitConverter.ToBoolean(boolByte, startIndex);
    }
}