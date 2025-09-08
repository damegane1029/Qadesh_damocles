
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AudioEffectOffArea : UdonSharpBehaviour
{
    [SerializeField] GameObject[] AreaOffObjs;
    [SerializeField] AudioEffectOffArea OtherArea;
    public bool isIn;

    public void _Enter(VRCPlayerApi player)
    {
        if (Networking.LocalPlayer == player)
        {
            if (OtherArea != null)
            {
                if (!OtherArea.isIn)
                {
                    for (int i = 0; i < AreaOffObjs.Length; i++)
                    {
                        if (AreaOffObjs[i]) AreaOffObjs[i].SetActive(false);
                    }
                }
            }
            else
            {
                for (int i = 0; i < AreaOffObjs.Length; i++)
                {
                    if (AreaOffObjs[i]) AreaOffObjs[i].SetActive(false);
                }
            }
            isIn=true;
        }
    }

    public void _Exit(VRCPlayerApi player)
    {
        if (Networking.LocalPlayer == player)
        {
            if (OtherArea != null)
            {
                if (!OtherArea.isIn)
                {
                    for (int i = 0; i < AreaOffObjs.Length; i++)
                    {
                        if (AreaOffObjs[i]) AreaOffObjs[i].SetActive(true);
                    }
                }
            }
            else
            {
                for (int i = 0; i < AreaOffObjs.Length; i++)
                {
                    if (AreaOffObjs[i]) AreaOffObjs[i].SetActive(true);
                }
            }
            isIn = false;
        }
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        _Enter(player);
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        _Exit(player);
    }

    public override void OnPlayerRespawn(VRCPlayerApi player)
    {
        _Exit(player);
    }
}
