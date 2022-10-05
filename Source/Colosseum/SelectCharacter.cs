using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectCharacter : MonoBehaviour
{
    public GameObject casey;
    public GameObject rora;

    public bool CaseyChecked = true;

    void Start()
    {
        if (CaseyChecked)
        {
            rora.SetActive(false);
            casey.SetActive(true);
        }
        else
        {
            rora.SetActive(true);
            casey.SetActive(false);
        }
        /*
      */
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F1))
        {
            casey.SetActive(true);
            rora.SetActive(false);
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            casey.SetActive(false);
            rora.SetActive(true);
        }
    }
}
