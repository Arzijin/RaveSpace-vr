using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace App
{
    public class ButtonColor : Button 
    {
        [SerializeField]
        private GameBehaviour.SymbolColor _color;

        public GameBehaviour.SymbolColor Color
        {
            get { return _color; }
        }
    }
}

