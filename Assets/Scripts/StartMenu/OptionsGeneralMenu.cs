using UnityEngine;
using UnityEngine.UI;

public class OptionsGeneralMenu : MonoBehaviour
{
    [Header("Scrollbar ")]
    public Scrollbar volumeScrollbar;
    public Scrollbar brightnessScrollbar;

    [Header("Toggles")]
    public Toggle colorBlindToggle;   // default: false
    public Toggle fullBodyToggle;     // default: false
    public Toggle bgMusicToggle;      // default: true
    public Toggle animSoundToggle;    // default: true

    [Header("Buttons")]
    public Button backToMainMenuButton;    // Regresa a la ventana principal
    public Button goToOptionsInGameButton; // Abre la ventana OptionsInGame

    [Header("Game Objects")]
    public GameObject gameStartMenuObject;

    [Header("Player's Parts")]
    public GameObject avatar;
    public GameObject rightHand;
    public GameObject leftHand;

    private GameStartMenu gameStartMenu;

    // Variables booleanas para cada modo
    private bool isColorBlindModeOn = false;
    private bool isFullBodyModeOn   = false;
    private bool isBGMusicOn        = true;
    private bool isAnimSoundOn      = true;

    private void Start()
    {
        // Comprobamos referencias
        if (!volumeScrollbar) Debug.LogWarning("[OptionsGeneralMenu] volumeScrollbar no asignado.");
        if (!brightnessScrollbar) Debug.LogWarning("[OptionsGeneralMenu] brightnessScrollbar no asignado.");
        if (!colorBlindToggle) Debug.LogWarning("[OptionsGeneralMenu] colorBlindToggle no asignado.");
        if (!fullBodyToggle) Debug.LogWarning("[OptionsGeneralMenu] fullBodyToggle no asignado.");
        if (!bgMusicToggle) Debug.LogWarning("[OptionsGeneralMenu] bgMusicToggle no asignado.");
        if (!animSoundToggle) Debug.LogWarning("[OptionsGeneralMenu] animSoundToggle no asignado.");
        if (!backToMainMenuButton) Debug.LogWarning("[OptionsGeneralMenu] backToMainMenuButton no asignado.");
        if (!goToOptionsInGameButton) Debug.LogWarning("[OptionsGeneralMenu] goToOptionsInGameButton no asignado.");
        if (!avatar) Debug.LogWarning("[OptionsGeneralMenu] avatar no asignado.");
        if (!rightHand) Debug.LogWarning("[OptionsGeneralMenu] rightHand no asignado.");
        if (!leftHand) Debug.LogWarning("[OptionsGeneralMenu] leftHand no asignado.");
        if (!gameStartMenuObject) Debug.LogWarning("[OptionsGeneralMenu] gameStartMenuObject no asignado.");

        // Obtenemos el componente 'GameStartMenu' del GameObject asignado
        if (gameStartMenuObject)
            gameStartMenu = gameStartMenuObject.GetComponent<GameStartMenu>();
        if (!gameStartMenu)
            Debug.LogError("[OptionsGeneralMenu] No se encontró GameStartMenu en gameStartMenuObject.");

        // Cargar los valores de PlayerPrefs
        volumeScrollbar.value     = PlayerPrefs.GetFloat("globalVolume", 1f);
        brightnessScrollbar.value = PlayerPrefs.GetFloat("globalBrightness", 1f);

        // Toggles
        colorBlindToggle.isOn = (PlayerPrefs.GetInt("colorBlindMode", 0) == 1);
        fullBodyToggle.isOn   = (PlayerPrefs.GetInt("fullBodyMode", 0) == 1);
        bgMusicToggle.isOn    = (PlayerPrefs.GetInt("bgMusicOn", 1) == 1);
        animSoundToggle.isOn  = (PlayerPrefs.GetInt("animSoundOn", 1) == 1);

        // Suscribir métodos a los eventos
        volumeScrollbar.onValueChanged.AddListener(OnVolumeChanged);
        brightnessScrollbar.onValueChanged.AddListener(OnBrightnessChanged);

        colorBlindToggle.onValueChanged.AddListener(OnColorBlindToggle);
        fullBodyToggle.onValueChanged.AddListener(OnFullBodyToggle);
        bgMusicToggle.onValueChanged.AddListener(OnBGMusicToggle);
        animSoundToggle.onValueChanged.AddListener(OnAnimSoundToggle);

        // Botones
        if (backToMainMenuButton && gameStartMenu)
            backToMainMenuButton.onClick.AddListener(gameStartMenu.EnableMainMenu);

        if (goToOptionsInGameButton && gameStartMenu)
            goToOptionsInGameButton.onClick.AddListener(gameStartMenu.EnableOptionsInGame);

        // Aplicar los valores iniciales
        ApplyVolume(volumeScrollbar.value);
        ApplyBrightness(brightnessScrollbar.value);
        ApplyColorBlindMode(colorBlindToggle.isOn);
        ApplyFullBodyMode(fullBodyToggle.isOn);
        ApplyBGMusic(bgMusicToggle.isOn);
        ApplyAnimSound(animSoundToggle.isOn);
    }

