using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    GameObject obj;

    // Start is called before the first frame update
    void Start()
    {
        obj = gameObject;
        obj.GetComponent<MeshRenderer>().enabled = false;
    }
}
