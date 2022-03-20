using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicScript : MonoBehaviour
{
    [SerializeField]
    AudioSource audioSource;

    [SerializeField]
    Sprite spriteON;

    [SerializeField]
    Sprite spriteOFF;
    
    // Start is called before the first frame update
    void Start()
    {
        audioSource.mute = true;
        audioSource.Play();
    }

    private void OnMouseUp()
    {
        audioSource.mute = !audioSource.mute;
        if (audioSource.mute)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = spriteOFF;
        }else
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = spriteON;
        }
    }
}
