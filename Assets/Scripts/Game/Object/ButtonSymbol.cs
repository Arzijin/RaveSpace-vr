using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace App
{
    public class ButtonSymbol : Button
    {
        [SerializeField]
        private Symbol _symbol;

        public Symbol Symbol
        {
            get { return _symbol; }
        }

        [SerializeField]
        private MeshFilter _symbolMeshFilter;

        private void Awake()
        {
            _symbolMeshFilter.mesh = _symbol.Mesh;
        }

        private void Update()
        {
            _symbolMeshFilter.transform.RotateAround(_symbolMeshFilter.transform.position, Vector3.up, 0.1f);
        }
    }
}

