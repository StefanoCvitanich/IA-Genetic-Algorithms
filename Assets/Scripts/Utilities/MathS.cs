using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IA
{
    // Custom math class
    public struct MathS
    {
        public static float AsyntotalGrowth(float x, float asyntote, float curvature = 5)
        {
            return -(curvature / (x + (curvature / asyntote))) + asyntote;
        }

        public static float ExponentialGrowth(float x, float innerAmplitude = 1, float outerAmplitude = 1)
        {
            return ((x * x) / innerAmplitude) * outerAmplitude;
        }

        public static float LinearGrowth(float x, float lowerAngle = 1, float upperAngle = 1)
        {
            return (x * lowerAngle) / upperAngle;
        }

        public static Vector3 GetIntersectionPoint(Vector3 targetPosition, Vector3 targetVelocity, Vector3 myPosition, float mySpeed)
        {
            Vector3 totarget = targetPosition - myPosition;

            float a = Vector3.Dot(targetVelocity, targetVelocity) - (mySpeed * mySpeed);
            float b = 2 * Vector3.Dot(targetVelocity, totarget);
            float c = Vector3.Dot(totarget, totarget);

            float p = -b / (2 * a);
            float q = (float)Mathf.Sqrt((b * b) - 4 * a * c) / (2 * a);

            float t1 = p - q;
            float t2 = p + q;
            float t;

            if (t1 > t2 && t2 > 0)
            {
                t = t2;
            }
            else
            {
                t = t1;
            }

            Vector3 aimSpot = targetPosition + targetVelocity * t;
            return aimSpot;
        }

        public static float Squish(float n, float max, float min)
        {
            return (n - min) / (max - min);
        }

        public static float InverseProportion(float val, float max)
        {
            if (val > max)
                val = max;
            return Mathf.Abs(val - max);
        }

        public static float Sigmoid(float x, float curve = 1)
        {
            return 1 / (1 + Mathf.Pow((float)System.Math.E, (-x / curve)));
        }
    }
}
