using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    AudioSource audioSource;


    public static SoundController Instance { get; private set; }
    private void Awake()
    {
        // Singleton kontrolü
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Sahne geçiþlerinde yok olmasýn
    }
    void Start()
    {
        audioSource = GetComponent<AudioSource>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void PlayChoiseVoice(AudioClip audioClip)
    {
        audioSource.clip =audioClip;
        audioSource.Play();

    }


   
}
