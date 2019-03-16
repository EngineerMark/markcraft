using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Unity Mathematics is based on GLM, OpenGL Mathematics
// Created as base math helper functions for Markcraft
// Doesn't depend on anything but standard System namespaces and Unity classes for conversions

namespace UM
{
    public struct vec3
    {
        public float x;
        public float y;
        public float z;

        /// <summary>
        /// vec3 constructor with a single value that applies to all
        /// </summary>
        /// <param name="x"></param>
        public vec3(float x)
        {
            this.x = x;
            this.y = x;
            this.z = x;
        }

        /// <summary>
        /// vec3 constructor with all 3 values
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public vec3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// Converts the current object to Unity's Vector3
        /// </summary>
        /// <returns></returns>
        public Vector3 ToVector3()
        {
            return ToVector3(this);
        }

        /// <summary>
        /// Converts a custom vec3 to Unity's Vector3
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector3 ToVector3(vec3 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        /// <summary>
        /// Convert a Unity Vector3 to custom vec3
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static vec3 ToVec3(Vector3 v)
        {
            return new vec3(v.x, v.y, v.z);
        }

        /// <summary>
        /// Adds up 2 vectors and returns a new one
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static vec3 operator +(vec3 a, vec3 b)
        {
            return new vec3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        /// <summary>
        /// Substracts 2 vectors from eachother and returns a new vector
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static vec3 operator -(vec3 a, vec3 b)
        {
            return new vec3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        /// <summary>
        /// Negate a vector
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static vec3 operator -(vec3 v)
        {
            return new vec3() - v;
        }

        /// <summary>
        /// Multiplies 2 vectors into a new vector
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static vec3 operator *(vec3 a, vec3 b)
        {
            return new vec3(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        /// <summary>
        /// Divides 2 vectors and returns a new vector
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static vec3 operator /(vec3 a, vec3 b)
        {
            return new vec3(a.x / b.x, a.y / b.y, a.z / b.z);
        }
    }
}