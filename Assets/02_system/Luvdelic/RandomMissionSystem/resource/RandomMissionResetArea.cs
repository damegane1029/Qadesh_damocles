
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

//Noneに固定する
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]

public class RandomMissionResetArea : UdonSharpBehaviour
{
    private PlayerMission PlayerData;

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (player != Networking.LocalPlayer) return;

        if (!PlayerData)
        {
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

        if (PlayerData)
        {
            PlayerData.MissionReset();
        }
    }

}
