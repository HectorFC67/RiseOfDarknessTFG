using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager singleton;

    private void Awake()
    {
        if (singleton && singleton != this)
        {
            Destroy(singleton.gameObject);
        }
        singleton = this;
    }

    /// <summary>
    /// Carga la escena de forma inmediata (sin fundido) de forma "sincrónica".
    /// </summary>
    public void GoToScene(int sceneIndex)
    {
        Debug.Log("[SceneTransitionManager] Cargando escena " + sceneIndex + " inmediatamente (GoToScene).");
        SceneManager.LoadScene(sceneIndex);
    }

    /// <summary>
    /// Carga la escena de forma inmediata (sin fundido) usando Loading asíncrono,
    /// pero sin esperar ni hacer fundido.
    /// </summary>
    public void GoToSceneAsync(int sceneIndex)
    {
        Debug.Log("[SceneTransitionManager] Iniciando carga asíncrona de escena " + sceneIndex + " (GoToSceneAsync).");
        StartCoroutine(GoToSceneAsyncRoutine(sceneIndex));
    }

    private IEnumerator GoToSceneAsyncRoutine(int sceneIndex)
    {
        // Lanza la carga asíncrona
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        // Permitimos la activación inmediata de la escena
        operation.allowSceneActivation = true;

        // Esperamos hasta que termine
        while (!operation.isDone)
        {
            yield return null;
        }

        // Aquí ya se habrá cargado la escena
        Debug.Log("[SceneTransitionManager] Escena " + sceneIndex + " cargada.");
    }
}
