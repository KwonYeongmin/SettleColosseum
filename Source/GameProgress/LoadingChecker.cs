using UnityEngine;
using Photon.Pun;

public class LoadingChecker : MonoBehaviour
{
    PlayerManager[] players;
    
    public GameObject Prefab_CharacterSelectUI;

    int CountOfPlayer => PhotonNetwork.CurrentRoom.PlayerCount;

    // Update is called once per frame
    void Update()
    {
        // ��� �÷��̾ �����ߴٸ� ĳ���� ����â�� ����.
        players = FindObjectsOfType<PlayerManager>();
        if (players.Length >= CountOfPlayer && ObjectPoolManager.Inst.bPoolCreated)
        {
            Debug.Log("Player Counts: " + players.Length);
            PhotonNetwork.CurrentRoom.IsOpen = false;
            Instantiate(Prefab_CharacterSelectUI);
            Destroy(gameObject);
        }
    }
}