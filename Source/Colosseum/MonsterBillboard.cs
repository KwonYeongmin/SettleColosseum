using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterBillboard : MonoBehaviour
{
    Transform cam;

    void Start()
    {
        cam = Camera.main.transform;
    }

    void Update()
    {
        cam = Camera.main.transform;
        transform.LookAt(transform.position + cam.rotation * Vector3.forward, cam.rotation * Vector3.up);
    }
    
}