using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EG_RopeTest2 : MonoBehaviour
{
   


    public static void ContstrainLengthMin(ref Vector2 p1, ref Vector2 p2, float minDistance, float compensate1 = 0.5f, float compensate2 = 0.5f)
    {
        Vector2 delta = p2 - p1;
        float deltaLength = delta.magnitude;
        if (deltaLength > 0 && deltaLength < minDistance)
        {
            float diff = (deltaLength - minDistance) / deltaLength;
            p1 += delta * compensate1 * diff;
            p2 -= delta * compensate2 * diff;
        }
    }

    public static void ContstrainLengthMax(ref Vector2 p1, ref Vector2 p2, float maxDistance, float compensate1 = 0.5f, float compensate2 = 0.5f)
    {
        Vector2 delta = p2 - p1;
        float deltaLength = delta.magnitude;
        if (deltaLength > maxDistance)
        {
            float diff = (deltaLength - maxDistance) / deltaLength;
            p1 += delta * compensate1 * diff;
            p2 -= delta * compensate2 * diff;
        }
    }
}
