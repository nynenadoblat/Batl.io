using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

[ExecuteInEditMode]
public class ProgressBar : MonoBehaviour
{
    [SerializeField] Image bar;
    [SerializeField] Image hiddenBar;
    [SerializeField] Image glow;
    [SerializeField] float speedFilling = 0.3f;
    [SerializeField] float speedDecline = 0.8f;

    [Header("FILLING SETTINGS")]
    [SerializeField] InitialFilling initFilling;
    [SerializeField] bool twoLayerFill = true;
    [SerializeField] bool glowFill;
    [SerializeField] bool glowDecline;

    [Space(8)]
    [Range(0,1)] public float value = 1f;

    [Header("GLOW SETTINGS")]
    public Color colorGlowFill;
    public Color colorGlowDecline;
    [SerializeField] [Range(0.1f, 1f)] float duration = 0.5f;


    [HideInInspector] [SerializeField] float sizeX;
    float oldValue;
    float currentValue;

    RectPosition oldTransform;
    RectPosition crtTransform;
    Coroutine smoothFilling;
    Coroutine smoothDecline;
    Coroutine glowStart;

    void Awake()
    {
        hiddenBar.rectTransform.offsetMax = Vector2.zero;
        hiddenBar.rectTransform.offsetMin = Vector2.zero;

        value = 1;
        sizeX = hiddenBar.rectTransform.rect.width;
        if (initFilling == 0)
        {
            value = 0;
        }

        oldValue = value;
        currentValue = value;
    }
    void Start()
    {
        Color c = glow.color;
        c.a = 0;
        glow.color = c;

        RectTransform rect = GetComponent<RectTransform>();
        oldTransform = new RectPosition(rect.anchorMin, rect.anchorMax, rect.anchoredPosition);
        crtTransform = oldTransform;

        UpdateView(hiddenBar, oldValue);
        hiddenBar.enabled = twoLayerFill;

        glow.gameObject.SetActive(false);
    }

   
    void Update()
    {
        value = Mathf.Clamp01(value);

        if (glowFill)
        {
            if (oldValue < value)
            {
                Glow(colorGlowFill);
            }
        }
        if (glowDecline)
        {
            if(currentValue > value)
            {
                Glow(colorGlowDecline);
            }
        }

        if (value > currentValue && smoothFilling == null)
        {
            smoothFilling = StartCoroutine(SmoothFilling());
            if (!twoLayerFill)
            {
                StopCoroutine(ref smoothDecline);
            }
            else
            {
                oldValue = value;
            }
        }
        else if (value <= currentValue)
        {
            currentValue = value;
        }

        if(value < oldValue && smoothDecline == null)
        {
            smoothDecline = StartCoroutine(SmoothDecline());
            if (twoLayerFill)
            {
                StopCoroutine(ref smoothFilling);
            }
        }
        else if(value >= oldValue)
        {
            oldValue = value;
        }
        
        UpdateView(bar, currentValue);
    }


//#if UNITY_EDITOR
//    private void LateUpdate()
//    {
        
//        RectTransform rect = GetComponent<RectTransform>();
//        crtTransform = new RectPosition(rect.anchorMin, rect.anchorMax, rect.anchoredPosition);

//        if (crtTransform != oldTransform)
//        {
//            bar.rectTransform.offsetMax = Vector2.zero;
//            bar.rectTransform.offsetMin = Vector2.zero;
//            sizeX = bar.rectTransform.rect.width;
//            oldTransform = crtTransform;
//        }
//    }
//#endif

    void Glow(Color color)
    {
        glow.color = color;
        if (glowStart != null) StopCoroutine(ref glowStart);
        glowStart = StartCoroutine(GlowStart());
    }

    IEnumerator SmoothFilling()
    {
        float velocity = 0;
       
        if (twoLayerFill)
        {
            oldValue = value;
            UpdateView(hiddenBar, oldValue);
        }

        while (!Mathf.Approximately(value, currentValue))
        {
            yield return null;
            currentValue = Mathf.SmoothDamp(currentValue, value, ref velocity, speedFilling);

            if (twoLayerFill)
            {
                UpdateView(hiddenBar, oldValue);
            }
        }

        smoothFilling = null;
        currentValue = value;
    }

    IEnumerator SmoothDecline()
    {
        float velocity = 0;
        hiddenBar.enabled = true;

        while (!Mathf.Approximately(value, oldValue))
        {
            yield return null;
            oldValue = Mathf.SmoothDamp(oldValue, value, ref velocity, speedDecline);
            UpdateView(hiddenBar, oldValue);
        }

        oldValue = value;
        smoothDecline = null;
        UpdateView(hiddenBar, oldValue);
    }

    IEnumerator GlowStart()
    {
        float velocity = 0f;
        float speed = duration;
        glow.gameObject.SetActive(true);
        Color c = glow.color;

        while (!Mathf.Approximately(c.a, 1f))
        {
            c.a = Mathf.SmoothDamp(c.a, 1, ref velocity, speed);
            glow.color = c;
            speed -= 0.01f;
            yield return null;
        }
        velocity = 0f;
        speed = duration;
        while (c.a > 0.01f)
        {
            c.a = Mathf.SmoothDamp(c.a, 0, ref velocity, speed);
            glow.color = c;
            yield return null;
        }
        glow.gameObject.SetActive(false);

        glowStart = null;
    }

    

    void UpdateView (Image bar, float v)
    {
        float subtractValue = sizeX * v;
        
        bar.rectTransform.sizeDelta = new Vector2(subtractValue - sizeX, 0);
        bar.rectTransform.anchoredPosition = new Vector2((subtractValue - sizeX) / 2, 0);
    }


    //private void OnDisable()
    //{
    //    value = 1;
    //    currentValue = 1;
    //    UpdateView(bar, currentValue);
    //}

    void StopCoroutine(ref Coroutine cor)
    {
        if (cor != null)
        {
            base.StopCoroutine(cor);
            cor = null;
        }
    }


    enum InitialFilling
    {
        Empty,
        Full
    }

//#pragma warning disable CS0660 // Тип определяет оператор == или оператор !=, но не переопределяет Object.Equals(object o)
//#pragma warning disable CS0661 // Тип определяет оператор == или оператор !=, но не переопределяет Object.GetHashCode()
    private struct RectPosition
//#pragma warning restore CS0661 // Тип определяет оператор == или оператор !=, но не переопределяет Object.GetHashCode()
//#pragma warning restore CS0660 // Тип определяет оператор == или оператор !=, но не переопределяет Object.Equals(object o)
    {
        public Vector2 anchorMin;
        public Vector2 anchorMax;
        public Vector2 anchorPos;

        public RectPosition (Vector2 min, Vector2 max, Vector2 pos)
        {
            this.anchorMin = min;
            this.anchorMax = max;
            this.anchorPos = pos;
        }

        public static bool operator == (RectPosition rp1, RectPosition rp2)
        {
            return rp1.anchorMin == rp2.anchorMin && rp1.anchorMax == rp2.anchorMax && rp1.anchorPos == rp2.anchorPos;
        }

        public static bool operator != (RectPosition rp1, RectPosition rp2)
        {
            return rp1.anchorMin != rp2.anchorMin || rp1.anchorMax != rp2.anchorMax || rp1.anchorPos != rp2.anchorPos;
        }
    }
}
