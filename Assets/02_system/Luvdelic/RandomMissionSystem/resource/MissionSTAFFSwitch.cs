
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

//Noneに固定する
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]

public class MissionSTAFFSwitch : UdonSharpBehaviour
{
    [SerializeField] private RandomMissionSystem System;

    public override void Interact()
    {
        System.ToggleDisplayMission();
    }
}
