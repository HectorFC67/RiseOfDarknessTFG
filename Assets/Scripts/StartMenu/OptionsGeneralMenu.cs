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

    // Variables booleanas para cada modo:
    private bool isColorBlindModeOn = false;
    private bool isFullBodyModeOn   = false;
    private bool isBGMusicOn        = true;
    private bool isAnimSoundOn      = true;

    private void Start()
    {
        // Obtenemos el componente 'GameStartMenu' del GameObject asignado
        gameStartMenu = gameStartMenuObject.GetComponent<GameStartMenu>();

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
        backToMainMenuButton.onClick.AddListener(gameStartMenu.EnableMainMenu);
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

    // -----------------------
    // Métodos de Aplicación 
    // -----------------------
    private void ApplyVolume(float value)
    {
        AudioListener.volume = value;
    }

    private void ApplyBrightness(float value)
    {
        RenderSettings.ambientLight = Color.white * value;
    }

    private void ApplyColorBlindMode(bool value)
    {
        isColorBlindModeOn = value;
        Debug.Log("ColorBlindMode -> " + isColorBlindModeOn);
        // Aquí podrías activar lógica específica o cambios de materiales
    }

    private void ApplyFullBodyMode(bool value)
    {
        isFullBodyModeOn = value;
        Debug.Log("FullBodyMode -> " + isFullBodyModeOn);

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

    private void ApplyBGMusic(bool value)
    {
        isBGMusicOn = value;
        Debug.Log("BGMusicOn -> " + isBGMusicOn);
        // sourceBGMusic.mute = !isBGMusicOn;
    }

    private void ApplyAnimSound(bool value)
    {
        isAnimSoundOn = value;
        Debug.Log("AnimationSoundOn -> " + isAnimSoundOn);
        // footstepSource.mute = !isAnimSoundOn;
    }
}
