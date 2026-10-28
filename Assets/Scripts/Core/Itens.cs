using UnityEngine;

namespace MongeArcano
{
    public enum TipoItem { Arma, Armadura, Amuleto, Pocao, Pergaminho }
    public enum Raridade { Comum, Incomum, Raro, Epico, Lendario }

    /// <summary>Item do inventário. Ocupação: arma 6 espaços, armadura 4, poção/elixir/amuleto 1, pergaminho 2 (capacidade total 20).</summary>
    [System.Serializable]
    public class Item
    {
        public string nome = "Item";
        public TipoItem tipo;
        public Raridade raridade;
        public int nivel = 1;
        public float bonusDano;
        public int bInt, bWis, bStr, bDex, bEsp;
        public float vida, mana;
        public int magia;
        public string descricao = "";

        public int Slots
        {
            get
            {
                switch (tipo)
                {
                    case TipoItem.Arma: return 6;
                    case TipoItem.Armadura: return 4;
                    case TipoItem.Pergaminho: return 2;
                    default: return 1;
                }
            }
        }

        public Color Cor
        {
            get
            {
                switch (raridade)
                {
                    case Raridade.Incomum: return new Color(0.4f, 0.9f, 0.4f);
                    case Raridade.Raro: return new Color(0.35f, 0.6f, 1f);
                    case Raridade.Epico: return new Color(0.75f, 0.4f, 1f);
                    case Raridade.Lendario: return new Color(1f, 0.6f, 0.15f);
                    default: return new Color(0.8f, 0.8f, 0.8f);
                }
            }
        }

        public string Resumo()
        {
            var sb = new System.Text.StringBuilder();
            sb.Append(nome + "  (" + raridade + ", nível " + nivel + ")\n");
            if (bonusDano > 0f) sb.Append("Dano das magias +" + Mathf.RoundToInt(bonusDano * 100f) + "%\n");
            if (bInt > 0) sb.Append("INT +" + bInt + "  ");
            if (bWis > 0) sb.Append("WIS +" + bWis + "  ");
            if (bStr > 0) sb.Append("STR +" + bStr + "  ");
            if (bDex > 0) sb.Append("DEX +" + bDex + "  ");
            if (bEsp > 0) sb.Append("ESP +" + bEsp + "  ");
            if (vida > 0f) sb.Append(tipo == TipoItem.Pocao ? "\nRestaura " + Mathf.RoundToInt(vida) + " de vida" : "\nVida máxima +" + Mathf.RoundToInt(vida));
            if (mana > 0f) sb.Append("\nRestaura " + Mathf.RoundToInt(mana) + " de essência");
            if (tipo == TipoItem.Pergaminho) sb.Append("\nEvolui: " + PlayerController.NomesMagias[Mathf.Clamp(magia, 0, 4)] + " (+1 nível)");
            sb.Append("\nOcupa " + Slots + (Slots == 1 ? " espaço" : " espaços"));
            return sb.ToString();
        }
    }

    public static class Drops
    {
        static readonly string[] armas = { "Cajado de Carvalho Negro", "Cajado do Penitente", "Bastão de Osso", "Cajado Rúnico", "Foice do Ceifador", "Báculo das Cinzas", "Cajado da Lua Morta" };
        static readonly string[] armaduras = { "Hábito Remendado", "Manto do Peregrino", "Túnica de Cinzas", "Manto das Sombras", "Vestes do Abade", "Mortalha Rúnica" };
        static readonly string[] amuletos = { "Amuleto de Osso", "Selo do Mosteiro", "Olho do Corvo", "Relicário Quebrado", "Terço Maldito", "Talismã Rubro" };

        public static Item Gerar(int nivel, System.Random r, float bonusRaridade)
        {
            float sorte = (float)r.NextDouble() + bonusRaridade;
            Raridade rar = sorte > 1.02f ? Raridade.Lendario : sorte > 0.95f ? Raridade.Epico : sorte > 0.82f ? Raridade.Raro : sorte > 0.52f ? Raridade.Incomum : Raridade.Comum;
            double t = r.NextDouble();
            Item it = new Item { raridade = rar, nivel = Mathf.Max(1, nivel) };
            float m = 1f + (int)rar * 0.55f;
            int pts = Mathf.Max(1, Mathf.RoundToInt((1f + nivel * 0.18f) * m));

            if (t < 0.30)
            {
                bool vida = r.NextDouble() < 0.55;
                it.tipo = TipoItem.Pocao;
                it.nome = (vida ? "Elixir de Vida" : "Poção de Essência") + (rar >= Raridade.Raro ? " Superior" : "");
                if (vida) it.vida = (35f + nivel * 4f) * (1f + (int)rar * 0.3f); else it.mana = (10f + nivel * 0.8f) * (1f + (int)rar * 0.3f);
            }
            else if (t < 0.45)
            {
                it.tipo = TipoItem.Pergaminho;
                it.magia = r.Next(5);
                it.nome = "Pergaminho: " + PlayerController.NomesMagias[it.magia];
            }
            else if (t < 0.66)
            {
                it.tipo = TipoItem.Arma;
                it.nome = armas[r.Next(armas.Length)];
                it.bonusDano = 0.08f * m + nivel * 0.004f;
                it.bInt = pts; if (r.NextDouble() < 0.5) it.bWis = pts / 2; if (rar >= Raridade.Raro) it.bEsp = pts / 2;
            }
            else if (t < 0.86)
            {
                it.tipo = TipoItem.Armadura;
                it.nome = armaduras[r.Next(armaduras.Length)];
                it.vida = 12f * m + nivel * 2f;
                it.bStr = pts / 2; it.bDex = pts / 2;
            }
            else
            {
                it.tipo = TipoItem.Amuleto;
                it.nome = amuletos[r.Next(amuletos.Length)];
                int a = r.Next(5);
                if (a == 0) it.bInt = pts; else if (a == 1) it.bWis = pts; else if (a == 2) it.bStr = pts; else if (a == 3) it.bDex = pts; else it.bEsp = pts;
            }
            return it;
        }

        public static void Soltar(Vector3 pos, int nivel, System.Random r, float bonusRaridade)
        {
            Item it = Gerar(nivel, r, bonusRaridade);
            GameObject go = new GameObject("Item_" + it.nome);
            go.transform.position = new Vector3(pos.x, pos.y + 0.6f, pos.z);
            SphereCollider sc = go.AddComponent<SphereCollider>();
            sc.isTrigger = true;
            sc.radius = 0.9f;
            ItemNoChao ic = go.AddComponent<ItemNoChao>();
            ic.item = it;
            GameObject g = ProceduralModels.Prim(it.tipo == TipoItem.Pocao ? PrimitiveType.Sphere : PrimitiveType.Cube, go.transform, "Gema", Vector3.zero, Vector3.one * (it.tipo == TipoItem.Pocao ? 0.3f : 0.34f), "EmissaoBranca", false, new Vector3(35f, 25f, 0f));
            Material mat = new Material(ProceduralModels.Mat("EmissaoBranca"));
            mat.color = it.Cor;
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", it.Cor * 1.6f);
            g.GetComponent<Renderer>().sharedMaterial = mat;
            ProceduralModels.Luz(go.transform, "Luz", Vector3.up * 0.2f, it.Cor, 1.2f, 4f, true);
            Fx.Halo(go.transform, it.Cor, 1.4f);
        }
    }
}
