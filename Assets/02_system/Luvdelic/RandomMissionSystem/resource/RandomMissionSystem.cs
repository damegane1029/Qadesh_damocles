
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

//Noneに固定する
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]

public class RandomMissionSystem : UdonSharpBehaviour
{
    [SerializeField] private AudioSource Source;
    [SerializeField] private RandomMissionManager Manager;
    private PlayerMission PlayerData;
    private GameObject instance = null;
    private bool DisplayMission = false;
    private bool isJumping = false;
    private float count = 0.0f;
    float LimitTime = 0.0f;

    public void ToggleDisplayMission() { DisplayMission = !DisplayMission; }
    public bool ReturnDisplayMission() { return DisplayMission; }

    void Update()
    {
        if (isJumping)
        {
            count += Time.deltaTime;

            if (count > LimitTime)
            {
                LimitTime = Manager.ReturnPressTime();
                count = 0.0f;

                if (!PlayerData)
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

                if (PlayerData)
                {
                    bool successLottery = PlayerData.MissionActivate();
                    SpawnDisplay(PlayerData);
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
        else
        {
            count = 0.0f;
        }
    }

    public GameObject SpawnDisplay(PlayerMission playerData)
    {
        PlayerData = playerData;

        if (PlayerData)
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

        return instance;

    }

    public override void InputJump(bool value, UdonInputEventArgs args)
    {
        LimitTime = Manager.ReturnPressTime();
        if (PlayerData)
        {
            if (PlayerData.ReturnPlayerMissionNumber() != -1)
            {
                LimitTime = 0.001f;
            }
            else
            {
                LimitTime = Manager.ReturnPressTime();
            }
        }
        isJumping = value;
        if (Utilities.IsValid(instance))
        {
            if(isJumping)
            {
                Destroy(instance);
            }
        }
    }

}
