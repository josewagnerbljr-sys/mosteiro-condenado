using UnityEngine;

namespace MongeArcano
{
    /// <summary>Personagem não jogável com quem se conversa (tecla C). A câmera se aproxima e a fala aparece em tela.</summary>
    public class NpcFalante : Interagivel
    {
        public string nome = "Mensageiro";
        [TextArea] public string fala = "";
        public Transform alvoCamera;

        public override string Rotulo { get { return "Falar com " + nome; } }

        public override void Interagir(PlayerController jogador)
        {
            GameManager gm = GameManager.Instance;
            gm.AbrirDialogo(nome, fala, alvoCamera != null ? alvoCamera : transform);
            gm.Evento("npc", nome);
        }
    }
}
