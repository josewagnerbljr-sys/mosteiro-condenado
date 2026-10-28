using System.Collections.Generic;
using UnityEngine;

namespace MongeArcano
{
    /// <summary>
    /// Animação procedural por OSSOS: caminhada e corrida com passada, flexão de joelho, braços em contrapeso, torção do tronco, respiração,
    /// ataque, conjuração e reação a dano. Usa o esqueleto criado nos modelos (Hips, Spine, Chest, Neck, Head, UpperArm/LowerArm/Hand, UpperLeg/LowerLeg/Foot, lados _E e _D).
    /// Os sentidos de rotação são calibrados sozinhos na partida (vale para qualquer conversão de eixos da importação).
    /// </summary>
    public class RigAnimador : MonoBehaviour
    {
        public float ampPernas = 28f, ampBracos = 24f, ampTronco = 7f, frequencia = 8f, balancoQuadril = 0.05f;
        public bool roupao;

        static readonly string[] nomes =
        {
            "Hips", "Spine", "Chest", "Neck", "Head",
            "UpperArm_E", "LowerArm_E", "Hand_E", "UpperArm_D", "LowerArm_D", "Hand_D",
            "UpperLeg_E", "LowerLeg_E", "Foot_E", "UpperLeg_D", "LowerLeg_D", "Foot_D"
        };
        const int Hips = 0, Spine = 1, Chest = 2, Neck = 3, Head = 4, BraE = 5, AntE = 6, MaoE = 7, BraD = 8, AntD = 9, MaoD = 10, PerE = 11, JoeE = 12, PeE = 13, PerD = 14, JoeD = 15, PeD = 16;

        readonly Transform[] ossos = new Transform[nomes.Length];
        readonly Quaternion[] repouso = new Quaternion[nomes.Length];
        readonly Vector3[] acc = new Vector3[nomes.Length];
        Vector3 hipsRepouso;
        float sinalPerna = 1f, sinalBraco = 1f, sinalTronco = 1f;
        bool pronto;
        Vector3 posAnt;
        float fase, velSuave, atqT = 1f, gestoT = 1f, hitT = 1f;

        /// <summary>Adiciona o animador se o modelo tem pele e esqueleto. Devolve null se não tiver.</summary>
        public static RigAnimador Montar(GameObject raiz, GameObject modelo)
        {
            if (modelo == null || modelo.GetComponentInChildren<SkinnedMeshRenderer>() == null) return null;
            if (Achar(modelo.transform, "Hips") == null) return null;
            RigAnimador r = raiz.GetComponent<RigAnimador>();
            if (r == null) r = raiz.AddComponent<RigAnimador>();
            return r;
        }

        static Transform Achar(Transform t, string nome)
        {
            if (t.name == nome) return t;
            for (int i = 0; i < t.childCount; i++)
            {
                Transform r = Achar(t.GetChild(i), nome);
                if (r != null) return r;
            }
            return null;
        }

        void Start()
        {
            for (int i = 0; i < nomes.Length; i++)
            {
                ossos[i] = Achar(transform, nomes[i]);
                if (ossos[i] == null) return;
                repouso[i] = ossos[i].localRotation;
            }
            hipsRepouso = ossos[Hips].localPosition;
            Vector3 frente = transform.forward;
            sinalPerna = Calibrar(PerE, PeE, frente);
            sinalBraco = Calibrar(BraE, MaoE, frente);
            sinalTronco = Calibrar(Spine, Head, frente);
            posAnt = transform.position;
            pronto = true;
        }

        float Calibrar(int osso, int ponta, Vector3 frente)
        {
            Transform o = ossos[osso], p = ossos[ponta];
            Quaternion q0 = o.localRotation;
            Vector3 p0 = p.position;
            o.localRotation = q0 * Quaternion.AngleAxis(25f, Vector3.right);
            Vector3 p1 = p.position;
            o.localRotation = q0;
            return Vector3.Dot(p1 - p0, frente) >= 0f ? 1f : -1f;
        }

        public void Atacar() { atqT = 0f; }
        public void Conjurar() { gestoT = 0f; }
        public void Atingido() { hitT = 0f; }

        void Add(int i, float x, float y = 0f, float z = 0f) { acc[i].x += x; acc[i].y += y; acc[i].z += z; }

