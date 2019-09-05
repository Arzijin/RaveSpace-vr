using System.Collections;
using App.Utils;
using CINEVR.App.Networking;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace App.UI
{
    [RequireComponent(typeof(Canvas))]
    public class UICentralManager : Singleton<UICentralManager>
    {
        #region Private Fields
        [SerializeField] private Text _yourScore;
        [SerializeField] private Text _challengerScore;
        #endregion

        [SerializeField] private Transform _uiTransformPlayer1;
        [SerializeField] private Transform _uiTransformPlayer2;

        private Canvas _canvas;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _canvas.enabled = false;
        }

        public void StartLookingScore()
        {
            Transform t = NetworkManager.Instance.YourID == 0 ? _uiTransformPlayer1 : _uiTransformPlayer2;

            transform.position = t.position;
            transform.rotation = t.rotation;

            _canvas.enabled = true;
            
            StartCoroutine(LookingForScore());
        }

        public void StopLookingScore()
        {
            StopAllCoroutines();

            _canvas.enabled = false;
        }

        WaitForSeconds sec = new WaitForSeconds(0.5f);
        private IEnumerator LookingForScore()
        {
            while (true)
            {
                yield return sec;

                Hashtable customProperties = PhotonNetwork.room.CustomProperties;

                if (customProperties["Player1"] == null || customProperties["Player2"] == null)
                {
                    yield return null;
                }

                _yourScore.text = string.Format("Your score : {0}", NetworkManager.Instance.YourID == 0 ? customProperties["Player1"] : customProperties["Player2"]);
                _challengerScore.text = string.Format("Challenger score : {0}", NetworkManager.Instance.YourID == 0 ? customProperties["Player2"] : customProperties["Player1"]);
            }
        }
    }
}

