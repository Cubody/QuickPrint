using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TextMoveController : MonoBehaviour
{
    public GameObject textPrefab;
    public GameObject notePrefab;

    private float _widthDelta => LayoutUtility.GetPreferredWidth(_movingText.GetComponent<RectTransform>()) /
                                 _regexRichFormatter.Replace(_textComp.text, "").Length;

    public float pointerHeightDelta;
    public Color currentSymbolColor;
    public Color errorColor;
    private GameObject _movingText;
    private Vector3 _movingTextPosition;
    private AudioSource _levelAudio;
    private AudioSource _errorSound;
    private AudioSource _timeOutSound;
    private readonly Regex _regexRichFormatter = new Regex(@"<\/?[a-z][a-z0-9]*[^<>]*>|<!--.*?-->");

    private void Start()
    {
        // Получение звуковых дорожек
        _levelAudio = transform.GetChild(2).GetComponent<AudioSource>();
        _errorSound = transform.GetChild(3).GetComponent<AudioSource>();
        _timeOutSound = transform.GetChild(4).GetComponent<AudioSource>();
        // Создание движущегося текста
        _movingText = Instantiate(textPrefab, transform);
        _textComp = _movingText.GetComponent<Text>();
        _movingTextPosition = transform.GetChild(0).transform.localPosition + new Vector3(0, pointerHeightDelta) -
                              new Vector3(_widthDelta / 2, 0);
        _movingText.transform.SetSiblingIndex(transform.childCount - 2);
        _textComp.text =
            "Экземпляр текста для проверки ввода. Экземпляр текста для проверки ввода. Экземпляр текста для проверки ввода. Экземпляр текста для проверки ввода. Экземпляр текста для проверки ввода. Экземпляр текста для проверки ввода. Экземпляр текста для проверки ввода. Экземпляр текста для проверки ввода. ";
        _movingText.name = "MovingText";
        MoveTextToIndex(_index);
    }

    private int _index;
    private Text _textComp;
    private DateTime _lastKeyPlayed;
    private const float TimeForPlay = 2.0f;
    private const int NeedKeysToContinue = 2;
    private int _pressedCombo;
    private bool _isStopped;

    private void Update()
    {
        // Проверяем на долгий ввод
        if (_lastKeyPlayed != default && (DateTime.Now - _lastKeyPlayed).TotalSeconds >= TimeForPlay)
            StopMusic(false);
        if (!Input.anyKeyDown) return;
        // Удаляем Rich форматирование из текста регуляркой
        var check = _regexRichFormatter.Replace(_textComp.text, "");
        if (Input.inputString == "")
            return;
        // Проверяем на соответствие ожидаемого ввода
        if (Input.inputString != check[_index].ToString())
        {
            StopMusic(true);
            return;
        }

        MoveTextToIndex(_index + 1);
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

    private void StopMusic(bool isError)
    {
        if (isError)
        {
            _errorSound.Play();
            _errorSound.pitch = Random.Range(0.6f, 1.8f);
            SpawnErrorNote();
            ChangeColorOfCurrentSymbol(_index, errorColor);
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

    // Двигаем индексированную букву текста на указатель
    private void MoveTextToIndex(int idx)
    {
        ChangeColorOfCurrentSymbol(idx, currentSymbolColor);
        _movingText.transform.localPosition = _movingTextPosition - new Vector3(_widthDelta, 0) * idx;
        _index = idx;
    }

    private void SpawnErrorNote()
    {
        var note = Instantiate(notePrefab, transform);
        note.name = "Note";
        note.transform.rotation =
            new Quaternion(0, 0, Random.Range(-200.0f, 200.0f), Random.Range(-200.0f, 200.0f));
        note.transform.GetComponent<Image>().color = errorColor;
        StartCoroutine(DeleteNoteAfterAnimation(note));
    }
    
    private IEnumerator DeleteNoteAfterAnimation(GameObject note)
    {
        yield return new WaitForSecondsRealtime(1.0f);
        Destroy(note);
    }

    private void ChangeColorOfCurrentSymbol(int idx, Color color)
    {
        var textWithoutRich = _regexRichFormatter.Replace(_textComp.text, "");
        _textComp.text =
            $"{textWithoutRich.Substring(0, idx)}<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{textWithoutRich[idx]}</color>{textWithoutRich.Substring(idx + 1)}";
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        StopMusic(false);
    }
}