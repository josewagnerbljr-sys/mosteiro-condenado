using UnityEngine;

namespace MongeArcano
{
    /// <summary>Torre de 3 andares, câmara secreta, baús e a masmorra de ondas.</summary>
    public static partial class ConstrutorMundo
    {
        public static void BauDinamico(Vector3 pos, int itens, float bonus) { CriarBau(null, pos, 0f, itens, bonus); }

        static void CriarBau(Transform pai, Vector3 pos, float rotY, int itens, float bonus)
        {
            GameObject raiz = new GameObject("Bau");
            if (pai != null) raiz.transform.SetParent(pai, false);
            raiz.transform.position = pos;
            raiz.transform.rotation = Quaternion.Euler(0f, rotY, 0f);
            Transform t = raiz.transform;
            ProceduralModels.Prim(PrimitiveType.Cube, t, "Base", new Vector3(0f, 0.35f, 0f), new Vector3(1.3f, 0.7f, 0.85f), "Madeira", true);
            ProceduralModels.Prim(PrimitiveType.Cube, t, "Faixa", new Vector3(0f, 0.35f, 0f), new Vector3(1.34f, 0.1f, 0.9f), "Ouro");
            GameObject pivo = new GameObject("Tampa");
            pivo.transform.SetParent(t, false);
            pivo.transform.localPosition = new Vector3(0f, 0.7f, -0.42f);
            ProceduralModels.Prim(PrimitiveType.Cube, pivo.transform, "TampaMesh", new Vector3(0f, 0.12f, 0.42f), new Vector3(1.3f, 0.25f, 0.85f), "Madeira");
            ProceduralModels.Prim(PrimitiveType.Sphere, t, "Fechadura", new Vector3(0f, 0.62f, 0.45f), Vector3.one * 0.14f, "EmissaoDourada");
            ProceduralModels.Luz(t, "Luz", new Vector3(0f, 1.2f, 0f), new Color(1f, 0.75f, 0.3f), 1.1f, 5f, true);
            Bau b = raiz.AddComponent<Bau>();
            b.tampa = pivo.transform;
            b.itens = itens;
            b.bonusRaridade = bonus;
        }

        static void Degraus(Transform pai, float x0, float sentido, float z, float yBase, int n)
        {
            for (int i = 0; i < n; i++)
            {
                float topo = yBase + (i + 1) * 0.25f;
                float x = x0 + sentido * (0.175f + i * 0.35f);
                Cubo(pai, "Degrau", new Vector3(x, topo * 0.5f, z), new Vector3(0.35f, topo, 2f), "PedraClara");
            }
        }

