using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Muzzle : MonoBehaviour
{
    [SerializeField] Transform point;
    [SerializeField] public Material friendly;
    [SerializeField] public Material enemy;

    [Tooltip("Множитель размера снаряда для этого ствола. Подберите так, чтобы снаряд соответствовал диаметру дула.")]
    [SerializeField] float projectileScale = 1f;

    public Transform Point => point;
    public float ProjectileScale => projectileScale;
}
