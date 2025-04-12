using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ChatIgris : MonoBehaviour
{
    public GameObject conversationCanvas;

    public XRRayInteractor leftRayInteractor;
    public XRRayInteractor rightRayInteractor;

    public void CloseConversation()
    {
        // Desactiva la interfaz de conversación
        conversationCanvas.SetActive(false);

        // Desactiva los ray interactors
        leftRayInteractor.enabled = false;
        rightRayInteractor.enabled = false;
    }
}