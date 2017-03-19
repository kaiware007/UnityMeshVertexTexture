using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Runtime.InteropServices;


public static class MeshVertexTextureCreator {

    const int texWidthMax = 8192;
    const int texHeightMax = 8192;

    static string directoryName = "MeshVertexTextures";

    public static void Create(Mesh mesh)
    {

        Vector3[] vertices = mesh.vertices;
        int[] indices = mesh.GetIndices(0);
        List<Vector3> vertexList = new List<Vector3>();
        Bounds bounds = mesh.bounds;
        Vector3 boundsMax = bounds.max;
        Vector3 boundsCenter = bounds.center;

        Debug.Log("Bounds max " + bounds.max + " extents " + bounds.extents + " center " + bounds.center);

        // Index順にVertex格納(正規化）
        for (int i = 0; i < indices.Length; i++)
        {
            int idx = indices[i];
            Vector3 v = vertices[idx];

            v.x = (v.x / boundsMax.x) * 0.5f + 0.5f;
            v.y = (v.y / boundsMax.y) * 0.5f + 0.5f;
            v.z = (v.z / boundsMax.z) * 0.5f + 0.5f;

            vertexList.Add(v);
        }
        
        int texWidthCount = vertexList.Count;
        int texHeight = 4;  // 1行目はBoundsなどの定義データ部、2行目から頂点データ部
        int positionLines = 1;

        // 最低8ピクセル必要らしい
        if (texWidthCount < 8)
        {
            texWidthCount = 8;
        }

        if (texWidthCount > texWidthMax)
        {
            // 8K以上は折り返す
            int remain = texWidthCount;
            while (positionLines < texHeightMax)
            {
                positionLines++;
                remain -= texWidthMax-1;
                if (remain < texWidthMax)
                    break;
            }
            texWidthCount = texWidthMax;
        }

        texHeight = RoundUpPowerOf2(Mathf.Max(positionLines, texHeight));

        if (texHeight >= texHeightMax)
        {
            Debug.LogError("Texture Maximum Size Over 8K");
            return;
        }

        int texWidth = RoundUpPowerOf2(texWidthCount);

        Debug.Log("vertexList " + vertexList.Count + " texWidth " + texWidth);
        Debug.Log("texWidth " + texWidth + " texHeight " + texHeight + " positionLines " + positionLines);

        // テクスチャ作成
        Texture2D tex = new Texture2D(texWidth, texHeight, TextureFormat.RGBA32, false);

        Color vertexCount = MeshVertexTextureUtil.GetIntToColor(vertexList.Count);

        // メッシュの情報をColorに変換
        Color maxX = MeshVertexTextureUtil.GetFloatToColor(boundsMax.x);
        Color maxY = MeshVertexTextureUtil.GetFloatToColor(boundsMax.y);
        Color maxZ = MeshVertexTextureUtil.GetFloatToColor(boundsMax.z);

        Color centerX = MeshVertexTextureUtil.GetFloatToColor(boundsCenter.x);
        Color centerY = MeshVertexTextureUtil.GetFloatToColor(boundsCenter.y);
        Color centerZ = MeshVertexTextureUtil.GetFloatToColor(boundsCenter.z);

        int meshTopologyNum = MeshVertexTextureUtil.GetMeshTopologyNum(mesh);
        Color meshTopology = MeshVertexTextureUtil.GetIntToColor(meshTopologyNum);

        Debug.Log("vertexCount: " + vertexList.Count + " " + vertexCount + " MeshTopologyNum " + meshTopologyNum);
        Debug.Log("Max X: " + boundsMax.x + " : " + maxX + " Y: " + boundsMax.y + " : " + maxY + " Z: " + boundsMax.z + " : " + maxZ);
        Debug.Log("Center X: " + boundsCenter.x + " : " + centerX + " Y: " + boundsCenter.y + " : " + centerY + " Z: " + boundsCenter.z + " : " + centerZ);

        // テクスチャに書き込む
        int index = 0;

        // [0] VertexCount
        tex.SetPixel(index++, 0, vertexCount);

        // [1..3] Bounds.Max(x,y,z)
        tex.SetPixel(index++, 0, maxX);
        tex.SetPixel(index++, 0, maxY);
        tex.SetPixel(index++, 0, maxZ);

        // [4..6] Bounds.Center(x,y,z)
        tex.SetPixel(index++, 0, centerX);
        tex.SetPixel(index++, 0, centerX);
        tex.SetPixel(index++, 0, centerX);

        // [7] meshTopologyNum 
        tex.SetPixel(index++, 0, meshTopology);

        for (int y = 0; y < positionLines; y++)
        {
            for(int x = 0; x < texWidth; x++)
            {
                int idx = y * texWidth + x;
                if (idx >= vertexList.Count)
                    break;
                Color pos = MeshVertexTextureUtil.GetNormalizedVector3ToColor(vertexList[idx]);
                //Debug.Log("[" + x + "," + y + "] " + idx + " pos " + vertexList[idx] + " : " + pos);

                tex.SetPixel(x, y + 1, pos);
            }
        }
        tex.Apply();

        // 保存
        string path = Application.dataPath;
        path += "/" + directoryName;
        if (!File.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        path += "/" + mesh.name + ".png";
        string relativePath = "Assets/" + directoryName + "/" + mesh.name + ".png";

        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        AssetDatabase.ImportAsset(relativePath);
        Object.DestroyImmediate(tex);

        TextureImporter texImporter = AssetImporter.GetAtPath(relativePath) as TextureImporter;

        if(texImporter != null)
        {
            texImporter.wrapMode = TextureWrapMode.Clamp;
            texImporter.filterMode = FilterMode.Point;
            texImporter.anisoLevel = 0;
            texImporter.mipmapEnabled = false;
            texImporter.textureCompression = TextureImporterCompression.Uncompressed;
            texImporter.maxTextureSize = Mathf.Max(texWidth, texHeight);
            texImporter.isReadable = true;
            texImporter.SaveAndReimport();
        }

        Debug.Log("Create Mesh Vertex Texture " + path);
    }
    
    /// <summary>
    /// 指定数X以上で最も近い２の乗数を返す
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    static int RoundUpPowerOf2(int x)
    {
        x--;
        x |= x >> 1;
        x |= x >> 2;
        x |= x >> 4;
        x |= x >> 8;
        x |= x >> 16;

        return (x + 1);
    }
}
