using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class StartingChat : MonoBehaviour
{
    public GameObject conversationCanvas;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Activa la UI de conversación
            conversationCanvas.SetActive(true);
        }
    }
}
