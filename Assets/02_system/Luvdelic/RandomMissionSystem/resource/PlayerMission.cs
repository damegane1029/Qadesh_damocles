
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

//Continuousに固定する
[UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]

public class PlayerMission : UdonSharpBehaviour
{
    [SerializeField] private RandomMissionSystem System;
    [SerializeField] private RandomMissionManager Manager;
    [UdonSynced] private int PlayerMissionNumber = -1;
    [SerializeField] private GameObject CurrentDisplay;
    [SerializeField] private TMP_Text tmpText;

    public int ReturnPlayerMissionNumber() { return PlayerMissionNumber; }

    void Start()
    {
        PlayerMissionNumber = -1;
    }

    void Update()
    {
        CurrentDisplay.SetActive(System.ReturnDisplayMission());

        if(PlayerMissionNumber == -1)
        {
            tmpText.text = Manager.ReturnNoMissionName();
        }
        else
        {
            tmpText.text = Manager.ReturnMissionName()[PlayerMissionNumber];
        }
    }

    public bool MissionActivate()
    {
        // Missionが選択されていない
        if(PlayerMissionNumber == -1)
        {
            // 抽選開始
            int randomValue = Random.Range(0, 101);

            int percentdetectValue = 0;
            int[] percentRateList = Manager.ReturnPercentRate();

            for(int i = 0; i < percentRateList.Length; i++)
            {
                percentdetectValue += percentRateList[i];

                // もし抽選確率に収まる数字だったら
                if (randomValue <= percentdetectValue)
                {
                    // ミッション決定
                    PlayerMissionNumber = i;
                    return true;
                }
            }
            return false;
        }
        return false;
    }

    public void MissionReset()
    {
        PlayerMissionNumber = -1;
    }

    public void Display(bool enable)
    {
        CurrentDisplay.SetActive(enable);
    }
}
