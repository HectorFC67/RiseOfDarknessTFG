using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;
using UnityEngine.InputSystem;
using UnityEngine.AI;

public class SliceObject : MonoBehaviour
{
    public Transform startSlicePoint;
    public Transform endSlicePoint;
    public VelocityEstimator velocityEstimator;
    public LayerMask sliceableLayer;
    public Material crossSection;
    public float cutForce = 2000;

    // Lista de armas para spawnear si el objeto cortado tiene el TAG "Crate".
    public List<GameObject> randomWeapons;

    void FixedUpdate()
    {
        bool hasHit = Physics.Linecast(startSlicePoint.position, endSlicePoint.position, out RaycastHit hit, sliceableLayer);
        if (hasHit)
        {
            // Obtenemos la raíz del objeto que recibe el corte
            GameObject target = hit.transform.root.gameObject;

            // Desactivamos la IA del propio target (por si fuera necesario)
            DeactivateEnemyAI(target);

            // Activamos el ragdoll y desactivamos RandomMovement en el padre del target
            ActivateRagdoll(target);

            // Realizamos la "slice"
            Slice(target);
        }
    }

    public void DeactivateEnemyAI(GameObject target)
    {
        RandomMovement enemyAI = target.GetComponent<RandomMovement>();
        if (enemyAI != null)
        {
            enemyAI.enabled = false;
            Debug.Log("✅ AI desactivada en el enemigo (componente en el mismo objeto).");
        }
    }

    public void ActivateRagdoll(GameObject target)
    {
        // 1. Buscar el padre del objeto que está recibiendo el corte
        Transform parentTransform = target.transform.parent;
        if (parentTransform != null)
        {
            // 2. Desactivar el RandomMovement en el padre
            RandomMovement parentRandomMovement = parentTransform.GetComponent<RandomMovement>();
            if (parentRandomMovement != null)
            {
                parentRandomMovement.enabled = false;
                Debug.Log("✅ Script RandomMovement desactivado en el padre del enemigo.");
            }
        }

        // 3. Desactivar el NavMeshAgent (si existe) en el target
        NavMeshAgent agent = target.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.isStopped = true;   // Detiene el movimiento actual
            agent.ResetPath();        // Limpia cualquier ruta pendiente
            Debug.Log("✅ NavMeshAgent detenido y desactivado en el enemigo.");
        }

        // 4. Deshabilitar el Animator en el target
        Animator animator = target.GetComponent<Animator>();
        if (animator != null)
        {
            animator.enabled = false;
        }

        // 5. Activar físicas en todos los rigidbodies hijos
        Rigidbody[] rigidbodies = target.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = false;
        }

        Debug.Log("✅ Ragdoll activado en el enemigo.");
    }

    public void Slice(GameObject target)
    {
        if (target == null)
        {
            Debug.Log("❌ Slice() - No se encontró el objeto a cortar.");
            return;
        }

        Debug.Log($"✅ Slice() - Intentando cortar: {target.name}");

        // Obtenemos la velocidad estimada y calculamos la normal del plano de corte
        Vector3 velocity = velocityEstimator.GetVelocityEstimate();
        Vector3 planeNormal = Vector3.Cross(endSlicePoint.position - startSlicePoint.position, velocity);
        planeNormal.Normalize();

        // Obtenemos la malla del objeto a cortar
        Mesh meshToSlice = GetMeshFromObject(target);
        if (meshToSlice == null)
        {
            Debug.Log("❌ Slice() - No se encontró una malla válida en el objeto.");
            return;
        }

        // Realizamos el corte con EzySlice
        SlicedHull hull = target.Slice(endSlicePoint.position, planeNormal);
        if (hull != null)
        {
            // Si el objeto tiene el tag "Crate", spawneamos un arma aleatoria
            if (target.CompareTag("Crate") && randomWeapons != null && randomWeapons.Count > 0)
            {
                Vector3 spawnPos = target.transform.position;
                Quaternion spawnRot = target.transform.rotation;

                int randomIndex = Random.Range(0, randomWeapons.Count);
                Instantiate(randomWeapons[randomIndex], spawnPos, spawnRot);

                Debug.Log("✅ Crate cortado: se ha instanciado un arma aleatoria.");
            }

            // Creamos las dos partes
            GameObject upperHull = hull.CreateUpperHull(target, crossSection);
            SetupSlicedComponent(upperHull);

            GameObject lowerHull = hull.CreateLowerHull(target, crossSection);
            SetupSlicedComponent(lowerHull);

            // Destruimos el objeto original
            Destroy(target);
        }
    }

    private Mesh GetMeshFromObject(GameObject target)
    {
        // Primero buscamos un SkinnedMeshRenderer en el padre para ver si hay animaciones
        SkinnedMeshRenderer skinnedMeshRenderer = target.GetComponentInParent<SkinnedMeshRenderer>();
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

        // Si no hay SkinnedMeshRenderer, buscamos un MeshFilter
        MeshFilter meshFilter = target.GetComponentInChildren<MeshFilter>();
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

        // Ajustamos el layer para que siga siendo "sliceable" si así lo deseas
        slicedObject.layer = LayerMask.NameToLayer("Sliceable");

        // Agregamos Rigidbody y MeshCollider para que el trozo cortado tenga físicas
        Rigidbody rb = slicedObject.AddComponent<Rigidbody>();
        MeshCollider collider = slicedObject.AddComponent<MeshCollider>();

        // Verificamos que el collider tenga asignada la malla
        if (collider.sharedMesh == null)
        {
            Debug.Log("❌ SetupSlicedComponent() - El MeshCollider no tiene una malla asignada.");
            return;
        }

        // Hacemos el MeshCollider "convexo" y aplicamos una fuerza de “explosión”
        collider.convex = true;
        rb.AddExplosionForce(cutForce, slicedObject.transform.position, 1);

        Debug.Log("✅ SetupSlicedComponent() - Se agregó Rigidbody y MeshCollider correctamente.");
    }
}
