using System.Collections;
using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    public DungeonCreator dungeonCreator; // Referencia al componente DungeonCreator

    private void Start()
    {
        StartCoroutine(GenerateDungeonWithRetries());
    }

    private IEnumerator GenerateDungeonWithRetries()
    {
        bool dungeonCreated = false;

        // Intento de generar el dungeon hasta que la creación sea exitosa
        while (!dungeonCreated)
        {
            dungeonCreated = dungeonCreator.GenerateDungeonWithResult();

            if (dungeonCreated)
            {
                Debug.Log("Dungeon generado exitosamente.");
            }
            else
            {
                Debug.LogWarning("Falló la generación del dungeon. Reintentando...");
                yield return null; // Espera un frame antes de reintentar
            }
        }
    }
}
