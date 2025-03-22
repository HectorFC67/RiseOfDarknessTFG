using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameStartMenu : MonoBehaviour
{
    [Header("UI Pages")]
    public GameObject mainMenu;
    public GameObject optionsGeneral;  // Aquí unificas OptionsGeneral + OptionsInGame
    public GameObject about;

    [Header("Main Menu Buttons")]
    public Button startButton;
    public Button optionButton;
    public Button aboutButton;
    public Button quitButton;

    public List<Button> returnButtons;
    private void Start()
    {
        // Comprobamos que todos los elementos están asignados
        if (!mainMenu) Debug.LogWarning("[GameStartMenu] MainMenu no asignado.");
        if (!optionsGeneral) Debug.LogWarning("[GameStartMenu] OptionsGeneral no asignado.");
        if (!about) Debug.LogWarning("[GameStartMenu] About no asignado.");
        if (!startButton) Debug.LogWarning("[GameStartMenu] startButton no asignado.");
        if (!optionButton) Debug.LogWarning("[GameStartMenu] optionButton no asignado.");
        if (!aboutButton) Debug.LogWarning("[GameStartMenu] aboutButton no asignado.");
        if (!quitButton) Debug.LogWarning("[GameStartMenu] quitButton no asignado.");

        // Mostramos el menú principal al inicio
        EnableMainMenu();

        // Asignamos listeners a los botones principales
        startButton.onClick.AddListener(StartGame);
        optionButton.onClick.AddListener(EnableOption);
        aboutButton.onClick.AddListener(EnableAbout);
        quitButton.onClick.AddListener(QuitGame);

        // Cada botón de "Return" en la lista vuelve al Main Menu
        foreach (var item in returnButtons)
        {
            if (!item) continue;
            item.onClick.AddListener(EnableMainMenu);
        }
    }

    public void QuitGame()
    {
        Debug.Log("[GameStartMenu] QuitGame pulsado. Cerrando aplicación...");
        Application.Quit();
    }

    public void StartGame()
    {
        Debug.Log("[GameStartMenu] StartGame pulsado. Ocultando UI y cargando escena 1 de inmediato...");
        HideAll();

        if (!SceneTransitionManager.singleton)
        {
            Debug.LogError("[GameStartMenu] No hay SceneTransitionManager en la escena. " +
                           "Se cargará directamente la escena con SceneManager.LoadScene(1).");
            UnityEngine.SceneManagement.SceneManager.LoadScene(1);
            return;
        }

        // Carga inmediata asíncrona
        SceneTransitionManager.singleton.GoToSceneAsync(1);
        // Carga sincrónica:
        // SceneTransitionManager.singleton.GoToScene(1);
    }

    public void HideAll()
    {
        mainMenu.SetActive(false);
        optionsGeneral.SetActive(false);
        about.SetActive(false);
    }

    public void EnableMainMenu()
    {
        Debug.Log("[GameStartMenu] Volviendo al Menú Principal.");
        mainMenu.SetActive(true);
        optionsGeneral.SetActive(false);
        about.SetActive(false);
    }

    public void EnableOption()
    {
        Debug.Log("[GameStartMenu] Mostrando Options (General + InGame en uno).");
        mainMenu.SetActive(false);
        optionsGeneral.SetActive(true);
        about.SetActive(false);
    }

    public void EnableAbout()
    {
        Debug.Log("[GameStartMenu] Mostrando About.");
        mainMenu.SetActive(false);
        optionsGeneral.SetActive(false);
        about.SetActive(true);
    }
}
