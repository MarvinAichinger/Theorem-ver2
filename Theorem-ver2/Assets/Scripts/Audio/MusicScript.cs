using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicScript : MonoBehaviour
{
    [SerializeField]
    AudioSource audioSource;
    
    // Start is called before the first frame update
    void Start()
    {
        audioSource.Play();
    }

    private void OnMouseUp()
    {
        audioSource.mute = !audioSource.mute;
    }
}
