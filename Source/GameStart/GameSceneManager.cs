using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

// ���� ���� �ٲ����� �����ؾ� ������ ���ִ� �Ŵ���
public class GameSceneManager : MonoBehaviourPunCallbacks
{
    private static GameSceneManager gameStartManager;

    public static GameSceneManager Inst { get { return gameStartManager; } }

    void Awake()
    {
        InitManager();
    }

    private void InitManager()
    {
        if(gameStartManager)
        {
            Destroy(gameObject);
            return;
        }

        gameStartManager = this;
        DontDestroyOnLoad(this);
    }

    public override void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoad;
    }

    public override void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
    }

    private void OnSceneLoad(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.name == "02_Wegheim" || scene.name == "03_Exe")
        {
            // �̹� ���۵� ���ӿ� �ٸ� ������ ������ ���ϰ� ���� ���� �÷��̾ �����ϴ� �÷��̾� �Ŵ����� �����Ѵ�.
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerManager"), Vector3.zero, Quaternion.identity);
        }
    }

    public void DestroyManager()
    {
        if(gameObject != null)
            Destroy(gameObject);
    }
}
