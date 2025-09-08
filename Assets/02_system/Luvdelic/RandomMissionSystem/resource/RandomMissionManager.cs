
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

    public string ReturnNoMissionName() { return NoMissionName; }
    public string[] ReturnMissionName() { return MissionName; }
    public GameObject[] ReturDisplayPrefab() { return DisplayPrefab; }
    public int[] ReturnPercentRate() { return PercentRate; }

}
