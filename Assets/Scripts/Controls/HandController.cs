using UnityEngine;

using CodingDuff.Objects;
using UnityEngine.EventSystems;

namespace Game.Controls
{
    public class HandController : MonoBehaviour
    {
        #region Constants 
        private const float TRIGGER_THRESHOLD = 0.5f;
        #endregion

        #region Private Fields 
        [SerializeField]
        private OVRInput.Controller _controller;

        private HandableObject _currentObject;

        private Vector3 lastVelocity = Vector3.zero;
        #endregion

        #region Public Properties
        public Vector3 Velocity
        {
            get;
            internal set;
        }

        public bool IsGrabbing { get { return _currentObject != null; } }
        #endregion

        #region MonoBehaviour Functions
        public void Update()
        {
            // Already Grabbing object
            if (IsGrabbing)
            {
                // CurrentObject is grabbed
                //
                if (_currentObject.IsGrabbed)
                {
                    // Index trigger
                    //
                    if (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, _controller) > TRIGGER_THRESHOLD)
                    {
                        _currentObject.DoPimaryAction();
                    }

                    if (OVRInput.Get(OVRInput.Button.One, _controller))
                    {
                        _currentObject.DoSecondaryAction();
                    }

                    if (OVRInput.Get(OVRInput.Button.Two, _controller))
                    {
                        _currentObject.IsLocked = !_currentObject.IsLocked;
                    }

                    // Primary Hand trigger when _currentObject isn't locked
                    // 
                    if (!_currentObject.IsLocked && OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, _controller) < TRIGGER_THRESHOLD)
                    {
                        UnGrabObject();
                    }
                }
            }
 
            // TODO : Improve that...
            Velocity = (transform.position - lastVelocity) / Time.fixedDeltaTime;
            lastVelocity = transform.position;
        }

        private void OnCollisionEnter(Collision collision)
        {
                GameObject g = ExecuteEvents.GetEventHandler<IGrabbable>(collision.collider.gameObject);

                if (g)
                {
                    HandableObject hO = g.GetComponent<HandableObject>();
                    if (hO && !hO.IsGrabbed)
                    {
                        if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, _controller) > TRIGGER_THRESHOLD)
                        {
                            GrabObject(hO);
                        }
                    }
                }
            
        }
        #endregion

        #region Protected Functions
        /// <summary>
        /// GrabObject
        /// Grab a HandableObject
        /// </summary>
        /// <param name="hObj"> Selected object </param>
        protected void GrabObject(HandableObject hObj)
        {
            if (hObj == null)
            {
                return;
            }

            hObj.transform.SetParent(transform);
            hObj.transform.localPosition = -(_controller == OVRInput.Controller.RTouch ? hObj.GrabTransform.localPosition : Vector3.Scale(hObj.GrabTransform.localPosition, new Vector3(-1, 1, 1)));
            hObj.transform.localRotation = Quaternion.identity;
            // TODO : Grab with the good rotation
            hObj.Grab(_controller);

            _currentObject = hObj;
        }

        /// <summary>
        /// UnGrabObject
        /// UnGrab the current grabbed object
        /// </summary>
        protected void UnGrabObject()
        {
            if (_currentObject == null)
            {
                return;
            }

            _currentObject.transform.SetParent(null);
            _currentObject.UnGrab();

            _currentObject = null;
        }
        #endregion
    }
}
