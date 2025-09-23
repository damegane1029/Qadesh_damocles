
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

//Noneに固定する
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]

public class RandomMissionManager : UdonSharpBehaviour
{
    [Header("ミッション未受注名:STAFF頭上確認名")]
    [SerializeField] private string NoMissionName;
    [Header("ミッション名:要素数は全て揃える")]
    [SerializeField] private string[] MissionName;
    [Header("ミッション確率:全ての合計が100％になるように")]
    [SerializeField, Range(0, 100)] private int[] PercentRate;
    [Header("ミッションPrefab:UI表示するゲームオブジェクト")]
    [SerializeField] private GameObject[] DisplayPrefab;
    [Header("操作設定:再表示時間")]
    [SerializeField, Tooltip("再表示フェードイン時間")] private float ReFadeInTime = 1.0f;
    [SerializeField, Tooltip("再表示待機時間")] private float ReWaitingTime = 1.0f;
    [SerializeField, Tooltip("再表示フェードアウト時間")] private float ReFadeOutTime = 1.0f;
    [Header("操作設定:長押し時間")]
    [SerializeField, Tooltip("長押し時間")] private float PressTime = 2.0f;


    public string ReturnNoMissionName() { return NoMissionName; }
    public string[] ReturnMissionName() { return MissionName; }
    public GameObject[] ReturDisplayPrefab() { return DisplayPrefab; }
    public int[] ReturnPercentRate() { return PercentRate; }
    public float ReturnReFadeInTime() { return ReFadeInTime; }
    public float ReturnReWaitingTime() { return ReWaitingTime; }
    public float ReturnReFadeOutTime() { return ReFadeOutTime; }
    public float ReturnPressTime() { return PressTime; }
}