        static void Torre()
        {
            Transform t = Filho(tEst, "Torre");
            Cubo(t, "TorrePiso", new Vector3(34f, -0.2f, 0f), new Vector3(16f, 0.4f, 16f), "Piso");
            Cubo(t, "ParedeTorreLeste", new Vector3(42.25f, 8f, 0f), new Vector3(0.5f, 16f, 17f), "Pedra");
            Cubo(t, "ParedeTorreNorte", new Vector3(34f, 8f, 8.25f), new Vector3(17f, 16f, 0.5f), "Pedra");
            Cubo(t, "ParedeTorreSul", new Vector3(34f, 8f, -8.25f), new Vector3(17f, 16f, 0.5f), "Pedra");
            Cubo(t, "ParedeTorreOesteA", new Vector3(26.25f, 8f, -4.75f), new Vector3(0.5f, 16f, 6.5f), "Pedra");
            Cubo(t, "ParedeTorreOesteB", new Vector3(26.25f, 8f, 4.75f), new Vector3(0.5f, 16f, 6.5f), "Pedra");
            Cubo(t, "ParedeTorreOesteVerga", new Vector3(26.25f, 9.5f, 0f), new Vector3(0.5f, 13f, 3f), "PedraClara");
            Cubo(t, "TorreAndar2", new Vector3(38f, 4.8f, 0f), new Vector3(8f, 0.4f, 16f), "Piso");
            Cubo(t, "TorreAndar3", new Vector3(30f, 9.8f, 0f), new Vector3(8f, 0.4f, 16f), "Piso");
            Degraus(t, 27f, 1f, -6.5f, 0f, 20);
            Degraus(t, 34f, -1f, 6.5f, 5f, 20);

            ProceduralModels.Luz(tProps, "LuzTorre1", new Vector3(34f, 3.6f, 0f), new Color(1f, 0.6f, 0.35f), 1.0f, 16f, false);
            ProceduralModels.Luz(tProps, "LuzTorre2", new Vector3(38f, 8.6f, 0f), new Color(0.9f, 0.5f, 0.6f), 1.0f, 14f, false);
            ProceduralModels.Luz(tProps, "LuzTorre3", new Vector3(30f, 13.6f, 0f), new Color(0.6f, 0.45f, 1f), 1.2f, 14f, false);
            Tocha(new Vector3(26.7f, 2.4f, 6.2f)); Tocha(new Vector3(41.7f, 2.4f, 4f)); Tocha(new Vector3(41.7f, 2.4f, -4f));
            Tocha(new Vector3(41.7f, 7.4f, 4f)); Tocha(new Vector3(41.7f, 7.4f, -4f)); Tocha(new Vector3(36f, 7.4f, 7.7f));
            Tocha(new Vector3(26.7f, 12.4f, -4f)); Tocha(new Vector3(26.7f, 12.4f, 4f)); Tocha(new Vector3(33f, 12.4f, 7.7f));
            Zona("Torre", new Vector3(34f, 0f, 0f), 17f, new Vector3(-8f, 13f, -9f), 62f);

            Mob(TipoInimigo.Armadura, 30f, 0.05f, 3.5f, 90f); Mob(TipoInimigo.Armadura, 38f, 0.05f, 3.5f, 270f);
            Mob(TipoInimigo.Esqueleto, 32f, 0.05f, -2f, 90f); Mob(TipoInimigo.Esqueleto, 38f, 0.05f, -3f, 270f);
            Item(TipoColetavel.Essencia, new Vector3(29f, 0.6f, -2f), "", "", ""); Item(TipoColetavel.Erva, new Vector3(40f, 0.55f, 5f), "", "", "");
            Mob(TipoInimigo.BruxaNegra, 39f, 5.05f, -4f, 270f); Mob(TipoInimigo.Esqueleto, 38f, 5.05f, 4f, 270f); Mob(TipoInimigo.Esqueleto, 40f, 5.05f, 0f, 270f);
            CriarBau(tProps, new Vector3(41f, 5f, 5.8f), 180f, 3, 0.15f);
            Item(TipoColetavel.Essencia, new Vector3(37f, 5.6f, 2f), "", "", "");
            Mob(TipoInimigo.BruxaVermelha, 29f, 10.05f, 3f, 90f); Mob(TipoInimigo.Armadura, 31f, 10.05f, -3f, 90f);
            CriarBau(tProps, new Vector3(27.5f, 10f, -6.2f), 0f, 4, 0.35f);
            Item(TipoColetavel.Frasco, new Vector3(30f, 10.6f, 6f), "", "", "");
        }

        static void Secreta()
        {
            Vector3 c = new Vector3(-16f, 0f, -16f);
            Sala("Secreta", c, 12f, Lado.N);
            Corredor("Corredor_Secreto", new Vector3(-16f, 0f, -8f), false);
            Porta p = CriarPorta("Parede_Secreta", new Vector3(-16f, 0f, -8f), false, new string[0], "porta_secreta");
            p.rotuloCustom = "Examinar a parede rachada";
            Renderer[] rs = p.painel.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < rs.Length; i++)
            {
                string n = rs[i].gameObject.name;
                rs[i].sharedMaterial = ProceduralModels.Mat(n == "Emblema" ? "EmissaoVermelha" : (n == "Placa" ? "PedraMusgo" : "Pedra"));
            }
            LuzSala(c, new Color(0.6f, 0.35f, 1f), 1.0f);
            Zona("Secreta", c, 12f, new Vector3(5.2f, 6.8f, -5.4f), 55f);
            Altar(new Vector3(-16f, 0f, -20.2f));
            CriarBau(tProps, new Vector3(-20.5f, 0f, -18f), 90f, 4, 0.25f);
            CriarBau(tProps, new Vector3(-11.5f, 0f, -18f), 270f, 4, 0.25f);
            Tocha(new Vector3(-21.7f, 2.2f, -13f)); Tocha(new Vector3(-10.3f, 2.2f, -13f)); Tocha(new Vector3(-16f, 2.2f, -21.7f));
            Mob(TipoInimigo.Armadura, -19f, 0.05f, -13f, 0f); Mob(TipoInimigo.Armadura, -13f, 0.05f, -13f, 0f); Mob(TipoInimigo.Monstro, -16f, 0.05f, -16.5f, 0f);
            Essencia(-18f, -12f); Essencia(-14f, -12f); Erva(-16f, -12.5f);
        }

