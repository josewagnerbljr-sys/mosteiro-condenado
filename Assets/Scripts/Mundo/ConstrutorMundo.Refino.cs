using System.Collections.Generic;
using UnityEngine;

namespace MongeArcano
{
    /// <summary>
    /// Refinamento do cenário com peças modulares (Quaternius, KayKit, CC0): árvores mortas, vegetação, rochas, arcos, cercas e adereços de interior.
    /// Todas as peças são GLB leves e escurecidas para o clima do jogo (veja CREDITOS.md).
    /// </summary>
    public static partial class ConstrutorMundo
    {
        static GameObject Peca(string nome, Transform pai, Vector3 pos, float yaw, float escala, Colisor col = Colisor.Nenhum)
        {
            GameObject g = Modelos.Instanciar(nome, pai, 0f, yaw, null, col);
            if (g == null) return null;
            g.transform.position = pos;
            g.transform.localScale = Vector3.one * escala;
            return g;
        }

        static bool Livre(List<Vector2> usados, float x, float z, float dist)
        {
            for (int i = 0; i < usados.Count; i++)
                if ((usados[i].x - x) * (usados[i].x - x) + (usados[i].y - z) * (usados[i].y - z) < dist * dist) return false;
            return true;
        }

        static void Espalhar(Transform pai, string[] nomes, int quantidade, float escMin, float escMax, float xMin, float zMin, float zMax, float distMin, float folga, Colisor col, List<Vector2> usados)
        {
            int criados = 0, tentativas = 0;
            while (criados < quantidade && tentativas < quantidade * 25)
            {
                tentativas++;
                float x = R(-33f, 33f), z = R(zMin, zMax);
                if (Mathf.Abs(x) < xMin || !Livre(usados, x, z, distMin)) continue;
                string n = nomes[rnd.Next(nomes.Length)];
                Peca(n, pai, new Vector3(x, 0f, z), R(0f, 360f), R(escMin, escMax), col);
                usados.Add(new Vector2(x, z));
                criados++;
            }
        }

