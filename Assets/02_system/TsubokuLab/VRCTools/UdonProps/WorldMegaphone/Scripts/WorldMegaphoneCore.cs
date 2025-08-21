/*
 * //============ Copyright (c) Teruaki Tsubokura @kohack_v ============//
 * Created by Teruaki Tsubokura (@kohack_v) 2022/02/28
 * 
 * NOTE:
 *  VRCPickup & VRCObjectSyncとManualSyncでの変数同期が同居出来ない為、同期変数のみ別オブジェクト管理するためのスクリプト
 */
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class WorldMegaphoneCore : UdonSharpBehaviour
{
    [SerializeField, Header("有効時オブジェクト")]
    private GameObject EnableObject;
    [SerializeField, Header("無効時オブジェクト")]
    private GameObject DisableObject;

    [SerializeField, Header("演出用メッシュ")]
    private MeshRenderer ModelMesh;

    [Header("[ 通常時ボイス設定 ]")]
    [SerializeField, Header("声の減衰開始距離 (default:0)")]
    private float voiceDistanceNear = 0.0f;
    [SerializeField, Header("声の減衰完了距離 (default:25)")]
    private float voiceDistanceFar = 25.0f;
    [SerializeField, Header("声の体積半径 (default:0)")]
    private float voiceVolumetricRadius = 0.0f;
    [SerializeField, Range(0.0f, 24.0f),Header("声のゲイン (default:15)")]
    private float voiceGain = 15.0f;

    [Header("[ 拡声時ボイス設定 ]")]
    [SerializeField, Header("声の減衰開始距離 (max:1000)")]
    private float voiceDistanceNear_Loud = 1000.0f;
    [SerializeField, Header("声の減衰完了距離 (max:1000000)")]
    private float voiceDistanceFar_Loud = 1000000.0f;
    [SerializeField, Header("声の体積半径 (max:1000)")]
    private float voiceVolumetricRadius_Loud = 1000.0f;
    [SerializeField, Range(0.0f, 24.0f), Header("声のゲイン (max:24)")]
    private float voiceGain_Loud = 15.0f;

    [UdonSynced, FieldChangeCallback(nameof(isUsingChanged))]
    private bool isUsing = false;
    public bool isUsingChanged
    {
        get => isUsing;
        set
        {
            isUsing = value;
            StatusChanged();
        }
    }

    void Start()
    {
        if (EnableObject) EnableObject.SetActive(false);
        if (DisableObject) DisableObject.SetActive(true);

        // マテリアルを起こす
        if (ModelMesh)
        {
            ModelMesh.material.EnableKeyword("_EMISSION");
            ModelMesh.material.DisableKeyword("_EMISSION");
        }
    }

    public void StatusChanged()
    {
        Debug.Log("[ WorldMegaphone ] StatusChanged( " + isUsing + " )");
        if (isUsing)
        {
            StartSpeaking();
        }
        else
        {
            FinishSpeaking();
        }
    }

    public void StartSpeaking()
    {
        Debug.Log("[ WorldMegaphone ] 全体アナウンス開始");
        isUsing = true;
        if (EnableObject) EnableObject.SetActive(true);
        if (DisableObject) DisableObject.SetActive(false);
        if (ModelMesh) ModelMesh.material.EnableKeyword("_EMISSION");

        VRCPlayerApi _owner = Networking.GetOwner(gameObject);
        if (_owner.isLocal) return;
        _owner.SetVoiceDistanceNear(voiceDistanceNear_Loud);
        _owner.SetVoiceDistanceFar(voiceDistanceFar_Loud);
        _owner.SetVoiceVolumetricRadius(voiceVolumetricRadius_Loud);
        _owner.SetVoiceGain(voiceGain_Loud);
    }

    public void FinishSpeaking()
    {
        Debug.Log("[ WorldMegaphone ] 全体アナウンス終了");
        isUsing = false;
        if (EnableObject) EnableObject.SetActive(false);
        if (DisableObject) DisableObject.SetActive(true);
        if (ModelMesh) ModelMesh.material.DisableKeyword("_EMISSION");

        VRCPlayerApi _owner = Networking.GetOwner(gameObject);
        if (_owner.isLocal) return;
        _owner.SetVoiceDistanceNear(voiceDistanceNear);
        _owner.SetVoiceDistanceFar(voiceDistanceFar);
        _owner.SetVoiceVolumetricRadius(voiceVolumetricRadius);
        _owner.SetVoiceGain(voiceGain);
        _owner.SetVoiceGain(voiceGain);
    }

}
