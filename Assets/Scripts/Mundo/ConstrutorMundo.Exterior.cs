using UnityEngine;

namespace MongeArcano
{
    /// <summary>Exterior do mosteiro (cemitério, caminho, portão) e decoração do interior com os modelos 3D.</summary>
    public static partial class ConstrutorMundo
    {
        static System.Random rnd;

        static float R(float a, float b) { return a + (float)rnd.NextDouble() * (b - a); }

        static void Poste(float x, float z)
        {
            Transform t = Filho(tProps, "Poste");
            t.position = new Vector3(x, 0f, z);
            ProceduralModels.Prim(PrimitiveType.Cylinder, t, "Haste", new Vector3(0f, 0.8f, 0f), new Vector3(0.12f, 0.8f, 0.12f), "Obsidiana", true);
            GameObject lan = Modelos.Instanciar("lanterna", t, 0.7f, 0f);
            if (lan != null) lan.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            else ProceduralModels.Prim(PrimitiveType.Sphere, t, "Chama", new Vector3(0f, 1.8f, 0f), new Vector3(0.25f, 0.3f, 0.25f), "EmissaoLaranja");
            ProceduralModels.Luz(t, "Luz", new Vector3(0f, 2f, 0f), new Color(1f, 0.55f, 0.2f), 1.7f, 10f, true);
        }

        static void Colocar(string nome, Transform pai, Vector3 pos, float altura, float rotY, Colisor col, string fallbackMat, Vector3 fallbackEsc)
        {
            GameObject g = Modelos.Instanciar(nome, pai, altura, rotY, null, col);
            if (g != null) { g.transform.localPosition = pos; return; }
            GameObject f = ProceduralModels.Prim(PrimitiveType.Cube, pai, nome + "_simples", pos + new Vector3(0f, fallbackEsc.y * 0.5f, 0f), fallbackEsc, fallbackMat, col != Colisor.Nenhum);
            f.transform.localRotation = Quaternion.Euler(0f, rotY, 0f);
        }

        static void Luar(Transform pai)
        {
            GameObject lua = new GameObject("Luar");
            lua.transform.SetParent(pai, false);
            lua.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
            Light ld = lua.AddComponent<Light>();
            ld.type = LightType.Directional;
            ld.color = new Color(0.45f, 0.52f, 0.85f);
            ld.intensity = 0.5f;
            ld.shadows = LightShadows.None;

            GameObject rubra = new GameObject("LuzRubra");
            rubra.transform.SetParent(pai, false);
            rubra.transform.rotation = Quaternion.Euler(18f, 160f, 0f);
            Light lr = rubra.AddComponent<Light>();
            lr.type = LightType.Directional;
            lr.color = new Color(0.9f, 0.28f, 0.16f);
            lr.intensity = 0.28f;
            lr.shadows = LightShadows.None;
        }

