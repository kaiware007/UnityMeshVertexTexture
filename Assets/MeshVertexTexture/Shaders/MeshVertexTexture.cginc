sampler2D _VertexTex;
float4 _VertexTex_TexelSize;

int _VertexCount;		// 頂点数
float3 _MeshSize;		// メッシュのサイズ
float3 _MeshCenter;		// メッシュの中心座標
int _MeshTopologyNum;	

// 頂点番号のはみ出しチェック
float CheckVertexIndex(float idx) {
	idx = idx % _VertexCount;
	idx = (idx < 0) ? idx + _VertexCount : idx; // 始点はみ出しチェック
	idx = (idx % _MeshTopologyNum) >= (_MeshTopologyNum - 1) ? idx + 1 : idx;	// MeshTopologyチェック
	idx = (idx > ((float)_VertexCount)) ? idx - _VertexCount : idx; // 終点はみ出しチェック
	return idx;
}

// 頂点番号からVertexTexture上のUV座標取得
float2 GetVertexPositionUV(int idx) {
	float idxX = (idx) % (_VertexTex_TexelSize.z);
	float idxY = 1.0 + (int)(idx / _VertexTex_TexelSize.z);

	return float2(idxX, idxY) * _VertexTex_TexelSize.xy;
}

// 頂点番号から頂点座標取得
float3 GetVertexPosition(int idx) {
	float2 uv = GetVertexPositionUV(idx);
	float3 pos = tex2Dlod(_VertexTex, float4(uv, 0, 0)).rgb;
	
	return _MeshCenter + (pos - 0.5) * 2.0 * _MeshSize;
}