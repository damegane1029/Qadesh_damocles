
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

//Noneに固定する
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]

public class MissionPlayerFollwer : UdonSharpBehaviour
{
    void Update()
    {
        FollowPlayer();
    }

    private void FollowPlayer() // Follow処理
    {
        var player = Networking.GetOwner(this.gameObject);
        if (player != null)
        {
            bool hasHead = player.GetBonePosition(HumanBodyBones.Head).magnitude > 0.001f; // humanoidか判定
            if (hasHead)
            {
                var headData = player.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
                transform.position = new Vector3(headData.position.x, headData.position.y, headData.position.z);
                transform.rotation = Quaternion.Euler(new Vector3(0.0f, headData.rotation.eulerAngles.y - 90.0f, -headData.rotation.eulerAngles.x));
            }
            else
            {
                var originData = player.GetTrackingData(VRCPlayerApi.TrackingDataType.Origin);
                transform.position = new Vector3(originData.position.x, originData.position.y, originData.position.z);
                transform.rotation = Quaternion.Euler(new Vector3(0.0f, originData.rotation.eulerAngles.y - 90.0f, -originData.rotation.eulerAngles.x));
            }
        }
    }
}
