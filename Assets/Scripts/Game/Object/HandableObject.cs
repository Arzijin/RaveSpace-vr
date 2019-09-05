using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CodingDuff.Objects
{
    [RequireComponent(typeof(Rigidbody))]
    public abstract class HandableObject : MonoBehaviour, IGrabbable, IPointerEnterHandler, IPointerExitHandler
    {
        #region Private Fields
        [SerializeField, HideInInspector]
        private Material _focusedMaterial;  // Same material for all

        [Header("HandableObject")]
        [SerializeField]
        private Transform _grabTransform;

        [Space]

        [SerializeField]
        private bool _isUseGravityOnStart = false;

        [SerializeField]
        private bool _isLockedByDefault = false;

        
        private Rigidbody _rigidBody;
        private Material _defaultMaterial;
        #endregion

        #region Protected Fields
        protected OVRInput.Controller _controllerGrabbing;
        public OVRInput.Controller ControllerGrabbing
        {
            get { return _controllerGrabbing; }
            set
            {
                if (_controllerGrabbing == value)
                {
                    return;
                }

                _controllerGrabbing = value;

                if (_controllerGrabbing == OVRInput.Controller.RTouch)
                {
                    SetRightHand();
                }
                else
                {
                    SetLeftHand();
                }
            }
        }
        #endregion

        #region Public Properties
        public Transform GrabTransform
        {
            get { return _grabTransform; }
        }

        /// <summary>
        /// Is object focused ?
        /// </summary>
        private bool _isFocused;
        public bool IsFocused
        {
            get { return _isFocused; }
            internal set
            {
                if (_isFocused == value)
                {
                    return;
                }

                _isFocused = value;

                GetComponentInChildren<MeshRenderer>().material = _isFocused ? _focusedMaterial : _defaultMaterial;
            }
        }
        public virtual bool IsGrabbed { get; internal set; }
        public bool IsLocked { get; set; }

        /// <summary>
        /// Apply Velocity of Object
        /// </summary>
        public Vector3 Velocity
        {
            get
            {
                if (_rigidBody == null)
                {
                    return Vector3.zero;
                }

                return _rigidBody.velocity;
            }

            set
            {
                if (_rigidBody == null)
                {
                    return;
                }

                _rigidBody.AddForce(value);
            }
        }
        #endregion

        #region Monobehaviour Functions
        protected void Awake()
        {
            _rigidBody = GetComponent<Rigidbody>();
            _rigidBody.useGravity = _isUseGravityOnStart;

            _defaultMaterial = GetComponentInChildren<MeshRenderer>().material;
        }

        private void OnApplicationQuit()
        {
            UnGrab();
            OnPointerExit(null);
        }
        #endregion

        #region Protected Virtual Functions
        protected virtual void SetLeftHand() { } 
        protected virtual void SetRightHand() { }
        #endregion

        #region Actions
        /// <summary>
        /// Primary action ( Triggered by index trigger on oculus touch ) 
        /// </summary>
        public void DoPimaryAction()
        {
        }

        /// <summary>
        /// Secondary Action ( Triggered by two button on oculus touch )
        /// </summary>
        public void DoSecondaryAction()
        {
        }
        #endregion

        #region IGrabble Functions
        public void Grab(OVRInput.Controller controller)
        {
            ControllerGrabbing = controller;

            IsGrabbed = true;

            _rigidBody.useGravity = false;
            _rigidBody.constraints = RigidbodyConstraints.FreezeAll;
            _rigidBody.isKinematic = true;

            IsLocked = _isLockedByDefault;
        }
        public void UnGrab()
        {
            IsGrabbed = false;

            _rigidBody.useGravity = true;
            _rigidBody.constraints = RigidbodyConstraints.None;
            _rigidBody.isKinematic = false;
        }
        #endregion

        #region IPointer
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (IsGrabbed)
            {
                return;
            }

            IsFocused = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            IsFocused = false;
        }
        #endregion
    }
}
