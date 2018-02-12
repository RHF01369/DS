//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public sealed class ReverseComparer<T> : IComparer<T>
//{
//    private readonly IComparer<T> original;

//    public ReverseComparer(IComparer<T> original)
//    {
//        this.original = original;
//    }

//    public int Compare(T left, T right)
//    {
//        return original.Compare(right, left);
//    }
//}