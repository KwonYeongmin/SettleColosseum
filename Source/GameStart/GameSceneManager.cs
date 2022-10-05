using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

// 게임 씬이 바꼈을때 수행해야 할일을 해주는 매니저
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
            // 이미 시작된 게임에 다른 유저가 들어오지 못하게 막고 들어온 플레이어를 관리하는 플레이어 매니저를 생성한다.
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerManager"), Vector3.zero, Quaternion.identity);
        }
    }

    public void DestroyManager()
    {
        if(gameObject != null)
            Destroy(gameObject);
    }
}
