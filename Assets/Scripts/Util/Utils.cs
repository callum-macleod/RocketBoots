using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;
//using static UnityEditor.Experimental.GraphView.GraphView;

public class Utils
{
    public static int LayerToLayerMask(int layer)
    {
        return 1 << layer;
    }

    public static int LayerToLayerMask(Layers layer)
    {
        return 1 << (int)layer;
    }

    public static Vector2 AdjustVec(Vector2 vector, float x = 0, float y = 0, float z = 0)
    {
        Vector2 adjusted = vector;
        adjusted.x += x;
        adjusted.y += y;

        return adjusted;
    }

    public static Vector3 RemoveY(Vector3 vector)
    {
        return vector - new Vector3(0, vector.y, 0);
    }

    public static Vector2 AsV2(Vector3 vector3)
    {
        return new Vector2(vector3.x, vector3.y);
    }

    public static GameObject InstantiateWithOptions(GameObject prefab, Vector3 location, Quaternion quaternion, string name = "")
    {
        GameObject obj = GameObject.Instantiate(prefab, location, quaternion);
        if (name != "") obj.name = name;
        return obj;
    }

    public static Quaternion LookAt2D(Vector3 self, Vector3 target, int rotation = 0)
    {
        // adapted from https://discussions.unity.com/t/lookat-2d-equivalent/88118/6
        Vector3 distanceToBodyCentre = (target - self).normalized;
        float rot_z = Mathf.Atan2(distanceToBodyCentre.y, distanceToBodyCentre.x) * Mathf.Rad2Deg;
        return Quaternion.Euler(0f, 0f, rot_z - 180 + rotation);
    }
}
