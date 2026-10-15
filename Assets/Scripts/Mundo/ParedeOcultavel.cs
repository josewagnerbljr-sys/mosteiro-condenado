using UnityEngine;

namespace MongeArcano
{
    /// <summary>Parede que some da vista (só o visual, a colisão continua) quando fica entre a câmera e o jogador.</summary>
    public class ParedeOcultavel : MonoBehaviour
    {
        Renderer r;

        void Awake() { r = GetComponent<Renderer>(); }

        public void Ocultar(bool ocultar)
        {
            if (r == null) r = GetComponent<Renderer>();
            if (r != null) r.enabled = !ocultar;
        }
    }
}
