using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.UIElements;

namespace CodingDuff.Objects
{
    interface IGrabbable : IEventSystemHandler
    {
        void Grab(OVRInput.Controller controller);
        void UnGrab();
    }
}
