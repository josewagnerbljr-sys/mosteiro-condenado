using UnityEngine;

namespace MongeArcano
{
    /// <summary>Esfera luminosa que se expande e some (impactos, explosões, altar). Sem partículas = funciona em qualquer build WebGL.</summary>
    public class EfeitoTemporario : MonoBehaviour
    {
        public float duracao = 0.5f;
        public float tamanhoFinal = 4f;
        public Light luz;

        float t;
        float luzInicial;

        void Start() { if (luz != null) luzInicial = luz.intensity; }

        void Update()
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duracao);
            transform.localScale = Vector3.one * Mathf.Lerp(0.2f, tamanhoFinal, 1f - (1f - k) * (1f - k));
            if (luz != null) luz.intensity = luzInicial * (1f - k);
            if (k >= 1f) Destroy(gameObject);
        }
    }
}
