using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ChatIgris : MonoBehaviour
{
    public GameObject conversationCanvas;

    public void CloseConversation()
    {
        // Desactiva la interfaz de conversación
        conversationCanvas.SetActive(false);
    }
}