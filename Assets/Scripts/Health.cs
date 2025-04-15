using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // Importante para usar Coroutines

public class Health : MonoBehaviour
{
    [SerializeField] private int maxLives = 3;
    private int currentLives;

    // GameObjects para los corazones llenos y vacíos
    [SerializeField] private GameObject[] fullHearts; 
    [SerializeField] private GameObject[] emptyHearts;

    // Panel de golpe
    [SerializeField] private GameObject hitPanel;

    void Start()
    {
        currentLives = maxLives;

        // Configuramos corazones al inicio
        for (int i = 0; i < maxLives; i++)
        {
            fullHearts[i].SetActive(true);
            emptyHearts[i].SetActive(false);
        }

        // Asegurarnos de que el panel de golpe se inicie desactivado
        if (hitPanel != null)
            hitPanel.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnemyWeapon"))
        {
            currentLives--;
            Debug.Log($"Jugador golpeado. Vidas restantes: {currentLives}");

            // Activar panel de golpe
            if (hitPanel != null)
                StartCoroutine(ShowHitPanel());

            // Cambiar corazón lleno a vacío
            if (currentLives >= 0 && currentLives < maxLives)
            {
                fullHearts[currentLives].SetActive(false);
                emptyHearts[currentLives].SetActive(true);
            }

            // Comprobar si las vidas llegan a 0
            if (currentLives <= 0)
            {
                Debug.Log("El jugador ha muerto. Cargando MainMenu...");
                SceneManager.LoadScene("MainMenu");
            }
        }
    }

    // Coroutine para mostrar temporalmente el panel de golpe
    private IEnumerator ShowHitPanel()
    {
        hitPanel.SetActive(true);
        yield return new WaitForSeconds(1f);
        hitPanel.SetActive(false);
    }
}
