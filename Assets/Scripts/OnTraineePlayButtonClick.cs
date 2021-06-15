using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnTraineePlayButtonClick : MonoBehaviour
{
    public GameObject GamePrefab;
    public bool UseSymbol;
    void Start()
    {
        var btn = GetComponent<Button>();
        btn.onClick.AddListener(TaskOnClick);
    }

    void TaskOnClick()
    {
        GamePrefab.GetComponent<CircleMoveContoller>().useSymbol = UseSymbol;
        GamePrefab.SetActive(true);
    }
}
