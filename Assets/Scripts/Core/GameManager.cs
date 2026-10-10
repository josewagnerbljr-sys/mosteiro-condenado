using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MongeArcano
{
    public enum EstadoJogo { Titulo, Jogando, Pausado, Lendo, FimDeJogo, Vitoria }

    /// <summary>Cérebro do jogo: estados, inventário de chaves, objetivos, checkpoint e fluxo de vitória/derrota.</summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public PlayerController jogador;
        public GameObject portalFinal;
        [HideInInspector] public Inimigo chefe;

        public EstadoJogo Estado { get; private set; }
        public readonly HashSet<string> Chaves = new HashSet<string>();
        public readonly HashSet<string> Marcos = new HashSet<string>();
        public readonly List<string> NotasLidas = new List<string>();
        public int InimigosDerrotados;
        public float TempoDeJogo;
        public string Dica = "";
        public string Mensagem = "";
        public float MensagemAte;
        public float FlashAte;
        public string NotaTitulo = "";
        public string NotaTexto = "";
        public string Objetivo = "";
        public const int TotalNotas = 5;

        Vector3 checkpointPos;
        float checkpointRot;
        float bloqueioAte;
        AudioSource ambiente;

        static readonly Dictionary<string, string> nomes = new Dictionary<string, string>
        {
            { "ferro", "Chave de Ferro" },
            { "osso", "Chave de Osso" },
            { "sigilo_lua", "Sigilo da Lua" },
            { "sigilo_fogo", "Sigilo do Fogo" },
            { "sigilo_terra", "Sigilo da Terra" }
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
            AtualizarObjetivo();
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

        static bool Enter()
        {
            return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
        }

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
                    if (Input.GetKeyDown(KeyCode.E) || Enter() || Input.GetKeyDown(KeyCode.Escape)) FecharNota();
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
            if (ambiente == null)
            {
                ambiente = gameObject.AddComponent<AudioSource>();
                ambiente.clip = Som.Ambiente();
                ambiente.loop = true;
                ambiente.volume = 0.4f;
                ambiente.spatialBlend = 0f;
                ambiente.Play();
            }
            Mostrar("Mosteiro de Névoa Negra. Algo respira nas sombras...", 4f);
        }

        // ---------------------------------------------------------------- mensagens / notas
        public void Mostrar(string texto, float segundos = 3f)
        {
            Mensagem = texto;
            MensagemAte = Time.unscaledTime + segundos;
        }

        public void AbrirNota(string titulo, string texto)
        {
            NotaTitulo = titulo;
            NotaTexto = texto;
            if (!NotasLidas.Contains(titulo)) NotasLidas.Add(titulo);
            Estado = EstadoJogo.Lendo;
            Time.timeScale = 0f;
            bloqueioAte = Time.unscaledTime + 0.3f;
        }

        void FecharNota()
        {
            Estado = EstadoJogo.Jogando;
            Time.timeScale = 1f;
            bloqueioAte = Time.unscaledTime + 0.3f;
        }

        public void PiscarDano() { FlashAte = Time.unscaledTime + 0.4f; }

        public void Tremer(float forca)
        {
            if (CameraDirector.Instance != null) CameraDirector.Instance.Tremer(forca);
        }

        // ---------------------------------------------------------------- inventário / progresso
        public bool TemChave(string id) { return Chaves.Contains(id); }

        public void DarChave(string id)
        {
            Chaves.Add(id);
            Mostrar("Obtido: " + NomeChave(id), 3f);
            AtualizarObjetivo();
        }

        public void UsarChave(string id)
        {
            Chaves.Remove(id);
        }

        public void Marco(string id)
        {
            Marcos.Add(id);
            AtualizarObjetivo();
        }

        public void AtualizarObjetivo()
        {
            bool tresSigilos = Chaves.Contains("sigilo_lua") && Chaves.Contains("sigilo_fogo") && Chaves.Contains("sigilo_terra");
            if (Marcos.Contains("chefe")) Objetivo = "Atravesse o portal de luz no Santuário.";
            else if (Marcos.Contains("porta_santuario")) Objetivo = "Destrua o Arconte no Santuário.";
            else if (tresSigilos) Objetivo = "Leve os 3 sigilos ao portão do Santuário (norte das Catacumbas).";
            else if (Marcos.Contains("porta_catacumbas")) Objetivo = "Recupere o Sigilo da Terra e reúna os 3 sigilos (Lua, Fogo, Terra).";
            else if (Marcos.Contains("porta_biblioteca")) Objetivo = "Ache a Chave de Osso e o Sigilo do Fogo na Biblioteca.";
            else if (Chaves.Contains("ferro")) Objetivo = "Abra a Biblioteca (leste do Salão) com a Chave de Ferro.";
            else Objetivo = "Encontre a Chave de Ferro na Capela (oeste do Salão).";
        }

        public void DefinirCheckpoint(Vector3 pos, float rotY)
        {
            checkpointPos = pos;
            checkpointRot = rotY;
        }

        // ---------------------------------------------------------------- fim de jogo
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
