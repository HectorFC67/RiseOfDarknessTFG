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
        // Comprobamos referencias
        if (!difficultyDropdown) Debug.LogWarning("[OptionsInGameMenu] difficultyDropdown no asignado.");
        if (!sliceToggle) Debug.LogWarning("[OptionsInGameMenu] sliceToggle no asignado.");
        if (!stickToggle) Debug.LogWarning("[OptionsInGameMenu] stickToggle no asignado.");
        if (!familyFriendlyToggle) Debug.LogWarning("[OptionsInGameMenu] familyFriendlyToggle no asignado.");
        if (!exitButton) Debug.LogWarning("[OptionsInGameMenu] exitButton no asignado.");
        if (!previousButton) Debug.LogWarning("[OptionsInGameMenu] previousButton no asignado.");
        if (!gameStartMenuObject) Debug.LogWarning("[OptionsInGameMenu] gameStartMenuObject no asignado.");

        if (gameStartMenuObject)
            gameStartMenu = gameStartMenuObject.GetComponent<GameStartMenu>();
        if (!gameStartMenu)
            Debug.LogError("[OptionsInGameMenu] No se encontró GameStartMenu en gameStartMenuObject.");

        // 1. Cargar PlayerPrefs
        int savedDifficulty = PlayerPrefs.GetInt("gameDifficulty", 2);
        difficultyDropdown.SetValueWithoutNotify(savedDifficulty);

        bool savedSlice          = (PlayerPrefs.GetInt("optionSlice", 1) == 1); 
        bool savedStick          = (PlayerPrefs.GetInt("optionStick", 0) == 1);
        bool savedFamilyFriendly = (PlayerPrefs.GetInt("familyFriendly", 0) == 1);

        // Asignamos estado inicial a los toggles
        sliceToggle.isOn = savedSlice;
        stickToggle.isOn = savedStick;
        familyFriendlyToggle.isOn = savedFamilyFriendly;

        // 2. Suscribir a los eventos
        difficultyDropdown.onValueChanged.AddListener(OnDifficultyChanged);

        sliceToggle.onValueChanged.AddListener(OnSliceChanged);
        stickToggle.onValueChanged.AddListener(OnStickChanged);
        familyFriendlyToggle.onValueChanged.AddListener(OnFamilyFriendlyChanged);

        if (exitButton && gameStartMenu)
            exitButton.onClick.AddListener(gameStartMenu.EnableMainMenu);
        if (previousButton && gameStartMenu)
            previousButton.onClick.AddListener(gameStartMenu.EnableOption);

        // 3. Aplicar valores iniciales
        ApplyDifficulty(savedDifficulty);
        ApplyFamilyFriendly(savedFamilyFriendly);

        // Si FamilyFriendly estaba OFF, aplicamos slice y stick
        // para que no se sobrescriban.
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
        Debug.Log("[OptionsInGameMenu] Difficulty cambiado a índice: " + value);
        PlayerPrefs.SetInt("gameDifficulty", value);
        ApplyDifficulty(value);
    }

    private void OnSliceChanged(bool value)
    {
        Debug.Log("[OptionsInGameMenu] Toggle Slice -> " + value);
        PlayerPrefs.SetInt("optionSlice", value ? 1 : 0);
        ApplySlice(value);
    }

    private void OnStickChanged(bool value)
    {
        Debug.Log("[OptionsInGameMenu] Toggle Stick -> " + value);
        PlayerPrefs.SetInt("optionStick", value ? 1 : 0);
        ApplyStick(value);
    }

    private void OnFamilyFriendlyChanged(bool value)
    {
        Debug.Log("[OptionsInGameMenu] Toggle FamilyFriendly -> " + value);
        PlayerPrefs.SetInt("familyFriendly", value ? 1 : 0);
        ApplyFamilyFriendly(value);

        if (value)
        {
            // Forzamos slice=OFF y stick=ON, y bloqueamos sus toggles
            PlayerPrefs.SetInt("optionSlice", 0);
            PlayerPrefs.SetInt("optionStick", 1);

            sliceToggle.isOn = false;
            stickToggle.isOn = true;
        }
    }

    // ----------------------------------------------------------------------------
    // Métodos de Aplicación
    // ----------------------------------------------------------------------------
    private void ApplyDifficulty(int difficultyValue)
    {
        // Aquí tu lógica. Ejemplo:
        string textDiff = "";
        switch (difficultyValue)
        {
            case 0: textDiff = "Peaceful"; break;
            case 1: textDiff = "Easy";     break;
            case 2: textDiff = "Normal";   break;
            case 3: textDiff = "Hard";     break;
            case 4: textDiff = "Extreme";  break;
        }
        Debug.Log("[OptionsInGameMenu] ApplyDifficulty => " + textDiff);
    }

    private void ApplySlice(bool value)
    {
        Debug.Log("[OptionsInGameMenu] ApplySlice => " + value);
        // Ejemplo: sliceSystem.enabled = value;
    }

    private void ApplyStick(bool value)
    {
        Debug.Log("[OptionsInGameMenu] ApplyStick => " + value);
        // Ejemplo: swordPrefab.SetActive(!value);
        //          stickPrefab.SetActive(value);
    }

    private void ApplyFamilyFriendly(bool value)
    {
        Debug.Log("[OptionsInGameMenu] ApplyFamilyFriendly => " + value);

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
            //  - slice y stick editables
            sliceToggle.interactable = true;
            stickToggle.interactable = true;
        }
    }
}
