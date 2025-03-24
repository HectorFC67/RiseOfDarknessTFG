using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsGeneralMenu : MonoBehaviour
{
    [Header("Scrollbar (General)")]
    public Scrollbar volumeScrollbar;
    public Scrollbar brightnessScrollbar;

    [Header("Toggles (General)")]
    public Toggle colorBlindToggle;   // default: false
    public Toggle fullBodyToggle;     // default: false
    public Toggle bgMusicToggle;      // default: true
    public Toggle animSoundToggle;    // default: true

    [Header("Sección General (Mostrar/ocultar)")]
    public GameObject volumeOptions;
    public GameObject brightnessOptions;
    public GameObject backgroundMusicOptions;
    public GameObject animationsSoundsOptions;
    public GameObject daltonismOptions;
    public GameObject fullBodyOptions;
    public Button nextButton;

    [Header("Sección InGame (Mostrar/ocultar)")]
    public GameObject difficultyOptions;
    public GameObject familyFriendlyOptions;
    public GameObject sliceOptions;
    public GameObject stickOptions;
    public Button previousButton;

    [Header("Botón Exit")]
    public Button exitButton;

    [Header("Dropdown (InGame)")]
    public TMP_Dropdown difficultyDropdown;

    [Header("Toggles (InGame)")]
    public Toggle sliceToggle;           // default: ON
    public Toggle stickToggle;           // default: OFF
    public Toggle familyFriendlyToggle;  // default: OFF

    [Header("Player's Parts")]
    public GameObject avatar;
    public GameObject rightHand;
    public GameObject leftHand;

    [Header("GameStartMenu")]
    public GameObject gameStartMenuObject;

    private GameStartMenu gameStartMenu;

    // -----------------------
    // Variables internas
    // -----------------------
    private bool isColorBlindModeOn = false;
    private bool isFullBodyModeOn = false;
    private bool isBGMusicOn = true;
    private bool isAnimSoundOn = true;

    private void Start()
    {
        // Referencia al GameStartMenu si fuera necesario
        if (gameStartMenuObject)
            gameStartMenu = gameStartMenuObject.GetComponent<GameStartMenu>();

        // ---------- 1. Cargar y asignar valores (General) ----------
        volumeScrollbar.value = PlayerPrefs.GetFloat("globalVolume", 1f);
        brightnessScrollbar.value = PlayerPrefs.GetFloat("globalBrightness", 1f);

        colorBlindToggle.isOn = (PlayerPrefs.GetInt("colorBlindMode", 0) == 1);
        fullBodyToggle.isOn = (PlayerPrefs.GetInt("fullBodyMode", 0) == 1);
        bgMusicToggle.isOn = (PlayerPrefs.GetInt("bgMusicOn", 1) == 1);
        animSoundToggle.isOn = (PlayerPrefs.GetInt("animSoundOn", 1) == 1);

        // Suscribir eventos (General)
        volumeScrollbar.onValueChanged.AddListener(OnVolumeChanged);
        brightnessScrollbar.onValueChanged.AddListener(OnBrightnessChanged);

        colorBlindToggle.onValueChanged.AddListener(OnColorBlindToggle);
        fullBodyToggle.onValueChanged.AddListener(OnFullBodyToggle);
        bgMusicToggle.onValueChanged.AddListener(OnBGMusicToggle);
        animSoundToggle.onValueChanged.AddListener(OnAnimSoundToggle);

        // ---------- 2. Cargar y asignar valores (InGame) ----------
        int savedDifficulty = PlayerPrefs.GetInt("gameDifficulty", 2);
        difficultyDropdown.SetValueWithoutNotify(savedDifficulty);

        bool savedSlice = (PlayerPrefs.GetInt("optionSlice", 1) == 1);
        bool savedStick = (PlayerPrefs.GetInt("optionStick", 0) == 1);
        bool savedFamilyFriendly = (PlayerPrefs.GetInt("familyFriendly", 0) == 1);

        difficultyDropdown.onValueChanged.AddListener(OnDifficultyChanged);
        sliceToggle.onValueChanged.AddListener(OnSliceChanged);
        stickToggle.onValueChanged.AddListener(OnStickChanged);
        familyFriendlyToggle.onValueChanged.AddListener(OnFamilyFriendlyChanged);

        // ---------- 3. Botones Next, Previous y Exit ----------
        if (nextButton) nextButton.onClick.AddListener(OnNextClicked);
        if (previousButton) previousButton.onClick.AddListener(OnPreviousClicked);

        // Si quieres que el botón Exit vuelva al menú principal (por ejemplo):
        if (exitButton && gameStartMenu)
            exitButton.onClick.AddListener(gameStartMenu.EnableMainMenu);

        // ---------- 4. Aplicar valores iniciales ----------
        // General
        ApplyVolume(volumeScrollbar.value);
        ApplyBrightness(brightnessScrollbar.value);
        ApplyColorBlindMode(colorBlindToggle.isOn);
        ApplyFullBodyMode(fullBodyToggle.isOn);
        ApplyBGMusic(bgMusicToggle.isOn);
        ApplyAnimSound(animSoundToggle.isOn);

        // InGame
        ApplyDifficulty(savedDifficulty);
        familyFriendlyToggle.isOn = savedFamilyFriendly; // Asignación en el toggle
        ApplyFamilyFriendly(savedFamilyFriendly);

        // Si FamilyFriendly estaba desactivado, aplicamos slice y stick normalmente
        if (!savedFamilyFriendly)
        {
            sliceToggle.isOn = savedSlice;
            stickToggle.isOn = savedStick;

            ApplySlice(savedSlice);
            ApplyStick(savedStick);
        }

        // Al arrancar, asumimos que estás en la sección "General"
        ShowGeneralSection();
    }

    // -------------------------------------------------------------------------
    // Eventos de Scrollbar y Toggles (General)
    // -------------------------------------------------------------------------
    private void OnVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("globalVolume", value);
        ApplyVolume(value);
    }

    private void OnBrightnessChanged(float value)
    {
        PlayerPrefs.SetFloat("globalBrightness", value);
        ApplyBrightness(value);
    }

    private void OnColorBlindToggle(bool value)
    {
        PlayerPrefs.SetInt("colorBlindMode", value ? 1 : 0);
        ApplyColorBlindMode(value);
    }

    private void OnFullBodyToggle(bool value)
    {
        PlayerPrefs.SetInt("fullBodyMode", value ? 1 : 0);
        ApplyFullBodyMode(value);
    }

    private void OnBGMusicToggle(bool value)
    {
        PlayerPrefs.SetInt("bgMusicOn", value ? 1 : 0);
        ApplyBGMusic(value);
    }

    private void OnAnimSoundToggle(bool value)
    {
        PlayerPrefs.SetInt("animSoundOn", value ? 1 : 0);
        ApplyAnimSound(value);
    }

    // -------------------------------------------------------------------------
    // Eventos de Dropdown y Toggles (InGame)
    // -------------------------------------------------------------------------
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

        // Si FamilyFriendly se activa, forzamos slice=OFF y stick=ON
        if (value)
        {
            PlayerPrefs.SetInt("optionSlice", 0);
            PlayerPrefs.SetInt("optionStick", 1);

            sliceToggle.isOn = false;
            stickToggle.isOn = true;
        }
    }

    // -------------------------------------------------------------------------
    // Métodos "Apply" (General)
    // -------------------------------------------------------------------------
    private void ApplyVolume(float value)
    {
        AudioListener.volume = value;
        Debug.Log($"[OptionsGeneralMenu] Volume => {value}");
    }

    private void ApplyBrightness(float value)
    {
        // En URP/HDRP se hace de otra forma, pero aquí un ejemplo:
        RenderSettings.ambientLight = Color.white * value;
        Debug.Log($"[OptionsGeneralMenu] Brightness => {value}");
    }

    private void ApplyColorBlindMode(bool value)
    {
        isColorBlindModeOn = value;
        Debug.Log($"[OptionsGeneralMenu] ColorBlind => {value}");
        // Lógica extra para materiales, shaders, etc.
    }

    private void ApplyFullBodyMode(bool value)
    {
        isFullBodyModeOn = value;
        Debug.Log($"[OptionsGeneralMenu] FullBody => {value}");

        if (avatar && leftHand && rightHand)
        {
            if (value)
            {
                // Modo cuerpo entero
                avatar.SetActive(true);
                leftHand.SetActive(false);
                rightHand.SetActive(false);
            }
            else
            {
                // Modo solo manos
                avatar.SetActive(false);
                leftHand.SetActive(true);
                rightHand.SetActive(true);
            }
        }
    }

    private void ApplyBGMusic(bool value)
    {
        isBGMusicOn = value;
        Debug.Log($"[OptionsGeneralMenu] BGMusic => {value}");
        // Ejemplo: sourceBGMusic.mute = !value;
    }

    private void ApplyAnimSound(bool value)
    {
        isAnimSoundOn = value;
        Debug.Log($"[OptionsGeneralMenu] AnimSound => {value}");
        // Ejemplo: footstepSource.mute = !value;
    }

    // -------------------------------------------------------------------------
    // Métodos "Apply" (InGame)
    // -------------------------------------------------------------------------
    private void ApplyDifficulty(int difficultyValue)
    {
        // Lógica según valor
        string textDiff = "Normal";
        switch (difficultyValue)
        {
            case 0: textDiff = "Peaceful"; break;
            case 1: textDiff = "Easy"; break;
            case 2: textDiff = "Normal"; break;
            case 3: textDiff = "Hard"; break;
            case 4: textDiff = "Extreme"; break;
        }
        Debug.Log($"[OptionsGeneralMenu] Difficulty => {textDiff}");
    }

    private void ApplySlice(bool value)
    {
        Debug.Log($"[OptionsGeneralMenu] Slice => {value}");
        // sliceSystem.enabled = value; etc.
    }

    private void ApplyStick(bool value)
    {
        Debug.Log($"[OptionsGeneralMenu] Stick => {value}");
        // swordPrefab.SetActive(!value);
        // stickPrefab.SetActive(value);
    }

    private void ApplyFamilyFriendly(bool value)
    {
        Debug.Log($"[OptionsGeneralMenu] FamilyFriendly => {value}");

        // Si FamilyFriendly está ON:
        //  - slice = OFF y no editable
        //  - stick = ON y no editable
        if (value)
        {
            sliceToggle.interactable = false;
            stickToggle.interactable = false;
        }
        else
        {
            sliceToggle.interactable = true;
            stickToggle.interactable = true;
        }
    }

    // -------------------------------------------------------------------------
    // Métodos para cambiar de sección (Botones Next y Previous)
    // -------------------------------------------------------------------------
    private void OnNextClicked()
    {
        // Desactivamos los elementos de la sección "General"
        volumeOptions.SetActive(false);
        brightnessOptions.SetActive(false);
        backgroundMusicOptions.SetActive(false);
        animationsSoundsOptions.SetActive(false);
        daltonismOptions.SetActive(false);
        fullBodyOptions.SetActive(false);
        nextButton.gameObject.SetActive(false);

        // Activamos la sección "InGame"
        difficultyOptions.SetActive(true);
        familyFriendlyOptions.SetActive(true);
        sliceOptions.SetActive(true);
        stickOptions.SetActive(true);
        previousButton.gameObject.SetActive(true);
    }

    private void OnPreviousClicked()
    {
        // Sección "General" visible
        volumeOptions.SetActive(true);
        brightnessOptions.SetActive(true);
        backgroundMusicOptions.SetActive(true);
        animationsSoundsOptions.SetActive(true);
        daltonismOptions.SetActive(true);
        fullBodyOptions.SetActive(true);
        nextButton.gameObject.SetActive(true);

        // Sección "InGame" la ocultamos
        difficultyOptions.SetActive(false);
        familyFriendlyOptions.SetActive(false);
        sliceOptions.SetActive(false);
        stickOptions.SetActive(false);
        previousButton.gameObject.SetActive(false);
    }

    // Método auxiliar para arrancar mostrando la sección "General"
    private void ShowGeneralSection()
    {
        // Activa General, Desactiva InGame
        OnPreviousClicked();
    }
}
