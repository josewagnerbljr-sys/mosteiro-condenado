using UnityEngine;

namespace MongeArcano
{
    public enum TipoColetavel { Essencia, Erva, Chave, Nota, Frasco }

    /// <summary>Essência, ervas e frascos são coletados ao encostar; chaves e sigilos aproximam a câmera ao pegar; documentos abrem em tela de leitura.</summary>
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
        bool coletado;

        public override bool Disponivel { get { return enabled && !coletado; } }

        public override string Rotulo
        {
            get
            {
                switch (tipo)
                {
                    case TipoColetavel.Chave: return "Pegar: " + GameManager.NomeChave(chaveId);
                    case TipoColetavel.Nota: return "Ler: " + titulo;
                    case TipoColetavel.Erva: return "Pegar Erva Sagrada";
                    case TipoColetavel.Frasco: return "Pegar Frasco de Essência";
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
                    coletado = true;
                    Destroy(gameObject);
                    break;

                case TipoColetavel.Erva:
                    if (p.ervas >= 9) return;
                    p.AddErva();
                    gm.Mostrar("+ Erva Sagrada  (tecla [1] ou [V])", 2f);
                    Som.Tocar(TipoSom.Coleta, transform.position, 0.8f);
                    coletado = true;
                    Destroy(gameObject);
                    break;

                case TipoColetavel.Frasco:
                    if (p.frascos >= 9) return;
                    p.AddFrasco();
                    gm.Mostrar("+ Frasco de Essência  (tecla [2])", 2f);
                    Som.Tocar(TipoSom.Coleta, transform.position, 0.8f);
                    coletado = true;
                    Destroy(gameObject);
                    break;

                case TipoColetavel.Chave:
                    {
                        coletado = true;
                        string id = chaveId;
                        GameObject go = gameObject;
                        Som.Tocar(TipoSom.Coleta, transform.position, 1f);
                        gm.CenaItem(transform, GameManager.NomeChave(id), "Item chave obtido", () =>
                        {
                            gm.DarChave(id);
                            Destroy(go);
                        });
                        break;
                    }

                case TipoColetavel.Nota:
                    Som.Tocar(TipoSom.Coleta, transform.position, 0.6f);
                    gm.AbrirNota(titulo, texto, transform);
                    break;
            }
        }
    }
}
