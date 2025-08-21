//============ Copyright (c) Teruaki Tsubokura @kohack_v ============//
// Created by Teruaki Tsubokura (@kohack_v) 2022/02/28

using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using System;
using System.IO;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UnityEditor;
using UdonSharpEditor;
#endif

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SlideshowScript : UdonSharpBehaviour
{
    [Header("[ スライドショー設定 ]")]
    [SerializeField, Header("スライド画像リスト")]
    private Texture[] SlideList;
    [SerializeField, Header("スライドを同期させる")]
    private bool isGlobal = false;
    [SerializeField, Header("オーナー以外の操作ロック")]
    private bool isOwnerLock = false;
    [UdonSynced, FieldChangeCallback(nameof(_isAutoChanged)), Header("自動スライド送り")]
    public bool isAuto = true;
    public bool _isAutoChanged
    {
        get => isAuto;
        set
        {
            if (isGlobal)
            {
                Debug.Log("[ SlideshowScript ] _isAutoChanged : " + isAuto);
                isAuto = value;
                if (isAutoToggle) isAutoToggle.isOn = isAuto;
                isAutoChanged();
            }
        }
    }
    [SerializeField, Header("自動スライド送り秒数")]
    private float NextSeconds = 3.0f;
    private float _NextTimer = 0.0f;
    [SerializeField, Header("自動スライド送りToggle")]
    private Toggle isAutoToggle;
    [SerializeField, Header("自動スライド送り秒数設定スライダー")]
    private Slider NextSecondsSlider;

    [UdonSynced, FieldChangeCallback(nameof(_currentNumChanged)), Header("現在スライド番号")]
    public int currentNum = 0;
    public int _currentNumChanged
    {
        get => currentNum;
        set
        {
            if (isGlobal)
            {
                Debug.Log("[ SlideshowScript ] _currentNumChanged : " + currentNum);
                currentNum = value;
                SlideChanged();
            }
        }
    }

    [Header("[ テクスチャ設定 ]")]
    [SerializeField, Header("ターゲットメッシュ")]
    private MeshRenderer TargetMesh;
    //[SerializeField, Header("ターゲットマテリアル")]
    private Material TargetMat;
    [SerializeField, Header("ターゲットテクスチャ")]
    private string TargetTex = "_MainTex";
    [SerializeField, Header("SharedMaterialを使うかどうか")]
    private bool useSharedMaterial = true;

    [Header("[ UI表示設定 ]")]
    [SerializeField, Header("現在スライド番号Text")]
    private Text[] CurrentSlideNumTextList;
    [SerializeField, Header("最終スライド番号Text")]
    private Text[] LastSlideNumTextList;
    [SerializeField, Header("現在スライド番号スライダー")]
    private Slider currentNumSlider;
    [SerializeField, Header("日付Text")]
    private Text DateText;
    [SerializeField, Header("時間Text")]
    private Text TimeText;
    private DateTime _now;

    void Start()
    {
        Debug.Log("[ SlideshowScript ] Start()");

        if (!TargetMesh) TargetMesh = GetComponent<MeshRenderer>();
        if (TargetMesh)
        {
            if (useSharedMaterial)
            {
                TargetMat = TargetMesh.sharedMaterial;
            }
            else
            {
                TargetMat = TargetMesh.material;
            }
        }
        if (SlideList.Length > 0 || TargetMat)
        {
            SlideChanged();
        }
        // 最終スライド番号をセット
        for (int i = 0; i < LastSlideNumTextList.Length; i++)
        {
            if (LastSlideNumTextList[i]) LastSlideNumTextList[i].text = SlideList.Length.ToString();
        }

        if (isAuto)
        {
            //SendCustomEventDelayedSeconds("Next", NextSeconds);
            _NextTimer = NextSeconds;
            if (isAutoToggle) isAutoToggle.isOn = isAuto;
        }

        if (TimeText)
        {
            _now = DateTime.Now;
            UpdateDateTime();
        }
    }

    private void Update()
    {
        if (isGlobal)
        {
            // Sync設定時はオーナーのみ処理を行う
            if (!Networking.IsOwner(gameObject)) return;
        }

        if (isAuto && _NextTimer > 0.0f)
        {
            _NextTimer -= Time.deltaTime;
            if (_NextTimer <= 0.0f)
            {
                Next();
            }
        }
    }

    public void NextButtonPressed()
    {
        Debug.Log("[ SlideshowScript ] NextButtonPressed()");
        if (isOwnerLock)
        {
            Debug.Log("[ SlideshowScript ] オーナーによりスライドロック中");
            if (!Networking.IsOwner(gameObject)) return;
        }
        else
        {
            if (!Networking.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        isAuto = false;
        if (isAutoToggle)
        {
            isAutoToggle.isOn = false;
            isAutoChanged();
        }
        Next();
    }
    public void PrevButtonPressed()
    {
        Debug.Log("[ SlideshowScript ] PrevButtonPressed()");
        if (isOwnerLock)
        {
            Debug.Log("[ SlideshowScript ] オーナーによりスライドロック中");
            if (!Networking.IsOwner(gameObject)) return;
        }
        else
        {
            if (!Networking.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        isAuto = false;
        if (isAutoToggle)
        {
            isAutoToggle.isOn = false;
            isAutoChanged();
        }
        Prev();
    }

    public void Next()
    {
        Debug.Log("[ SlideshowScript ] Next()");
        if (SlideList.Length < 1 || !TargetMat)
        {
            Debug.Log("[ SlideshowScript ] スライドテクスチャ及びマテリアルをセットして下さい");
            return;
        }

        currentNum++;
        if (currentNum >= SlideList.Length) currentNum = 0;
        SlideChanged();

        RequestSerialization();

        if (isAuto)
        {
            //SendCustomEventDelayedSeconds("Next", NextSeconds);
            _NextTimer = NextSeconds;
        }
    }
    public void Prev()
    {
        Debug.Log("[ SlideshowScript ] Prev()");
        if (SlideList.Length < 1 || !TargetMat)
        {
            Debug.Log("[ SlideshowScript ] スライドテクスチャ及びマテリアルをセットして下さい");
            return;
        }

        currentNum--;
        if (currentNum < 0) currentNum = SlideList.Length - 1;
        SlideChanged();

        RequestSerialization();
    }

    private void SlideChanged()
    {
        if(SlideList[currentNum]) TargetMat.SetTexture(TargetTex, SlideList[currentNum]);
        for(int i = 0; i < CurrentSlideNumTextList.Length; i++)
        {
            if(CurrentSlideNumTextList[i]) CurrentSlideNumTextList[i].text = (currentNum + 1).ToString();
        }

        if (currentNumSlider)
        {
            currentNumSlider.value = (float)currentNum / (float)(SlideList.Length - 1);
        }
    }

    public void ChangeSecondsSlider()
    {
        if (NextSecondsSlider)
        {
            NextSeconds = NextSecondsSlider.value;
        }
    }

    // TODO:オーナーによるロック機構を作成
    public void ToggleOwnerLock()
    {
        isOwnerLock = !isOwnerLock;
    }

    public void isAutoChanged()
    {
        if (!isAutoToggle) return;
        if (isOwnerLock)
        {
            Debug.Log("[ SlideshowScript ] オーナーによりスライドロック中");
            if (!Networking.IsOwner(gameObject)) return;
        }
        else
        {
            if (!Networking.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        isAuto = isAutoToggle.isOn;
        RequestSerialization();

        if (isAuto)
        {
            //Next();
            _NextTimer = NextSeconds;
        }
    }

    public void UpdateDateTime()
    {
        if (DateText)
        {
            DateText.text = _now.ToString("yyyy/MM/dd(ddd)");
        }
        if (TimeText)
        {
            _now = DateTime.Now;
            TimeText.text = _now.ToString("HH:mm");
            SendCustomEventDelayedSeconds("UpdateDateTime", 60.0f);
        }
    }
}
