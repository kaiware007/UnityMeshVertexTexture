using UnityEngine;
using UnityEditor;

public class MeshVertexTextureCreatorWindow : ScriptableWizard {

    public Mesh mesh;

    [MenuItem("Window/MeshVertexTexture Creator")]
    static void Open()
    {
        DisplayWizard<MeshVertexTextureCreatorWindow>("Create MeshVertexTexture");
    }

    private void OnWizardCreate()
    {
        // todo: Meshの頂点座標をテクスチャに書き込む
        if (mesh != null)
        {
            MeshVertexTextureCreator.Create(mesh);
        }
    }
}
