using System.Collections;
using App;
using CINEVR.App.Networking;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlatformManager : MonoBehaviour
{
    [SerializeField]
    private AudioClip _wrongClip;

    [SerializeField]
    private AudioClip _transmitClip;

    [SerializeField] private Light _answerLight;
    [SerializeField] private Color _wrongColor;
    [SerializeField] private Color _goodColor;

    [SerializeField] private Button _transmitButton;

    public GameBehaviour.SymbolSet CurrenSymbolSet
    {
        get
        {
            if (_currentSelectedButtonSymbol == null || _currentSelectedButtonColor == null)
            {
                return null;
            }

            return new GameBehaviour.SymbolSet
            {
                Color = _currentSelectedButtonColor.Color,
                Symbol = _currentSelectedButtonSymbol.Symbol
            };
        }
    }

    private AudioSource _audioSource;

    private ButtonColor _currentSelectedButtonColor;
    private ButtonSymbol _currentSelectedButtonSymbol;

    [SerializeField] private GameObject _lightFlash;

    private Button[] _buttons;

    private bool _isRunning;
    public bool IsRunning
    {
        get
        {
            return _isRunning;
        }
        set
        {
            if (_isRunning == value)
            {
                return;
            }

            _isRunning = value;
            
            _lightFlash.SetActive(_isRunning);

            ResetButtons();

            foreach (var b in _buttons)
            {
                b.IsInteractable = _isRunning;
            }
        }
    }

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _lightFlash.SetActive(false);

        _buttons = GetComponentsInChildren<Button>();

        foreach (var b in _buttons)
        {
            b.IsInteractable = false;
        }
    }

    public void WrongFlash()
    {
        StopAllCoroutines();

        StartCoroutine(FlashCoroutine(_wrongColor));
    }

    public void GoodFlash()
    {
        StopAllCoroutines();

        StartCoroutine(FlashCoroutine(_goodColor));
    }

    public void ResetButtons()
    {
        OnColorChosen(null);
        OnSymbolChosen(null);
        _transmitButton.IsPressed = false;
    }

    public void OnColorChosen(ButtonColor button)
    {
        if (!IsRunning)
        {
            return;
        }

        if (_currentSelectedButtonColor != null)
        {
            _currentSelectedButtonColor.IsPressed = false;
        }

        _currentSelectedButtonColor = button;
    }

    public void OnSymbolChosen(ButtonSymbol symbol)
    {
        if (!IsRunning)
        {
            return;
        }

        if (_currentSelectedButtonSymbol != null)
        {
            _currentSelectedButtonSymbol.IsPressed = false;
        }

        _currentSelectedButtonSymbol = symbol;
    }

    public void OnTransmitPushed()
    {
        if (!IsRunning)
        {
            return;
        }

        if (CurrenSymbolSet == null)
        {
            _audioSource.clip = _wrongClip;
            _audioSource.Play();
            ResetButtons();
            return;
        }

        GameBehaviour.Instance.RandomShapeGenerator.CheckPlayerEntry(NetworkManager.Instance.YourID, CurrenSymbolSet);
        ResetButtons();
    }

    private IEnumerator FlashCoroutine(Color color)
    {
        _answerLight.color = color;
        _answerLight.intensity = 2.5f;

        yield return new WaitForSeconds(0.5f);

        _answerLight.intensity = 0.0f;
    }
}