        void LateUpdate()
        {
            if (!pronto) return;
            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            Vector3 d = transform.position - posAnt;
            d.y = 0f;
            posAnt = transform.position;
            velSuave = Mathf.Lerp(velSuave, d.magnitude / dt, 1f - Mathf.Exp(-10f * dt));
            float k = Mathf.Clamp01(velSuave / 3.2f);
            float corre = Mathf.Clamp01((velSuave - 3.6f) / 2.5f);
            fase += dt * frequencia * (0.6f + velSuave * 0.18f);
            float s = Mathf.Sin(fase), c = Mathf.Cos(fase), t = Time.time;
            atqT += dt / 0.5f; gestoT += dt / 0.45f; hitT += dt / 0.35f;

            for (int i = 0; i < acc.Length; i++) acc[i] = Vector3.zero;

            float aP = ampPernas * (1f + 0.35f * corre) * k * (roupao ? 0.2f : 1f);
            float aB = ampBracos * (1f + 0.3f * corre) * k;
            float gl = roupao ? 1.5f : 1f;

            Add(PerE, sinalPerna * s * aP); Add(PerD, -sinalPerna * s * aP);
            Add(JoeE, -sinalPerna * Mathf.Max(0f, c) * aP * 1.2f); Add(JoeD, -sinalPerna * Mathf.Max(0f, -c) * aP * 1.2f);
            Add(PeE, sinalPerna * Mathf.Max(0f, c) * aP * 0.35f); Add(PeD, sinalPerna * Mathf.Max(0f, -c) * aP * 0.35f);

            Add(BraE, -sinalBraco * s * aB); Add(BraD, sinalBraco * s * aB);
            Add(AntE, sinalBraco * (8f * k + Mathf.Max(0f, s) * 14f * k)); Add(AntD, sinalBraco * (8f * k + Mathf.Max(0f, -s) * 14f * k));

            Add(Hips, 0f, -s * 5f * gl * k, s * 3.5f * gl * k);
            Add(Spine, sinalTronco * (4f * k + 9f * corre), s * 6f * gl * k);
            Add(Chest, sinalTronco * (3f * k + 5f * corre), s * 5f * gl * k);
            Add(Head, -sinalTronco * (3f * k + 8f * corre), -s * 5f * k, 0f);

            float resp = Mathf.Sin(t * 1.6f);
            Add(Chest, sinalTronco * resp * 1.4f);
            Add(Head, 0f, Mathf.Sin(t * 0.7f) * 4f * (1f - k), 0f);
            Add(BraE, 0f, 0f, 4f + Mathf.Sin(t * 1.3f) * 2f); Add(BraD, 0f, 0f, -(4f + Mathf.Sin(t * 1.3f + 1f) * 2f));

            if (atqT < 1f)
            {
                float a = atqT, ang;
                if (a < 0.3f) ang = -140f * (a / 0.3f);
                else if (a < 0.5f) ang = Mathf.Lerp(-140f, 55f, (a - 0.3f) / 0.2f);
                else ang = Mathf.Lerp(55f, 0f, (a - 0.5f) / 0.5f);
                float peso = Mathf.Sin(Mathf.Clamp01(a) * Mathf.PI);
                Add(BraD, sinalBraco * ang);
                Add(AntD, sinalBraco * (a < 0.3f ? 40f * (a / 0.3f) : 15f));
                Add(Spine, sinalTronco * (a < 0.3f ? -8f * (a / 0.3f) : 18f * peso), a < 0.3f ? 12f * (a / 0.3f) : -18f * peso);
                Add(Chest, sinalTronco * 6f * peso);
                Add(PerE, sinalPerna * 18f * peso); Add(PerD, -sinalPerna * 10f * peso);
            }

            if (gestoT < 1f)
            {
                float g = Mathf.Sin(gestoT * Mathf.PI);
                Add(BraE, sinalBraco * 75f * g, 0f, 14f * g); Add(BraD, sinalBraco * 75f * g, 0f, -14f * g);
                Add(AntE, sinalBraco * 20f * g); Add(AntD, sinalBraco * 20f * g);
                Add(Head, -sinalTronco * 10f * g);
                Add(Spine, -sinalTronco * 5f * g);
            }

            if (hitT < 1f)
            {
                float h = 1f - hitT;
                Add(Spine, -sinalTronco * 18f * h); Add(Head, -sinalTronco * 12f * h);
                Add(BraE, -sinalBraco * 20f * h); Add(BraD, -sinalBraco * 20f * h);
            }

            for (int i = 0; i < ossos.Length; i++) ossos[i].localRotation = repouso[i] * Quaternion.Euler(acc[i]);
            float bob = Mathf.Abs(c) * balancoQuadril * hipsRepouso.y * k + resp * 0.006f * hipsRepouso.y;
            ossos[Hips].localPosition = hipsRepouso + new Vector3(0f, bob, 0f);
        }
    }
}
