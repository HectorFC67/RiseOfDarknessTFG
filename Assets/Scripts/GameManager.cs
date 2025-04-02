using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // --- Todas las opciones que quieres compartir entre escenas ---
    public float globalVolume = 1f;
    public float globalBrightness = 1f;

    public bool isColorBlindModeOn = false;
    public bool isFullBodyModeOn = false;
    public bool isBGMusicOn = true;
    public bool isAnimSoundOn = true;

    public int gameDifficulty = 2;   // Por ejemplo: 0=Peaceful, 1=Easy, 2=Normal, 3=Hard, 4=Extreme
    public bool slice = true;        // Opción "Slice"
    public bool stick = false;       // Opción "Stick"
    public bool familyFriendly = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Sin llamadas a LoadPrefs (no usamos PlayerPrefs).
            // Dejará los valores que ves arriba como iniciales (o los que asignes en el Inspector).
        }
        else
        {
            Destroy(gameObject);
        }
    }
}