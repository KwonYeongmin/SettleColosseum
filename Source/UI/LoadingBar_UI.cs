using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;

public class LoadingBar_UI : MonoBehaviour
{

    public TextMeshProUGUI TXT_info;
    //public string[] TXT_stroies;
    public int storyNum = 17;

    List<string> TXT_stroies;

    private void Awake()
    {
        TXT_stroies = new List<string>();
        ReadFile();
    }

    private void OnEnable()
    {
        TXT_info.text = TXT_stroies[Random.Range(0, storyNum)];
    }


    private void ReadFile()
    {
        int count = 0;

        TextAsset textFile = Resources.Load("Info/LoadingText") as TextAsset;
        StringReader stringReader = new StringReader(textFile.text);

        while (stringReader != null)
        {
            string line = stringReader.ReadLine();

            if (line == null) break;

            //StageData stageData = new StageData();
            count++;

            if (count == 1) continue;

            //stageData.Stage = int.Parse(line.Split(',')[0]);
            //stageData.Type = (EnemyType)int.Parse(line.Split(',')[1]);
            //stageData.ColorType = (EnemyColorType)int.Parse(line.Split(',')[2]);
            //stageData.ItemIndex = int.Parse(line.Split(',')[3]);
            //for (int i = 0; i < 3; i++) stageData.VikingSpeed[i] = (float.Parse(line.Split(',')[i + 4]));
            //stageSetting.Add(stageData);
            TXT_stroies.Add(line);
        }
        stringReader.Close();
    }
}
