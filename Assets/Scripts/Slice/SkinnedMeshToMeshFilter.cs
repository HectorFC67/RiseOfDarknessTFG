using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinnedMeshToMeshFilter : MonoBehaviour
{
    private SkinnedMeshRenderer skinnedMeshRenderer;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh bakedMesh;

    void Start()
    {
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();

        // Buscar o crear un MeshFilter en el objeto
        GameObject bakedMeshHolder = new GameObject("BakedMeshHolder");
        bakedMeshHolder.transform.SetParent(transform);
        bakedMeshHolder.transform.localPosition = Vector3.zero;
        bakedMeshHolder.transform.localRotation = Quaternion.identity;

        meshFilter = bakedMeshHolder.AddComponent<MeshFilter>();
        meshRenderer = bakedMeshHolder.AddComponent<MeshRenderer>();

        // 🔹 Asegurar que la malla no sea visible
        meshRenderer.enabled = false;
        bakedMesh = new Mesh();
    }

    void Update()
    {
        skinnedMeshRenderer.BakeMesh(bakedMesh); // Actualiza la malla con la animación
        meshFilter.mesh = bakedMesh; // Pasa la malla al MeshFilter
    }

    public Mesh GetBakedMesh()
    {
        skinnedMeshRenderer.BakeMesh(bakedMesh);
        return bakedMesh;
    }

    public void DestroyBakedMesh()
    {
        if (meshFilter != null)
        {
            Destroy(meshFilter.gameObject); // 🔹 Elimina por completo el objeto
        }
    }
}
