using UnityEngine;

namespace MongeArcano
{
    /// <summary>Projétil físico (Rigidbody). Amigável = Raio Arcano do monge; hostil = bola infernal do chefe.</summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Projetil : MonoBehaviour
    {
        public bool amigavel = true;
        public float dano = 25f;
        public float velocidade = 17f;
        public float empurrao = 5f;
        public float duracao = 3.5f;
        public string efeitoMaterial = "EmissaoAzul";

        Rigidbody rb;
        Vector3 ultimaDir = Vector3.forward;
        bool usado;

        void Start()
        {
            Destroy(gameObject, duracao);
        }

        public void Lancar(Vector3 dir)
        {
            rb = GetComponent<Rigidbody>();
            ultimaDir = dir.normalized;
            rb.velocity = ultimaDir * velocidade;
            transform.forward = ultimaDir;
        }

        void FixedUpdate()
        {
            if (rb != null && rb.velocity.sqrMagnitude > 1f) ultimaDir = rb.velocity.normalized;
        }

        void OnCollisionEnter(Collision c)
        {
            if (usado) return;
            usado = true;

            IDanificavel alvo = c.collider.GetComponentInParent<IDanificavel>();
            if (alvo != null)
            {
                bool ehJogador = alvo is PlayerController;
                if (ehJogador != amigavel) alvo.ReceberDano(dano, transform.position - ultimaDir * 0.5f, empurrao);
            }

            Rigidbody outro = c.collider.attachedRigidbody;
            if (outro != null && !outro.isKinematic && c.collider.GetComponentInParent<Inimigo>() == null)
                outro.AddForce(ultimaDir * empurrao, ForceMode.Impulse);

            Efeitos.Explosao(transform.position, efeitoMaterial, amigavel ? 1.6f : 2.6f, 0.25f);
            Som.Tocar(TipoSom.Impacto, transform.position, 0.6f);
            Destroy(gameObject);
        }
    }
}
