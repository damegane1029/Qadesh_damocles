
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Sleepion3 : UdonSharpBehaviour
{
    bool Is_Active = false;
    AudioSource MainAudio;
    Animator MainAnimator;

    private void Start()
    {
        MainAudio = gameObject.GetComponent<AudioSource>();
        MainAnimator = gameObject.GetComponent<Animator>();
    }

    public override void Interact()
    {
        _StateControl();
        _AudioControl();
        _EmissionControl();
    }

    void _StateControl()
    {
        Is_Active = !Is_Active;
    }

    void _AudioControl()
    {
        if(Is_Active)
        {
            MainAudio.Play();
        }
        else
        {
            MainAudio.Stop();
        }
    }

    void _EmissionControl()
    {
        MainAnimator.SetBool("AnimBool", Is_Active);
    }
}
