using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MongeArcano
{
    public enum EstadoJogo { Titulo, Jogando, Pausado, Lendo, Cinematica, FimDeJogo, Vitoria }

    public struct SlotInfo
    {
        public string icone, nome;
        public int qtd;
        public bool usavel;
    }

    /// <summary>Estados, inventário, missões (por eventos), checkpoint, cenas de foco (câmera de aproximação) e fim de jogo.</summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public PlayerController jogador;
        public GameObject portalFinal;
        [HideInInspector] public Inimigo chefe;

        public EstadoJogo Estado { get; private set; }
        public readonly HashSet<string> Chaves = new HashSet<string>();
        public readonly List<string> ChavesOrdem = new List<string>();
        public readonly HashSet<string> Marcos = new HashSet<string>();
        public readonly List<string> NotasLidas = new List<string>();
        public List<Missao> Missoes;
        public int InimigosDerrotados;
        public float TempoDeJogo;
        public string Dica = "";
        public string Mensagem = "";
        public float MensagemAte;
        public float FlashAte;
        public string Banner = "";
        public float BannerAte;
        public Color BannerCor = Color.white;
        public string TituloCena = "", SubtituloCena = "";
        public string NotaTitulo = "", NotaTexto = "";
        public string Objetivo = "";
        public const int TotalNotas = 5;

        readonly Dictionary<string, int> historico = new Dictionary<string, int>();
        Vector3 checkpointPos;
        float checkpointRot, bloqueioAte;

        static readonly Dictionary<string, string> nomes = new Dictionary<string, string>
        {
            { "ferro", "Chave de Ferro" }, { "osso", "Chave de Osso" },
            { "sigilo_lua", "Sigilo da Lua" }, { "sigilo_fogo", "Sigilo do Fogo" }, { "sigilo_terra", "Sigilo da Terra" }
        };

        public static string NomeChave(string id)
        {
            string n;
            return nomes.TryGetValue(id, out n) ? n : id;
        }

        public bool InteracaoBloqueada { get { return Time.unscaledTime < bloqueioAte; } }

        void Awake()
        {
            Instance = this;
            Estado = EstadoJogo.Titulo;
            Time.timeScale = 0f;
            ZonaCamera.Resetar();
            Missoes = MissoesDoJogo.Criar();
            AtualizarMissoes();
        }

        void Start()
        {
            if (jogador != null)
            {
                checkpointPos = jogador.transform.position;
                checkpointRot = jogador.transform.eulerAngles.y;
                ZonaCamera.AtivarPara(jogador.transform.position);
            }
        }

        static bool Enter() { return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter); }

        void Update()
        {
            switch (Estado)
            {
                case EstadoJogo.Titulo:
                    if (Enter()) Iniciar();
                    break;
                case EstadoJogo.Jogando:
                    TempoDeJogo += Time.deltaTime;
                    if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Tab))
                    {
                        Estado = EstadoJogo.Pausado;
                        Time.timeScale = 0f;
                    }
                    break;
                case EstadoJogo.Pausado:
                    if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Tab) || Enter())
                    {
                        Estado = EstadoJogo.Jogando;
                        Time.timeScale = 1f;
                    }
                    else if (Input.GetKeyDown(KeyCode.R)) Reiniciar();
                    break;
                case EstadoJogo.Lendo:
                    if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.C) || Enter() || Input.GetKeyDown(KeyCode.Escape)) FecharNota();
                    break;
                case EstadoJogo.FimDeJogo:
                    if (Enter()) ContinuarDoCheckpoint();
                    else if (Input.GetKeyDown(KeyCode.R)) Reiniciar();
                    break;
                case EstadoJogo.Vitoria:
                    if (Input.GetKeyDown(KeyCode.R) || Enter()) Reiniciar();
                    break;
            }
        }

        void Iniciar()
        {
            Estado = EstadoJogo.Jogando;
            Time.timeScale = 1f;
            MusicaDinamica m = GetComponent<MusicaDinamica>();
            if (m != null) m.Iniciar();
            Mostrar("Mosteiro de Névoa Negra. Algo respira nas sombras...", 4f);
        }

        // ------------------------------------------------------------ mensagens
        public void Mostrar(string texto, float segundos = 3f)
        {
            Mensagem = texto;
            MensagemAte = Time.unscaledTime + segundos;
        }

        public void MostrarBanner(string texto, Color cor, float segundos)
        {
            Banner = texto;
            BannerCor = cor;
            BannerAte = Time.unscaledTime + segundos;
        }

        public void PiscarDano() { FlashAte = Time.unscaledTime + 0.4f; }

        public void Tremer(float forca)
        {
            if (CameraDirector.Instance != null) CameraDirector.Instance.Tremer(forca);
        }

        // ------------------------------------------------------------ cenas de aproximação (câmera 2D de item)
        public void CenaItem(Transform t, string titulo, string sub, System.Action aoFim)
        {
            StartCoroutine(CenaFoco(t, titulo, sub, 1.7f, aoFim));
        }

        IEnumerator CenaFoco(Transform t, string titulo, string sub, float seg, System.Action aoFim)
        {
            Estado = EstadoJogo.Cinematica;
            Time.timeScale = 0f;
            TituloCena = titulo;
            SubtituloCena = sub;
            if (CameraDirector.Instance != null) CameraDirector.Instance.IniciarFoco(t);
            float f = 0f;
            while (f < seg)
            {
                f += Time.unscaledDeltaTime;
                if (t != null) t.Rotate(0f, 140f * Time.unscaledDeltaTime, 0f, Space.World);
                yield return null;
            }
            TituloCena = "";
            SubtituloCena = "";
            if (CameraDirector.Instance != null) CameraDirector.Instance.FimFoco();
            if (aoFim != null) aoFim();
            Estado = EstadoJogo.Jogando;
            Time.timeScale = 1f;
            bloqueioAte = Time.unscaledTime + 0.25f;
        }

        public void AbrirNota(string titulo, string texto, Transform t)
        {
            if (!NotasLidas.Contains(titulo))
            {
                NotasLidas.Add(titulo);
                Evento("nota", titulo);
            }
            NotaTitulo = titulo;
            NotaTexto = texto;
            StartCoroutine(NotaFoco(t));
        }

        public void AbrirDialogo(string nome, string texto, Transform t)
        {
            NotaTitulo = nome;
            NotaTexto = texto;
            StartCoroutine(NotaFoco(t));
        }

        IEnumerator NotaFoco(Transform t)
        {
            Estado = EstadoJogo.Cinematica;
            Time.timeScale = 0f;
            if (CameraDirector.Instance != null && t != null) CameraDirector.Instance.IniciarFoco(t);
            float f = 0f;
            while (f < 0.8f) { f += Time.unscaledDeltaTime; yield return null; }
            Estado = EstadoJogo.Lendo;
        }

        void FecharNota()
        {
            if (CameraDirector.Instance != null) CameraDirector.Instance.FimFoco();
            Estado = EstadoJogo.Jogando;
            Time.timeScale = 1f;
            bloqueioAte = Time.unscaledTime + 0.3f;
        }

        // ------------------------------------------------------------ inventário
        public bool TemChave(string id) { return Chaves.Contains(id); }

        public void DarChave(string id)
        {
            if (Chaves.Add(id)) ChavesOrdem.Add(id);
            Mostrar("Obtido: " + NomeChave(id), 3f);
            Evento("item", id);
        }

        public void UsarChave(string id)
        {
            Chaves.Remove(id);
            ChavesOrdem.Remove(id);
        }

        public void MontarSlots(List<SlotInfo> lista)
        {
            lista.Clear();
            lista.Add(new SlotInfo { icone = "erva", nome = "Erva Sagrada", qtd = jogador.ervas, usavel = true });
            lista.Add(new SlotInfo { icone = "frasco", nome = "Frasco de Essência", qtd = jogador.frascos, usavel = true });
            for (int i = 0; i < 4; i++)
            {
                if (i < ChavesOrdem.Count) lista.Add(new SlotInfo { icone = ChavesOrdem[i], nome = NomeChave(ChavesOrdem[i]), qtd = 1, usavel = false });
                else lista.Add(new SlotInfo { icone = "", nome = "", qtd = 0, usavel = false });
            }
        }

        // ------------------------------------------------------------ missões
        public void Marco(string id)
        {
            Marcos.Add(id);
            Evento("marco", id);
        }

        int Historico(string ev, string alvo)
        {
            if (alvo.EndsWith("*"))
            {
                string prefixo = ev + ":" + alvo.Substring(0, alvo.Length - 1);
                int soma = 0;
                foreach (KeyValuePair<string, int> kv in historico) if (kv.Key.StartsWith(prefixo)) soma += kv.Value;
                return soma;
            }
            int v;
            historico.TryGetValue(ev + ":" + alvo, out v);
            return v;
        }

        public void Evento(string ev, string alvo)
        {
            string k = ev + ":" + alvo;
            int v;
            historico.TryGetValue(k, out v);
            historico[k] = v + 1;
            AtualizarMissoes();
        }

        public Missao PrincipalAtual()
        {
            for (int i = 0; i < Missoes.Count; i++) if (Missoes[i].principal && !Missoes[i].concluida) return Missoes[i];
            return null;
        }

        bool Ativa(Missao m) { return !m.principal || m == PrincipalAtual(); }

        void AtualizarMissoes()
        {
            for (int i = 0; i < Missoes.Count; i++)
            {
                Missao m = Missoes[i];
                if (m.concluida) continue;
                for (int j = 0; j < m.objetivos.Count; j++)
                {
                    ObjetivoMissao o = m.objetivos[j];
                    o.atual = Mathf.Min(o.necessario, Historico(o.evento, o.alvo));
                }
            }

            bool mudou = true;
            int guarda = 0;
            while (mudou && guarda++ < 20)
            {
                mudou = false;
                for (int i = 0; i < Missoes.Count; i++)
                {
                    Missao m = Missoes[i];
                    if (m.concluida || !Ativa(m) || !m.Cumprida) continue;
                    Concluir(m);
                    mudou = true;
                }
            }

            Missao p = PrincipalAtual();
            if (p == null) Objetivo = "Todas as missões principais concluídas!";
            else
            {
                string obj = "";
                for (int j = 0; j < p.objetivos.Count; j++) if (!p.objetivos[j].Completo) { obj = p.objetivos[j].descricao; break; }
                Objetivo = p.titulo + ": " + obj;
            }
        }

        void Concluir(Missao m)
        {
            m.concluida = true;
            if (jogador != null) jogador.GanharXp(m.xp);
            MostrarBanner("MISSÃO CUMPRIDA: " + m.titulo + "  (+" + m.xp + " XP)", new Color(1f, 0.85f, 0.3f), 4f);
            Som.Tocar(TipoSom.Missao, jogador != null ? jogador.transform.position : Vector3.zero, 0.9f);
        }

        // ------------------------------------------------------------ checkpoint / fim
        public void DefinirCheckpoint(Vector3 pos, float rotY)
        {
            checkpointPos = pos;
            checkpointRot = rotY;
        }

        public void JogadorMorreu()
        {
            Estado = EstadoJogo.FimDeJogo;
            Time.timeScale = 0f;
        }

        void ContinuarDoCheckpoint()
        {
            jogador.Reviver(checkpointPos, checkpointRot);
            Estado = EstadoJogo.Jogando;
            Time.timeScale = 1f;
            ZonaCamera.Resetar();
            ZonaCamera.AtivarPara(checkpointPos);
            Mostrar("Você retorna ao último altar...", 3f);
        }

        public void ChefeDerrotado()
        {
            Marco("chefe");
            if (portalFinal != null) portalFinal.SetActive(true);
            Mostrar("O Arconte foi banido! Um portal de luz se abriu no Santuário.", 6f);
            Tremer(0.8f);
        }

        public void Vitoria()
        {
            Evento("vitoria", "");
            Estado = EstadoJogo.Vitoria;
            Time.timeScale = 0f;
        }

        public void Reiniciar()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
