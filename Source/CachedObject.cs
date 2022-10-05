using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CachedObject : MonoBehaviour, IPunPrefabPool
{
    public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation)
    {
        return PhotonNetwork.PrefabPool.Instantiate(prefabId, position, rotation);
    }

    public void Destroy(GameObject gameObject)
    {
        PhotonNetwork.PrefabPool.Destroy(gameObject);
    }
}
