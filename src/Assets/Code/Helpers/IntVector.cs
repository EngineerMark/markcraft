using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct IntVector
{
    public int x;
}

public struct IntVector2
{
    public int x;
    public int y;

    public IntVector2(int x = 0, int y = 0)
    {
        this.x = x;
        this.y = y;
    }
}

public struct IntVector3
{
    public int x;
    public int y;
    public int z;

    public IntVector3(int x = 0, int y = 0, int z = 0){
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public IntVector3(float x = 0, float y = 0, float z = 0)
    {
        this.x = (int)x;
        this.y = (int)y;
        this.z = (int)z;
    }

    public static IntVector3 FromVector3(Vector3 v){
        return new IntVector3(v.x, v.y, v.z);
    }

    public static bool operator ==(IntVector3 a, IntVector3 b)
    {
        return (a.x == b.x && a.y == b.y && a.z == b.z);
    }

    public static bool operator !=(IntVector3 a, IntVector3 b)
    {
        return !(a.x == b.x && a.y == b.y && a.z == b.z);
    }
}
