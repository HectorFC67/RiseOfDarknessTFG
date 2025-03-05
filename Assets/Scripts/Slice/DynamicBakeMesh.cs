using UnityEngine;

/// <summary>
/// Se encarga de crear un MeshFilter + MeshRenderer "oculto",
/// bakear la malla del SkinnedMeshRenderer y conmutar cuando se requiera.
/// </summary>
public class DynamicBakeMesh : MonoBehaviour
{
    [Header("SkinnedMeshRenderer a bakear")]
    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;

    private MeshFilter dynamicMeshFilter;
    private MeshRenderer dynamicMeshRenderer;
    private Mesh bakedMesh;

    private void Awake()
    {
        // Si no se asignó por Inspector, lo buscamos en los hijos
        if (skinnedMeshRenderer == null)
            skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

        // Creamos un GameObject hijo que guardará el MeshFilter + MeshRenderer
        GameObject dynamicMeshGO = new GameObject("DynamicMesh");
        dynamicMeshGO.transform.SetParent(this.transform);
        dynamicMeshGO.transform.localPosition = Vector3.zero;
        dynamicMeshGO.transform.localRotation = Quaternion.identity;
        dynamicMeshGO.transform.localScale = Vector3.one;

        dynamicMeshFilter = dynamicMeshGO.AddComponent<MeshFilter>();
        dynamicMeshRenderer = dynamicMeshGO.AddComponent<MeshRenderer>();

        // Lo ocultamos hasta que lo necesitemos
        dynamicMeshRenderer.enabled = false;

        // Inicializamos la malla "bakeada"
        bakedMesh = new Mesh();
    }

    /// <summary>
    /// Hornea la malla del SkinnedMeshRenderer a la malla estática "bakedMesh".
    /// </summary>
    public void BakeCurrentMesh()
    {
        if (!skinnedMeshRenderer)
        {
            Debug.LogWarning("DynamicBakeMesh: No se encontró SkinnedMeshRenderer.");
            return;
        }

        skinnedMeshRenderer.BakeMesh(bakedMesh);

        if (bakedMesh.vertexCount == 0)
        {
            Debug.LogWarning("DynamicBakeMesh: BakeMesh devolvió 0 vértices.");
            return;
        }

        dynamicMeshFilter.sharedMesh = bakedMesh;
        // Opcional: Copiar materiales para que se vea igual
        dynamicMeshRenderer.sharedMaterials = skinnedMeshRenderer.sharedMaterials;

        Debug.Log($"DynamicBakeMesh: Malla bakeada con {bakedMesh.vertexCount} vértices.");
    }

    /// <summary>
    /// Apaga el SkinnedMeshRenderer y enciende el MeshRenderer con la malla estática.
    /// </summary>
    public void SwitchToBakedMesh()
    {
        if (skinnedMeshRenderer)
            skinnedMeshRenderer.enabled = false;

        dynamicMeshRenderer.enabled = true;
    }

    /// <summary>
    /// Devuelve el GameObject que contiene el MeshFilter+MeshRenderer estático.
    /// </summary>
    public GameObject GetDynamicMeshObject()
    {
        return dynamicMeshFilter != null ? dynamicMeshFilter.gameObject : null;
    }
}
