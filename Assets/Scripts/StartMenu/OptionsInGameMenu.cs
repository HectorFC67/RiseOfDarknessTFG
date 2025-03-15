using UnityEngine;
using UnityEngine.UI;

public class OptionsInGameMenu : MonoBehaviour
{
    [Header("Dropdown")]
    public Dropdown difficultyDropdown;
    // Opciones en el Inspector:
    // 0 -> Peaceful
    // 1 -> Easy
    // 2 -> Normal
    // 3 -> Hard
    // 4 -> Extreme

    [Header("Toggles")]
    public Toggle sliceToggle;           // default: ON
    public Toggle stickToggle;           // default: OFF
    public Toggle familyFriendlyToggle;  // default: OFF (principal)

    [Header("Buttons")]
    public Button exitButton;       // Vuelve al menú principal
    public Button previousButton;   // Vuelve a OptionsGeneral

    [Header("GameStartMenu")]
    public GameObject gameStartMenuObject;

    private GameStartMenu gameStartMenu;

    private void Start()
    {
        // Obtener la referencia al componente GameStartMenu
        gameStartMenu = gameStartMenuObject.GetComponent<GameStartMenu>();

        // ------------------------
        // 1. Cargar PlayerPrefs
        // ------------------------
        int savedDifficulty = PlayerPrefs.GetInt("gameDifficulty", 2);
        difficultyDropdown.SetValueWithoutNotify(savedDifficulty);

        // Toggles
        bool savedSlice = (PlayerPrefs.GetInt("optionSlice", 1) == 1); // default ON -> 1
        bool savedStick = (PlayerPrefs.GetInt("optionStick", 0) == 1); // default OFF -> 0
        bool savedFamilyFriendly = (PlayerPrefs.GetInt("familyFriendly", 0) == 1); // default OFF -> 0

        sliceToggle.isOn = savedSlice;
        stickToggle.isOn = savedStick;
        familyFriendlyToggle.isOn = savedFamilyFriendly;

        // ------------------------
        // 2. Suscribir a los eventos
        // ------------------------
        difficultyDropdown.onValueChanged.AddListener(OnDifficultyChanged);

        sliceToggle.onValueChanged.AddListener(OnSliceChanged);
        stickToggle.onValueChanged.AddListener(OnStickChanged);
        familyFriendlyToggle.onValueChanged.AddListener(OnFamilyFriendlyChanged);

        exitButton.onClick.AddListener(gameStartMenu.EnableMainMenu);
        previousButton.onClick.AddListener(gameStartMenu.EnableOption);

        // ------------------------
        // 3. Aplicar valores iniciales
        // ------------------------
        ApplyDifficulty(savedDifficulty);
        ApplyFamilyFriendly(savedFamilyFriendly);
        // ^ Al aplicar FamilyFriendly aquí, Slice y Stick quedarán
        //   bloqueados o no, según corresponda.
        //   Luego, si familyFriendly estaba OFF, aplicamos su estado:
        if (!savedFamilyFriendly)
        {
            ApplySlice(savedSlice);
            ApplyStick(savedStick);
        }
    }

    // ----------------------------------------------------------------------------
    // Métodos de respuesta a los eventos
    // ----------------------------------------------------------------------------
    private void OnDifficultyChanged(int value)
    {
        PlayerPrefs.SetInt("gameDifficulty", value);
        ApplyDifficulty(value);
    }

    private void OnSliceChanged(bool value)
    {
        PlayerPrefs.SetInt("optionSlice", value ? 1 : 0);
        ApplySlice(value);
    }

    private void OnStickChanged(bool value)
    {
        PlayerPrefs.SetInt("optionStick", value ? 1 : 0);
        ApplyStick(value);
    }

    private void OnFamilyFriendlyChanged(bool value)
    {
        PlayerPrefs.SetInt("familyFriendly", value ? 1 : 0);
        ApplyFamilyFriendly(value);

        // Si se activa, forzamos Slice=OFF / Stick=ON
        if (value)
        {
            // Guardamos en PlayerPrefs directamente
            PlayerPrefs.SetInt("optionSlice", 0);
            PlayerPrefs.SetInt("optionStick", 1);

            // Actualizamos estados
            sliceToggle.isOn = false;
            stickToggle.isOn = true;
        }
    }

    // ----------------------------------------------------------------------------
    // Métodos de Aplicación (para que en un futuro podamos forzar re-aplicar todo)
    // ----------------------------------------------------------------------------
    private void ApplyDifficulty(int difficultyValue)
    {
        // Aquí tu lógica para cambiar parámetros de juego según la dificultad
        // Ejemplo de debug:
        string textDiff = "";
        switch (difficultyValue)
        {
            case 0: textDiff = "Peaceful"; break;
            case 1: textDiff = "Easy"; break;
            case 2: textDiff = "Normal"; break;
            case 3: textDiff = "Hard"; break;
            case 4: textDiff = "Extreme"; break;
        }
        Debug.Log("ApplyDifficulty => " + textDiff);
    }

    private void ApplySlice(bool value)
    {
        // Activa/desactiva la capacidad de "cortar enemigos"
        Debug.Log("ApplySlice => " + value);
        // Ejemplo: sliceSystem.enabled = value;
    }

    private void ApplyStick(bool value)
    {
        // Activa/desactiva usar palos en vez de espadas
        Debug.Log("ApplyStick => " + value);
        // Ejemplo: swordPrefab.SetActive(!value);
        //          stickPrefab.SetActive(value);
    }

    private void ApplyFamilyFriendly(bool value)
    {
        Debug.Log("ApplyFamilyFriendly => " + value);

        if (value)
        {
            // FamilyFriendly ON:
            //  - slice = OFF y bloqueado
            //  - stick = ON y bloqueado
            sliceToggle.interactable = false;
            stickToggle.interactable = false;
        }
        else
        {
            // FamilyFriendly OFF:
            //  - se pueden editar Slice y Stick libremente
            sliceToggle.interactable = true;
            stickToggle.interactable = true;
        }
    }
}
