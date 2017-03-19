using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;


/// <summary>
/// float型/int型をRGBAに変換するための共用体もどき
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public struct UnionFloatToRGBA
{
    [FieldOffset(0)]
    public float f;

    [FieldOffset(0)]
    public int i;

    [FieldOffset(0)]
    public byte a;

    [FieldOffset(1)]
    public byte b;

    [FieldOffset(2)]
    public byte g;

    [FieldOffset(3)]
    public byte r;
}

public static class MeshVertexTextureUtil {

    #region decode_function
    /// <summary>
    /// Color To Int
    /// </summary>
    /// <param name="col"></param>
    /// <returns></returns>
    public static int GetColorToInt(Color col)
    {
        const int xx = 255;
        const int yy = 256;
        int a = Mathf.CeilToInt(col.a * xx);
        int b = Mathf.CeilToInt(col.b * xx) * yy;
        int g = Mathf.CeilToInt(col.g * xx) * yy * yy;
        int r = Mathf.CeilToInt(col.r * xx) * yy * yy * yy;

        return a + b + g + r;
    }

    static UnionFloatToRGBA frgba;

    /// <summary>
    /// Colot To Float
    /// </summary>
    /// <param name="col"></param>
    /// <returns></returns>
    public static float GetColorToFloat(Color col)
    {
        int bai = 255;
        byte a = (byte)Mathf.CeilToInt(col.a * bai);
        byte b = (byte)Mathf.CeilToInt(col.b * bai);
        byte g = (byte)Mathf.CeilToInt(col.g * bai);
        byte r = (byte)Mathf.CeilToInt(col.r * bai);

        frgba.a = a;
        frgba.b = b;
        frgba.g = g;
        frgba.r = r;
        float f = frgba.f;

        return f;
    }

    public static int GetMeshTopologyNum(Mesh mesh)
    {
        MeshTopology topology = mesh.GetTopology(0);

        int num = 0;
        switch (topology)
        {
            case MeshTopology.Points:
                num = 1;
                break;
            case MeshTopology.Lines:
            case MeshTopology.LineStrip:
                num = 2;
                break;
            case MeshTopology.Triangles:
                num = 3;
                break;
            case MeshTopology.Quads:
                num = 4;
                break;
        }
        return num;
    }
    #endregion

    #region encode_function
    /// <summary>
    /// Float to Color
    /// </summary>
    /// <param name="f"></param>
    /// <returns></returns>
    public static Color GetFloatToColor(float f)
    {
        UnionFloatToRGBA data = new UnionFloatToRGBA();
        data.f = f;
        //Debug.Log("f " + f + " r " + data.r + " g " + data.g + " b " + data.b + " a " + data.a);
        return new Color32(data.r, data.g, data.b, data.a);
    }

    /// <summary>
    /// Int to Color
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public static Color GetIntToColor(int i)
    {
        UnionFloatToRGBA data = new UnionFloatToRGBA();
        data.i = i;
        //Debug.Log("i " + i + " r " + data.r + " g " + data.g + " b " + data.b + " a " + data.a);
        return new Color32(data.r, data.g, data.b, data.a);
    }

    /// <summary>
    /// 正規化済みVector3をRGBAに変換
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public static Color GetNormalizedVector3ToColor(Vector3 v)
    {
        return new Color(v.x, v.y, v.z, 1);
    }

    /// <summary>
    /// Get Vertex Num from MeshVertexTexture
    /// </summary>
    /// <param name="vertexTex"></param>
    /// <returns></returns>
    public static int GetVertexNum(Texture2D vertexTex)
    {
        Color col = vertexTex.GetPixel(0, 0);
        return GetColorToInt(col);
    }

    /// <summary>
    /// Get Mesh Size from MeshVertexTexture
    /// </summary>
    /// <param name="vertexTex"></param>
    /// <returns></returns>
    public static Vector3 GetMeshSize(Texture2D vertexTex)
    {
        float x = GetColorToFloat(vertexTex.GetPixel(1, 0));
        float y = GetColorToFloat(vertexTex.GetPixel(2, 0));
        float z = GetColorToFloat(vertexTex.GetPixel(3, 0));

        return new Vector3(x, y, z);
    }

    /// <summary>
    /// Get Mesh Center Positin from MeshVertexTexture
    /// </summary>
    /// <param name="vertexTex"></param>
    /// <returns></returns>
    public static Vector3 GetMeshCenter(Texture2D vertexTex)
    {
        float x = GetColorToFloat(vertexTex.GetPixel(4, 0));
        float y = GetColorToFloat(vertexTex.GetPixel(5, 0));
        float z = GetColorToFloat(vertexTex.GetPixel(6, 0));

        return new Vector3(x, y, z);
    }

    /// <summary>
    /// Get MeshTopologyNum from MeshVertexTexture
    /// </summary>
    /// <param name="vertexTex"></param>
    /// <returns></returns>
    public static int GetMeshTopologyNum(Texture2D vertexTex)
    {
        Color col = vertexTex.GetPixel(7, 0);
        return GetColorToInt(col);
    }
    #endregion
}
