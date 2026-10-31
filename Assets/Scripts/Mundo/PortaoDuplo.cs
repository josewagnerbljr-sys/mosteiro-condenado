using System.Collections;
using UnityEngine;

namespace MongeArcano
{
    /// <summary>Portão gótico de duas folhas de ferro com dobradiças: fechado bloqueia a passagem; ao interagir as folhas giram e abrem de verdade.</summary>
    public class PortaoDuplo : Interagivel
    {
        public Transform folhaE, folhaD;
        public string marco = "porta_principal";
        public float anguloAbertura = 105f;
        public string[] chavesExigidas = new string[0];
        bool aberta;

        public override string Rotulo { get { return "Abrir o portão"; } }
        public override bool Disponivel { get { return enabled && !aberta; } }

        public override void Interagir(PlayerController jogador)
        {
            GameManager gm = GameManager.Instance;
            for (int i = 0; i < chavesExigidas.Length; i++)
                if (!gm.TemChave(chavesExigidas[i])) { gm.Mostrar("Trancado.", 2.5f); Som.Tocar(TipoSom.Trancada, transform.position, 0.9f); return; }
            aberta = true;
            if (!string.IsNullOrEmpty(marco)) gm.Marco(marco);
            StartCoroutine(Abrir());
        }

        IEnumerator Abrir()
        {
            Som.Tocar(TipoSom.Porta, transform.position, 1f);
            GameManager.Instance.Tremer(0.25f);
            Quaternion e0 = folhaE.localRotation, d0 = folhaD.localRotation;
            Quaternion e1 = e0 * Quaternion.Euler(0f, anguloAbertura, 0f), d1 = d0 * Quaternion.Euler(0f, -anguloAbertura, 0f);
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / 2.2f;
                float k = Mathf.SmoothStep(0f, 1f, t);
                folhaE.localRotation = Quaternion.Slerp(e0, e1, k);
                folhaD.localRotation = Quaternion.Slerp(d0, d1, k);
                yield return null;
            }
            foreach (Collider c in folhaE.GetComponentsInChildren<Collider>()) c.enabled = false;
            foreach (Collider c in folhaD.GetComponentsInChildren<Collider>()) c.enabled = false;
        }

        /// <summary>Cria o portão a partir do modelo dividido (Moldura, FolhaE, FolhaD) com colisores nas colunas e nas folhas.</summary>
        public static PortaoDuplo Montar(GameObject modelo, string marco)
        {
            Transform m = Achar(modelo.transform, "Moldura"), e = Achar(modelo.transform, "FolhaE"), d = Achar(modelo.transform, "FolhaD");
            if (m == null || e == null || d == null) return null;
            Caixa(m, new Vector3(-0.325f, 0.38f, 0f), new Vector3(0.26f, 0.76f, 0.44f));
            Caixa(m, new Vector3(0.325f, 0.38f, 0f), new Vector3(0.26f, 0.76f, 0.44f));
            Caixa(m, new Vector3(0f, 0.62f, 0f), new Vector3(0.42f, 0.30f, 0.34f));
            Caixa(e, new Vector3(0.1f, 0.22f, 0f), new Vector3(0.2f, 0.44f, 0.08f));
            Caixa(d, new Vector3(-0.1f, 0.22f, 0f), new Vector3(0.2f, 0.44f, 0.08f));
            PortaoDuplo p = modelo.AddComponent<PortaoDuplo>();
            p.folhaE = e; p.folhaD = d; p.marco = marco;
            return p;
        }

        static void Caixa(Transform t, Vector3 centro, Vector3 tamanho)
        {
            BoxCollider bc = t.gameObject.AddComponent<BoxCollider>();
            bc.center = centro; bc.size = tamanho;
        }

        static Transform Achar(Transform t, string nome)
        {
            if (t.name == nome) return t;
            for (int i = 0; i < t.childCount; i++) { Transform r = Achar(t.GetChild(i), nome); if (r != null) return r; }
            return null;
        }
    }
}
