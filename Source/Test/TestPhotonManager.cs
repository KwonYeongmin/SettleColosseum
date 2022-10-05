using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class TestPhotonManager : MonoBehaviourPunCallbacks
{
    public static TestPhotonManager testPhotonManager;

    public static TestPhotonManager Inst { get { return testPhotonManager; } }

    // Start is called before the first frame update
    void Awake()
    {
        InitManager();
    }

    private void InitManager()
    {
        if (testPhotonManager)
        {
            Destroy(this);
            return;
        }

        testPhotonManager = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinOrCreateRoom("Test", new RoomOptions(), null);
    }
}
