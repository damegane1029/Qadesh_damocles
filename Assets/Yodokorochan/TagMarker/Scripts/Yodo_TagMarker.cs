
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace Yodokorochan
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Yodo_TagMarker : UdonSharpBehaviour
    {
        private const int MAX_PLAYERS_IN_INSTANCE = 82;    // インスタンスの最大人数。減らすとわずかにネットワーク負荷軽減になる。
        private const int MAX_BUTTONS = 64;                // ボタン最大数 = 64bit
		private const float DEFAULT_AVATAR_HEIGHT = 1.2f;

		[Header("頭上オフセット")]
        public float Yodo_MarkerOffset = 0.5f;
        [Header("Headが無かった時の足元からのオフセット")]
        public float Yodo_MarkerOffset_Generic = 1.7f;
		[Header("アバタースケーリングに応じて高さを微調節する")]
		[Tooltip("有効にするとアバター身長が1.2mの時に指定のオフセット値になり、そこからスケールに応じて倍率がかかるようになります")]
		public bool Yodo_AdjustHeightByAvatarScale = true;
		[Header("マーカーの親オブジェクト")]
        public GameObject Yodo_MarkersRoot = null;
        [Header("同期待ちブロックオブジェクト")]
        public GameObject Yodo_InitialBlocker = null;
        [Header("表示/非表示切り換え(スイッチ用)")]
        public bool Yodo_TagMarker_Active = true;


        [HideInInspector]
        [UdonSynced] public ulong[] Yodo_TagStatus = null;
        [HideInInspector]
        [UdonSynced] public int[] Yodo_PlayerIds = null;

        private Vector3 head_offset = Vector3.zero;
        private Vector3 generic_offset = Vector3.zero;
        private ulong[] recentTagStatus;
        private int[] recentPlayerIds;
        private GameObject[] markers;
        private VRCPlayerApi[] playerApis = new VRCPlayerApi[MAX_PLAYERS_IN_INSTANCE];
        private int myCursor = -1;
        private bool initialized = false;

        private void Start()
        {
            if (!Yodo_MarkersRoot)
            {
                Debug.LogError("[Yodo]Yodo_MarkersRootにマーカーの親オブジェクトを設定してください。");
            }

            markers = new GameObject[MAX_PLAYERS_IN_INSTANCE];
            Yodo_TagStatus = new ulong[MAX_PLAYERS_IN_INSTANCE];
            Yodo_PlayerIds = new int[MAX_PLAYERS_IN_INSTANCE];
            recentTagStatus = new ulong[MAX_PLAYERS_IN_INSTANCE];
            recentPlayerIds = new int[MAX_PLAYERS_IN_INSTANCE];
			Yodo_MarkersRoot.SetActive(false);

			for (int cur = 0; cur < MAX_PLAYERS_IN_INSTANCE; cur++)
            {
				Yodo_TagStatus[cur] = 0LU;
				recentTagStatus[cur] = 0LU;
				Yodo_PlayerIds[cur] = -1;
				recentPlayerIds[cur] = -1;
			}

            head_offset = new Vector3(0f, Yodo_MarkerOffset, 0f);
            generic_offset = new Vector3(0f, Yodo_MarkerOffset_Generic, 0f);

            if (Yodo_InitialBlocker)
            {
                Yodo_InitialBlocker.SetActive(true);
            }
        }

        private void Update()
        {
            if (Yodo_PlayerIds == null) { Debug.LogError("[Yodo]Yodo_PlayerIds Array not initialized at Update"); return; }
            if (Yodo_TagStatus == null) { Debug.LogError("[Yodo]Yodo_TagStatus Array not initialized at Update"); return; }
            if (!initialized) { return; }
            for (int cur = 0; cur < MAX_PLAYERS_IN_INSTANCE; cur++)
            {
                if (Yodo_PlayerIds[cur] < 0) { continue; }

                if (playerApis[cur] != null)    // LeftからDeserializeまでの数フレームはnullで来るので毎回チェックする
                {
                    if (playerApis[cur].IsValid())//自分がワールド退出する時のエラー対策
                    {
                        if (Yodo_TagMarker_Active && (Yodo_TagStatus[cur] != 0UL))
                        {
							markers[cur].SetActive(true);
                            Vector3 head_pos = playerApis[cur].GetBonePosition(HumanBodyBones.Head);
							float heightMultiplier = Yodo_AdjustHeightByAvatarScale ? playerApis[cur].GetAvatarEyeHeightAsMeters() / DEFAULT_AVATAR_HEIGHT : 1.0f;
                            if (head_pos != null)
                            {
                                if (head_pos != Vector3.zero)
                                {
                                    markers[cur].transform.position = head_pos + (head_offset * heightMultiplier);
								}
                                else
                                {
                                    markers[cur].transform.position = playerApis[cur].GetPosition() + (generic_offset * heightMultiplier);
                                }
                            }
                            else
                            {
                                markers[cur].transform.position = playerApis[cur].GetPosition() + (generic_offset * heightMultiplier);
                            }
                            if (playerApis[cur].isLocal)
                            {   // 自分のパネルは鏡で見るので正面へ向ける
                                markers[cur].transform.rotation = playerApis[cur].GetRotation();
                            }
                            else
                            {   // 他人のパネルは常に自分の視点へ向ける。反転してるので自分→パネルのLookAtを取る
                                Quaternion look = Quaternion.LookRotation(markers[cur].transform.position - Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position);
                                markers[cur].transform.rotation = look;
                            }
                        }
                        else
                        {
                            markers[cur].transform.localPosition = Vector3.zero;
							markers[cur].SetActive(false);
                        }
                    }
                }
            }
        }
        public override void OnDeserialization()
        {
            if (Yodo_PlayerIds == null) { Debug.LogError("[Yodo]Yodo_PlayerIds Array not initialized at OnDeserialization"); return; }
            if (Yodo_TagStatus == null) { Debug.LogError("[Yodo]Yodo_TagStatus Array not initialized at OnDeserialization"); return; }
            if (0 <= myCursor)
            {
                _ResolveConfliction();
            }
            _UpdateTags();
            if (!initialized)
            {   // 最初の同期が来たら操作ブロック解除
                _CompleteInitialize();
            }
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (Networking.IsOwner(this.gameObject))
            {
                if (player.isLocal)
                {   // 部屋に入った時に自分がOwnerなら最初の一人なので同期待ちはしない
                    _CompleteInitialize();
                }
                else
                {   // 自分以外ならOwnerが同期要求を出す
                    RequestSerialization();
                }
            }
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (player == null) { return; }// 自分が抜ける時はnullが来るのでもう無視
            for (int cur = 0; cur < Yodo_PlayerIds.Length; cur++)
            {
                if (Yodo_PlayerIds[cur] == player.playerId)
                {
                    if (markers[cur])
                    {
                        markers[cur].transform.localPosition = Vector3.zero;
                        markers[cur].SetActive(false);
                    }
                    playerApis[cur] = null;
                    if (Networking.IsOwner(this.gameObject))
                    {
                        Yodo_PlayerIds[cur] = -1;
                        Yodo_TagStatus[cur] = 0LU;
                        RequestSerialization();
                    }
                    break;
                }
            }
        }

        public void Yodo_ToggleMarkerActive()   // ローカルの表示/非表示スイッチ。外部から呼び出す用
        {
            Yodo_TagMarker_Active = !Yodo_TagMarker_Active;
        }

        public void Yodo_TagReset()
        {
            if (0 <= myCursor)
            {
                Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
                Yodo_TagStatus[myCursor] = 0;
                RequestSerialization();
                _UpdateTags();
            }
        }

        private void _CompleteInitialize()
        {
            initialized = true;
            if (Yodo_InitialBlocker)
            {
                Yodo_InitialBlocker.SetActive(false);
            }
            Debug.Log("[Yodo]Initialize tag marker completed successfully.");
        }

        private void _ResolveConfliction()  // 競合した時の救済措置
        {
            int id = Networking.LocalPlayer.playerId;
            if (Yodo_PlayerIds[myCursor] != id)
            {
                //Debug.Log($"[Yodo]_ResolveConfliction() resets myCursor[{myCursor}]");
                for (int cur = 0; cur < Yodo_PlayerIds.Length; cur++)
                {
                    if (Yodo_PlayerIds[cur] == id)
                    {
                        myCursor = cur;
                        //Debug.Log($"[Yodo]_ResolveConfliction() another player sets cursor to [{myCursor}]");
                        return;
                    }
                }
                Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
                _AppendLocalId();
                RequestSerialization();
                //Debug.Log($"[Yodo]_ResolveConfliction() new myCursor is [{myCursor}]");
            }
        }

        private void _UpdateTags()
        {
            if (Yodo_PlayerIds == null) { Debug.LogError("[Yodo]Yodo_PlayerIds Array not initialized at _UpdateTags"); return; }
            if (Yodo_TagStatus == null) { Debug.LogError("[Yodo]Yodo_TagStatus Array not initialized at _UpdateTags"); return; }
            VRCPlayerApi[] playerApisBuf = new VRCPlayerApi[MAX_PLAYERS_IN_INSTANCE];
            VRCPlayerApi.GetPlayers(playerApisBuf);
            Yodo_Dump();
            for (int cur = 0; cur < MAX_PLAYERS_IN_INSTANCE; cur++)
            {
                if (recentPlayerIds[cur] != Yodo_PlayerIds[cur])
                {
                    if (Yodo_PlayerIds[cur] < 0)
                    {
                        playerApis[cur] = null;
						if (markers[cur] != null)
						{
							Destroy(markers[cur]);
						}
                    }
                    else
                    {
                        foreach (VRCPlayerApi playerApi in playerApisBuf)
                        {
                            if (playerApi == null) { continue; }
                            if (!playerApi.IsValid()) { continue; }

                            if (playerApi.playerId == Yodo_PlayerIds[cur])
                            {
                                playerApis[cur] = playerApi;
								break;
                            }
                        }
						if (markers[cur] == null)
						{
							markers[cur] = Instantiate(Yodo_MarkersRoot,transform,false);
						}
                        markers[cur].SetActive(true);
                    }
                    _UpdateTagObject(cur);
                    recentTagStatus[cur] = Yodo_TagStatus[cur];
                    recentPlayerIds[cur] = Yodo_PlayerIds[cur];
                }
                else if (recentTagStatus[cur] != Yodo_TagStatus[cur])
                {
                    _UpdateTagObject(cur);
                    recentTagStatus[cur] = Yodo_TagStatus[cur];
                }
            }
        }

        private void _UpdateTagObject(int player_cur)
        {
			if (markers[player_cur] == null) { return; }
            for (int button_id = 0; button_id < MAX_BUTTONS; button_id++)
            {
                GameObject tag = _GetTagInMarker(markers[player_cur], button_id);
                if (tag)
                {
                    tag.SetActive(((1LU << button_id) & Yodo_TagStatus[player_cur]) != 0LU);
                }
            }

            // 子要素が0個のPanelは非表示にする
            Image[] imgs = markers[player_cur].GetComponentsInChildren<Image>(true);
            foreach (Image img in imgs)
            {
                if (string.Compare("Panel", 0, img.name, 0, 5) == 0)
                {
                    img.gameObject.SetActive(_HasActiveObjectInChild(img.transform));
                }
            }
        }

        private bool _HasActiveObjectInChild(Transform target)
        {
            foreach (Transform child in target)
            {
                if (child.gameObject.activeSelf)
                {
                    return true;
                }
            }
            return false;
        }

        private GameObject _GetTagInMarker(GameObject marker, int button_id)
        {
            Transform[] tags = marker.GetComponentsInChildren<Transform>(true);    // タグはButtonコンポーネントじゃなくてもいいのでTransformで全検索
            string targetTagName = $"Button ({button_id})";
            if (tags == null) { return null; }
            foreach (Transform tag in tags)
            {
                if (tag.name == targetTagName)
                {
                    return tag.gameObject;
                }
            }
            return null;
        }

        private bool _IsAttachedTag(int id, ulong status)
        {
            return (status & (1LU << id)) != 0LU;
        }

        private ulong _AttachTag(int id, ulong status)
        {
            return status | (1LU << id);
        }
        private ulong _DetachTag(int id, ulong status)
        {
            return status & ~(1LU << id);
        }

        private void _AppendLocalId()
        {
            for (int cur = 0; cur < Yodo_PlayerIds.Length; cur++)
            {
                if (Yodo_PlayerIds[cur] < 0)
                {
                    myCursor = cur;
                    Yodo_PlayerIds[myCursor] = Networking.LocalPlayer.playerId;
                    Yodo_TagStatus[myCursor] = 0LU;
                    break;
                }
            }
        }

        private void _TagButton(int button_id)
        {
            if (Yodo_PlayerIds == null) { Debug.LogError("[Yodo]Yodo_PlayerIds Array not initialized at _TagButton"); return; }
            if (Yodo_TagStatus == null) { Debug.LogError("[Yodo]Yodo_TagStatus Array not initialized at _TagButton"); return; }
            if (!initialized) { return; }

            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);

            if (myCursor < 0)  // 初回登録
            {
                _AppendLocalId();
            }

            //        Debug.Log($"[Yodo]_TagButton({button_id}) at {myCursor}, status changed from {Yodo_TagStatus[myCursor]:X8}.");
            if (_IsAttachedTag(button_id, Yodo_TagStatus[myCursor]))
            {
                Yodo_TagStatus[myCursor] = _DetachTag(button_id, Yodo_TagStatus[myCursor]);
            }
            else
            {
                Yodo_TagStatus[myCursor] = _AttachTag(button_id, Yodo_TagStatus[myCursor]);
            }
            RequestSerialization();
            _UpdateTags();
        }

        private void Yodo_Dump()    // デバッグ用
        {
            string dump = "[Yodo][Dump]";
            if (Yodo_PlayerIds != null)
            {
                for (int cur = 0; cur < MAX_PLAYERS_IN_INSTANCE; cur++)
                {
                    dump += $"[{cur},{Yodo_PlayerIds[cur]},{Yodo_TagStatus[cur]:X8}],";
                }
                Debug.Log(dump);
            }
            else
            {
                Debug.Log("[Yodo][Dump]Arrays not initialized");
            }
        }
        public void Yodo_TagButton_0() { _TagButton(0); }
        public void Yodo_TagButton_1() { _TagButton(1); }
        public void Yodo_TagButton_2() { _TagButton(2); }
        public void Yodo_TagButton_3() { _TagButton(3); }
        public void Yodo_TagButton_4() { _TagButton(4); }
        public void Yodo_TagButton_5() { _TagButton(5); }
        public void Yodo_TagButton_6() { _TagButton(6); }
        public void Yodo_TagButton_7() { _TagButton(7); }
        public void Yodo_TagButton_8() { _TagButton(8); }
        public void Yodo_TagButton_9() { _TagButton(9); }
        public void Yodo_TagButton_10() { _TagButton(10); }
        public void Yodo_TagButton_11() { _TagButton(11); }
        public void Yodo_TagButton_12() { _TagButton(12); }
        public void Yodo_TagButton_13() { _TagButton(13); }
        public void Yodo_TagButton_14() { _TagButton(14); }
        public void Yodo_TagButton_15() { _TagButton(15); }
        public void Yodo_TagButton_16() { _TagButton(16); }
        public void Yodo_TagButton_17() { _TagButton(17); }
        public void Yodo_TagButton_18() { _TagButton(18); }
        public void Yodo_TagButton_19() { _TagButton(19); }
        public void Yodo_TagButton_20() { _TagButton(20); }
        public void Yodo_TagButton_21() { _TagButton(21); }
        public void Yodo_TagButton_22() { _TagButton(22); }
        public void Yodo_TagButton_23() { _TagButton(23); }
        public void Yodo_TagButton_24() { _TagButton(24); }
        public void Yodo_TagButton_25() { _TagButton(25); }
        public void Yodo_TagButton_26() { _TagButton(26); }
        public void Yodo_TagButton_27() { _TagButton(27); }
        public void Yodo_TagButton_28() { _TagButton(28); }
        public void Yodo_TagButton_29() { _TagButton(29); }
        public void Yodo_TagButton_30() { _TagButton(30); }
        public void Yodo_TagButton_31() { _TagButton(31); }
        public void Yodo_TagButton_32() { _TagButton(32); }
        public void Yodo_TagButton_33() { _TagButton(33); }
        public void Yodo_TagButton_34() { _TagButton(34); }
        public void Yodo_TagButton_35() { _TagButton(35); }
        public void Yodo_TagButton_36() { _TagButton(36); }
        public void Yodo_TagButton_37() { _TagButton(37); }
        public void Yodo_TagButton_38() { _TagButton(38); }
        public void Yodo_TagButton_39() { _TagButton(39); }
        public void Yodo_TagButton_40() { _TagButton(40); }
        public void Yodo_TagButton_41() { _TagButton(41); }
        public void Yodo_TagButton_42() { _TagButton(42); }
        public void Yodo_TagButton_43() { _TagButton(43); }
        public void Yodo_TagButton_44() { _TagButton(44); }
        public void Yodo_TagButton_45() { _TagButton(45); }
        public void Yodo_TagButton_46() { _TagButton(46); }
        public void Yodo_TagButton_47() { _TagButton(47); }
        public void Yodo_TagButton_48() { _TagButton(48); }
        public void Yodo_TagButton_49() { _TagButton(49); }
        public void Yodo_TagButton_50() { _TagButton(50); }
        public void Yodo_TagButton_51() { _TagButton(51); }
        public void Yodo_TagButton_52() { _TagButton(52); }
        public void Yodo_TagButton_53() { _TagButton(53); }
        public void Yodo_TagButton_54() { _TagButton(54); }
        public void Yodo_TagButton_55() { _TagButton(55); }
        public void Yodo_TagButton_56() { _TagButton(56); }
        public void Yodo_TagButton_57() { _TagButton(57); }
        public void Yodo_TagButton_58() { _TagButton(58); }
        public void Yodo_TagButton_59() { _TagButton(59); }
        public void Yodo_TagButton_60() { _TagButton(60); }
        public void Yodo_TagButton_61() { _TagButton(61); }
        public void Yodo_TagButton_62() { _TagButton(62); }
        public void Yodo_TagButton_63() { _TagButton(63); }
    }
}
