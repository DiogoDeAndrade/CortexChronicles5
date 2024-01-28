using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetZFromY : MonoBehaviour
{
    void LateUpdate()
    {
        var currentPos = transform.position;
        currentPos.z = GetZ(currentPos.y);
        transform.position = currentPos;
    }

    float GetZ(float y)
    {
        return 1.0f * (y + 500.0f);
    }
}
