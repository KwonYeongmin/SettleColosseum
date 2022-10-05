using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class InstanceItem : MonoBehaviour
{
    public GameObject itemPack;
    Vector3 pos1 = new Vector3(107, 50, 75);
    Quaternion rot1 = new Quaternion(0, 0, 0, 0);

    void Start()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        PhotonNetwork.InstantiateRoomObject("ItemPack", this.transform.position, rot1);
    }
    
    void Update()  
    {

    }
}