        static void Masmorra()
        {
            Transform pt = Filho(tProps, "Masmorra");
            Vector3 entrada = new Vector3(-26f, 0f, -22f);
            ProceduralModels.Prim(PrimitiveType.Cylinder, pt, "ColunaA", entrada + new Vector3(-1.9f, 1.6f, 0f), new Vector3(0.7f, 1.6f, 0.7f), "PedraMusgo", true);
            ProceduralModels.Prim(PrimitiveType.Cylinder, pt, "ColunaB", entrada + new Vector3(1.9f, 1.6f, 0f), new Vector3(0.7f, 1.6f, 0.7f), "PedraMusgo", true);
            ProceduralModels.Prim(PrimitiveType.Cube, pt, "Arco", entrada + new Vector3(0f, 3.4f, 0f), new Vector3(4.6f, 0.6f, 0.9f), "Pedra");
            ProceduralModels.Prim(PrimitiveType.Cylinder, pt, "Disco", entrada + new Vector3(0f, 1.7f, 0f), new Vector3(3.2f, 0.03f, 3.2f), "EmissaoRoxa", false, new Vector3(90f, 0f, 0f));
            ProceduralModels.Luz(pt, "LuzPortal", entrada + new Vector3(0f, 1.8f, 0.5f), new Color(0.65f, 0.3f, 1f), 2.2f, 10f, true);
            Fx.Brasas(pt, entrada + new Vector3(0f, 2f, 0f), new Vector3(3f, 3f, 1.5f), new Color(0.7f, 0.35f, 1f));

            Vector3 c = new Vector3(85f, 0f, -45f);
            Transform ar = Filho(tEst, "Arena");
            Cubo(ar, "ArenaPiso", c + new Vector3(0f, -0.25f, 0f), new Vector3(32f, 0.5f, 32f), "Piso");
            Cubo(ar, "ParedeArenaN", c + new Vector3(0f, 3f, 16.25f), new Vector3(33f, 6f, 0.5f), "Pedra");
            Cubo(ar, "ParedeArenaS", c + new Vector3(0f, 3f, -16.25f), new Vector3(33f, 6f, 0.5f), "Pedra");
            Cubo(ar, "ParedeArenaL", c + new Vector3(16.25f, 3f, 0f), new Vector3(0.5f, 6f, 33f), "Pedra");
            Cubo(ar, "ParedeArenaO", c + new Vector3(-16.25f, 3f, 0f), new Vector3(0.5f, 6f, 33f), "Pedra");
            ProceduralModels.Prim(PrimitiveType.Cylinder, ar, "Glifo", c + new Vector3(0f, 0.03f, 0f), new Vector3(9f, 0.01f, 9f), "EmissaoRoxa");
            for (int i = 0; i < 8; i++)
            {
                float a = i * Mathf.PI * 2f / 8f;
                Vector3 p = c + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * 13f;
                ProceduralModels.Prim(PrimitiveType.Cylinder, ar, "PilarArena", p + new Vector3(0f, 2f, 0f), new Vector3(1f, 2f, 1f), "PedraClara", true);
                ProceduralModels.Luz(ar, "LuzArena", p + new Vector3(0f, 4.4f, 0f), new Color(1f, 0.4f, 0.2f), 1.8f, 10f, true);
            }
            ProceduralModels.Luz(ar, "LuzCentro", c + new Vector3(0f, 6f, 0f), new Color(0.7f, 0.4f, 1f), 1.4f, 24f, false);

            GameObject go = new GameObject("ArenaOndas");
            go.transform.SetParent(ar, false);
            go.transform.position = c;
            ArenaOndas arena = go.AddComponent<ArenaOndas>();
            arena.centro = c;
            arena.pai = tIni;

            Vector3 ps = c + new Vector3(0f, 0f, 12f);
            GameObject sg = Modelos.Gatilho("PortalSaida", ar, ps + Vector3.up * 1.5f, 1.6f);
            ProceduralModels.Prim(PrimitiveType.Cylinder, sg.transform, "Disco", Vector3.zero, new Vector3(3f, 0.03f, 3f), "EmissaoDourada", false, new Vector3(90f, 0f, 0f));
            ProceduralModels.Luz(sg.transform, "Luz", Vector3.zero, new Color(1f, 0.8f, 0.4f), 2.2f, 10f, true);
            PortalMasmorra saida = sg.AddComponent<PortalMasmorra>();
            saida.rotulo = "Voltar ao cemitério";
            saida.destino = entrada + new Vector3(0f, 0.05f, 2.5f);
            sg.SetActive(false);
            arena.portalSaida = saida;

            GameObject eg = Modelos.Gatilho("PortalEntrada", pt, entrada + Vector3.up * 1.5f, 1.8f);
            PortalMasmorra pe = eg.AddComponent<PortalMasmorra>();
            pe.rotulo = "Entrar na Masmorra do Eco (5 ondas)";
            pe.destino = c + new Vector3(-12f, 0.05f, 0f);
            pe.arena = arena;
        }
    }
}
