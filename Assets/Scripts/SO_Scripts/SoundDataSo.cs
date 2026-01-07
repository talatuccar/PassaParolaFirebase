using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "SoundDataSo", menuName = "ScriptableObjects/SoundDataSo")]

public class SoundDataSo : ScriptableObject
{
   [Header("Sound Settings")]
    public AudioClip correctClip;  // Ses dosyasý
    public float volume = 1.0f;  // Sesin ses seviyesi
    public AudioClip wrongClip;
    public AudioClip passedClip;

    [Header("Sound Type")]
    public SoundType soundType;  // Sesin türü (örn. efekt, müzik vs.)

    public enum SoundType
    {
        SFX,   // Ses efektleri
        Music, // Müzik
        Ambient // Ortam sesleri
    }
}
