using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager cursorManager;
    public static CursorManager Inst { get { return cursorManager; } }

    void Start()
    {
        cursorManager = this;
    }


    void Update()
    {
        CurSorLockMode();
        if (Input.GetKeyDown(KeyCode.Escape)) { CurSorNoneMode(); }
    }

    void OnDestroy()
    {
        CurSorNoneMode();
    }

    void CurSorLockMode()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void CurSorNoneMode()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

    }
}