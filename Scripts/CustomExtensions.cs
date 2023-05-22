using UnityEngine;

public static class CustomExtensions
{
    public static bool HasTag(this Transform transform, string tag)
    {
        if (transform.CompareTag(tag))
            return true;
        if (transform.TryGetComponent<CustomTags>(out var customTags))
            return customTags.HasTag(tag);
        return false;
    }

    public static Vector3 FixEulers(this Vector3 eulerAngles)
    {
        float x = eulerAngles.x > 180 ? eulerAngles.x - 360 : eulerAngles.x;
        float y = eulerAngles.y > 180 ? eulerAngles.y - 360 : eulerAngles.y;
        float z = eulerAngles.z > 180 ? eulerAngles.z - 360 : eulerAngles.z;
        return new Vector3(x, y, z);
    }

    public static Vector3 Clamp(Vector3 vector, Vector3 min, Vector3 max)
    {
        return new Vector3(Mathf.Clamp(vector.x, min.x, max.x), Mathf.Clamp(vector.y, min.y, max.y), Mathf.Clamp(vector.z, min.z, max.z));
    }

    public static Vector3 Clamp(Vector3 vector, float min, float max)
    {
        return new Vector3(Mathf.Clamp(vector.x, min, max), Mathf.Clamp(vector.y, min, max), Mathf.Clamp(vector.z, min, max));
    }

    public static Vector3 WrapClamp(Vector3 vector, Vector3 min, Vector3 max)
    {
        Vector3 returnVector = vector;
        if (vector.x < min.x)
        {
            returnVector.x = max.x;
        }
        else if (vector.x > max.x)
        {
            returnVector.x = min.x;
        }
        if (vector.y < min.y)
        {
            returnVector.y = max.y;
        }
        else if (vector.y > max.y)
        {
            returnVector.y = min.y;
        }
        if (vector.z < min.z)
        {
            returnVector.z = max.z;
        }
        else if (vector.z > max.z)
        {
            returnVector.z = min.z;
        }
        return returnVector;
    }

    public static Vector3 WrapClamp(Vector3 vector, float min, float max)
    {
        Vector3 returnVector = vector;
        if(vector.x < min)
        {
            returnVector.x = max;
        }
        else if(vector.x > max)
        {
            returnVector.x = min;
        }
        if (vector.y < min)
        {
            returnVector.y = max;
        }
        else if (vector.y > max)
        {
            returnVector.y = min;
        }
        if (vector.z < min)
        {
            returnVector.z = max;
        }
        else if (vector.z > max)
        {
            returnVector.z = min;
        }
        return returnVector;
    }

    public static float SqrDistance(Vector3 a, Vector3 b)
    {
        float distanceX = a.x - b.x;
        float distanceY = a.y - b.y;
        float distanceZ = a.z - b.z;
        return distanceX * distanceX + distanceY * distanceY + distanceZ * distanceZ;
    }

    public static float normalize(float value, float min, float max)
    {
        return (value - min) / (max - min);
    }
}
