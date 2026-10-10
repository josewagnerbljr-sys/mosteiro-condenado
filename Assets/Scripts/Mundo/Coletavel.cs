using UnityEngine;

namespace MongeArcano
{
    public enum TipoColetavel { Essencia, Erva, Chave, Nota }

    /// <summary>Itens do mundo. Essência e Erva são coletadas ao encostar; chaves, sigilos e arquivos com [E].</summary>
    public class Coletavel : Interagivel
    {
        public TipoColetavel tipo;
        public string chaveId = "";
        public string titulo = "";
        [TextArea] public string texto = "";
        public float valor = 5f;
        public bool automatico = true;

        Vector3 basePos;
        float fase;

        public override string Rotulo
        {
            get
            {
                switch (tipo)
                {
                    case TipoColetavel.Chave: return "Pegar: " + GameManager.NomeChave(chaveId);
                    case TipoColetavel.Nota: return "Ler: " + titulo;
                    case TipoColetavel.Erva: return "Pegar Erva Sagrada";
                    default: return "Pegar essência arcana";
                }
            }
        }

        void Start()
        {
            basePos = transform.position;
            fase = Random.value * 6.28f;
        }

        void Update()
        {
            transform.position = basePos + Vector3.up * Mathf.Sin(Time.time * 2f + fase) * 0.08f;
            transform.Rotate(0f, 70f * Time.deltaTime, 0f, Space.World);
        }

        public override void Interagir(PlayerController p)
        {
            GameManager gm = GameManager.Instance;
            switch (tipo)
            {
                case TipoColetavel.Essencia:
                    if (p.mana >= p.manaMax) return;
                    p.AddMana(valor);
                    gm.Mostrar("+ Essência arcana", 1.5f);
                    Som.Tocar(TipoSom.Coleta, transform.position, 0.8f);
                    Destroy(gameObject);
                    break;

                case TipoColetavel.Erva:
                    if (p.ervas >= 9) return;
                    p.AddErva();
                    gm.Mostrar("+ Erva Sagrada  (use com [H])", 2f);
                    Som.Tocar(TipoSom.Coleta, transform.position, 0.8f);
                    Destroy(gameObject);
                    break;

                case TipoColetavel.Chave:
                    gm.DarChave(chaveId);
                    Som.Tocar(TipoSom.Coleta, transform.position, 1f);
                    Destroy(gameObject);
                    break;

                case TipoColetavel.Nota:
                    Som.Tocar(TipoSom.Coleta, transform.position, 0.6f);
                    gm.AbrirNota(titulo, texto);
                    break;
            }
        }
    }
}