    private void OnVolumeChanged(float value)
    {
        Debug.Log("[OptionsGeneralMenu] Volume cambiado a: " + value);
        PlayerPrefs.SetFloat("globalVolume", value);
        ApplyVolume(value);
    }

    private void OnBrightnessChanged(float value)
    {
        Debug.Log("[OptionsGeneralMenu] Brightness cambiado a: " + value);
        PlayerPrefs.SetFloat("globalBrightness", value);
        ApplyBrightness(value);
    }

    private void OnColorBlindToggle(bool value)
    {
        Debug.Log("[OptionsGeneralMenu] Toggle ColorBlind -> " + value);
        PlayerPrefs.SetInt("colorBlindMode", value ? 1 : 0);
        ApplyColorBlindMode(value);
    }

    private void OnFullBodyToggle(bool value)
    {
        Debug.Log("[OptionsGeneralMenu] Toggle FullBody -> " + value);
        PlayerPrefs.SetInt("fullBodyMode", value ? 1 : 0);
        ApplyFullBodyMode(value);
    }

    private void OnBGMusicToggle(bool value)
    {
        Debug.Log("[OptionsGeneralMenu] Toggle BGMusic -> " + value);
        PlayerPrefs.SetInt("bgMusicOn", value ? 1 : 0);
        ApplyBGMusic(value);
    }

    private void OnAnimSoundToggle(bool value)
    {
        Debug.Log("[OptionsGeneralMenu] Toggle AnimSound -> " + value);
        PlayerPrefs.SetInt("animSoundOn", value ? 1 : 0);
        ApplyAnimSound(value);
    }

    // -----------------------
    // Métodos de Aplicación 
    // -----------------------
    private void ApplyVolume(float value)
    {
        AudioListener.volume = value;
    }

    private void ApplyBrightness(float value)
    {
        // OJO: En URP/HDRP, RenderSettings.ambientLight no es el método principal
        // de modificar el brillo. Aquí podría no notarse. Podrías usar PostProcess (Exposure).
        RenderSettings.ambientLight = Color.white * value;
    }

    private void ApplyColorBlindMode(bool value)
    {
        isColorBlindModeOn = value;
        Debug.Log("[OptionsGeneralMenu] ApplyColorBlindMode -> " + isColorBlindModeOn);
        // Aquí podrías activar lógicas o cambios de materiales específicos
    }

    private void ApplyFullBodyMode(bool value)
    {
        isFullBodyModeOn = value;
        Debug.Log("[OptionsGeneralMenu] ApplyFullBodyMode -> " + isFullBodyModeOn);

        if (avatar && leftHand && rightHand)
        {
            if (isFullBodyModeOn)
            {
                avatar.SetActive(true);
                leftHand.SetActive(false);
                rightHand.SetActive(false);
            }
            else
            {
                avatar.SetActive(false);
                leftHand.SetActive(true);
                rightHand.SetActive(true);
            }
        }
    }

    private void ApplyBGMusic(bool value)
    {
        isBGMusicOn = value;
        Debug.Log("[OptionsGeneralMenu] ApplyBGMusic -> " + isBGMusicOn);
        // Ejemplo: sourceBGMusic.mute = !isBGMusicOn;
    }

    private void ApplyAnimSound(bool value)
    {
        isAnimSoundOn = value;
        Debug.Log("[OptionsGeneralMenu] ApplyAnimSound -> " + isAnimSoundOn);
        // Ejemplo: footstepSource.mute = !isAnimSoundOn;
    }
}
