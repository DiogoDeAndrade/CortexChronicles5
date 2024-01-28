using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceRotation : MonoBehaviour
{
    void LateUpdate()
    {
        transform.localRotation = Quaternion.Inverse(transform.parent.rotation);
    }
}
