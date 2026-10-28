using UnityEngine;

namespace MongeArcano
{
    /// <summary>Item largado por inimigos e baús. Pegue com [C] (se houver espaço no inventário de 20).</summary>
    public class ItemNoChao : Interagivel
    {
        public Item item;
        Vector3 basePos;
        float fase;
        bool pego;

        public override string Rotulo { get { return item != null ? "Pegar: " + item.nome + " (" + item.raridade + ")" : "Pegar item"; } }
        public override bool Disponivel { get { return enabled && !pego; } }

        void Start() { basePos = transform.position; fase = Random.value * 6.28f; }

        void Update()
        {
            transform.position = basePos + Vector3.up * Mathf.Sin(Time.time * 2.2f + fase) * 0.1f;
            transform.Rotate(0f, 80f * Time.deltaTime, 0f, Space.World);
        }

        public override void Interagir(PlayerController jogador)
        {
            GameManager gm = GameManager.Instance;
            if (item == null) return;
            if (!jogador.AdicionarItem(item)) { gm.Mostrar("Inventário cheio! Descarte algo com [I].", 2.5f); return; }
            pego = true;
            gm.Mostrar("Obtido: " + item.nome + " (" + item.raridade + ")", 2.5f);
            Som.Tocar(TipoSom.Coleta, transform.position, 0.8f);
            Destroy(gameObject);
        }
    }
}
