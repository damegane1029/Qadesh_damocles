
ワールドメガホン / WorldMegaphone 利用規約

-------------------
VRChatのワールド全体に声を届けることができるメガホンを配置できるUnity用のPrefabセットです。
メガホン3Dモデルと全8色のテクスチャ・マテリアルが付属します。(マイク3Dモデルのおまけ付き)
PC/Questでの動作確認済み☆

※使用するにはUnityにVCC(VRChat Creator Companion)で作成したUnity 2022 World Projectが必要です。

■VRChat上のデモワールドで試用頂けます
https://vrchat.com/home/launch?worldId=wrld_a0ebb20f-db83-476c-85da-c8d9b6b15920

-------------------
【 必要動作環境 】
- VCC(VRChat Creator Companion)
https://vrchat.com/home/download
Unity 2022 World Projectを作成し、本UnityPackageをインポートして下さい。

-------------------
【 内容物 】
UnityPackage形式(Unity2022.3.22f1で作成)

■ モデル関連
・ メガホン3Dモデルfbx 2253△ポリゴン
(BaseColorテクスチャ8種 / Metalicテクスチャ / NormalMapテクスチャ / Roughnessテクスチャ / Emissiveテクスチャ 付き)
・ マイク3Dモデルfbx 966△ポリゴン
(BaseColorテクスチャ / Metalicテクスチャ / NormalMapテクスチャ / Roughnessテクスチャ / Emissiveテクスチャ 2種類 付き)

■ Prefab関連
・ ワールドメガホン 全8種
・ ワールドマイク
・ ワールドマイク（位置リセットボタン付）
・ 位置リセットボタンサンプル

-------------------
【 使用方法 】
1. UnityPackageをUnityにインポートする。(VCCで作成されたUnity 2022 World Projectが必要です)
2. シーン内にPrefabを配置
3. [任意] 位置リセット時間を設定したい場合は、配置したPrefab内のWorldMegaphoneCoreのUdonスクリプトに位置リセット秒数を設定して下さい。(0=無効)
4. [任意] ワールドの音声減衰距離を変更している場合、配置したPrefab内のWorldMegaphoneCoreのUdonスクリプトに通常時の音声減衰距離を設定して下さい。
5. VRChatにワールドとしてアップロードしてVRChat上で動作を確認。

※※※声のボリュームを変えたい場合※※※
VRChatの仕様上、他のプレイヤーの声の大きさは本来メニューからでないと変更できないので、ワールドメガホンでは声の届く範囲（減衰距離）を最大値まで上げて擬似的に拡声を実現しています。

そのため、声のボリュームを変えたい場合はメガホンの減衰距離を最大値から少し下げる事で再現可能です。

WorldMgaphonePrefabの中にWorldMegaphoneCoreというオブジェクトがあるので、インスペクタからWorldMegaphoneCoreについているスクリプトの下部「拡声時ボイス設定」の数値を下げて下さい。
----------------------
Voice Distance Near_Loud = 0～10
Voice Distance Far_Loud = (ワールドのサイズに合わせて1000～10000)
Voice Volumetric Radius_Loud = 0～10
----------------------
数字はメートル表記で、声の届く範囲もそれに応じて狭くなってしまいますのでご注意下さい。

-------------------
【 使用上の注意 】
・ 他のボイス音量変更スクリプトと競合する為、同様のスクリプトとの併用はしないで下さい。
・ VRChatやUdonSharpの仕様変更により動作しなくなる場合があります。ご了承下さい。
・ 反射を綺麗に表現するには、ライティング及びReflection Probeの設定を行って下さい。
・ テクスチャ解像度が高めなので、コンパイル後のデータ容量を気にする場合は必要に応じてInspectorより画像の解像度の変更等を行って下さい。(デフォルトで圧縮設定済みです)

-------------------
【 利用規約 】
・ 商用利用可
・ 改変可
・ データを取り出せる形での再配布禁止
・ クレジット表記は任意
・ 本データの利用によって生じた損害等の一切の責任を負いかねます
・ 利用規約は予告なく変更される場合があります

■法人利用について
法人等で複数人でデータを共有される場合は、人数分の金額をBOOST購入お願い致します。

-------------------
【 更新履歴 】
- 2023/06/08 v1.1
マイクの使用中の見た目を更新しました。
VRChat Creator Companion(VCC)で正しくインポートされるように対応致しました。

- 2024/08/28 v1.2
Unity2022で正しく動作するように更新。
WorldMegaphoneパラメーターに位置リセット秒数を追加。(0=無効)
WorldMegaphoneCoreのパラメーターにゲインを追加。
サンプルプロジェクトに位置リセットボタン付きのサンプルを追加。

-------------------
制作： 坪倉輝明
ご不明点ございましたら、twitter: @kohack_v までDM下さい。