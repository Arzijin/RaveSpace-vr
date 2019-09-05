using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace App
{
    [RequireComponent(typeof(AudioSource))]
    public class RandomShapeGenerator : MonoBehaviour
    {
        #region Constants
        private int POINT_SCORE = 1000;
        private int ERROR_SCORE = 2000;
        private int BONUS_SCORE = 3000;
        #endregion
        
        #region Private Fields
        [SerializeField] private float _shapeFrequency = 10f;
        [SerializeField] private float _shapeRotation = 0.03f;
        [SerializeField] private float _shapeAmp = 0.2f;

        [SerializeField] private Material _shapeMaterial;
        [SerializeField]
        private Symbol[] _symbols;

        private GameBehaviour.SymbolSet _currentSymbolSet;
        private GameObject _currentShapeObj;

        [SerializeField] private AudioClip _wrongClip;
        [SerializeField] private AudioClip _goodClip;

        private AudioSource _audioSource;
        private Vector3 _defaultScale = Vector3.one;

        private int _nbSuccess;

        public bool IsRunning { get; internal set; }
        #endregion

        public void Awake()
        {
            _defaultScale = transform.localScale;
            _audioSource = GetComponent<AudioSource>();
        }

        public void StartGeneration()
        {
            if (IsRunning)
            {
                return;
            }

            Debug.Log("[RandomShapeGeneration] Start");

            IsRunning = true;

            _currentSymbolSet = GenerateNewSymbol();
            _currentShapeObj = InstantiateSymbolSet(_currentSymbolSet);
        }

        public void StopGeneration()
        {
            IsRunning = false;

            Destroy(_currentShapeObj);
            _currentSymbolSet = null;
        }

        public void Update()
        {
            if (_currentShapeObj == null)
            {
                return;
            }

            transform.RotateAround(transform.position, Vector3.up, _shapeRotation);
            transform.localScale = _defaultScale + Mathf.Sin(Time.time * _shapeFrequency) * _shapeAmp * Vector3.one;
        }

        private GameBehaviour.SymbolSet GenerateNewSymbol()
        {
            int r = Random.Range(0, _symbols.Length - 1);
            GameBehaviour.SymbolColor c = (GameBehaviour.SymbolColor) Random.Range(0, 3);

            return new GameBehaviour.SymbolSet
            {
                Symbol = _symbols[r],
                Color = c
            };
        }

        private GameObject InstantiateSymbolSet(GameBehaviour.SymbolSet symbolSet)
        {
            GameObject go = new GameObject {name = "Shape"};
            MeshRenderer r = go.AddComponent<MeshRenderer>();
            go.AddComponent<MeshFilter>().mesh = symbolSet.Symbol.Mesh;
            r.material = _shapeMaterial;
            Material m = r.material;
            m.SetColor("_EmissionColor", GetColor(symbolSet.Color));

            go.transform.SetParent(transform);
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = Vector3.zero;
            return go;
        }

        private Color GetColor(GameBehaviour.SymbolColor symbolColor)
        {
            switch (symbolColor)
            {
                case GameBehaviour.SymbolColor.YELLOW:
                    return Color.yellow;
                case GameBehaviour.SymbolColor.RED:
                    return Color.red;
                case GameBehaviour.SymbolColor.BLUE:
                    return Color.blue;
                case GameBehaviour.SymbolColor.GREEN:
                    return Color.green;
                default:
                    throw new ArgumentOutOfRangeException("symbolColor", symbolColor, null);
            }
        }

        /// <summary>
        /// OnPhotonSerializeView
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="info"></param>
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
        }

        public void CheckPlayerEntry(int id, GameBehaviour.SymbolSet entry)
        {
            if (_currentSymbolSet == null)
            {
                return;
            }

            // Good 
            //
            if (_currentSymbolSet.Symbol.Type == entry.Symbol.Type && _currentSymbolSet.Color == entry.Color)
            {
                _audioSource.clip = _goodClip;

                if (id == 0)
                {
                    GameBehaviour.Instance.ScorePlayer1 += POINT_SCORE;
                    GameBehaviour.Instance.PlatformPlayer1.GoodFlash();
                }
                else
                {
                    GameBehaviour.Instance.ScorePlayer2 += POINT_SCORE;
                    GameBehaviour.Instance.PlatformPlayer2.GoodFlash();
                }

                Destroy(_currentShapeObj);
                _currentSymbolSet = GenerateNewSymbol();
                _currentShapeObj = InstantiateSymbolSet(_currentSymbolSet);

                _nbSuccess++;

                if (_nbSuccess == 5)
                {
                    if (id == 0)
                    {
                        GameBehaviour.Instance.ScorePlayer1 += BONUS_SCORE;
                    }
                    else
                    {
                        GameBehaviour.Instance.ScorePlayer2 += BONUS_SCORE;
                    }
                }
            }
            // Wrong
            //
            else
            {
                _audioSource.clip = _wrongClip;

                if (id == 0)
                {
                    GameBehaviour.Instance.ScorePlayer1 -= ERROR_SCORE;
                    GameBehaviour.Instance.PlatformPlayer1.WrongFlash();
                }
                else
                {
                    GameBehaviour.Instance.ScorePlayer2 -= ERROR_SCORE;
                    GameBehaviour.Instance.PlatformPlayer2.WrongFlash();
                }

                _nbSuccess = 0;
            }

            _audioSource.Play();
        }
    }
}

