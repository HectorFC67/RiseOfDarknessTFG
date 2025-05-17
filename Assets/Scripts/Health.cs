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

    [Header("Audio")]
    [SerializeField] private AudioClip damageClip;
    [Range(0f, 1f)]
    [SerializeField] private float damageVolume = 1f;

    private AudioSource audioSrc;


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

        // -------- AUDIO --------
        audioSrc = gameObject.AddComponent<AudioSource>();
        audioSrc.playOnAwake = false;
        audioSrc.spatialBlend = 0f;     // sonido 2D (UI); pon 1f si quieres 3D
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnemyWeapon"))
        {
            if (damageClip != null)
                audioSrc.PlayOneShot(damageClip, damageVolume);

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
