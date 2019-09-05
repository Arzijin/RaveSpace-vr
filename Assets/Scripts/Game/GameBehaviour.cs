using System.Collections;
using App.UI;
using App.Utils;
using CINEVR.App.Networking;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace App
{
    public class GameBehaviour : Singleton<GameBehaviour>
    {
        #region enum
        public enum SymbolColor
        {
            YELLOW,
            RED,
            BLUE,
            GREEN
        }

        public enum SymbolType
        {
            UDU,
            BURU,
            BULBU,
            KATAK,
            RATAK,
            MISO
        }

        public class SymbolSet
        {
            public Symbol Symbol;
            public SymbolColor Color;
        }
        #endregion

        #region Public Properties

        private bool _isRunning;
        public bool IsRunning
        {
            get { return _isRunning; }
            set
            {
                if (_isRunning == value)
                {
                    return;
                }

                _isRunning = value;

                _platformPlayer1.IsRunning = value;
                _platformPlayer2.IsRunning = value;
            }
        }

        private int _scorePlayer1;
        public int ScorePlayer1
        {
            get { return _scorePlayer1; }
            set
            {
                _scorePlayer1 = value;

                if (_scorePlayer1 < 0)
                {
                    _scorePlayer1 = 0;
                }

                if (!PhotonNetwork.inRoom)
                {
                    return;
                }

                Hashtable customProperties = PhotonNetwork.room.CustomProperties;
                customProperties["Player1"] = _scorePlayer1;
                PhotonNetwork.room.SetCustomProperties(customProperties);
            }
        }

        private int _scorePlayer2;
        public int ScorePlayer2
        {
            get { return _scorePlayer2; }
            set
            {
                _scorePlayer2 = value;

                if (_scorePlayer2 < 0)
                {
                    _scorePlayer2 = 0;
                }

                if (!PhotonNetwork.inRoom)
                {
                    return;
                }

                Hashtable customProperties = PhotonNetwork.room.CustomProperties;
                customProperties["Player2"] = _scorePlayer2;
                PhotonNetwork.room.SetCustomProperties(customProperties);
            }
        }

        [SerializeField] private GameObject _randomGeneratorPrefab;
        private RandomShapeGenerator _randomShapeShapeGenerator;
        public RandomShapeGenerator RandomShapeGenerator { get { return _randomShapeShapeGenerator; } }

        private AudioSource _audioSource;

        [SerializeField] private AudioClip _readyClip;
        [SerializeField] private AudioClip _123Clip;
        [SerializeField] private AudioClip _goClip;

        [SerializeField] private AudioClip _musicClip;

        [SerializeField] private AudioClip _winClip;
        [SerializeField] private AudioClip _looseClip;

        [SerializeField] private PlatformManager _platformPlayer1; 
        [SerializeField] private PlatformManager _platformPlayer2;

        public PlatformManager PlatformPlayer1
        {
            get { return _platformPlayer1; }
        }

        public PlatformManager PlatformPlayer2
        {
            get { return _platformPlayer2; }
        }
        #endregion

        #region Monobehaviour Functions
        private void Awake()
        {
            _randomShapeShapeGenerator = FindObjectOfType<RandomShapeGenerator>();
            _audioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            //StartCountDown();
        }
        #endregion

        #region Public Functions
        public void StartCountDown()
        {
            Debug.Log("[GameBehaviour] StartCountDown(). ");
            StartCoroutine(CountDown());
        }
        #endregion

        #region Private Functions
        private void StartGame()
        {
            Debug.Log("[GameBehaviour] GameBehaviour().");

            _audioSource.clip = _musicClip;
            _audioSource.Play();

            RandomShapeGenerator.StartGeneration();
            UICentralManager.Instance.StartLookingScore();

            StartCoroutine(WaitForEndOfMusic());

            IsRunning = true;
        }
        #endregion

        #region Coroutines
        WaitForSeconds sec = new WaitForSeconds(1f);
        private IEnumerator CountDown()
        {
            //yield return new WaitForSeconds(3f);

            int count = 3;

            _audioSource.clip = _readyClip;
            _audioSource.Play();

            yield return sec;

            _audioSource.clip = _123Clip;
            _audioSource.Play();

            for (int i = 0; i < 3; i++)
            {
                int f = count - i;
                yield return sec;
            }

            _audioSource.clip = _goClip;
            _audioSource.Play();

            StartGame();
        }

        private IEnumerator WaitForEndOfMusic()
        {
            while (_audioSource.isPlaying)
            {
                yield return sec;
            }

            RandomShapeGenerator.StopGeneration();

            if (NetworkManager.Instance.YourID == 0)
            {
                _audioSource.clip = ScorePlayer1 > ScorePlayer2 ? _winClip : _looseClip;
            }
            else
            {
                _audioSource.clip = ScorePlayer1 > ScorePlayer2 ? _looseClip : _winClip;
            }

            _audioSource.Play();

            ScorePlayer1 = 0;
            ScorePlayer2 = 0;

            IsRunning = false;
        }
        #endregion
    }
}
