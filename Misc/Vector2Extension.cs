using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector2Extension
{

    public static Vector2 Rotate(this Vector2 v, float degrees)
    {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

        float tx = v.x;
        float ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);
        return v;
    }

    public static float DirectionAngle(Vector2 dir)
    {
        Vector2 direction = dir.normalized;
        //return angle of vector that is specifically a direction
        if (direction.y < 0)
        {
            return 360 - Vector2.Angle(direction, Vector2.right); ;
        }
        else
        {
            return Vector2.Angle(direction, Vector2.right);
        }
    }

    public static float DirectionAngle(Vector3 dir)
    {
        Vector2 direction = new Vector2(dir.x, dir.y);
        direction = dir.normalized;
        //return angle of vector that is specifically a direction
        if (direction.y < 0)
        {
            return 360 - Vector2.Angle(direction, Vector2.right); ;
        }
        else
        {
            return Vector2.Angle(direction, Vector2.right);
        }
    }

    
}
