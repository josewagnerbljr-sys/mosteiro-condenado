using System.Collections.Generic;
using UnityEngine;

namespace MongeArcano
{
    /// <summary>Portal que leva o jogador a outro ponto. O portal de entrada da Masmorra do Eco inicia a arena de ondas.</summary>
    public class PortalMasmorra : Interagivel
    {
        public string rotulo = "Atravessar o portal";
        public Vector3 destino;
        public ArenaOndas arena;

        public override string Rotulo { get { return rotulo; } }

        public override void Interagir(PlayerController jogador)
        {
            Fx.Explodir(jogador.transform.position + Vector3.up, new Color(0.7f, 0.35f, 1f), 5f, 0.7f);
            Som.Tocar(TipoSom.Super, transform.position, 0.5f);
            jogador.Teleportar(destino);
            if (arena != null) arena.Iniciar();
        }
    }

    /// <summary>Arena de ondas de monstros em 5 rodadas. Ao final: dois baús e o portal de volta.</summary>
    public class ArenaOndas : MonoBehaviour
    {
        public Vector3 centro;
        public PortalMasmorra portalSaida;
        public Transform pai;

        readonly List<Inimigo> vivos = new List<Inimigo>();
        int onda;
        bool ativa;
        float proxima;

        public void Iniciar()
        {
            if (ativa || onda >= 5) return;
            ativa = true;
            onda = 0;
            proxima = Time.time + 3f;
            GameManager.Instance.MostrarBanner("MASMORRA DO ECO - sobreviva a 5 ondas", new Color(0.8f, 0.5f, 1f), 4f);
        }

        void Update()
        {
            if (!ativa) return;
            for (int i = vivos.Count - 1; i >= 0; i--) if (vivos[i] == null || vivos[i].Morto) vivos.RemoveAt(i);
            if (vivos.Count > 0 || Time.time < proxima) return;
            if (onda >= 5) { Concluir(); return; }
            Lancar(onda++);
            proxima = Time.time + 2f;
        }

        void Criar(TipoInimigo t, int n)
        {
            for (int i = 0; i < n; i++)
            {
                float a = Random.value * 6.28f;
                Vector3 p = centro + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * Random.Range(8f, 12f);
                Inimigo e = ProceduralModels.CriarInimigo(t, new Vector3(p.x, 0.05f, p.z), 0f);
                e.deteccao = 80f;
                e.xp *= 1.4f;
                e.Despertar();
                if (pai != null) e.transform.SetParent(pai, true);
                vivos.Add(e);
                Fx.Explodir(p + Vector3.up, new Color(0.7f, 0.3f, 1f), 3f, 0.5f);
            }
        }

        void Lancar(int o)
        {
            GameManager.Instance.Mostrar("Onda " + (o + 1) + " de 5", 3f);
            switch (o)
            {
                case 0: Criar(TipoInimigo.Esqueleto, 4); break;
                case 1: Criar(TipoInimigo.Esqueleto, 3); Criar(TipoInimigo.Possuido, 2); break;
                case 2: Criar(TipoInimigo.Armadura, 2); Criar(TipoInimigo.Esqueleto, 3); Criar(TipoInimigo.BruxaCinza, 1); break;
                case 3: Criar(TipoInimigo.Infernal, 2); Criar(TipoInimigo.Armadura, 2); Criar(TipoInimigo.BruxaNegra, 1); break;
                default: Criar(TipoInimigo.Monstro, 1); Criar(TipoInimigo.Infernal, 2); Criar(TipoInimigo.BruxaVermelha, 1); break;
            }
        }

        void Concluir()
        {
            ativa = false;
            GameManager gm = GameManager.Instance;
            gm.MostrarBanner("MASMORRA CONCLUÍDA!", new Color(1f, 0.85f, 0.3f), 4f);
            gm.Evento("arena", "");
            gm.jogador.GanharXp(150f + gm.jogador.nivel * 10f);
            ConstrutorMundo.BauDinamico(centro + new Vector3(-1.8f, 0f, 0f), 4, 0.3f);
            ConstrutorMundo.BauDinamico(centro + new Vector3(1.8f, 0f, 0f), 4, 0.3f);
            if (portalSaida != null) portalSaida.gameObject.SetActive(true);
        }
    }
}
