using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MongeArcano
{
    /// <summary>Portão de pedra que desce ao chão. Pode exigir chaves/sigilos (puzzle estilo survival horror).</summary>
    public class Porta : Interagivel
    {
        public string[] chavesExigidas = new string[0];
        public bool consumirChaves = true;
        public string marco = "";
        public Transform painel;
        public Vector3 deslocamento = new Vector3(0f, -3.6f, 0f);
        public string rotuloCustom = "";

        bool aberta;

        public override string Rotulo
        {
            get { if (rotuloCustom.Length > 0) return rotuloCustom; return chavesExigidas.Length == 0 ? "Abrir portão" : "Abrir portão (trancado)"; }
        }

        public override bool Disponivel { get { return enabled && !aberta; } }

        public override void Interagir(PlayerController jogador)
        {
            GameManager gm = GameManager.Instance;
            var faltam = new List<string>();
            for (int i = 0; i < chavesExigidas.Length; i++)
                if (!gm.TemChave(chavesExigidas[i])) faltam.Add(GameManager.NomeChave(chavesExigidas[i]));

            if (faltam.Count > 0)
            {
                gm.Mostrar("Trancado. Falta: " + string.Join(", ", faltam.ToArray()), 3.5f);
                Som.Tocar(TipoSom.Trancada, transform.position, 0.9f);
                return;
            }

            if (consumirChaves) for (int i = 0; i < chavesExigidas.Length; i++) gm.UsarChave(chavesExigidas[i]);
            aberta = true;
            if (!string.IsNullOrEmpty(marco)) gm.Marco(marco);
            StartCoroutine(Abrir());
        }

        IEnumerator Abrir()
        {
            Som.Tocar(TipoSom.Porta, transform.position, 1f);
            GameManager.Instance.Tremer(0.2f);
            Vector3 p0 = painel.localPosition;
            Vector3 p1 = p0 + deslocamento;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / 1.3f;
                painel.localPosition = Vector3.Lerp(p0, p1, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }
            foreach (Collider c in painel.GetComponentsInChildren<Collider>()) c.enabled = false;
        }
    }
}
