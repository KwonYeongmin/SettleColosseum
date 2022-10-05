using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemEffectDestruct : MonoBehaviour
{
    public float selfdestruct_in = 2; // Setting this to 0 means no selfdestruct.
    void Start()
    {
        if (selfdestruct_in != 0)
        {
            Destroy(gameObject, selfdestruct_in);
        }
    }
}
