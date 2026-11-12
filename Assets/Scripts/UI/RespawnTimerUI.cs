using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class RespawnTimerUI : MonoBehaviour
{
    [SerializeField] TMP_Text labelValue;

    float value;

    public System.Action onTimerNotify;

    public void Init()
    {
        value = 3.5f;
    }

    private void Update()
    {
        if(value <= 0 && !GameManager.Instance.complete)
        {
            onTimerNotify?.Invoke();
        }
        else
        {
            value -= Time.deltaTime;
        }

        labelValue.text = $"{value:F1}";
    }
}
