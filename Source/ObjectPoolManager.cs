using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Photon.Pun;

public class ObjectPoolManager : MonoBehaviour
{
    static public ObjectPoolManager objectPoolManager;
    static public ObjectPoolManager Inst { get { return objectPoolManager; } }

    public bool bPoolCreated;

    private void Awake()
    {
        objectPoolManager = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        bPoolCreated = false;

        GameObject obj;

        obj = PhotonNetwork.Instantiate(
                Path.Combine("PhotonPrefabs/CaseyInstance", "MLBullet"),
                Vector3.zero, Quaternion.identity);
        PhotonNetwork.Destroy(obj);

        obj = PhotonNetwork.Instantiate(
                Path.Combine("PhotonPrefabs/CaseyInstance", "MRBullet"),
                Vector3.zero, Quaternion.identity);
        PhotonNetwork.Destroy(obj);

        obj = PhotonNetwork.Instantiate(
                Path.Combine("PhotonPrefabs/CaseyInstance", "QRocket"),
                Vector3.zero, Quaternion.identity);
        PhotonNetwork.Destroy(obj);
        obj = PhotonNetwork.Instantiate(
                Path.Combine("PhotonPrefabs/CaseyInstance", "QRocket"),
                Vector3.zero, Quaternion.identity);
        PhotonNetwork.Destroy(obj);
        obj = PhotonNetwork.Instantiate(
                Path.Combine("PhotonPrefabs/CaseyInstance", "QRocket"),
                Vector3.zero, Quaternion.identity);
        PhotonNetwork.Destroy(obj);
        obj = PhotonNetwork.Instantiate(
                Path.Combine("PhotonPrefabs/CaseyInstance", "QRocket"),
                Vector3.zero, Quaternion.identity);
        PhotonNetwork.Destroy(obj);
        obj = PhotonNetwork.Instantiate(
                Path.Combine("PhotonPrefabs/CaseyInstance", "QRocket"),
                Vector3.zero, Quaternion.identity);
        PhotonNetwork.Destroy(obj);
        obj = PhotonNetwork.Instantiate(
                Path.Combine("PhotonPrefabs/CaseyInstance", "QRocket"),
                Vector3.zero, Quaternion.identity);
        PhotonNetwork.Destroy(obj);
        obj = PhotonNetwork.Instantiate(
                Path.Combine("PhotonPrefabs/CaseyInstance", "QRocket"),
                Vector3.zero, Quaternion.identity);
        PhotonNetwork.Destroy(obj);
        obj = PhotonNetwork.Instantiate(
                Path.Combine("PhotonPrefabs/CaseyInstance", "QRocket"),
                Vector3.zero, Quaternion.identity);
        PhotonNetwork.Destroy(obj);

        obj = PhotonNetwork.Instantiate(
                Path.Combine("PhotonPrefabs/CaseyInstance", "BigExplosion"),
                Vector3.zero, Quaternion.identity);
        PhotonNetwork.Destroy(obj);
        obj = PhotonNetwork.Instantiate(
                Path.Combine("PhotonPrefabs/CaseyInstance", "BigExplosion"),
                Vector3.zero, Quaternion.identity);
        PhotonNetwork.Destroy(obj);
        obj = PhotonNetwork.Instantiate(
                Path.Combine("PhotonPrefabs/CaseyInstance", "BigExplosion"),
                Vector3.zero, Quaternion.identity);
        PhotonNetwork.Destroy(obj);
        obj = PhotonNetwork.Instantiate(
                Path.Combine("PhotonPrefabs/CaseyInstance", "BigExplosion"),
                Vector3.zero, Quaternion.identity);
        PhotonNetwork.Destroy(obj);
        obj = PhotonNetwork.Instantiate(
                Path.Combine("PhotonPrefabs/CaseyInstance", "BigExplosion"),
                Vector3.zero, Quaternion.identity);
        PhotonNetwork.Destroy(obj);
        obj = PhotonNetwork.Instantiate(
                Path.Combine("PhotonPrefabs/CaseyInstance", "BigExplosion"),
                Vector3.zero, Quaternion.identity);
        PhotonNetwork.Destroy(obj);
        obj = PhotonNetwork.Instantiate(
                Path.Combine("PhotonPrefabs/CaseyInstance", "BigExplosion"),
                Vector3.zero, Quaternion.identity);
        PhotonNetwork.Destroy(obj);
        obj = PhotonNetwork.Instantiate(
                Path.Combine("PhotonPrefabs/CaseyInstance", "BigExplosion"),
                Vector3.zero, Quaternion.identity);
        PhotonNetwork.Destroy(obj);

        obj = PhotonNetwork.Instantiate(
                Path.Combine("PhotonPrefabs/RoraInstance", "BlackHoleEffect"),
                Vector3.zero, Quaternion.identity);
        PhotonNetwork.Destroy(obj);

        obj = PhotonNetwork.Instantiate(
                Path.Combine("PhotonPrefabs/RoraInstance", "reflectionAura"),
                Vector3.zero, Quaternion.identity);
        PhotonNetwork.Destroy(obj);

        obj = PhotonNetwork.Instantiate(
                Path.Combine("PhotonPrefabs/RoraInstance", "LAttackLaser"),
                Vector3.zero, Quaternion.identity);
        PhotonNetwork.Destroy(obj);

        obj = PhotonNetwork.Instantiate(
                Path.Combine("PhotonPrefabs/RoraInstance", "RAttackLaser"),
                Vector3.zero, Quaternion.identity);
        PhotonNetwork.Destroy(obj);
        obj = PhotonNetwork.Instantiate(
                Path.Combine("PhotonPrefabs/RoraInstance", "RAttackLaser"),
                Vector3.zero, Quaternion.identity);
        PhotonNetwork.Destroy(obj);
        obj = PhotonNetwork.Instantiate(
                Path.Combine("PhotonPrefabs/RoraInstance", "RAttackLaser"),
                Vector3.zero, Quaternion.identity);
        PhotonNetwork.Destroy(obj);
        obj = PhotonNetwork.Instantiate(
                Path.Combine("PhotonPrefabs/RoraInstance", "RAttackLaser"),
                Vector3.zero, Quaternion.identity);
        PhotonNetwork.Destroy(obj);
        obj = PhotonNetwork.Instantiate(
                Path.Combine("PhotonPrefabs/RoraInstance", "RAttackLaser"),
                Vector3.zero, Quaternion.identity);
        PhotonNetwork.Destroy(obj);
        obj = PhotonNetwork.Instantiate(
                Path.Combine("PhotonPrefabs/RoraInstance", "RAttackLaser"),
                Vector3.zero, Quaternion.identity);
        PhotonNetwork.Destroy(obj);
        obj = PhotonNetwork.Instantiate(
                Path.Combine("PhotonPrefabs/RoraInstance", "RAttackLaser"),
                Vector3.zero, Quaternion.identity);
        PhotonNetwork.Destroy(obj);
        obj = PhotonNetwork.Instantiate(
                Path.Combine("PhotonPrefabs/RoraInstance", "RAttackLaser"),
                Vector3.zero, Quaternion.identity);
        PhotonNetwork.Destroy(obj);
        obj = PhotonNetwork.Instantiate(
                Path.Combine("PhotonPrefabs/RoraInstance", "RAttackLaser"),
                Vector3.zero, Quaternion.identity);
        PhotonNetwork.Destroy(obj);
        obj = PhotonNetwork.Instantiate(
                Path.Combine("PhotonPrefabs/RoraInstance", "RAttackLaser"),
                Vector3.zero, Quaternion.identity);
        PhotonNetwork.Destroy(obj);

        obj = PhotonNetwork.Instantiate(
                Path.Combine("PhotonPrefabs/RoraInstance", "ESkillLaser"),
                Vector3.zero, Quaternion.identity);
        PhotonNetwork.Destroy(obj);

        obj = PhotonNetwork.Instantiate(
                    Path.Combine("PhotonPrefabs/RoraInstance", "Turret"),
                    Vector3.zero, Quaternion.identity);
        PhotonNetwork.Destroy(obj);
        obj = PhotonNetwork.Instantiate(
                    Path.Combine("PhotonPrefabs/RoraInstance", "Turret"),
                    Vector3.zero, Quaternion.identity);
        PhotonNetwork.Destroy(obj);
        obj = PhotonNetwork.Instantiate(
                    Path.Combine("PhotonPrefabs/RoraInstance", "Turret"),
                    Vector3.zero, Quaternion.identity);
        PhotonNetwork.Destroy(obj);

        bPoolCreated = true;
    }
}
