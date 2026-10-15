using UnityEngine;

namespace MongeArcano
{
    /// <summary>Casal esquelético dançando uma valsa sombria entre os túmulos: gira, orbita em círculo e sobe e desce no compasso de 3 tempos.</summary>
    public class DancaValsa : MonoBehaviour
    {
        public float raio = 0.9f;
        public float velocidadeOrbita = 0.6f;
        public float velocidadeGiro = 75f;
        public float altura = 0.14f;

        Vector3 centro;
        float fase;

        void Start()
        {
            centro = transform.localPosition;
            fase = Random.value * 6.28f;
        }

        void Update()
        {
            float t = Time.time + fase;
            float a = t * velocidadeOrbita;
            float compasso = 0.5f - 0.5f * Mathf.Cos(t * 4.2f);
            transform.localPosition = centro + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * raio + Vector3.up * (altura * compasso);
            transform.localRotation = Quaternion.Euler(Mathf.Sin(t * 2.1f) * 4f, t * velocidadeGiro, Mathf.Sin(t * 2.1f + 1f) * 5f);
        }
    }
}
