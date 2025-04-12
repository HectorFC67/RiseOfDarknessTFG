using UnityEngine;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxLives = 3;
    private int currentLives;

    void Start()
    {
        // Iniciamos las vidas en el valor máximo
        currentLives = maxLives;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Comprobamos si el objeto con el que colisionamos tiene la etiqueta "enemyWeapon"
        if (other.CompareTag("EnemyWeapon"))
        {
            currentLives--;
            Debug.Log($"Jugador golpeado por un arma enemiga. Vidas restantes: {currentLives}");

            // Si nos quedamos sin vidas, cargamos la escena MainMenu
            if (currentLives <= 0)
            {
                Debug.Log("El jugador ha muerto. Cargando MainMenu...");
                SceneManager.LoadScene("MainMenu");
            }
        }
    }
}
