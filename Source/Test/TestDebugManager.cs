using Photon.Pun;
using TMPro;

public class TestDebugManager : MonoBehaviourPunCallbacks
{
    public static TestDebugManager testManager;

    public static TestDebugManager Inst { get { return testManager; } }

    public TextMeshProUGUI text;

    // Start is called before the first frame update
    void Awake()
    {
        InitManager();
    }

    private void InitManager()
    {
        if (testManager)
        {
            Destroy(this);
            return;
        }

        testManager = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetText(string txt)
    {
        text.text = txt;
    }
}
