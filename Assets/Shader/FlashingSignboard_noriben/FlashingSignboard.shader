Shader "Noriben/FlashingSignboard"
{
	Properties
	{
		_Color("Color", Color) = (1, 0, 0, 1)
		_MainTex("Texture", 2D) = "white" {}
		//点滅のスピード
		_Speed("FlashSpeed", Range(0, 10)) = 1.0
		//二箇所の点滅の間隔
		_Interval("Interval", Range(0, 1)) = 1.0
		//ピクセルの大きさ
		_Brightness("Brightness", Range(1, 0.01)) = 1
		//周辺減光
		_Vignette("Vignette", Range(0, 1)) = 1
		//環境マップ
		_Reflection("Reflection", Range(0, 1)) = 0.2
		//カリングのトグル設定
		[Enum(UnityEngine.Rendering.CullMode)] _Cull("Culling", Int) = 0
		
	}
	
	SubShader
	{
		Tags
		{
			"RenderType"="Opaque"
			"Queue" = "Geometry"
		}

		LOD 200
		Cull [_Cull] //カリングの設定
		
		
		Pass
		{
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog // fog
			#include "UnityCG.cginc"
			
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 pos2 : TEXCOORD1;
				float3 normal : TEXCOORD2;
				UNITY_FOG_COORDS(3) //fog
			};
			
			fixed4 _Color;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _Speed;
			float _Brightness;
			float _Interval;
			float _Vignette;
			float _Reflection;
			
			v2f vert (appdata v)
			{
				v2f o;
				
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.pos2 = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.normal = UnityObjectToWorldNormal(v.normal);
				o.uv = v.uv;
				UNITY_TRANSFER_FOG(o, o.vertex); //fog
				return o;
			}

			//ランダム
			float random (fixed2 p) {
				return frac(sin(dot(p, fixed2(12.9898, 78.233))) * 43758.5453);
			}

			fixed4 frag (v2f i) : SV_Target
			{	
				//環境マップ
                i.normal = normalize(i.normal);
                half3 viewDir = normalize(_WorldSpaceCameraPos - i.pos2);
                half3 reflDir = reflect(-viewDir, i.normal);
                // キューブマップと反射方向のベクトルから反射先の色を取得する
                half4 refColor = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, reflDir);
                refColor *= _Reflection; //Reflectionスライダーを反映

				float4 color = tex2D(_MainTex, TRANSFORM_TEX(i.uv, _MainTex));

				//点滅スピード
				float flashSpeed = _Time.y * _Speed;

				//インターバル
				float intervalTime = _Interval * 3.1415;

				//点滅のサインカーブ
				float flash01 = clamp(8.0 * acos(cos(flashSpeed)), 0.0 , 1.0); //いい感じの波形作成
				float flash02 = clamp(8.0 * acos(cos(flashSpeed + 0.1)), 0.0 , 1.0); //いい感じの波形作成
				float flash03 = clamp(8.0 * acos(cos(flashSpeed + 0.2)), 0.0 , 1.0); //いい感じの波形作成

				float flash04 = clamp(8.0 * acos(cos(flashSpeed + intervalTime)), 0.0 , 1.0); //いい感じの波形作成
				float flash05 = clamp(8.0 * acos(cos(flashSpeed + 0.1 + intervalTime)), 0.0 , 1.0); //いい感じの波形作成
				float flash06 = clamp(8.0 * acos(cos(flashSpeed + 0.2 + intervalTime)), 0.0 , 1.0); //いい感じの波形作成

				float flashcurv = flash01 * flash02 * flash03 * flash04 * flash05 * flash06;

				//周辺減光
				float r = distance(i.uv, fixed2(0.5, 0.5));
				float Vignette = 1.0 - smoothstep(0.0, 0.7, r) * _Vignette;

				//テクスチャと点滅の乗算・輝度変更
				color = (clamp(color * _Color * flashcurv * Vignette, 0.0, 1.0) / _Brightness) + refColor;

				//fog
				UNITY_APPLY_FOG(i.fogCoord, color);
				float4 Emissive = color;
				return Emissive;
				
				
			}
			
			ENDCG
		}
	}
}
			
