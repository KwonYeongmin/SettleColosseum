using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionWindow : MonoBehaviour
{
  
    void Start()
    {
        
    }

    void Update()
    {
        if (this.gameObject.activeSelf) //옵션창이 켜져 있다면
        {
            Time.timeScale = 0;
        }        


    }
}
