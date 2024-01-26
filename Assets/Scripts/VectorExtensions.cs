﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorExtensions
{
    public static Vector3 x0z(this Vector3 inV)
    {
        return new Vector3(inV.x, 0.0f, inV.z);
    }

    public static Vector2 xz(this Vector3 inV)
    {
        return new Vector2(inV.x, inV.z);
    }

    public static Vector2 xy(this Vector3 inV)
    {
        return new Vector2(inV.x, inV.y);
    }

    public static Vector3 xy0(this Vector2 inV)
    {
        return new Vector3(inV.x, inV.y, 0);
    }

    public static Vector2 yz(this Vector3 inV)
    {
        return new Vector2(inV.y, inV.z);
    }

    public static Vector2 zx(this Vector3 inV)
    {
        return new Vector2(inV.z, inV.x);
    }

    public static Vector2 yx(this Vector3 inV)
    {
        return new Vector2(inV.y, inV.x);
    }

    public static Vector2 zy(this Vector3 inV)
    {
        return new Vector2(inV.z, inV.y);
    }

    public static Vector3 xyz(this Vector4 v)
    {
        return new Vector3(v.x, v.y, v.z);
    }

    public static Vector4 xyz0(this Vector3 v)
    {
        return new Vector4(v.x, v.y, v.z, 0);
    }

    public static Vector4 xyz1(this Vector3 v)
    {
        return new Vector4(v.x, v.y, v.z, 1);
    }
};

