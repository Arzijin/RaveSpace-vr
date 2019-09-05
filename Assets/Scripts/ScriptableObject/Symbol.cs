using UnityEngine;

namespace App
{
    [CreateAssetMenu(fileName = "Symbol", menuName = "Shape")]
    public class Symbol : ScriptableObject
    {
        public Mesh Mesh;
        public GameBehaviour.SymbolType Type;
    }
}

