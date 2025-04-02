using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Asegúrate de que tu XR Rig o personaje tenga la tag "Player"
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player detectado. Cargando la escena FinalRoom...");
            SceneManager.LoadScene("FinalRoom");
        }
    }
}
