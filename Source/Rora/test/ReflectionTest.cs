using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReflectionTest : MonoBehaviour
{
    private GameObject blackHole;
    public GameObject Prefab;
    public Transform skillPoint;
    public Transform mCamera;

    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.H))
        {
            blackHole = Instantiate(Prefab, skillPoint.position, mCamera.rotation);
        }
    }
}
