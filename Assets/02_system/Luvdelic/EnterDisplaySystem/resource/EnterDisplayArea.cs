
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

//Noneに固定する
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]

public class EnterDisplayArea : UdonSharpBehaviour
{
    [SerializeField] private EnterDisplaySystem DisplaySystem;

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (player != Networking.LocalPlayer) return; //LocalPlayer以外はスルー

        DisplaySystem.SpawnDisplay();
    }
}
