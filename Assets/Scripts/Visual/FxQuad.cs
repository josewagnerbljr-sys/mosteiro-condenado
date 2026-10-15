using UnityEngine;

namespace MongeArcano
{
    /// <summary>Quad luminoso que cresce e desvanece (clarão, anel no chão) ou fica pulsando (halo de projétil).</summary>
    public class FxQuad : MonoBehaviour
    {
        public bool billboard = true;
        public bool perene;
        public float pulso;
        public float duracao = 0.5f;
        public float tamanhoInicial = 0.2f;
        public float tamanhoFinal = 3f;
        public Color cor = Color.white;
        public Light luz;

        Material mat;
        float t, luzInicial;

        void Start()
        {
            mat = GetComponent<Renderer>().sharedMaterial;
            if (luz != null) luzInicial = luz.intensity;
        }

        void LateUpdate()
        {
            t += Time.deltaTime;
            float k = perene ? 0f : Mathf.Clamp01(t / duracao);
            float e = 1f - (1f - k) * (1f - k);
            float tam = Mathf.Lerp(tamanhoInicial, tamanhoFinal, e);
            if (perene) tam *= 1f + pulso * Mathf.Sin(Time.time * 9f);
            transform.localScale = new Vector3(tam, tam, 1f);
            if (billboard && Camera.main != null) transform.rotation = Camera.main.transform.rotation;
            Color c = cor;
            c.a = perene ? cor.a : cor.a * (1f - k);
            if (mat != null) Fx.SetCor(mat, c);
            if (luz != null) luz.intensity = luzInicial * (1f - k);
            if (!perene && k >= 1f) Destroy(gameObject);
        }

        void OnDestroy() { if (mat != null) Destroy(mat); }
    }
}
