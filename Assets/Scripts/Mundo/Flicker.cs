using UnityEngine;

namespace MongeArcano
{
    /// <summary>Tremulação de chama em luzes (tochas, velas, braseiros).</summary>
    [RequireComponent(typeof(Light))]
    public class Flicker : MonoBehaviour
    {
        public float amplitude = 0.35f;
        Light luz;
        float baseIntensidade, fase;

        void Awake()
        {
            luz = GetComponent<Light>();
            baseIntensidade = luz.intensity;
            fase = Random.value * 100f;
        }

        void Update()
        {
            float n = Mathf.PerlinNoise(Time.unscaledTime * 6f, fase);
            luz.intensity = baseIntensidade * (1f - amplitude * 0.5f + amplitude * n);
        }
    }
}
