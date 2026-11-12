using UnityEngine;

namespace MongeArcano
{
    /// <summary>
    /// Guarda na cena as referências aos clipes de animação da UAL (ual_animacoes.glb). Assim o jogo não depende do modo de importação do glTFast
    /// (Animation legada ou Animator) para achar as animações em tempo de execução.
    /// </summary>
    public class BibliotecaUal : MonoBehaviour
    {
        public AnimationClip[] clipes;
        public static BibliotecaUal Instancia;

        /// <summary>Preenchido só pelo editor ao gerar a cena.</summary>
        public static System.Func<AnimationClip[]> Provedor;

        void Awake() { Instancia = this; }

        bool painel;
        void Update() { if (Input.GetKeyDown(KeyCode.F3)) painel = !painel; }

        /// <summary>Painel de depuração (F3): mostra o estado das animações UAL dos personagens mais próximos.</summary>
        void OnGUI()
        {
            if (!painel) return;
            GameManager gm = GameManager.Instance;
            if (gm == null || gm.jogador == null) return;
            Vector3 p = gm.jogador.transform.position;
            System.Collections.Generic.List<UalAnimador> lista = new System.Collections.Generic.List<UalAnimador>(UalAnimador.Todos);
            lista.Sort((a, b) => (a.transform.position - p).sqrMagnitude.CompareTo((b.transform.position - p).sqrMagnitude));
            float y = 10f;
            GUI.color = Color.yellow;
            GUI.Label(new Rect(10f, y, 900f, 22f), "PAINEL UAL (F3)  clipes na biblioteca: " + (clipes != null ? clipes.Length : 0) + "  personagens: " + lista.Count);
            y += 22f;
            GUI.color = Color.white;
            for (int i = 0; i < lista.Count && i < 10; i++)
            {
                UalAnimador u = lista[i];
                float d = (u.transform.position - p).magnitude;
                GUI.Label(new Rect(10f, y, 1000f, 20f), u.name + " [" + u.perfil + "] " + d.ToString("0") + "m  pronto=" + u.Pronto + "  clipe=" + u.ClipeAtual + "  vel=" + u.VelAtual.ToString("0.0") + "  " + u.Diagnostico);
                y += 20f;
            }
        }

        public static void Garantir()
        {
            if (Provedor == null) return;
            BibliotecaUal b = Object.FindAnyObjectByType<BibliotecaUal>();
            if (b == null)
            {
                GameObject go = new GameObject("BibliotecaUal");
                b = go.AddComponent<BibliotecaUal>();
            }
            if (b.clipes == null || b.clipes.Length == 0) b.clipes = Provedor();
        }
    }
}
