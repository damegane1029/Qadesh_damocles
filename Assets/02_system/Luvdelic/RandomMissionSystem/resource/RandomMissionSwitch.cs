
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

//Noneに固定する
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]

public class RandomMissionSwitch : UdonSharpBehaviour
{

    [SerializeField] private RandomMissionSystem System;
    [SerializeField] private RandomMissionManager Manager;
    private PlayerMission PlayerData;

    public override void Interact()
    {
        if(!PlayerData)
        {
            var player = Networking.LocalPlayer;
            GameObject[] playerObjectList = Networking.GetPlayerObjects(player);

            foreach (GameObject elem in playerObjectList)
            {
                PlayerMission playerData = elem.GetComponent<PlayerMission>();
                if (playerData)
                {
                    PlayerData = playerData;
                    break;
                }
            }
        }

        if(PlayerData)
        {
            bool successLottery = PlayerData.MissionActivate();
            GameObject instance = System.SpawnDisplay(PlayerData);
            if (!successLottery)
            {
                if (Utilities.IsValid(instance))
                {
                    EnterDisplay display = instance.GetComponent<EnterDisplay>();
                    display.SetTime(Manager.ReturnReFadeInTime(), Manager.ReturnReWaitingTime(), Manager.ReturnReFadeOutTime());
                }
            }

        }
    }
}
