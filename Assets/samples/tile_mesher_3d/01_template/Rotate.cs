using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    private static readonly Vector3 Up = new Vector3(0, 1, 0);

    [SerializeField]
    private float speed = 10f;

    void Update()
    {
        transform.rotation *= Quaternion.AngleAxis(speed * Time.deltaTime, Up);    
    }
}
