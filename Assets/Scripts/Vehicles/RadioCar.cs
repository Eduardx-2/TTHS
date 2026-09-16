using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RadioCar : MonoBehaviour
{
    public KeyCode radioKey = KeyCode.Q;

    private AudioSource audioSource;
    private AudioClip[] songs;
    private bool isOccupied;
    private bool isPaused;
    private int currentIndex = -1;
    private float salvaTiempo;
    private bool wasPlayingOnExit;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0.4f;

        songs = Resources.LoadAll<AudioClip>("musicCar");
        if (songs == null || songs.Length < 2)
        {
            //el pendejo marica de unity, no me estaba importando las canciones
            //el puto importaba 1, pero y la otra?? tenía que darle click yo mismo, si no, ni la tomaba en cuenta
            Debug.LogWarning("RadioCar: se esperaban al menos 2 canciones en Resources/musicCar. Encontradas: " + (songs == null ? 0 : songs.Length));
        }
    }

    void Update()
    {
        if (!isOccupied || songs == null || songs.Length == 0)
        {
            return;
        }

        if (!Input.GetKeyDown(radioKey))
        {
            return;
        }

        bool shiftBoton = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        if (shiftBoton)
        {
            musicaSkip();
            return;
        }

        if (audioSource.isPlaying)
        {
            PausaMusica();
            return;
        }

        if (isPaused && audioSource.clip != null)
        {
            ResumeCurrent();
            return;
        }

        PlayClip(PickRandomIndex(-1), 0f);
    }

    public void SetOccupied(bool occupied)
    {
        isOccupied = occupied;

        if (!occupied)
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                salvaTiempo = audioSource.time; //salvame el tiempo variable hija de puta
                wasPlayingOnExit = true;
                audioSource.Pause();
            }
            else
            {
                wasPlayingOnExit = false;
            }
        }
        else
        {
            if (wasPlayingOnExit && currentIndex >= 0)
            {
                ResumeCurrent();
            }
        }
    }

//aquí salvo el tiempo de la canción, si se pone pausa
    void PausaMusica()
    {
        salvaTiempo = audioSource.time;
        audioSource.Pause();
        isPaused = true;
    }

    void ResumeCurrent()
    {
        audioSource.UnPause();
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
            audioSource.time = salvaTiempo;
        }

        isPaused = false;
    }

    void musicaSkip()
    {
        PlayClip(PickRandomIndex(currentIndex), 0f);
    }

    void PlayClip(int index, float startTime)
    {
        if (index < 0 || index >= songs.Length || songs[index] == null)
        {
            return;
        }

        currentIndex = index;
        salvaTiempo = startTime;
        isPaused = false;
        audioSource.clip = songs[index];
        audioSource.Play();
        if (startTime > 0f)
        {
            audioSource.time = startTime;
        }
    }

    int PickRandomIndex(int excludeIndex)
    {
        if (songs.Length == 1)
        {
            return 0;
        }

        if (excludeIndex < 0 || excludeIndex >= songs.Length)
        {
            return Random.Range(0, songs.Length);
        }

        int index = Random.Range(0, songs.Length - 1);
        if (index >= excludeIndex)
        {
            index++;
        }

        return index;
    }
}