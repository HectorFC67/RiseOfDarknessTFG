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

    public List<GameObject> randomWeapons;

    public GameObject sliceFxPrefab;
    public AudioClip sliceSfx;
    [Range(0f, 1f)]
    public float sliceSfxVolume = 1f;

    [Header("Crates")]
    public Material crateCrossSection;      // Material interno exclusivo
    public bool crateUseFx = false;         // Mostrar FX en crates (false = NO)
    public AudioClip crateSliceSfx;         // SFX exclusivo (opcional)
    [Range(0f, 1f)]
    public float crateSliceSfxVolume = 1f;  // Volumen para ese SFX

    void FixedUpdate()
    {
        bool hasHit = Physics.Linecast(startSlicePoint.position, endSlicePoint.position, out RaycastHit hit, sliceableLayer);
        if (hasHit)
        {
            GameObject target = hit.transform.root.gameObject;

            // Desactivamos IA y ragdoll
            DeactivateEnemyAI(target);
            ActivateRagdoll(target);

            // 1) Preparamos la malla bakeada en el enemigo (si tiene DynamicBakeMesh)
            DynamicBakeMesh bakeMeshComp = target.GetComponentInChildren<DynamicBakeMesh>();
            if (bakeMeshComp != null)
            {
                bakeMeshComp.BakeCurrentMesh();
                bakeMeshComp.SwitchToBakedMesh();
            }
            else
            {
                Debug.LogWarning($"SliceObject: El enemigo {target.name} no tiene DynamicBakeMesh. " +
                                 "Se intentará cortar su malla 'tal cual'.");
            }

            // 2) Realizamos la slice
            //    - Si existe la malla dinámica, cortamos esa
            //    - Si no, cortamos el target original (quedaría en manos de GetMeshFromObject)
            GameObject objectToSlice = (bakeMeshComp != null) ? bakeMeshComp.GetDynamicMeshObject() : target;

            Slice(objectToSlice, target, hit.point);
        }
    }

    /// <summary>
    /// Desactiva (si existe) la IA del enemigo.
    /// </summary>
    public void DeactivateEnemyAI(GameObject target)
    {
        RandomMovement enemyAI = target.GetComponent<RandomMovement>();
        if (enemyAI != null)
        {
            enemyAI.enabled = false;
            Debug.Log("✅ AI desactivada en el enemigo (componente en el mismo objeto).");
        }
    }

    /// <summary>
    /// Activa el ragdoll en el target y su padre, desactiva animación y AI.
    /// </summary>
    public void ActivateRagdoll(GameObject target)
    {
        // 1. Desactivar RandomMovement en el padre, si existiera
        Transform parentTransform = target.transform.parent;
        if (parentTransform != null)
        {
            RandomMovement parentRandomMovement = parentTransform.GetComponent<RandomMovement>();
            if (parentRandomMovement != null)
            {
                parentRandomMovement.enabled = false;
                Debug.Log("✅ Script RandomMovement desactivado en el padre del enemigo.");
            }
        }

        // 2. Detener y desactivar NavMeshAgent
        NavMeshAgent agent = target.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
            Debug.Log("✅ NavMeshAgent detenido y desactivado en el enemigo.");
        }

        // 3. Desactivar Animator
        Animator animator = target.GetComponent<Animator>();
        if (animator != null)
        {
            animator.enabled = false;
        }

        // 4. Activar físicas en todos los rigidbodies hijos
        Rigidbody[] rigidbodies = target.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = false;
        }

        Debug.Log("✅ Ragdoll activado en el enemigo.");
    }

    /// <summary>
    /// Realiza el corte usando EzySlice en el objeto "objectToSlice".
    /// El parámetro "originalRoot" se usa para checks como el tag "Crate", o para destruir el root.
    /// </summary>
    public void Slice(GameObject objectToSlice,
                      GameObject originalRoot,
                      Vector3 fxPosition)
    {
        if (objectToSlice == null)
        {
            Debug.Log("❌ Slice() - El objeto a cortar es nulo.");
            return;
        }

        Vector3 velocity = velocityEstimator.GetVelocityEstimate();
        Vector3 planeNormal = Vector3.Cross(endSlicePoint.position - startSlicePoint.position, velocity);
        planeNormal.Normalize();

        // ------------------------------------------------------------------
        // 1. Detección de tipos
        // ------------------------------------------------------------------
        bool isCrate = originalRoot.CompareTag("Crate");
        bool isBrokenCrate = originalRoot.CompareTag("BrokenCrate");
        bool isAnyCrate = isCrate || isBrokenCrate;

        // ------------------------------------------------------------------
        // 2. Material, FX y SFX
        // ------------------------------------------------------------------
        Material sectionMat = (isAnyCrate && crateCrossSection != null)
                                ? crateCrossSection
                                : crossSection;

        // ➜ FX: solo para objetos que NO sean crate ni brokenCrate
        if (!isAnyCrate)
            Instantiate(sliceFxPrefab, fxPosition, Quaternion.LookRotation(planeNormal));

        AudioClip clip = (isAnyCrate && crateSliceSfx != null) ? crateSliceSfx : sliceSfx;
        float vol = isAnyCrate ? crateSliceSfxVolume : sliceSfxVolume;
        AudioSource.PlayClipAtPoint(clip, fxPosition, vol);

        // ------------------------------------------------------------------
        // 3. Corte con EzySlice
        // ------------------------------------------------------------------
        SlicedHull hull = objectToSlice.Slice(endSlicePoint.position, planeNormal, sectionMat);

        if (hull != null)
        {
            // 3.a. Spawn arma SOLO si era Crate
            if (isCrate && randomWeapons != null && randomWeapons.Count > 0)
            {
                Instantiate(randomWeapons[Random.Range(0, randomWeapons.Count)],
                            originalRoot.transform.position,
                            originalRoot.transform.rotation);

                Debug.Log("✅ Crate cortado: se ha instanciado un arma aleatoria.");
            }

            // 3.b. Crear mitades
            GameObject upperHull = hull.CreateUpperHull(objectToSlice, sectionMat);
            ApplyOriginalTransform(objectToSlice, upperHull);
            SetupSlicedComponent(upperHull);

            GameObject lowerHull = hull.CreateLowerHull(objectToSlice, sectionMat);
            ApplyOriginalTransform(objectToSlice, lowerHull);
            SetupSlicedComponent(lowerHull);

            // 3.c. Etiquetar como BrokenCrate (tanto si era Crate como BrokenCrate)
            if (isAnyCrate)
            {
                upperHull.tag = "BrokenCrate";
                lowerHull.tag = "BrokenCrate";
            }

            Destroy(objectToSlice);  // Eliminar original
        }
        else
        {
            Debug.Log("❌ Slice() - No se generó hull (posible malla no válida).");
        }
    }

    /// <summary>
    /// Añade RigidBody, MeshCollider y fuerza de "explosión" a cada parte.
    /// </summary>
    public void SetupSlicedComponent(GameObject slicedObject)
    {
        if (slicedObject == null)
        {
            Debug.Log("❌ SetupSlicedComponent() - El objeto cortado es nulo.");
            return;
        }

        // Ajustar el layer si se desea
        slicedObject.layer = LayerMask.NameToLayer("Sliceable");

        // Añadir Rigidbody y MeshCollider
        Rigidbody rb = slicedObject.AddComponent<Rigidbody>();
        MeshCollider collider = slicedObject.AddComponent<MeshCollider>();

        if (collider.sharedMesh == null)
        {
            Debug.Log("❌ SetupSlicedComponent() - El MeshCollider no tiene una malla asignada.");
            return;
        }

        collider.convex = true;
        rb.AddExplosionForce(cutForce, slicedObject.transform.position, 1);

        Debug.Log("✅ SetupSlicedComponent() - Se agregó Rigidbody y MeshCollider correctamente.");
    }

    /// <summary>
    /// Copia la posición, rotación y escala del objeto original al nuevo trozo cortado.
    /// </summary>
    private void ApplyOriginalTransform(GameObject original, GameObject slicedPart)
    {
        if (original == null || slicedPart == null) return;

        // Si quieres mantener la misma jerarquía, también puedes hacer:
        // slicedPart.transform.SetParent(original.transform.parent);

        // Copiamos posición/rotación/escala.
        slicedPart.transform.position = original.transform.position;
        slicedPart.transform.rotation = original.transform.rotation;
        slicedPart.transform.localScale = original.transform.localScale;
    }
}
