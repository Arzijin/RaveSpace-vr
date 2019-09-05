using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnLocator : MonoBehaviour
{
    public bool IsAvailable { get; set; }

    private void Awake()
    {
        IsAvailable = true;
    }
}
