using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnButtonPressed : MonoBehaviour
{
    public GameObject pressedSoundPrefab;
    private AudioSource pressedSound;
    
    void Start()
    {
        pressedSound = pressedSoundPrefab.GetComponent<AudioSource>();
        var btn = GetComponent<Button>();
        btn.onClick.AddListener(TaskOnClick);
    }

    private void TaskOnClick()
    {
        pressedSound.Play();
    }
}
