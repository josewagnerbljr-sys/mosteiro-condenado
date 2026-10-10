using UnityEngine;

namespace MongeArcano
{
    /// <summary>Qualquer coisa que pode levar dano (inimigos e jogador).</summary>
    public interface IDanificavel
    {
        void ReceberDano(float dano, Vector3 origem, float empurrao);
    }

    /// <summary>Qualquer objeto com o qual o jogador interage apertando [E].</summary>
    public abstract class Interagivel : MonoBehaviour
    {
        public abstract string Rotulo { get; }
        public virtual bool Disponivel { get { return enabled; } }
        public abstract void Interagir(PlayerController jogador);
    }
}
