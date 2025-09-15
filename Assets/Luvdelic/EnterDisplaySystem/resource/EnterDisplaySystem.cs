
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

//Noneに固定する
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]

public class EnterDisplaySystem : UdonSharpBehaviour
{
    [SerializeField] private AudioSource Source;
    [SerializeField] private GameObject DisplayPrefab;
    private GameObject instance = null;
    private bool SpawnEnable = true;

    public void SetSpawnEnable(bool enable) { SpawnEnable = enable; }

    public void SpawnDisplay()
    {
        if (!Utilities.IsValid(instance))
        {
            if (SpawnEnable)
            {
                if (!Source.isPlaying)
                {
                    instance = VRCInstantiate(DisplayPrefab);
                    SpawnEnable = false;
                    Source.Play();
                }
            }
        }
    }
}
