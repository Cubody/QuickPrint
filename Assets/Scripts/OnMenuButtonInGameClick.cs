using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnMenuButtonInGameClick : MonoBehaviour
{
    public GameObject menuPrefab;

    void Start()
    {
        var btn = GetComponent<Button>();
        btn.onClick.AddListener(TaskOnClick);
    }

    void TaskOnClick()
    {
        menuPrefab.SetActive(!menuPrefab.activeSelf);
    }
}