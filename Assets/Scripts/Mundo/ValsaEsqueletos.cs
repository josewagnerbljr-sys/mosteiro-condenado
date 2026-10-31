using UnityEngine;

namespace MongeArcano
{
    /// <summary>Casal de esqueletos dançando uma valsa sombria: dois dançarinos animados girando juntos em torno de um centro.</summary>
    public class ValsaEsqueletos : MonoBehaviour
    {
        public Transform a, b;
        public float distancia = 0.62f;
        public float giro = 38f;
        public float orbita = 0.5f;
        public float raioOrbita = 0.9f;
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
            float ang = t * giro;
            Vector3 c = new Vector3(Mathf.Cos(t * orbita), 0f, Mathf.Sin(t * orbita)) * raioOrbita;
            transform.localPosition = centro + c;
            Quaternion r = Quaternion.Euler(0f, ang, 0f);
            if (a != null) { a.localPosition = r * new Vector3(distancia, 0f, 0f); a.localRotation = Quaternion.Euler(0f, ang + 270f, 0f); }
            if (b != null) { b.localPosition = r * new Vector3(-distancia, 0f, 0f); b.localRotation = Quaternion.Euler(0f, ang + 90f, 0f); }
        }
    }
}