        static void Exterior()
        {
            rnd = new System.Random(2026);
            Transform ext = Filho(tEst, "Exterior");
            Luar(ext);

            // terreno, caminho e limites
            ProceduralModels.Prim(PrimitiveType.Cube, ext, "Terreno", new Vector3(0f, -0.25f, -47.75f), new Vector3(74f, 0.5f, 74.5f), "Terra", true);
            ProceduralModels.Prim(PrimitiveType.Cube, ext, "Caminho", new Vector3(0f, 0.02f, -47.75f), new Vector3(4f, 0.05f, 74f), "Caminho");
            Cubo(ext, "Soleira", new Vector3(0f, -0.2f, -10.25f), new Vector3(4f, 0.4f, 0.5f), "Piso");
            Cubo(ext, "MuroOeste", new Vector3(-36.5f, 2.25f, -47.75f), new Vector3(0.5f, 4.5f, 76f), "PedraMusgo");
            Cubo(ext, "MuroLeste", new Vector3(36.5f, 2.25f, -47.75f), new Vector3(0.5f, 4.5f, 76f), "PedraMusgo");
            Cubo(ext, "MuroSul", new Vector3(0f, 2.25f, -85.25f), new Vector3(74f, 4.5f, 0.5f), "PedraMusgo");
            Cubo(ext, "FachadaOeste", new Vector3(-19f, 3f, -10.25f), new Vector3(34f, 6f, 0.5f), "Pedra");
            Cubo(ext, "FachadaLeste", new Vector3(19f, 3f, -10.25f), new Vector3(34f, 6f, 0.5f), "Pedra");

            // câmeras fixas do exterior (usadas no modo de câmera fixa)
            Zona("Cemiterio", new Vector3(0f, 0f, -72f), 26f, new Vector3(-8f, 9f, -13f), 55f);
            Zona("Cemiterio", new Vector3(0f, 0f, -50f), 26f, new Vector3(9f, 9f, -13f), 55f);
            Zona("Portao", new Vector3(0f, 0f, -28f), 26f, new Vector3(-7f, 8.5f, -12f), 55f);

            // castelo ao fundo (silhueta na névoa)
            GameObject castelo = Modelos.Instanciar("monasterio_exterior", ext, 55f, 180f, "Pedra");
            if (castelo != null) castelo.transform.localPosition = new Vector3(0f, -6f, 78f);

            // entrada
            Colocar("portao_gotico", ext, new Vector3(0f, 0f, -10.9f), 3.9f, 180f, Colisor.Nenhum, "Obsidiana", new Vector3(4f, 3.9f, 0.3f));
            Colocar("bandeira", ext, new Vector3(-4.8f, 0f, -11.2f), 4.5f, 180f, Colisor.Nenhum, "Tapete", new Vector3(1f, 4.5f, 0.1f));
            Colocar("bandeira", ext, new Vector3(4.8f, 0f, -11.2f), 4.5f, 180f, Colisor.Nenhum, "Tapete", new Vector3(1f, 4.5f, 0.1f));
            Colocar("carruagem", ext, new Vector3(-10f, 0f, -22f), 2.8f, 150f, Colisor.Caixa, "Madeira", new Vector3(2f, 2.8f, 3f));

            // lampiões no caminho
            float[] zs = { -76f, -64f, -52f, -40f, -28f, -16f };
            for (int i = 0; i < zs.Length; i++) Poste(i % 2 == 0 ? -3.2f : 3.2f, zs[i]);

            // lápides (proporção original entre elas)
            var lapides = Modelos.Variantes("lapides", 1.25f, null, Colisor.Caixa);
            for (int i = 0; i < 40; i++)
            {
                float lado = rnd.NextDouble() < 0.5 ? -1f : 1f;
                Vector3 p = new Vector3(lado * R(4.5f, 33f), 0f, R(-81f, -14f));
                if (Mathf.Abs(p.x) < 8f && p.z < -64f) continue;
                if (lapides.Count > 0) Modelos.Clonar(lapides, rnd, ext, p, 180f + R(-25f, 25f), 0.9f, 1.2f);
                else Cubo(ext, "Lapide", p + new Vector3(0f, 0.5f, 0f), new Vector3(0.7f, 1f, 0.2f), "PedraMusgo");
            }
            Modelos.Liberar(lapides);

            // criptas (cada uma separada)
            var criptas = Modelos.Variantes("criptas", 3.2f, null, Colisor.Caixa);
            float[,] cp = { { -16f, -62f }, { 17f, -56f }, { -22f, -44f }, { 21f, -38f }, { -17f, -30f }, { 24f, -24f } };
            for (int i = 0; i < cp.GetLength(0); i++)
            {
                if (criptas.Count > 0) Modelos.Clonar(criptas, rnd, ext, new Vector3(cp[i, 0], 0f, cp[i, 1]), 180f + R(-20f, 20f), 0.95f, 1.15f);
                else Cubo(ext, "Cripta", new Vector3(cp[i, 0], 1.2f, cp[i, 1]), new Vector3(2.5f, 2.4f, 2.5f), "PedraMusgo");
            }
            Modelos.Liberar(criptas);

            // árvores mortas
            var arvores = Modelos.Variantes("arvores_mortas", 4.8f, null, Colisor.Tronco);
            for (int i = 0; i < 18; i++)
            {
                float lado = rnd.NextDouble() < 0.5 ? -1f : 1f;
                Vector3 p = new Vector3(lado * R(7f, 35f), 0f, R(-83f, -13f));
                if (arvores.Count > 0) Modelos.Clonar(arvores, rnd, ext, p, R(0f, 360f), 0.85f, 1.3f);
                else Cubo(ext, "Tronco", p + new Vector3(0f, 2f, 0f), new Vector3(0.5f, 4f, 0.5f), "MadeiraMorta");
            }
            Modelos.Liberar(arvores);

            // casal esquelético dançando valsa sombria entre os túmulos
            float[,] dp = { { -9f, -58f }, { 10f, -46f }, { -10f, -34f }, { 12f, -26f } };
            for (int i = 0; i < dp.GetLength(0); i++)
            {
                Vector3 p = new Vector3(dp[i, 0], 0f, dp[i, 1]);
                GameObject pivo = new GameObject("Valsa_" + i);
                pivo.transform.SetParent(ext, false);
                pivo.transform.position = p;
                GameObject casal = Modelos.Instanciar("dancarinos_cemiterio", pivo.transform, 1.9f, 0f);
                if (casal == null) ProceduralModels.Prim(PrimitiveType.Capsule, pivo.transform, "CasalSimples", new Vector3(0f, 1f, 0f), new Vector3(0.8f, 1f, 0.8f), "Osso");
                DancaValsa dv = pivo.AddComponent<DancaValsa>();
                dv.velocidadeGiro = i % 2 == 0 ? 75f : -75f;
                ProceduralModels.Luz(pivo.transform, "LuzDanca", new Vector3(0f, 1.6f, 0f), new Color(0.5f, 0.7f, 1f), 1.1f, 6f, true);
            }

            // altar de salvamento inicial e itens do caminho
            Altar(new Vector3(-6f, 0f, -68f));
            float[] ze = { -62f, -50f, -38f, -26f };
            for (int i = 0; i < ze.Length; i++) Essencia(i % 2 == 0 ? 2.4f : -2.4f, ze[i]);
            Essencia(5f, -70f); Essencia(-5f, -20f);
            Erva(-2.8f, -56f); Erva(2.8f, -30f);
            Item(TipoColetavel.Frasco, new Vector3(2.6f, 0.6f, -44f), "", "", "");

            // Mensageiro (NPC). O gatilho de conversa tem raio em metros reais.
            Vector3 pm = new Vector3(5f, 0f, -64f);
            GameObject msg = Modelos.Instanciar("mensageiro", tProps, 2.2f, 200f);
            if (msg == null) msg = ProceduralModels.Prim(PrimitiveType.Capsule, tProps, "Mensageiro", pm + new Vector3(0f, 1f, 0f), new Vector3(0.8f, 1f, 0.8f), "TecidoEscuro", false);
            else msg.transform.localPosition = pm;
            GameObject gat = Modelos.Gatilho("ConversaMensageiro", tProps, pm + new Vector3(0f, 1f, 0f), 1.4f);
            NpcFalante npc = gat.AddComponent<NpcFalante>();
            npc.alvoCamera = msg.transform;
            npc.nome = "Mensageiro";
            npc.fala = "[Texto provisório - a história será escrita pelo autor]\n\nBem-vindo ao Mosteiro de Névoa Negra, monge. Poucos chegam até aqui sem perder a esperança.\n\nSiga o caminho entre os túmulos. Os mortos não descansam nesta terra, e o que dorme dentro do mosteiro é pior.\n\nReze no altar antes de seguir. Ele guardará o seu progresso.";
            ProceduralModels.Luz(tProps, "LuzMensageiro", pm + new Vector3(0f, 2.4f, 0f), new Color(0.6f, 0.7f, 1f), 1.2f, 6f, true);

            // brasas no ar (ambiente)
            Fx.Brasas(ext, new Vector3(0f, 3f, -47f), new Vector3(70f, 6f, 74f), new Color(1f, 0.45f, 0.15f));

            // inimigos do cemitério
            float[,] esq = { { -8f, -52f }, { 9f, -50f }, { -10f, -38f }, { 11f, -36f }, { -7f, -26f }, { 8f, -22f } };
            for (int i = 0; i < esq.GetLength(0); i++) Mob(TipoInimigo.Esqueleto, esq[i, 0], 0.05f, esq[i, 1], esq[i, 0] < 0 ? 90f : 270f);
            Mob(TipoInimigo.Possuido, 14f, 0.05f, -44f, 270f);
            Mob(TipoInimigo.Possuido, -15f, 0.05f, -32f, 90f);
            Mob(TipoInimigo.BruxaCinza, -14f, 0.05f, -18f, 90f);
        }

