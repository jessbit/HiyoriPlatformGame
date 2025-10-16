using System;
using UnityEngine;

public class Rotator :MonoBehaviour
{
    public Space space;
    public Vector3 eulers = new Vector3(0, -100, 0);
    public void LateUpdate()
    {
        transform.Rotate(eulers*Time.deltaTime,space);
    }
}