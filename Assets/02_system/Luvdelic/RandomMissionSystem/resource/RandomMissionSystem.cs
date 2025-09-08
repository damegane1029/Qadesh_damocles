
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

//Noneに固定する
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]

public class RandomMissionSystem : UdonSharpBehaviour
{
    [SerializeField] private AudioSource Source;
    [SerializeField] private RandomMissionManager Manager;
    private PlayerMission PlayerData;
    private GameObject instance = null;
    private bool DisplayMission = false;

    public void ToggleDisplayMission() { DisplayMission = !DisplayMission; }
    public bool ReturnDisplayMission() { return DisplayMission;  }

    public void SpawnDisplay(PlayerMission playerData)
    {
        PlayerData = playerData;

        if(PlayerData)
        {
            if (!Utilities.IsValid(instance))
            {
                if (!Source.isPlaying)
                {
                    instance = VRCInstantiate(Manager.ReturDisplayPrefab()[PlayerData.ReturnPlayerMissionNumber()]);
                    Source.Play();
                }
            }
        }

    }

}
