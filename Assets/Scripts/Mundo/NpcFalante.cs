using UnityEngine;

namespace MongeArcano
{
    /// <summary>Personagem não jogável com quem se conversa (tecla C). A câmera se aproxima e a fala aparece em tela.</summary>
    public class NpcFalante : Interagivel
    {
        public string nome = "Mensageiro";
        [TextArea] public string fala = "";
        public Transform alvoCamera;
        public Transform corpo;
        public UalAnimador animador;
        public float alcanceOlhar = 9f;

        public override string Rotulo { get { return "Falar com " + nome; } }

        void Update()
        {
            GameManager g = GameManager.Instance;
            if (corpo == null || g == null || g.jogador == null) return;
            Vector3 d = g.jogador.transform.position - corpo.position;
            d.y = 0f;
            if (d.sqrMagnitude > alcanceOlhar * alcanceOlhar || d.sqrMagnitude < 0.01f) return;
            corpo.rotation = Quaternion.Slerp(corpo.rotation, Quaternion.LookRotation(d), 1f - Mathf.Exp(-3f * Time.deltaTime));
        }

        public override void Interagir(PlayerController jogador)
        {
            if (animador != null) animador.Interagir();
            GameManager gm = GameManager.Instance;
            gm.AbrirDialogo(nome, fala, alvoCamera != null ? alvoCamera : transform);
            gm.Evento("npc", nome);
        }
    }
}
