using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static System.String;
using Image = UnityEngine.UI.Image;
using Random = UnityEngine.Random;

public class CircleMoveContoller : MonoBehaviour
{
    public bool useSymbol;
    public List<Section> sections;
    private AudioSource _levelAudio;
    private AudioSource _errorSound;
    private AudioSource _timeOutSound;
    private List<Circle> _circles;

    // Start is called before the first frame update
    void Start()
    {
        _levelAudio = transform.GetChild(0).GetComponent<AudioSource>();
        _errorSound = transform.GetChild(1).GetComponent<AudioSource>();
        _timeOutSound = transform.GetChild(2).GetComponent<AudioSource>();

        CreateCircles();
    }
    
    private void OnEnable()
    {
        CreateCircles();
        _pressedCombo = 0;
        _isStopped = false;
        _lastKeyPlayed = DateTime.MinValue;
    }

    void CreateCircles()
    {
        var childs = transform.GetChild(4);
        _circles = new List<Circle>();
        for (var i = 0; i < childs.childCount; i++)
        {
            var nextSection = sections[Random.Range(0, sections.Count)];
            _circles.Add(
                new Circle(nextSection.color, childs.GetChild(i).gameObject,
                    nextSection.Symbols[Random.Range(0, nextSection.Symbols.Length)].ToString().ToUpper(), useSymbol));
            if (!useSymbol)
                _circles[i].Label.gameObject.SetActive(false);
        }

        for (var i = 0; i < 3; i++)
            _circles[i].GameObject.SetActive(false);
    }

    private DateTime _lastKeyPlayed;
    private const float TimeForPlay = 4.0f;
    private const int NeedKeysToContinue = 1;
    private int _pressedCombo;
    private bool _isStopped;

    private void Update()
    {
        // Проверяем на долгий ввод
        if (_lastKeyPlayed != default && (DateTime.Now - _lastKeyPlayed).TotalSeconds >= TimeForPlay)
            StopMusic(false);
        if (!Input.anyKeyDown) return;
        if (Input.inputString.Trim() == "")
            return;
        // Проверяем на соответствие ожидаемого ввода\
        var symbol = Input.inputString.ToLowerInvariant()[0];
        var needSymbol = _circles[3].Label.text;
        if (useSymbol && !string.Equals(needSymbol, symbol.ToString(), StringComparison.CurrentCultureIgnoreCase) ||
            !sections.First(x => x.color == _circles[3].Image.color)
                .Symbols
                .Contains(symbol))
        {
            StopMusic(true);
            return;
        }

        // Обновляем круги
        UpdateCircles();
        // Добавляем подряд нажатую клавишу (комбо)
        _pressedCombo++;
        // Проверяем, набрано ли необходимо комбо для продолжения музыки
        if (_pressedCombo >= NeedKeysToContinue)
        {
            _levelAudio.UnPause();
            _errorSound.Stop();
            _timeOutSound.Stop();
        }

        // Обновляем музыку и время последнего ввода
        if (_lastKeyPlayed == default)
            _levelAudio.Play();
        _lastKeyPlayed = DateTime.Now;
        _isStopped = false;
    }

    private void UpdateCircles()
    {
        for (var i = 0; i < _circles.Count - 1; i++)
        {
            var nextCircle = _circles[i + 1];
            if (!nextCircle.GameObject.activeSelf) continue;
            var currentCircle = _circles[i];
            currentCircle.GameObject.SetActive(true);
            currentCircle.Image.color = nextCircle.Image.color;
            currentCircle.Label.text = nextCircle.Label.text;
        }

        var nextSection = sections[Random.Range(0, sections.Count)];
        var lastCircle = _circles[_circles.Count - 1];
        lastCircle.Image.color = nextSection.color;
        lastCircle.Label.text = nextSection.Symbols[Random.Range(0, nextSection.Symbols.Length)].ToString().ToUpper();
    }

    private IEnumerator DeleteNoteAfterAnimation(GameObject note)
    {
        yield return new WaitForSecondsRealtime(1.0f);
        Destroy(note);
    }

    private void StopMusic(bool isError)
    {
        if (isError)
        {
            _errorSound.Play();
            _errorSound.pitch = Random.Range(0.6f, 1.8f);
            //SpawnErrorNote();
            //ChangeColorOfCurrentSymbol(_index, errorColor);
        }
        else
        {
            if (_isStopped) return;
            _timeOutSound.Play();
        }

        _isStopped = true;

        _pressedCombo = 0;
        _levelAudio.Pause();
    }

    private class Circle
    {
        public readonly Text Label;
        public readonly GameObject GameObject;
        public readonly Image Image;

        public Circle(Color color, GameObject gameObject, string symbol, bool useSymbol)
        {
            GameObject = gameObject;
            Image = gameObject.GetComponent<Image>();
            Label = gameObject.transform.GetChild(1).GetComponent<Text>();
            Label.text = symbol;
            if (useSymbol)
                gameObject.transform.GetChild(1).gameObject.SetActive(true);
            Image.color = color;
        }
    }

    [Serializable]
    public class Section
    {
        [SerializeField] public string name;
        [SerializeField] public string symbolsString;
        [SerializeField] public Color color;

        public char[] Symbols => symbolsString.ToCharArray();
    }
}