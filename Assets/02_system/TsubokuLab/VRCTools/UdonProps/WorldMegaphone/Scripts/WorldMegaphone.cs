//============ Copyright (c) Teruaki Tsubokura @kohack_v ============//
// Updated by Teruaki Tsubokura (@kohack_v) 2024/08/28

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class WorldMegaphone : UdonSharpBehaviour
{
    [SerializeField, Header("コア機能スクリプト(減衰距離などの詳細設定はここ)")]
    private UdonBehaviour WorldMegaphoneCore;

    [SerializeField, Header("トグルモード")]
    private bool isToggleMode = false;

    // 位置リセット用
    private VRC_Pickup _pickup;
    private Vector3 defaultPosition;
    private Quaternion defaultRotation;
    private bool _isUsing = false;
    [SerializeField, Header("位置リセット秒数(0=無効)")]
    private float resetTime = 0.0f;

    [SerializeField, Header("リセットボタンオブジェクト(使用中は非表示になる)")]
    private GameObject ResetBtnObject;

    void Start()
    {
        defaultPosition = gameObject.transform.position;
        defaultRotation = gameObject.transform.rotation;
        _pickup = (VRC_Pickup)GetComponent(typeof(VRC_Pickup));
        ShowResetBtn();
    }

    public override void OnPickup()
    {
        if (!Networking.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);
        if (!Networking.IsOwner(WorldMegaphoneCore.gameObject)) Networking.SetOwner(Networking.LocalPlayer, WorldMegaphoneCore.gameObject);
        _isUsing = true;

        if (ResetBtnObject) SendCustomNetworkEvent( VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "HideResetBtn" );
    }

    // 押しっぱなしで話すVer ///////////////
    public override void OnPickupUseDown()
    {
        if (WorldMegaphoneCore == null) return;
        if (!Networking.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);
        if (!Networking.IsOwner(WorldMegaphoneCore.gameObject)) Networking.SetOwner(Networking.LocalPlayer, WorldMegaphoneCore.gameObject);

        if (isToggleMode)
        {
            ToggleUsing();
        }
        else
        {
            // 話し始めを取りこぼししないように、ManualSyncで高速同期
            StartSpeaking();
        }
    }
    
    public override void OnPickupUseUp()
    {
        if (!isToggleMode)
        {
            // 話し終わりはManualSyncでは同期が早すぎてブツ切りになるのでSendCustomNetworkEventでゆっくり同期
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "FinishSpeaking");
        }
    }
    public override void OnDrop()
    {
        _isUsing = false;
        if (isToggleMode) FinishSpeaking();
        if (resetTime > 0) SendCustomEventDelayedSeconds("ResetPosition", resetTime);

        if (ResetBtnObject) SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ShowResetBtn");
    }

    public void ResetPosition()
    {
        if (_pickup.IsHeld) return;
        gameObject.transform.position = defaultPosition;
        gameObject.transform.rotation = defaultRotation;
    }
    public void SyncedResetPosition()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "ResetPosition");
    }

    public void StartSpeaking()
    {
        if (WorldMegaphoneCore == null) return;
        WorldMegaphoneCore.SetProgramVariable("isUsing", true);
        WorldMegaphoneCore.RequestSerialization();
        // 他のUdonBehaviourの場合は自分にイベントを叩かなくても良い？
        //WorldMegaphoneCore.SendCustomEvent("StatusChanged");
    }
    public void FinishSpeaking()
    {
        if (WorldMegaphoneCore == null) return;
        WorldMegaphoneCore.SetProgramVariable("isUsing", false);
        WorldMegaphoneCore.RequestSerialization();
        // 他のUdonBehaviourの場合は自分にイベントを叩かなくても良い？
        //WorldMegaphoneCore.SendCustomEvent("StatusChanged");
    }

    // トリガーでトグルVer /////////////
    public void ToggleUsing()
    {
        if (WorldMegaphoneCore == null) return;
        bool _isUsing = !(bool)WorldMegaphoneCore.GetProgramVariable("isUsing");
        WorldMegaphoneCore.SetProgramVariable("isUsing", _isUsing);
        WorldMegaphoneCore.RequestSerialization();
        // 他のUdonBehaviourの場合は自分にイベントを叩かなくても良い？
        //WorldMegaphoneCore.SendCustomEvent("StatusChanged");
    }

    public void HideResetBtn()
    {
        if (ResetBtnObject) ResetBtnObject.SetActive(false);
    }
    public void ShowResetBtn()
    {
        if (ResetBtnObject) ResetBtnObject.SetActive(true);
    }
}