        // ------------------------------------------------------------------ interior
        static void DecoracaoInterior()
        {
            // Salão: armaduras assombradas (parecem decoração até você passar perto) e bandeiras
            Mob(TipoInimigo.Armadura, -5.2f, 0.05f, -1.5f, 90f);
            Mob(TipoInimigo.Armadura, 5.2f, 0.05f, -1.5f, 270f);
            Mob(TipoInimigo.Armadura, -5.2f, 0.05f, 2.5f, 90f);
            Mob(TipoInimigo.Armadura, 5.2f, 0.05f, 2.5f, 270f);
            Colocar("bandeira", tProps, new Vector3(-5.6f, 0f, 5.2f), 3.6f, 90f, Colisor.Nenhum, "Tapete", new Vector3(0.1f, 3.6f, 1f));
            Colocar("bandeira", tProps, new Vector3(5.6f, 0f, 5.2f), 3.6f, 270f, Colisor.Nenhum, "Tapete", new Vector3(0.1f, 3.6f, 1f));

            // Capela e Biblioteca: bruxas
            Mob(TipoInimigo.BruxaCinza, -18.5f, 0.05f, 0.5f, 90f);
            Mob(TipoInimigo.BruxaNegra, 19.5f, 0.05f, 0f, 270f);

            // Catacumbas: monstro
            Mob(TipoInimigo.Monstro, 0f, 0.05f, 20.5f, 180f);

            // Santuário: tronos, altares e a bruxa mestra
            Colocar("trono_boss", tProps, new Vector3(0f, 0.25f, 41f), 3.4f, 180f, Colisor.Caixa, "Obsidiana", new Vector3(1.6f, 3.4f, 1f));
            Colocar("trono_lateral", tProps, new Vector3(-6.2f, 0f, 40f), 2.2f, 180f, Colisor.Caixa, "Obsidiana", new Vector3(2.2f, 2.2f, 1f));
            Colocar("trono_lateral", tProps, new Vector3(6.2f, 0f, 40f), 2.2f, 180f, Colisor.Caixa, "Obsidiana", new Vector3(2.2f, 2.2f, 1f));
            Colocar("altar_ossos", tProps, new Vector3(6.2f, 0f, 29f), 1.8f, 270f, Colisor.Caixa, "Osso", new Vector3(3f, 1.8f, 2.4f));
            Colocar("altar_foco_boss", tProps, new Vector3(0f, 0f, 31f), 0.45f, 180f, Colisor.Caixa, "PedraClara", new Vector3(1.2f, 0.45f, 1.1f));
            Mob(TipoInimigo.BruxaVermelha, -5f, 0.05f, 36.5f, 120f);
            Fx.Brasas(tProps, new Vector3(0f, 3f, 34f), new Vector3(15f, 5f, 15f), new Color(1f, 0.25f, 0.12f));
        }
    }
}
