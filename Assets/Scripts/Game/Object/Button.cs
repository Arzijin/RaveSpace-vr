using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace App
{
    [RequireComponent(typeof(AudioSource))]
    public class Button : MonoBehaviour
    {
        public UnityEvent OnPushedEvent = new UnityEvent();

        #region Private Fields
        [SerializeField]
        private Material _outlineMaterial;

        [SerializeField] private Material _defaultMaterial;

        [SerializeField]
        private AudioClip _clickAudio;

        [SerializeField]
        private KeyCode _debugKeyCode;

        [SerializeField]
        private AudioSource _audioSource;

        [SerializeField]
        private float _distClick;
        #endregion

        #region Public Properties

        private bool _isPressed;
        public bool IsPressed
        {
            get { return _isPressed; }
            set
            {
                if (value == _isPressed)
                {
                    return;
                }

                _isPressed = value;

                MeshRenderer r = GetComponent<MeshRenderer>();

                var p = transform.localPosition;
                p.y += _isPressed ? -_distClick : _distClick;
                transform.localPosition = p;

                if (_isPressed)
                {
                    OnPushedEvent.Invoke();

                    _audioSource.Play();

                    r.materials = new[]
                    {
                        _defaultMaterial,
                        _outlineMaterial
                    };
                }
                else
                {
                    r.materials = new[]
                    {
                        _defaultMaterial
                };
                }
            }
        }


        private bool _isInteractable;
        public bool IsInteractable
        {
            get { return _isInteractable; }
            set
            {
                if (_isInteractable == value)
                {
                    return;
                }

                _isInteractable = value;

                if (!_isInteractable)
                {
                    IsPressed = false;
                }
            }
        }
        #endregion

        #region MonoBehaviour Functions
        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            //_audioSource.clip = _clickAudio;

            _defaultMaterial = GetComponent<MeshRenderer>().material;
        }

        private void Update()
        {
            if (Input.GetKeyDown(_debugKeyCode))
            {
                OnCollisionEnter(null);
            }
        }

        protected void OnCollisionEnter(Collision collision)
        {
            if (!IsInteractable)
            {
                return;
            }

            IsPressed = true;

            StopAllCoroutines();
            StartCoroutine(Vibro());
        }
        #endregion

        #region Coroutines

        private IEnumerator Vibro()
        {
            OVRInput.SetControllerVibration(0.1f, 0.8f, OVRInput.Controller.RTouch);
            OVRInput.SetControllerVibration(0.1f, 0.8f, OVRInput.Controller.LTouch);

            yield return new WaitForSeconds(0.25f);

            OVRInput.SetControllerVibration(0.0f, 0.0f, OVRInput.Controller.RTouch);
            OVRInput.SetControllerVibration(0.0f, 0.0f, OVRInput.Controller.LTouch);
        }
        #endregion
    }
}

