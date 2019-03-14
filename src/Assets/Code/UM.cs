using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Unity Mathematics is based on GLM, OpenGL Mathematics
// Created as base math helper functions for Markcraft
// Doesn't depend on anything but standard System namespaces and Unity classes for conversions

namespace UM{
    public struct vec3{
        public float x;
        public float y;
        public float z;

        public vec3(float x){
            this.x = x;
            this.y = x;
            this.z = x;
        }

        public vec3(float x,float y,float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3 ToVector3(){
            return ToVector3(this);
        }

        public static Vector3 ToVector3(vec3 v){
            return new Vector3(v.x, v.y, v.z);
        }

        public static vec3 ToVec3(Vector3 v){
            return new vec3(v.x, v.y, v.z);
        }
    }
}