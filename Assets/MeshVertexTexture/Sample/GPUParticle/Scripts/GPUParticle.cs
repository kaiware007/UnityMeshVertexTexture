using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

struct ParticleData
{
    public Vector3 position;
    public Vector3 orgPosition;
    public float vertexIndex; // 参照する頂点の番号
    public Color color;

    public ParticleData(Vector3 pos, Vector3 org, float vidx, Color col)
    {
        position = pos;
        orgPosition = org;
        vertexIndex = vidx;
        color = col;
    }
}

public class GPUParticle : MonoBehaviour {
    #region define
    const int THREAD_GROUP_X = 8;
    #endregion

    #region public
    public Material material;
    public ComputeShader cs;
    public Texture2D vertexTex;

    public int particleNum = 10000;
    public float vertexSpeed = 10f;
    public float vertexScale = 1;
    public float particleScale = 0.05f;

    [Range(0, 1)]
    public float positionRatio = 0;
    #endregion

    #region private
    ComputeBuffer particleBuffer;
    //Texture2D vertexTex;
    int vertexNum;
    Vector3 meshSize;
    Vector3 meshCenter;
    int meshTopologyNum;
    Vector4 texelSize;
    #endregion

    void InitializeParticle()
    {
        //vertexTex = material.GetTexture("_VertexTex") as Texture2D;
        material.SetTexture("_VertexTex", vertexTex);
        particleNum = Mathf.CeilToInt((float)particleNum / THREAD_GROUP_X) * THREAD_GROUP_X;
        particleBuffer = new ComputeBuffer(particleNum, Marshal.SizeOf(typeof(ParticleData)));

        vertexNum = MeshVertexTextureUtil.GetVertexNum(vertexTex);
        meshSize = MeshVertexTextureUtil.GetMeshSize(vertexTex);
        meshCenter = MeshVertexTextureUtil.GetMeshCenter(vertexTex);
        meshTopologyNum = MeshVertexTextureUtil.GetMeshTopologyNum(vertexTex);

        texelSize.x = 1f / vertexTex.width;
        texelSize.y = 1f / vertexTex.height;
        texelSize.z = vertexTex.width;
        texelSize.w = vertexTex.height;

        Debug.Log("Vertex Num " + vertexNum + " meshSize " + meshSize + " meshCenter " + meshCenter + " meshTopologyNum " + meshTopologyNum);
        ParticleData[] particles = new ParticleData[particleNum];
        float indexStep = (float)vertexNum / particleNum;
        for (int i = 0; i < particleNum; i++)
        {
            particles[i] = new ParticleData(Vector3.zero, Random.insideUnitSphere * 5, indexStep * i, Color.HSVToRGB((float)i / particleNum, 0.75f, 1));
            //particles[i] = new ParticleData(Random.onUnitSphere * 100f, indexStep * i, Color.red);
        }

        particleBuffer.SetData(particles);
    }

    //int GetColorToInt(Color col)
    //{
    //    const int xx = 255;
    //    const int yy = 256;
    //    int a = Mathf.CeilToInt(col.a * xx);
    //    int b = Mathf.CeilToInt(col.b * xx) * yy;
    //    int g = Mathf.CeilToInt(col.g * xx) * yy * yy;
    //    int r = Mathf.CeilToInt(col.r * xx) * yy * yy * yy;

    //    return a + b + g + r;
    //}
    
    //UnionFloatToRGBA frgba;
    //float GetColorToFloat(Color col)
    //{
    //    int bai = 255;
    //    byte a = (byte)Mathf.CeilToInt(col.a * bai);
    //    byte b = (byte)Mathf.CeilToInt(col.b * bai);
    //    byte g = (byte)Mathf.CeilToInt(col.g * bai);
    //    byte r = (byte)Mathf.CeilToInt(col.r * bai);
    //    //int xx = x1 + x2 + x3 + x4;
    //    frgba.a = a;
    //    frgba.b = b;
    //    frgba.g = g;
    //    frgba.r = r;
    //    float f = frgba.f;
    //    //Debug.Log("Color " + col + " a " + r + " b " + b + " g " + g + " r " + r);
    //    return f;
    //}

    //int GetVertexNum(Texture2D vertexTex)
    //{
    //    Color col = vertexTex.GetPixel(0, 0);
    //    return GetColorToInt(col);
    //}

    //Vector3 GetMeshSize(Texture2D vertexTex)
    //{
    //    float x = GetColorToFloat(vertexTex.GetPixel(1, 0));
    //    float y = GetColorToFloat(vertexTex.GetPixel(2, 0));
    //    float z = GetColorToFloat(vertexTex.GetPixel(3, 0));

    //    return new Vector3(x, y, z);
    //}

    //Vector3 GetMeshCenter(Texture2D vertexTex)
    //{
    //    float x = GetColorToFloat(vertexTex.GetPixel(4, 0));
    //    float y = GetColorToFloat(vertexTex.GetPixel(5, 0));
    //    float z = GetColorToFloat(vertexTex.GetPixel(6, 0));

    //    return new Vector3(x, y, z);
    //}

    //int GetMeshTopologyNum(Texture2D vertexTex)
    //{
    //    Color col = vertexTex.GetPixel(7, 0);
    //    return GetColorToInt(col);
    //}

    void UpdateParticle()
    {
        int kernel = cs.FindKernel("CSMain");
        cs.SetFloat("_DT", Time.deltaTime);
        cs.SetFloat("_VertexSpeed", vertexSpeed);
        cs.SetInt("_VertexCount", vertexNum);
        cs.SetVector("_MeshSize", meshSize * vertexScale);
        cs.SetVector("_MeshCenter", meshCenter);
        cs.SetInt("_MeshTopologyNum", meshTopologyNum);
        cs.SetVector("_VertexTex_TexelSize", texelSize);
        cs.SetTexture(kernel, "_VertexTex", vertexTex);
        cs.SetBuffer(kernel, "_ParticleBuffer", particleBuffer);

        cs.Dispatch(kernel, particleNum / THREAD_GROUP_X, 1, 1);
    }

	// Use this for initialization
	void Start () {
        InitializeParticle();
    }
	
	// Update is called once per frame
	void Update () {
        UpdateParticle();
    }

    void OnRenderObject()
    {

        //material.SetTexture("_MainTex", bulletsTexture);
        material.SetBuffer("_ParticleBuffer", particleBuffer);
        material.SetInt("_VertexCount", vertexNum);
        material.SetVector("_MeshSize", meshSize * vertexScale);
        material.SetVector("_MeshCenter", meshCenter);
        material.SetInt("_MeshTopologyNum", meshTopologyNum);
        material.SetFloat("_ParticleScale", particleScale);
        material.SetFloat("_PositionRatio", positionRatio);
        material.SetPass(0);

        Graphics.DrawProcedural(MeshTopology.Points, particleNum);
    }

    private void OnDestroy()
    {
        particleBuffer.Release();
        particleBuffer = null;
    }
}
