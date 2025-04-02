using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsGeneralMenu : MonoBehaviour
{
    [Header("Scrollbar (General)")]
    public Scrollbar volumeScrollbar;
    public Scrollbar brightnessScrollbar;

    [Header("Toggles (General)")]
    public Toggle colorBlindToggle;
    public Toggle fullBodyToggle;
    public Toggle bgMusicToggle;
    public Toggle animSoundToggle;

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
    public Toggle sliceToggle;
    public Toggle stickToggle;
    public Toggle familyFriendlyToggle;

    [Header("Player's Parts")]
    public GameObject avatar;
    public GameObject rightHand;
    public GameObject leftHand;

    [Header("GameStartMenu")]
    public GameObject gameStartMenuObject;
    private GameStartMenu gameStartMenu;

    // -------------------------------------------------------------------------
    // Métodos de Unity
    // -------------------------------------------------------------------------
    private void Start()
    {
        if (gameStartMenuObject != null)
            gameStartMenu = gameStartMenuObject.GetComponent<GameStartMenu>();

        // -- 1. Asignamos a cada UI el valor del GameManager
        volumeScrollbar.value        = GameManager.Instance.globalVolume;
        brightnessScrollbar.value    = GameManager.Instance.globalBrightness;

        colorBlindToggle.isOn        = GameManager.Instance.isColorBlindModeOn;
        fullBodyToggle.isOn          = GameManager.Instance.isFullBodyModeOn;
        bgMusicToggle.isOn           = GameManager.Instance.isBGMusicOn;
        animSoundToggle.isOn         = GameManager.Instance.isAnimSoundOn;

        difficultyDropdown.SetValueWithoutNotify(GameManager.Instance.gameDifficulty);
        sliceToggle.isOn             = GameManager.Instance.slice;
        stickToggle.isOn             = GameManager.Instance.stick;
        familyFriendlyToggle.isOn    = GameManager.Instance.familyFriendly;

        // -- 2. Suscribimos eventos
        volumeScrollbar.onValueChanged.AddListener(OnVolumeChanged);
        brightnessScrollbar.onValueChanged.AddListener(OnBrightnessChanged);

        colorBlindToggle.onValueChanged.AddListener(OnColorBlindToggle);
        fullBodyToggle.onValueChanged.AddListener(OnFullBodyToggle);
        bgMusicToggle.onValueChanged.AddListener(OnBGMusicToggle);
        animSoundToggle.onValueChanged.AddListener(OnAnimSoundToggle);

        difficultyDropdown.onValueChanged.AddListener(OnDifficultyChanged);
        sliceToggle.onValueChanged.AddListener(OnSliceChanged);
        stickToggle.onValueChanged.AddListener(OnStickChanged);
        familyFriendlyToggle.onValueChanged.AddListener(OnFamilyFriendlyChanged);

        if (nextButton != null)     
            nextButton.onClick.AddListener(OnNextClicked);

        if (previousButton != null) 
            previousButton.onClick.AddListener(OnPreviousClicked);

        if (exitButton != null && gameStartMenu != null)
            exitButton.onClick.AddListener(gameStartMenu.EnableMainMenu);

        // -- 3. Aplicar la configuración inicial
        ApplyVolume(GameManager.Instance.globalVolume);
        ApplyBrightness(GameManager.Instance.globalBrightness);
        ApplyColorBlindMode(GameManager.Instance.isColorBlindModeOn);
        ApplyFullBodyMode(GameManager.Instance.isFullBodyModeOn);
        ApplyBGMusic(GameManager.Instance.isBGMusicOn);
        ApplyAnimSound(GameManager.Instance.isAnimSoundOn);

        ApplyDifficulty(GameManager.Instance.gameDifficulty);
        ApplyFamilyFriendly(GameManager.Instance.familyFriendly);
        ApplySlice(GameManager.Instance.slice);
        ApplyStick(GameManager.Instance.stick);

        // -- 4. Mostrar la sección "General" al arrancar
        ShowGeneralSection();
    }

    // -------------------------------------------------------------------------
    // Listeners (General)
    // -------------------------------------------------------------------------
    private void OnVolumeChanged(float value)
    {
        GameManager.Instance.globalVolume = value;
        ApplyVolume(value);
        // No usamos PlayerPrefs, así que no llamamos a SavePrefs
    }

    private void OnBrightnessChanged(float value)
    {
        GameManager.Instance.globalBrightness = value;
        ApplyBrightness(value);
    }

    private void OnColorBlindToggle(bool value)
    {
        GameManager.Instance.isColorBlindModeOn = value;
        ApplyColorBlindMode(value);
    }

    private void OnFullBodyToggle(bool value)
    {
        GameManager.Instance.isFullBodyModeOn = value;
        ApplyFullBodyMode(value);
    }

    private void OnBGMusicToggle(bool value)
    {
        GameManager.Instance.isBGMusicOn = value;
        ApplyBGMusic(value);
    }

    private void OnAnimSoundToggle(bool value)
    {
        GameManager.Instance.isAnimSoundOn = value;
        ApplyAnimSound(value);
    }

    // -------------------------------------------------------------------------
    // Listeners (InGame)
    // -------------------------------------------------------------------------
    private void OnDifficultyChanged(int value)
    {
        GameManager.Instance.gameDifficulty = value;
        ApplyDifficulty(value);
    }

    private void OnSliceChanged(bool value)
    {
        GameManager.Instance.slice = value;
        ApplySlice(value);
    }

    private void OnStickChanged(bool value)
    {
        GameManager.Instance.stick = value;
        ApplyStick(value);
    }

    private void OnFamilyFriendlyChanged(bool value)
    {
        GameManager.Instance.familyFriendly = value;
        ApplyFamilyFriendly(value);

        // Si FamilyFriendly se activa, forzamos slice=OFF y stick=ON
        if (value)
        {
            GameManager.Instance.slice = false;
            GameManager.Instance.stick = true;

            // Cambiamos también en la UI
            sliceToggle.isOn = false;
            stickToggle.isOn = true;
        }
    }

    // -------------------------------------------------------------------------
    // Métodos APPLY
    // -------------------------------------------------------------------------
    private void ApplyVolume(float value)
    {
        AudioListener.volume = value;
        Debug.Log($"[OptionsGeneralMenu] Volume => {value}");
    }

    private void ApplyBrightness(float value)
    {
        RenderSettings.ambientLight = Color.white * value;
        Debug.Log($"[OptionsGeneralMenu] Brightness => {value}");
    }

    private void ApplyColorBlindMode(bool value)
    {
        Debug.Log($"[OptionsGeneralMenu] ColorBlind => {value}");
    }

    private void ApplyFullBodyMode(bool value)
    {
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
        Debug.Log($"[OptionsGeneralMenu] BGMusic => {value}");
        // Ejemplo: sourceBGMusic.mute = !value;
    }

    private void ApplyAnimSound(bool value)
    {
        Debug.Log($"[OptionsGeneralMenu] AnimSound => {value}");
        // Ejemplo: footstepSource.mute = !value;
    }

    private void ApplyDifficulty(int difficultyValue)
    {
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
    }

    private void ApplyStick(bool value)
    {
        Debug.Log($"[OptionsGeneralMenu] Stick => {value}");
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
    // Métodos para cambiar de sección
    // -------------------------------------------------------------------------
    private void OnNextClicked()
    {
        // Ocultamos la sección "General"
        volumeOptions.SetActive(false);
        brightnessOptions.SetActive(false);
        backgroundMusicOptions.SetActive(false);
        animationsSoundsOptions.SetActive(false);
        daltonismOptions.SetActive(false);
        fullBodyOptions.SetActive(false);
        nextButton.gameObject.SetActive(false);

        // Mostramos la sección "InGame"
        difficultyOptions.SetActive(true);
        familyFriendlyOptions.SetActive(true);
        sliceOptions.SetActive(true);
        stickOptions.SetActive(true);
        previousButton.gameObject.SetActive(true);
    }

    private void OnPreviousClicked()
    {
        // Mostramos la sección "General"
        volumeOptions.SetActive(true);
        brightnessOptions.SetActive(true);
        backgroundMusicOptions.SetActive(true);
        animationsSoundsOptions.SetActive(true);
        daltonismOptions.SetActive(true);
        fullBodyOptions.SetActive(true);
        nextButton.gameObject.SetActive(true);

        // Ocultamos la sección "InGame"
        difficultyOptions.SetActive(false);
        familyFriendlyOptions.SetActive(false);
        sliceOptions.SetActive(false);
        stickOptions.SetActive(false);
        previousButton.gameObject.SetActive(false);
    }

    private void ShowGeneralSection()
    {
        // Forzamos la vista a la sección "General"
        OnPreviousClicked();
    }
}