using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RagdollApplier : MonoBehaviour
{
    public GameObject ModelTP;
    GameObject ModelRagdoll;

    public void ActiveRagdoll()
    {
        bool bIsCasey = (ModelTP.transform.root.GetComponent<Casey>() != null);

        if (bIsCasey)
        {
            ModelRagdoll = PhotonNetwork.Instantiate(
            "PhotonPrefabs/yellowcab_ragdoll Variant",
            ModelTP.transform.position,
            ModelTP.transform.rotation);
        }
        else
        {
            ModelRagdoll = PhotonNetwork.Instantiate(
            "PhotonPrefabs/Rora_Ragdoll",
            ModelTP.transform.position,
            ModelTP.transform.rotation);
        }
    }

    public void DeactiveRagdoll()
    {
        PhotonNetwork.Destroy(ModelRagdoll.gameObject);
    }
}
