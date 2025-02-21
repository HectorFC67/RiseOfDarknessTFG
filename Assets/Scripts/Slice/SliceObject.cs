using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;
using UnityEngine.InputSystem;

public class SliceObject : MonoBehaviour
{
    public Transform startSlicePoint;
    public Transform endSlicePoint;
    public VelocityEstimator velocityEstimator;
    public LayerMask sliceableLayer;
    public Material crossSection;
    public float cutForce = 2000;

    void FixedUpdate()
    {
        bool hasHit = Physics.Linecast(startSlicePoint.position, endSlicePoint.position, out RaycastHit hit, sliceableLayer);
        if (hasHit)
        {
            GameObject target = hit.transform.gameObject;
            Slice(target);
        }
    }

    public void Slice(GameObject target)
    {
        if (target == null)
        {
            Debug.Log("❌ Slice() - No se encontró el objeto a cortar.");
            return;
        }

        Debug.Log($"✅ Slice() - Intentando cortar: {target.name}");

        Vector3 velocity = velocityEstimator.GetVelocityEstimate();
        Vector3 planeNormal = Vector3.Cross(endSlicePoint.position - startSlicePoint.position, velocity);
        planeNormal.Normalize();

        Mesh meshToSlice = GetMeshFromObject(target);
        if (meshToSlice == null)
        {
            Debug.Log("❌ Slice() - No se encontró una malla válida en el objeto.");
            return;
        }

        // Crear un GameObject temporal con MeshFilter para EzySlice
        GameObject tempObject = new GameObject("TempSliceObject");
        MeshFilter tempMeshFilter = tempObject.AddComponent<MeshFilter>();
        tempMeshFilter.mesh = meshToSlice;
        MeshRenderer tempMeshRenderer = tempObject.AddComponent<MeshRenderer>();
        tempMeshRenderer.materials = target.GetComponent<Renderer>().materials;

        // Usar EzySlice en el MeshFilter
        SlicedHull hull = tempObject.Slice(endSlicePoint.position, planeNormal);

        // 🔹 Destruir inmediatamente el objeto temporal después de cortar
        Destroy(tempObject);

        if (hull != null)
        {
            GameObject upperHull = hull.CreateUpperHull(target, crossSection);
            SetupSlicedComponent(upperHull);

            GameObject lowerHull = hull.CreateLowerHull(target, crossSection);
            SetupSlicedComponent(lowerHull);

            // 🔹 Si el objeto cortado tenía SkinnedMeshToMeshFilter, destruir su malla bakeada
            SkinnedMeshToMeshFilter meshBaker = target.GetComponent<SkinnedMeshToMeshFilter>();
            if (meshBaker != null)
            {
                meshBaker.DestroyBakedMesh();
            }

            Destroy(target); // 🔹 Eliminar el objeto original
        }
    }

    private Mesh GetMeshFromObject(GameObject target)
    {
        SkinnedMeshRenderer skinnedMeshRenderer = target.GetComponent<SkinnedMeshRenderer>();
        if (skinnedMeshRenderer != null)
        {
            Mesh bakedMesh = new Mesh();
            skinnedMeshRenderer.BakeMesh(bakedMesh);

            if (bakedMesh.vertexCount == 0)
            {
                Debug.Log("❌ GetMeshFromObject() - BakeMesh() no generó una malla válida.");
                return null;
            }

            Debug.Log($"✅ GetMeshFromObject() - Se bakeó la malla con {bakedMesh.vertexCount} vértices.");
            return bakedMesh;
        }

        MeshFilter meshFilter = target.GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            return meshFilter.mesh;
        }

        Debug.Log("❌ GetMeshFromObject() - No se encontró un MeshFilter o SkinnedMeshRenderer.");
        return null;
    }

    public void SetupSlicedComponent(GameObject slicedObject)
    {
        if (slicedObject == null)
        {
            Debug.Log("❌ SetupSlicedComponent() - El objeto cortado es nulo.");
            return;
        }

        slicedObject.layer = LayerMask.NameToLayer("Sliceable");

        Rigidbody rb = slicedObject.AddComponent<Rigidbody>();
        MeshCollider collider = slicedObject.AddComponent<MeshCollider>();

        if (collider.sharedMesh == null)
        {
            Debug.Log("❌ SetupSlicedComponent() - El MeshCollider no tiene una malla asignada.");
            return;
        }

        collider.convex = true;
        rb.AddExplosionForce(cutForce, slicedObject.transform.position, 1);

        Debug.Log($"✅ SetupSlicedComponent() - Se agregó Rigidbody y MeshCollider correctamente.");
    }
}
