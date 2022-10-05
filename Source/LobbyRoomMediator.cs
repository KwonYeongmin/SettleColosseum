using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using HashTable = ExitGames.Client.Photon.Hashtable;

public class LobbyRoomMediator : MonoBehaviour
{
    public static LobbyRoomMediator lobbyRoomMediator;
    public static LobbyRoomMediator Inst { get { return lobbyRoomMediator; } }

    PhotonView pv;

    // Start is called before the first frame update
    void Start()
    {
        pv = GetComponent<PhotonView>();
    }

    public static void UpdateRoom()
    {
        
    }
}
