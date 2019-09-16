using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Unity Mathematics is based on GLM, OpenGL Mathematics
// Created as base math helper functions for Markcraft
// Doesn't depend on anything but standard System namespaces and Unity classes for conversions

namespace UM
{

    public struct sphere
    {

        public float radius;
        public vec3 position;

        public sphere(float radius, vec3 position)
        {
            this.radius = radius;
            this.position = position;
        }

        public static bool IsInSphere(sphere s, vec3 pos){
            return s.IsInSphere(pos);
        }

        public bool IsInSphere(vec3 p)
        {
            vec3 _p = p.Abs();

            float n = (float)(Math.Pow((double)(position.x - _p.x), 2)+ Math.Pow((double)(position.y - _p.y), 2)+ Math.Pow((double)(position.z - _p.z), 2));
            if (n < (double)Math.Pow((double)radius, 2))
                return true;
            return false;
        }
    }

    public struct vec2
    {
        public float x;
        public float y;

        public vec2(float x)
        {
            this.x = x;
            y = x;
        }

        public vec2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

    }

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
        /// Returns every variable as an absolute unit (inverts it if it is a negative value)
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static vec3 Abs(vec3 v)
        {
            return new vec3(System.Math.Abs(v.x), System.Math.Abs(v.y), System.Math.Abs(v.z));
        }

        /// <summary>
        /// Returns every variable as an absolute unit (inverts it if it is a negative value)
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public vec3 Abs(){
            return vec3.Abs(this);
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