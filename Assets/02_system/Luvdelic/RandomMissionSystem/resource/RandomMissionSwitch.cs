
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

//Noneに固定する
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]

public class RandomMissionSwitch : UdonSharpBehaviour
{

    [SerializeField] private RandomMissionSystem System;
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
            PlayerData.MissionActivate();
            System.SpawnDisplay(PlayerData);
        }
    }
}
