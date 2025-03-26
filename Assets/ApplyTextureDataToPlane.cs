using UnityEngine;

public class ApplyTextureDataToPlane : MonoBehaviour
{
    public TextureData textureData;
    public Material material;

    void Start()
    {
        if (textureData != null && material != null)
        {
            textureData.ApplyToMaterial(material);
        }
        else
        {
            Debug.LogError("ApplyTextureDataToPlane: TextureData or Material is missing!");
        }
    }
}
