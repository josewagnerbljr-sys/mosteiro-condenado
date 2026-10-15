using UnityEngine;

namespace MongeArcano
{
    /// <summary>
    /// Animação procedural estilizada: marcha com saltitos, balanço lateral e torção do tronco, esticar e achatar, respiração,
    /// ataque com antecipação e investida, conjuração e reação a dano. Funciona em modelos sem esqueleto (corpo inteiro) e,
    /// se houver braços/pernas separados, também os balança.
    /// </summary>
    public class AnimadorProcedural : MonoBehaviour
    {
        public Transform corpo, bracoE, bracoD, pernaE, pernaD;
        public Vector3 eixoPernas = Vector3.right;
        public Vector3 eixoBracos = Vector3.right;
        public float ampPernas = 30f, ampBracos = 25f, ampOciosoBracos = 2f;
        public float frequencia = 8f, inclinacao = 6f, balancoCorpo = 0.05f, oscilacaoOciosa = 0.012f, rolagem = 0f;
        public bool passos;
        public float estilo = 1.6f;

        Quaternion baseBE = Quaternion.identity, baseBD = Quaternion.identity, baseCorpoRot = Quaternion.identity;
        Vector3 baseCorpoPos, posAnt;
        float fase, velSuave, sinalAnt;
        float atqT = 1f, gestoT = 1f, hitT = 1f;

        void Start()
        {
            if (bracoE != null) baseBE = bracoE.localRotation;
            if (bracoD != null) baseBD = bracoD.localRotation;
            if (corpo != null) { baseCorpoPos = corpo.localPosition; baseCorpoRot = corpo.localRotation; }
            posAnt = transform.position;
        }

        public void Atacar() { atqT = 0f; }
        public void Conjurar() { gestoT = 0f; }
        public void Atingido() { hitT = 0f; }

        void LateUpdate()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            Vector3 d = transform.position - posAnt;
            d.y = 0f;
            posAnt = transform.position;
            velSuave = Mathf.Lerp(velSuave, d.magnitude / dt, 1f - Mathf.Exp(-10f * dt));
            float k = Mathf.Clamp01(velSuave / 3.2f);
            fase += dt * frequencia * (0.6f + velSuave * 0.18f);
            float s = Mathf.Sin(fase);

            if (passos && k > 0.3f && Mathf.Sign(s) != sinalAnt)
            {
                sinalAnt = Mathf.Sign(s);
                Som.Tocar(TipoSom.Passo, transform.position, 0.18f);
            }

            atqT += dt / 0.35f;
            gestoT += dt / 0.4f;
            hitT += dt / 0.25f;
            float gesto = gestoT < 1f ? Mathf.Sin(gestoT * Mathf.PI) : 0f;
            float hit = hitT < 1f ? 1f - hitT : 0f;

            if (pernaE != null) pernaE.localRotation = Quaternion.AngleAxis(s * ampPernas * k, eixoPernas);
            if (pernaD != null) pernaD.localRotation = Quaternion.AngleAxis(-s * ampPernas * k, eixoPernas);

            float atq = atqT < 1f ? Mathf.Sin(atqT * Mathf.PI) : 0f;
            float ociosa = Mathf.Sin(Time.time * 1.7f) * ampOciosoBracos;
            if (bracoE != null) bracoE.localRotation = baseBE * Quaternion.AngleAxis(-s * ampBracos * k + ociosa - gesto * 110f - atq * 70f, eixoBracos);
            if (bracoD != null) bracoD.localRotation = baseBD * Quaternion.AngleAxis(s * ampBracos * k - ociosa - atq * 70f, eixoBracos);

            if (corpo != null)
            {
                float passo = Mathf.Abs(Mathf.Sin(fase));
                float bob = passo * balancoCorpo * 1.8f * estilo * k + Mathf.Sin(Time.time * 1.6f) * oscilacaoOciosa;
                float investida = 0f;
                float inclinaAtaque = 0f;
                if (atqT < 1f)
                {
                    investida = -0.18f * Mathf.Sin(Mathf.Clamp01(atqT / 0.3f) * Mathf.PI) + 0.55f * Mathf.Sin(Mathf.Clamp01((atqT - 0.25f) / 0.5f) * Mathf.PI);
                    inclinaAtaque = atqT < 0.3f ? -10f * (atqT / 0.3f) : 18f * Mathf.Sin(Mathf.Clamp01((atqT - 0.25f) / 0.5f) * Mathf.PI);
                }
                corpo.localPosition = baseCorpoPos + new Vector3(0f, bob + gesto * 0.06f, investida - hit * 0.25f);
                corpo.localRotation = baseCorpoRot * Quaternion.Euler(inclinacao * k + inclinaAtaque - hit * 14f - gesto * 9f,
                    Mathf.Sin(fase) * 7f * estilo * k,
                    rolagem * Mathf.Sin(Time.time * 1.3f) + Mathf.Sin(fase) * 6f * estilo * k);
                float respira = Mathf.Sin(Time.time * 1.6f) * 0.012f;
                float esticar = Mathf.Cos(fase * 2f) * 0.05f * estilo * k - hit * 0.08f + (atqT < 1f ? 0.06f * Mathf.Sin(atqT * Mathf.PI) : 0f);
                corpo.localScale = new Vector3(1f + respira - esticar * 0.5f, 1f - respira + esticar, 1f + respira - esticar * 0.5f);
            }
        }
    }
}
