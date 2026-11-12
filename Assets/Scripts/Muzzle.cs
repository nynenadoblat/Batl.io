using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Muzzle : MonoBehaviour
{
    [SerializeField] Transform point;
    [SerializeField] public Material friendly;
    [SerializeField] public Material enemy;

    public Transform Point => point;
}
