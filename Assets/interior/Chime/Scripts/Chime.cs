using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace Purin
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Chime : UdonSharpBehaviour
    {
        public AudioSource audioSource;
        public AudioClip pingAudioClip;
        public AudioClip pongAudioClip;

        private bool interact = false;
        private bool pushMouse = false;
        private bool pushLeftTrigger = false;
        private bool pushRightTrigger = false;
        private bool push = false;

        private Transform button;

        private float lastDepthRatioView = 0.0f;

        private float smoothDepthRatio = 0.0f;
        private bool lastPushState = false;

        private const float DEPTH_OFFSET = 0.01f;
        private const float HEIGHT_OFFSET = 0.01f;

        private const float MIN_DEPTH = -0.1f + DEPTH_OFFSET;
        private const float CLAMP_DEPTH = 0.00395f + DEPTH_OFFSET;
        private const float MAX_DEPTH = 0.0057f + DEPTH_OFFSET;

        private const float WIDTH = 0.013f;
        private const float MIN_HEIGHT = -0.029f - HEIGHT_OFFSET;
        private const float MAX_HEIGHT = 0.0f;

        private const float ANGLE1 = 0.0f;
        private const float ANGLE2 = 6.0f;

        void Start()
        {
            button = transform.Find("Button");
        }

        void Update()
        {
            var localPlayer = Networking.LocalPlayer;

            var depthRatio = 0.0f;

            if (localPlayer != null)
            {
                depthRatio = Mathf.Max(depthRatio, GetDepthRatio(localPlayer.GetBonePosition(HumanBodyBones.LeftIndexDistal)));
                depthRatio = Mathf.Max(depthRatio, GetDepthRatio(localPlayer.GetBonePosition(HumanBodyBones.RightIndexDistal)));
            }

            depthRatio = Mathf.Max(depthRatio, GetDepthRatio(transform.Find("Dummy").position));

            var depthRatioView = depthRatio;

            if (push)
            {
                depthRatioView = 1.0f;
            }

            if (depthRatioView != lastDepthRatioView)
            {
                var angle = Mathf.Lerp(ANGLE1, ANGLE2, depthRatioView);
                button.localRotation = Quaternion.Euler(angle, 0.0f, 0.0f);

                lastDepthRatioView = depthRatioView;
            }

            smoothDepthRatio = Mathf.Lerp(depthRatio, smoothDepthRatio, Mathf.Pow(0.5f, Time.deltaTime / 0.01f));
            var pushState = smoothDepthRatio > 0.9f;

            if (pushState != lastPushState)
            {
                if (pushState)
                {
                    SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ChimeOn));
                }
                else
                {
                    SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ChimeOff));
                }

                lastPushState = pushState;
            }

            if (interact)
            {
                pushMouse |= GetMouseState();

                if (localPlayer != null)
                {
                    pushLeftTrigger |= GetLeftTriggerState();
                    pushRightTrigger |= GetRightTriggerState();
                }

                if (pushMouse || pushLeftTrigger || pushRightTrigger)
                {
                    interact = false;
                }
            }

            if (pushMouse && !GetMouseState())
            {
                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ChimeOff));
                pushMouse = false;
            }

            if (localPlayer != null)
            {
                if (pushLeftTrigger && !GetLeftTriggerState())
                {
                    SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ChimeOff));
                    pushLeftTrigger = false;
                }

                if (pushRightTrigger && !GetRightTriggerState())
                {
                    SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ChimeOff));
                    pushRightTrigger = false;
                }
            }
        }

        private float GetDepthRatio(Vector3 p)
        {
            var tp = transform.InverseTransformPoint(p);

            if (Mathf.Abs(tp.x) > WIDTH)
            {
                return 0.0f;
            }

            if (tp.y < MIN_HEIGHT || tp.y > MAX_HEIGHT)
            {
                return 0.0f;
            }

            if (tp.z < MIN_DEPTH || tp.z > MAX_DEPTH)
            {
                return 0.0f;
            }

            var ratio = Mathf.Clamp01(Mathf.InverseLerp(MAX_DEPTH, CLAMP_DEPTH, tp.z));

            return ratio;
        }

        public override void Interact()
        {
            interact = true;
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ChimeOn));
        }

        private bool GetMouseState()
        {
            return Input.GetMouseButton(0);
        }

        private bool GetLeftTriggerState()
        {
            return Input.GetAxis("Oculus_CrossPlatform_PrimaryIndexTrigger") != 0.0f;
        }

        private bool GetRightTriggerState()
        {
            return Input.GetAxis("Oculus_CrossPlatform_SecondaryIndexTrigger") != 0.0f;
        }

        public void ChimeOn()
        {
            push = true;
            audioSource.Stop();
            audioSource.clip = pingAudioClip;
            audioSource.PlayDelayed(0.01f);
        }

        public void ChimeOff()
        {
            push = false;
            audioSource.Stop();
            audioSource.clip = pongAudioClip;
            audioSource.PlayDelayed(0.01f);
        }
    }
}
