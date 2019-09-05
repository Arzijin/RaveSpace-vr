using UnityEngine;

namespace App.Networking
{
    public class NetworkController : MonoBehaviour
    {
        #region Private Fields
        [SerializeField]
        private GameObject _cameraRig;

        private PhotonView _photonView;
        #endregion
       
        #region Monobehaviour Functions
        private void Start()
        {
            _photonView = GetComponent<PhotonView>();

            if (!_photonView.isMine)
            {
                DestroyImmediate(_cameraRig);
            }
        }
        #endregion
    }
}

