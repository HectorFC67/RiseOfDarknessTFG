using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class StartingChat : MonoBehaviour
{
    public GameObject conversationCanvas;
    public XRRayInteractor leftRayInteractor;
    public XRRayInteractor rightRayInteractor;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Activa la UI de conversación
            conversationCanvas.SetActive(true);

            // Activa los ray interactors
            leftRayInteractor.enabled = true;
            rightRayInteractor.enabled = true;
        }
    }
}
