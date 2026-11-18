using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Muzzle : MonoBehaviour
{
    [SerializeField] Transform point;
    [SerializeField] public Material friendly;
    [SerializeField] public Material enemy;

    [Tooltip("Куб на конце ствола, из которого вылетает снаряд. Снаряд берёт его ширину.")]
    [SerializeField] Transform barrel;

    public Transform Point => point;

    // Ствол собран из стандартных кубов шириной 1, поэтому мировая ширина куба равна lossyScale.x.
    // В lossyScale входят масштаб ствола и масштаб танка, который растёт с уровнем.
    public float BarrelWidth => barrel.lossyScale.x;
}
