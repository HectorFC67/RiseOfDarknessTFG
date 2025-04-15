using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class StartingChat : MonoBehaviour
{
    public GameObject conversationCanvas;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Activa la UI de conversación al entrar
            conversationCanvas.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Desactiva la UI de conversación al salir
            conversationCanvas.SetActive(false);
        }
    }
}