        static void Refino()
        {
            Transform r = Filho(tProps, "Refino");
            List<Vector2> usados = new List<Vector2>();

            // árvores: mortas, retorcidas e nuas, longe do caminho
            Espalhar(r, new[] { "nat_arvmorta_1", "nat_arvmorta_2", "nat_arvmorta_3", "nat_arvmorta_4", "nat_arvmorta_5" }, 16, 0.8f, 1.15f, 6f, -83f, -14f, 6.5f, 0f, Colisor.Tronco, usados);
            Espalhar(r, new[] { "nat_arvtorta_1", "nat_arvtorta_2", "nat_arvtorta_3" }, 7, 0.35f, 0.5f, 8f, -83f, -16f, 8f, 0f, Colisor.Tronco, usados);
            Espalhar(r, new[] { "kk_arvnua_1", "kk_arvnua_2" }, 12, 1.5f, 2.4f, 5.5f, -83f, -14f, 4.5f, 0f, Colisor.Tronco, usados);

            // sub-bosque morto, cogumelos e rochas
            List<Vector2> baixo = new List<Vector2>();
            Espalhar(r, new[] { "nat_capim_alto", "nat_capim_baixo" }, 110, 0.8f, 1.4f, 2.6f, -83f, -13f, 1.6f, 0f, Colisor.Nenhum, baixo);
            Espalhar(r, new[] { "nat_samambaia" }, 24, 0.9f, 1.5f, 3f, -83f, -14f, 3f, 0f, Colisor.Nenhum, baixo);
            Espalhar(r, new[] { "nat_arbusto" }, 14, 0.8f, 1.3f, 4f, -83f, -14f, 4f, 0f, Colisor.Nenhum, baixo);
            Espalhar(r, new[] { "nat_cogumelo_1", "nat_cogumelo_2" }, 18, 1f, 1.7f, 3f, -83f, -14f, 2f, 0f, Colisor.Nenhum, baixo);
            Espalhar(r, new[] { "nat_rocha_1", "nat_rocha_2", "nat_rocha_3" }, 16, 0.45f, 1.0f, 5f, -83f, -14f, 4f, 0f, Colisor.Caixa, baixo);

            // pedras de trilha ao longo do caminho
            for (float z = -81f; z < -13f; z += 2.4f)
            {
                Peca("nat_pedra_trilha", r, new Vector3(R(-1.7f, -0.6f), 0.02f, z), R(0f, 360f), R(0.7f, 1.0f));
                Peca("nat_pedra_trilha", r, new Vector3(R(0.6f, 1.7f), 0.02f, z + 1.1f), R(0f, 360f), R(0.7f, 1.0f));
            }

            // arcos de pedra marcando o caminho
            for (int i = 0; i < 3; i++) Peca("med_arco", r, new Vector3(0f, 0f, -70f + i * 20f), 0f, 2.4f);

            // cercas de ferro ladeando o portão
            for (int i = 0; i < 6; i++)
            {
                Peca("med_cerca_ornada", r, new Vector3(-5f - i * 2f, 0f, -12.4f), 0f, 1f, Colisor.Caixa);
                Peca("med_cerca_ornada", r, new Vector3(5f + i * 2f, 0f, -12.4f), 0f, 1f, Colisor.Caixa);
            }

            // velas e castiçais entre os túmulos
            for (int i = 0; i < 14; i++)
            {
                float lado = rnd.NextDouble() < 0.5 ? -1f : 1f;
                Vector3 p = new Vector3(lado * R(3.2f, 22f), 0f, R(-78f, -16f));
                Peca(i % 3 == 0 ? "prop_castical_pe" : "prop_vela_1", r, p, R(0f, 360f), i % 3 == 0 ? 1.6f : 2.2f);
                ProceduralModels.Luz(r, "LuzVela", p + new Vector3(0f, 0.9f, 0f), new Color(1f, 0.7f, 0.35f), 0.9f, 5f, true);
            }
            Peca("med_carroca", r, new Vector3(-22f, 0f, -24f), 25f, 1.1f, Colisor.Caixa);
            for (int i = 0; i < 4; i++) Peca(i % 2 == 0 ? "prop_barril" : "prop_caixote", r, new Vector3(-20f + i * 1.1f, 0f, -20.5f), R(0f, 360f), 1.3f, Colisor.Caixa);

            // puglins rondando o cemitério
            float[,] pg = { { -12f, -66f }, { 14f, -60f }, { -14f, -48f }, { 12f, -40f }, { -10f, -28f }, { 11f, -22f } };
            for (int i = 0; i < pg.GetLength(0); i++) Mob(TipoInimigo.Puglin, pg[i, 0], 0.05f, pg[i, 1], R(0f, 360f));

            // cinzas e brasas caindo sobre o cemitério
            FxMagia.Cinzas(r, new Vector3(0f, 8f, -40f), new Vector3(32f, 10f, 32f));

            // ------------------------------------------------ interiores
            // poeira flutuando em cada sala
            Vector3[] salas = { new Vector3(0f, 2.2f, 0f), new Vector3(-16f, 2.2f, 0f), new Vector3(16f, 2.2f, 0f), new Vector3(0f, 2.2f, 16f), new Vector3(0f, 2.2f, 34f), new Vector3(-16f, 2.2f, -16f) };
            for (int i = 0; i < salas.Length; i++) FxMagia.Poeira(r, salas[i], new Vector3(11f, 4f, 11f), 120);

            // armaduras decorativas que ganham vida ao se aproximar
            Vector3[] arm = { new Vector3(11.4f, 0.05f, 5f), new Vector3(20.6f, 0.05f, -5f), new Vector3(-6.4f, 0.05f, 30f), new Vector3(6.4f, 0.05f, 30f) };
            for (int i = 0; i < arm.Length; i++)
            {
                Inimigo a = ProceduralModels.CriarInimigo(TipoInimigo.Armadura, arm[i], i % 2 == 0 ? 180f : 0f);
                a.deteccao = 4.5f;
                a.transform.SetParent(tIni, true);
            }

            // Salão
            Peca("prop_lustre", r, new Vector3(0f, 2.6f, 0f), 0f, 1.7f);
            Peca("prop_estandarte_1", r, new Vector3(-3.2f, 1.2f, 5.7f), 180f, 1.5f);
            Peca("prop_estandarte_2", r, new Vector3(3.2f, 1.2f, 5.7f), 180f, 1.5f);
            Peca("prop_barril", r, new Vector3(-5.2f, 0f, -5.2f), 0f, 1.3f, Colisor.Caixa);
            Peca("prop_barril", r, new Vector3(-4.3f, 0f, -5.4f), 40f, 1.2f, Colisor.Caixa);
            Peca("prop_caixote", r, new Vector3(5.1f, 0f, -5.2f), 15f, 1.4f, Colisor.Caixa);

            // Capela (-16,0,0)
            Vector3[] cast = { new Vector3(-21f, 0f, -4.5f), new Vector3(-21f, 0f, 4.5f), new Vector3(-11f, 0f, -4.5f), new Vector3(-11f, 0f, 4.5f) };
            for (int i = 0; i < cast.Length; i++)
            {
                Peca("prop_castical_pe", r, cast[i], 0f, 2.4f);
                ProceduralModels.Luz(r, "LuzCastical", cast[i] + new Vector3(0f, 1.9f, 0f), new Color(1f, 0.75f, 0.4f), 1.0f, 6f, true);
            }
            Peca("prop_estandarte_1", r, new Vector3(-21.8f, 1.2f, 0f), 90f, 1.6f);
            Peca("prop_calice", r, new Vector3(-16f, 1.05f, 4.8f), 0f, 1.6f);

            // Biblioteca (16,0,0)
            for (int i = 0; i < 6; i++)
            {
                Peca("prop_estante", r, new Vector3(11.8f + i * 1.65f, 0f, 5.55f), 180f, 1.15f, Colisor.Caixa);
                Peca("prop_estante", r, new Vector3(11.8f + i * 1.65f, 0f, -5.55f), 0f, 1.15f, Colisor.Caixa);
            }
            Peca("prop_mesa", r, new Vector3(13.5f, 0f, -2.2f), 0f, 1f, Colisor.Caixa);
            Peca("prop_cadeira", r, new Vector3(13.0f, 0f, -3.4f), 0f, 1.2f);
            Peca("prop_cadeira", r, new Vector3(14.2f, 0f, -0.9f), 180f, 1.2f);
            Peca("prop_livros_1", r, new Vector3(13.2f, 0.82f, -2.2f), 20f, 1.5f);
            Peca("prop_pergaminho", r, new Vector3(14.3f, 0.82f, -2.0f), 70f, 1.5f);
            Peca("prop_atril", r, new Vector3(19.5f, 0f, 3.6f), 200f, 1.3f);

            // Catacumbas (0,0,16)
            for (int i = 0; i < 3; i++) Peca("prop_caixote", r, new Vector3(-5f + i * 1.2f, 0f, 21.3f), R(0f, 40f), 1.3f, Colisor.Caixa);
            Peca("prop_barril", r, new Vector3(5f, 0f, 21f), 0f, 1.3f, Colisor.Caixa);
            Peca("prop_barril", r, new Vector3(4.2f, 0f, 21.4f), 30f, 1.2f, Colisor.Caixa);
            Peca("prop_caldeirao", r, new Vector3(-4.8f, 0f, 11f), 0f, 1.4f, Colisor.Caixa);
            Peca("prop_corrente", r, new Vector3(4.6f, 0f, 11.6f), 80f, 1.5f);
            Peca("prop_gaiola", r, new Vector3(-5.2f, 0f, 16.5f), 90f, 1.6f, Colisor.Caixa);
            Peca("prop_moedas", r, new Vector3(5.3f, 0f, 17f), 0f, 1.4f);

            // Santuário (0,0,34)
            for (int i = 0; i < 4; i++)
            {
                Vector3 p = new Vector3(i < 2 ? -6.6f : 6.6f, 0f, i % 2 == 0 ? 28.5f : 33f);
                Peca("prop_castical_pe", r, p, 0f, 2.6f);
                ProceduralModels.Luz(r, "LuzCastical", p + new Vector3(0f, 2.1f, 0f), new Color(1f, 0.5f, 0.3f), 1.2f, 7f, true);
            }
            Peca("prop_estandarte_2", r, new Vector3(-7.7f, 1.4f, 34f), 90f, 1.7f);
            Peca("prop_estandarte_2", r, new Vector3(7.7f, 1.4f, 34f), 270f, 1.7f);

            // Câmara secreta (-16,0,-16): tesouro
            Peca("prop_moedas", r, new Vector3(-22f, 0f, -17.4f), 20f, 1.6f);
            Peca("prop_moedas", r, new Vector3(-10f, 0f, -17.4f), 200f, 1.6f);
        }
    }
}
