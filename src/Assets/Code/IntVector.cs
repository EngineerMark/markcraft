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
}
