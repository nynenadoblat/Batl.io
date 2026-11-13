using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BatlIO.UI
{
    public class UIAmbientParticles : MonoBehaviour
    {
        [System.Serializable]
        public class ParticleData
        {
            public RectTransform rect;
            public Image image;
            public Vector2 basePos;
            public float speed;
            public float amplitudeX;
            public float amplitudeY;
            public float phase;
            public float minAlpha;
            public float maxAlpha;
            public float alphaSpeed;
        }

        [SerializeField] private List<ParticleData> particles = new List<ParticleData>();

        public void RegisterParticle(RectTransform rect, Image image, float speed, float ampX, float ampY, float minA, float maxA)
        {
            var p = new ParticleData
            {
                rect = rect,
                image = image,
                basePos = rect.anchoredPosition,
                speed = speed,
                amplitudeX = ampX,
                amplitudeY = ampY,
                phase = Random.Range(0f, Mathf.PI * 2f),
                minAlpha = minA,
                maxAlpha = maxA,
                alphaSpeed = Random.Range(0.8f, 1.6f)
            };
            particles.Add(p);
        }

        private void Update()
        {
            float t = Time.unscaledTime;
            for (int i = 0; i < particles.Count; i++)
            {
                var p = particles[i];
                if (p.rect == null) continue;

                float offsetX = Mathf.Sin(t * p.speed + p.phase) * p.amplitudeX;
                float offsetY = Mathf.Cos(t * (p.speed * 0.8f) + p.phase) * p.amplitudeY;
                p.rect.anchoredPosition = p.basePos + new Vector2(offsetX, offsetY);

                if (p.image != null)
                {
                    float alphaNorm = (Mathf.Sin(t * p.alphaSpeed + p.phase) + 1f) * 0.5f;
                    float alpha = Mathf.Lerp(p.minAlpha, p.maxAlpha, alphaNorm);
                    Color c = p.image.color;
                    c.a = alpha;
                    p.image.color = c;
                }
            }
        }
    }
}
