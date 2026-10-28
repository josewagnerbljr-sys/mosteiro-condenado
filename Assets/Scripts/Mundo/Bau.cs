using System.Collections;
using UnityEngine;

namespace MongeArcano
{
    /// <summary>Baú com equipamentos. Abre a tampa, solta itens ao redor e dá XP uma vez.</summary>
    public class Bau : Interagivel
    {
        public int itens = 3;
        public float bonusRaridade = 0.1f;
        public Transform tampa;
        bool aberto;

        public override string Rotulo { get { return "Abrir baú"; } }
        public override bool Disponivel { get { return enabled && !aberto; } }

        public override void Interagir(PlayerController jogador)
        {
            if (aberto) return;
            aberto = true;
            StartCoroutine(Abrir(jogador));
        }

        IEnumerator Abrir(PlayerController jogador)
        {
            Som.Tocar(TipoSom.Bau, transform.position, 1f);
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / 0.6f;
                if (tampa != null) tampa.localRotation = Quaternion.Euler(Mathf.Lerp(0f, -110f, Mathf.SmoothStep(0f, 1f, t)), 0f, 0f);
                yield return null;
            }
            Fx.Explodir(transform.position + Vector3.up * 0.8f, new Color(1f, 0.8f, 0.3f), 3f, 0.6f);
            System.Random r = new System.Random((int)(transform.position.x * 100f + transform.position.z * 31f) + Time.frameCount);
            for (int i = 0; i < itens; i++)
            {
                float a = i * 6.28f / Mathf.Max(1, itens) + 0.6f;
                Vector3 p = transform.position + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * 1.6f;
                Drops.Soltar(p, jogador.nivel + 1, r, bonusRaridade);
            }
            jogador.GanharXp(25f + jogador.nivel * 4f);
            GameManager.Instance.Evento("bau", "");
            GameManager.Instance.Mostrar("O baú revela " + itens + " itens! Pegue com [C].", 3f);
        }
    }
}
