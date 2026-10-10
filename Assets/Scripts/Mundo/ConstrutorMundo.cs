using UnityEngine;

namespace MongeArcano
{
    /// <summary>
    /// Constrói o Mosteiro de Névoa Negra inteiro por código: salas, corredores, portões, props com física,
    /// itens, inimigos, câmeras fixas, luzes e HUD. O menu "Monge Arcano > Gerar cena" executa este construtor
    /// no editor e salva a cena (assim você pode editar tudo visualmente depois).
    /// Todas as coordenadas são do mundo (o objeto "Mundo" fica na origem).
    /// </summary>
    public static class ConstrutorMundo
    {
        const float AltParede = 4.5f;
        const float Esp = 0.5f;
        const float LargPorta = 3f;

        [System.Flags]
        enum Lado { Nenhum = 0, N = 1, E = 2, S = 4, W = 8 }

        static Transform tEst, tProps, tItens, tIni, tPortas;

        // ------------------------------------------------------------------ textos do jogo (PT-BR)
        const string NotaSalao =
            "Terceira lua após o ritual falho. Os irmãos já não dormem: ouvem um canto vindo das catacumbas.\n\n" +
            "Selei o Santuário com os Três Sigilos - Lua, Fogo e Terra - e escondi cada um nos salões do mosteiro. " +
            "Quem os reunir poderá abrir o portão... ou libertar o que dorme lá dentro.\n\n" +
            "A Capela, a oeste, ainda é terra consagrada. Que a luz me perdoe.\n\n- Abade Iram";
        const string NotaCapela =
            "Os espectros dos irmãos mortos rondam a Capela. A essência azul do cajado os dispersa.\n\n" +
            "A Chave de Ferro, que abre a Biblioteca, deixei sobre o pedestal junto ao altar. " +
            "O Sigilo da Lua repousa na parede oeste; os espectros o guardam.\n\n" +
            "Reze no altar para restaurar as forças. É o único lugar seguro.";
        const string NotaBiblioteca =
            "Os possuídos vagam entre as estantes, repetindo versos que não entendem.\n\n" +
            "A Chave de Osso, que abre as Catacumbas, está na parede leste. " +
            "O Sigilo do Fogo queima sobre a mesa - não o toque sem estar preparado.\n\n- Bibliotecário Anselmo";
        const string NotaCatacumbas =
            "Aqui jaz o Sigilo da Terra, vigiado por criaturas infernais que já foram nossos irmãos.\n\n" +
            "Além do portão ao norte fica o Santuário, onde o Arconte das Cinzas aguarda.\n\n" +
            "Guarde sua essência. Cada raio conta.";
        const string NotaSantuario =
            "Se você lê isto, o Arconte ainda vive.\n\n" +
            "Fira-o até que sua chama vacile. Ele chamará espíritos quando estiver ferido. " +
            "Quando ele cair, um portal de luz se abrirá.\n\n" +
            "Não olhe para trás, monge.";

        // ------------------------------------------------------------------ construção principal
        public static GameObject Construir()
        {
            GameObject raiz = new GameObject("Mundo");
            Transform rt = raiz.transform;
            tEst = Filho(rt, "Estrutura");
            tPortas = Filho(rt, "Portas");
            tProps = Filho(rt, "Cenario");
            tItens = Filho(rt, "Itens");
            tIni = Filho(rt, "Inimigos");

            ConfigurarAmbiente();

            Salao();
            Capela();
            Biblioteca();
            Catacumbas();
            Santuario();

            // corredores e portões
            Corredor("Corredor_Salao_Capela", new Vector3(-8f, 0f, 0f), true);
            Corredor("Corredor_Salao_Biblioteca", new Vector3(8f, 0f, 0f), true);
            Corredor("Corredor_Salao_Catacumbas", new Vector3(0f, 0f, 8f), false);
            Corredor("Corredor_Catacumbas_Santuario", new Vector3(0f, 0f, 24f), false);

            CriarPorta("Portao_Capela", new Vector3(-8f, 0f, 0f), true, new string[0], "");
            CriarPorta("Portao_Biblioteca", new Vector3(8f, 0f, 0f), true, new string[] { "ferro" }, "porta_biblioteca");
            CriarPorta("Portao_Catacumbas", new Vector3(0f, 0f, 8f), false, new string[] { "osso" }, "porta_catacumbas");
            CriarPorta("Portao_Santuario", new Vector3(0f, 0f, 24f), false, new string[] { "sigilo_lua", "sigilo_fogo", "sigilo_terra" }, "porta_santuario");

            GameObject portal = CriarPortal(new Vector3(0f, 0f, 34f));
            PlayerController jogador = ProceduralModels.CriarJogador(new Vector3(0f, 0.05f, -4.2f), 0f);
            CriarCamera(jogador.transform);

            GameObject g = new GameObject("Gerenciador de Jogo");
            GameManager gm = g.AddComponent<GameManager>();
            gm.jogador = jogador;
            gm.portalFinal = portal;
            g.AddComponent<GameUI>();

            return raiz;
        }

        // ------------------------------------------------------------------ ambiente / câmera
        static void ConfigurarAmbiente()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.12f, 0.12f, 0.18f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.02f, 0.02f, 0.04f);
            RenderSettings.fogDensity = 0.022f;
        }

        static void CriarCamera(Transform alvo)
        {
            GameObject go = new GameObject("Camera Principal");
            go.tag = "MainCamera";
            Camera cam = go.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.01f, 0.01f, 0.02f);
            cam.fieldOfView = 52f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 90f;
            go.AddComponent<AudioListener>();
            CameraDirector d = go.AddComponent<CameraDirector>();
            d.alvo = alvo;
            go.transform.position = new Vector3(-5.2f, 6.8f, -5.4f);
            go.transform.LookAt(alvo.position + Vector3.up);
        }

        // ------------------------------------------------------------------ helpers
        static Transform Filho(Transform pai, string nome)
        {
            GameObject go = new GameObject(nome);
            go.transform.SetParent(pai, false);
            return go.transform;
        }

        static GameObject Cubo(Transform pai, string nome, Vector3 pos, Vector3 esc, string mat)
        {
            return ProceduralModels.Prim(PrimitiveType.Cube, pai, nome, pos, esc, mat, true);
        }

        static Coletavel Item(TipoColetavel tipo, Vector3 pos, string id, string titulo, string texto)
        {
            Coletavel c = ProceduralModels.CriarColetavel(tipo, pos, id, titulo, texto);
            c.transform.SetParent(tItens, true);
            return c;
        }

        static void Essencia(float x, float z) { Item(TipoColetavel.Essencia, new Vector3(x, 0.6f, z), "", "", ""); }
        static void Erva(float x, float z) { Item(TipoColetavel.Erva, new Vector3(x, 0.55f, z), "", "", ""); }

        static void Mob(TipoInimigo tipo, float x, float y, float z, float rotY)
        {
            Inimigo i = ProceduralModels.CriarInimigo(tipo, new Vector3(x, y, z), rotY);
            i.transform.SetParent(tIni, true);
        }

        static void Zona(string nome, Vector3 centro, float tam, Vector3 offsetCam, float fov)
        {
            GameObject go = new GameObject("ZonaCamera_" + nome);
            go.transform.SetParent(tEst, false);
            go.transform.position = centro + Vector3.up * 3f;
            BoxCollider bc = go.AddComponent<BoxCollider>();
            bc.isTrigger = true;
            bc.size = new Vector3(tam - 0.2f, 8f, tam - 0.2f);
            ZonaCamera z = go.AddComponent<ZonaCamera>();
            z.posicaoCamera = centro + offsetCam;
            z.campoDeVisao = fov;
        }

        static void LuzSala(Vector3 centro, Color cor, float intensidade)
        {
            ProceduralModels.Luz(tProps, "LuzSala", centro + new Vector3(0f, 3.6f, 0f), cor, intensidade, 15f, false);
        }

        // ------------------------------------------------------------------ estrutura
        static void Sala(string nome, Vector3 c, float tam, Lado portas)
        {
            Transform t = Filho(tEst, "Sala_" + nome);
            t.position = c;
            float m = tam / 2f;
            Cubo(t, "Piso", new Vector3(0f, -0.2f, 0f), new Vector3(tam, 0.4f, tam), "Piso");
            Parede(t, "ParedeNorte", new Vector3(0f, 0f, m + Esp / 2f), tam + Esp * 2f, true, (portas & Lado.N) != 0);
            Parede(t, "ParedeSul", new Vector3(0f, 0f, -(m + Esp / 2f)), tam + Esp * 2f, true, (portas & Lado.S) != 0);
            Parede(t, "ParedeLeste", new Vector3(m + Esp / 2f, 0f, 0f), tam, false, (portas & Lado.E) != 0);
            Parede(t, "ParedeOeste", new Vector3(-(m + Esp / 2f), 0f, 0f), tam, false, (portas & Lado.W) != 0);
        }

        static void Parede(Transform pai, string nome, Vector3 centro, float comp, bool aoLongoDeX, bool porta)
        {
            float y = AltParede / 2f;
            if (!porta)
            {
                Vector3 esc = aoLongoDeX ? new Vector3(comp, AltParede, Esp) : new Vector3(Esp, AltParede, comp);
                Cubo(pai, nome, centro + new Vector3(0f, y, 0f), esc, "Pedra");
                return;
            }
            float seg = (comp - LargPorta) / 2f;
            float off = LargPorta / 2f + seg / 2f;
            Vector3 escSeg = aoLongoDeX ? new Vector3(seg, AltParede, Esp) : new Vector3(Esp, AltParede, seg);
            Vector3 offA = aoLongoDeX ? new Vector3(-off, y, 0f) : new Vector3(0f, y, -off);
            Vector3 offB = aoLongoDeX ? new Vector3(off, y, 0f) : new Vector3(0f, y, off);
            Cubo(pai, nome + "_A", centro + offA, escSeg, "Pedra");
            Cubo(pai, nome + "_B", centro + offB, escSeg, "Pedra");
            float h = AltParede - 3f;
            Vector3 escVerga = aoLongoDeX ? new Vector3(LargPorta, h, Esp) : new Vector3(Esp, h, LargPorta);
            Cubo(pai, nome + "_Verga", centro + new Vector3(0f, 3f + h / 2f, 0f), escVerga, "PedraClara");
        }

        static void Corredor(string nome, Vector3 centro, bool aoLongoDeX)
        {
            Transform t = Filho(tEst, nome);
            t.position = centro;
            const float comp = 4f;
            Vector3 escPiso = aoLongoDeX ? new Vector3(comp, 0.4f, LargPorta + 1f) : new Vector3(LargPorta + 1f, 0.4f, comp);
            Cubo(t, "Piso", new Vector3(0f, -0.2f, 0f), escPiso, "Piso");
            float lado = LargPorta / 2f + Esp / 2f;
            Vector3 escParede = aoLongoDeX ? new Vector3(comp, AltParede, Esp) : new Vector3(Esp, AltParede, comp);
            float y = AltParede / 2f;
            Cubo(t, "ParedeA", aoLongoDeX ? new Vector3(0f, y, lado) : new Vector3(lado, y, 0f), escParede, "Pedra");
            Cubo(t, "ParedeB", aoLongoDeX ? new Vector3(0f, y, -lado) : new Vector3(-lado, y, 0f), escParede, "Pedra");
            Tocha(centro + (aoLongoDeX ? new Vector3(0f, 2.2f, lado - 0.2f) : new Vector3(lado - 0.2f, 2.2f, 0f)));
        }

        static void CriarPorta(string nome, Vector3 pos, bool aoLongoDeX, string[] chaves, string marco)
        {
            Transform t = Filho(tPortas, nome);
            t.position = pos;
            Transform painel = Filho(t, "Painel");
            Vector3 esc = aoLongoDeX ? new Vector3(0.35f, 3.4f, LargPorta + 0.2f) : new Vector3(LargPorta + 0.2f, 3.4f, 0.35f);
            Cubo(painel, "Placa", new Vector3(0f, 1.7f, 0f), esc, "Madeira");
            Vector3 escFaixa = aoLongoDeX ? new Vector3(0.42f, 0.16f, LargPorta + 0.25f) : new Vector3(LargPorta + 0.25f, 0.16f, 0.42f);
            ProceduralModels.Prim(PrimitiveType.Cube, painel, "FaixaSuperior", new Vector3(0f, 2.8f, 0f), escFaixa, "Ouro");
            ProceduralModels.Prim(PrimitiveType.Cube, painel, "FaixaInferior", new Vector3(0f, 0.6f, 0f), escFaixa, "Ouro");
            string emblema = chaves.Length >= 3 ? "EmissaoRoxa" : (chaves.Length == 1 ? "EmissaoDourada" : "EmissaoAzul");
            ProceduralModels.Prim(PrimitiveType.Sphere, painel, "Emblema", new Vector3(0f, 1.7f, 0f), Vector3.one * 0.4f, emblema);

            Porta p = t.gameObject.AddComponent<Porta>();
            p.painel = painel;
            p.chavesExigidas = chaves;
            p.marco = marco;
            p.deslocamento = new Vector3(0f, -3.6f, 0f);
        }

        static GameObject CriarPortal(Vector3 pos)
        {
            Transform t = Filho(tProps, "PortalDeLuz");
            t.position = pos;
            ProceduralModels.Prim(PrimitiveType.Cylinder, t, "Base", new Vector3(0f, 0.03f, 0f), new Vector3(3.2f, 0.03f, 3.2f), "EmissaoBranca");
            ProceduralModels.Prim(PrimitiveType.Cylinder, t, "Coluna", new Vector3(0f, 2.2f, 0f), new Vector3(1.6f, 2.2f, 1.6f), "Fantasma");
            ProceduralModels.Luz(t, "Luz", new Vector3(0f, 2f, 0f), Color.white, 3f, 14f, true);
            SphereCollider sc = t.gameObject.AddComponent<SphereCollider>();
            sc.isTrigger = true;
            sc.radius = 1.8f;
            sc.center = new Vector3(0f, 1f, 0f);
            t.gameObject.AddComponent<Portal>();
            t.gameObject.SetActive(false);
            return t.gameObject;
        }

        // ------------------------------------------------------------------ props
        static void Tocha(Vector3 pos)
        {
            Transform t = Filho(tProps, "Tocha");
            t.position = pos;
            ProceduralModels.Prim(PrimitiveType.Cube, t, "Suporte", new Vector3(0f, -0.2f, 0f), new Vector3(0.12f, 0.5f, 0.12f), "Madeira");
            ProceduralModels.Prim(PrimitiveType.Sphere, t, "Chama", new Vector3(0f, 0.15f, 0f), new Vector3(0.24f, 0.34f, 0.24f), "EmissaoLaranja");
            ProceduralModels.Luz(t, "Luz", new Vector3(0f, 0.3f, 0f), new Color(1f, 0.55f, 0.2f), 1.5f, 9f, true);
        }

        static void Pilar(float x, float z)
        {
            ProceduralModels.Prim(PrimitiveType.Cylinder, tProps, "Pilar", new Vector3(x, AltParede / 2f, z), new Vector3(0.9f, AltParede / 2f, 0.9f), "PedraClara", true);
            ProceduralModels.Prim(PrimitiveType.Cube, tProps, "PilarBase", new Vector3(x, 0.2f, z), new Vector3(1.3f, 0.4f, 1.3f), "Pedra", true);
        }

        static void Pedestal(float x, float z)
        {
            Cubo(tProps, "Pedestal", new Vector3(x, 0.45f, z), new Vector3(0.7f, 0.9f, 0.7f), "PedraClara");
        }

        static void Caixa(float x, float y, float z)
        {
            GameObject c = ProceduralModels.Prim(PrimitiveType.Cube, tProps, "Caixa", new Vector3(x, y, z), Vector3.one * 0.9f, "Madeira", true);
            Rigidbody rb = c.AddComponent<Rigidbody>();
            rb.mass = 4f;
        }

        static void Barril(float x, float z)
        {
            GameObject c = ProceduralModels.Prim(PrimitiveType.Cylinder, tProps, "Barril", new Vector3(x, 0.55f, z), new Vector3(0.7f, 0.55f, 0.7f), "Madeira", true);
            Rigidbody rb = c.AddComponent<Rigidbody>();
            rb.mass = 6f;
        }

        static void Banco(float x, float z)
        {
            Cubo(tProps, "BancoAssento", new Vector3(x, 0.45f, z), new Vector3(2.4f, 0.1f, 0.6f), "Madeira");
            Cubo(tProps, "BancoPerna1", new Vector3(x - 1f, 0.2f, z), new Vector3(0.1f, 0.4f, 0.5f), "Madeira");
            Cubo(tProps, "BancoPerna2", new Vector3(x + 1f, 0.2f, z), new Vector3(0.1f, 0.4f, 0.5f), "Madeira");
        }

        static void Estante(float x, float z, float comp)
        {
            Cubo(tProps, "Estante", new Vector3(x, 1.6f, z), new Vector3(comp, 3.2f, 0.7f), "Madeira");
            float lado = z > 0f ? -0.36f : 0.36f;
            string[] livros = { "Tapete", "TecidoEscuro", "Ouro", "Piso" };
            for (int i = 0; i < 4; i++)
                ProceduralModels.Prim(PrimitiveType.Cube, tProps, "Livros" + i, new Vector3(x, 0.55f + i * 0.72f, z + lado), new Vector3(comp - 0.4f, 0.36f, 0.1f), livros[i]);
        }

        static void Sarcofago(float x, float z)
        {
            Cubo(tProps, "SarcofagoBase", new Vector3(x, 0.3f, z), new Vector3(1.1f, 0.6f, 2.3f), "Pedra");
            GameObject tampa = ProceduralModels.Prim(PrimitiveType.Cube, tProps, "SarcofagoTampa", new Vector3(x + 0.1f, 0.7f, z), new Vector3(1.2f, 0.2f, 2.4f), "PedraClara", true, new Vector3(0f, 5f, 0f));
            tampa.name = "SarcofagoTampa";
        }

        static void Braseiro(float x, float z)
        {
            ProceduralModels.Prim(PrimitiveType.Cylinder, tProps, "BraseiroBase", new Vector3(x, 0.45f, z), new Vector3(0.6f, 0.45f, 0.6f), "Obsidiana", true);
            ProceduralModels.Prim(PrimitiveType.Sphere, tProps, "BraseiroChama", new Vector3(x, 1.05f, z), new Vector3(0.45f, 0.5f, 0.45f), "EmissaoLaranja");
            ProceduralModels.Luz(tProps, "BraseiroLuz", new Vector3(x, 1.4f, z), new Color(1f, 0.5f, 0.15f), 1.8f, 10f, true);
        }

        static void Altar(Vector3 pos)
        {
            Transform t = Filho(tProps, "AltarSalvamento");
            t.position = pos;
            Cubo(t, "Base", new Vector3(0f, 0.45f, 0f), new Vector3(2.2f, 0.9f, 0.9f), "PedraClara");
            ProceduralModels.Prim(PrimitiveType.Cube, t, "Topo", new Vector3(0f, 0.95f, 0f), new Vector3(2.4f, 0.1f, 1.1f), "Pedra");
            ProceduralModels.Prim(PrimitiveType.Sphere, t, "Vela1", new Vector3(-0.8f, 1.15f, 0f), new Vector3(0.1f, 0.18f, 0.1f), "EmissaoDourada");
            ProceduralModels.Prim(PrimitiveType.Sphere, t, "Vela2", new Vector3(0.8f, 1.15f, 0f), new Vector3(0.1f, 0.18f, 0.1f), "EmissaoDourada");
            ProceduralModels.Prim(PrimitiveType.Sphere, t, "Orbe", new Vector3(0f, 1.3f, 0f), Vector3.one * 0.28f, "EmissaoAzul");
            ProceduralModels.Prim(PrimitiveType.Cylinder, t, "Circulo", new Vector3(0f, 0.012f, -1.6f), new Vector3(2.6f, 0.01f, 2.6f), "EmissaoAzul");
            ProceduralModels.Luz(t, "Luz", new Vector3(0f, 1.9f, -0.6f), new Color(0.3f, 0.6f, 1f), 2f, 8f, true);
            t.gameObject.AddComponent<AltarSalvamento>();
        }

        // ================================================================== SALAS
        // ---- Salão de Entrada (0,0,0): ponto de partida, calmo
        static void Salao()
        {
            Vector3 c = Vector3.zero;
            Sala("Salao", c, 12f, Lado.N | Lado.E | Lado.W);
            Zona("Salao", c, 12f, new Vector3(-5.2f, 6.8f, -5.4f), 52f);
            LuzSala(c, new Color(0.55f, 0.65f, 1f), 0.8f);

            ProceduralModels.Prim(PrimitiveType.Cube, tProps, "Tapete", new Vector3(0f, 0.015f, 0f), new Vector3(3f, 0.03f, 10f), "Tapete");
            Pilar(-3.8f, -3.2f); Pilar(3.8f, -3.2f); Pilar(-3.8f, 3.8f); Pilar(3.8f, 3.8f);
            Tocha(new Vector3(-5.7f, 2.2f, -3f)); Tocha(new Vector3(5.7f, 2.2f, -3f));
            Tocha(new Vector3(-2.4f, 2.2f, 5.7f)); Tocha(new Vector3(2.4f, 2.2f, 5.7f));

            // portão principal selado (sul)
            Cubo(tEst, "PortaoSelado", new Vector3(0f, 2f, -5.85f), new Vector3(4f, 4f, 0.3f), "Obsidiana");
            for (int i = -1; i <= 1; i++)
                ProceduralModels.Prim(PrimitiveType.Cube, tEst, "RunaPortao" + i, new Vector3(i * 1f, 2f, -5.68f), new Vector3(0.12f, 2.6f, 0.05f), "EmissaoRoxa");

            Pedestal(-4.8f, 3f);
            Item(TipoColetavel.Nota, new Vector3(-4.8f, 1.1f, 3f), "", "Diário do Abade", NotaSalao);

            Caixa(4.6f, 0.45f, -4.6f); Caixa(4.6f, 0.45f, -3.6f); Caixa(4.6f, 1.35f, -4.6f);
            Barril(5.1f, 3.4f); Barril(4.2f, 4.5f);

            Erva(-4.5f, -4.5f);
            Essencia(4.8f, 0f); Essencia(4.8f, 1.8f);
        }

        // ---- Capela (-16,0,0): altar de salvamento, Chave de Ferro, Sigilo da Lua, espectros
        static void Capela()
        {
            Vector3 c = new Vector3(-16f, 0f, 0f);
            Sala("Capela", c, 12f, Lado.E);
            Zona("Capela", c, 12f, new Vector3(5.2f, 6.8f, -5.4f), 55f);
            LuzSala(c, new Color(0.45f, 0.55f, 1f), 0.7f);

            Altar(new Vector3(-16f, 0f, 4.6f));
            Banco(-14.5f, -2.5f); Banco(-14.5f, -4.5f); Banco(-17.8f, -2.5f); Banco(-17.8f, -4.5f);
            Tocha(new Vector3(-21.7f, 2.2f, 3f)); Tocha(new Vector3(-21.7f, 2.2f, -3f));
            Tocha(new Vector3(-16f, 2.2f, 5.7f)); Tocha(new Vector3(-13f, 2.2f, -5.7f)); Tocha(new Vector3(-19f, 2.2f, -5.7f));
            Pilar(-12.2f, 3.2f);

            Pedestal(-12.2f, 4.9f);
            Item(TipoColetavel.Chave, new Vector3(-12.2f, 1.2f, 4.9f), "ferro", "", "");
            Pedestal(-20.8f, 0f);
            Item(TipoColetavel.Chave, new Vector3(-20.8f, 1.25f, 0f), "sigilo_lua", "", "");
            Pedestal(-12f, -4.9f);
            Item(TipoColetavel.Nota, new Vector3(-12f, 1.1f, -4.9f), "", "Oração Rasgada", NotaCapela);

            Essencia(-14f, 2f); Essencia(-19f, -4.8f);

            Mob(TipoInimigo.Espectro, -19.5f, 0.05f, 3f, 90f);
            Mob(TipoInimigo.Espectro, -19f, 0.05f, -3.2f, 90f);
        }

        // ---- Biblioteca (16,0,0): Chave de Osso, Sigilo do Fogo, possuídos
        static void Biblioteca()
        {
            Vector3 c = new Vector3(16f, 0f, 0f);
            Sala("Biblioteca", c, 12f, Lado.W);
            Zona("Biblioteca", c, 12f, new Vector3(5.2f, 6.8f, 5.4f), 55f);
            LuzSala(c, new Color(1f, 0.75f, 0.5f), 0.7f);

            Estante(16f, 5.4f, 10f);
            Estante(16f, -5.4f, 10f);
            Cubo(tProps, "MesaTampo", new Vector3(16f, 0.8f, 0f), new Vector3(2.6f, 0.1f, 1.3f), "Madeira");
            Cubo(tProps, "MesaPerna1", new Vector3(14.9f, 0.4f, 0.5f), new Vector3(0.12f, 0.8f, 0.12f), "Madeira");
            Cubo(tProps, "MesaPerna2", new Vector3(17.1f, 0.4f, 0.5f), new Vector3(0.12f, 0.8f, 0.12f), "Madeira");
            Cubo(tProps, "MesaPerna3", new Vector3(14.9f, 0.4f, -0.5f), new Vector3(0.12f, 0.8f, 0.12f), "Madeira");
            Cubo(tProps, "MesaPerna4", new Vector3(17.1f, 0.4f, -0.5f), new Vector3(0.12f, 0.8f, 0.12f), "Madeira");
            Item(TipoColetavel.Chave, new Vector3(16.8f, 1.3f, 0f), "sigilo_fogo", "", "");

            Tocha(new Vector3(10.3f, 2.2f, 3.5f)); Tocha(new Vector3(10.3f, 2.2f, -3.5f)); Tocha(new Vector3(21.7f, 2.2f, 0f));

            Pedestal(21.2f, -3.8f);
            Item(TipoColetavel.Chave, new Vector3(21.2f, 1.2f, -3.8f), "osso", "", "");
            Pedestal(11.5f, 3.4f);
            Item(TipoColetavel.Nota, new Vector3(11.5f, 1.1f, 3.4f), "", "Registro do Bibliotecário", NotaBiblioteca);

            Caixa(21.2f, 0.45f, 2f); Caixa(21.2f, 0.45f, 3.1f); Barril(11.2f, -4.2f);
            Erva(21f, 4.2f);
            Essencia(20f, 3.3f); Essencia(12.8f, -4.3f);

            Mob(TipoInimigo.Possuido, 18.5f, 0.05f, 3f, 200f);
            Mob(TipoInimigo.Possuido, 13.5f, 0.05f, -3.2f, 120f);
            Mob(TipoInimigo.Possuido, 19.5f, 0.05f, -2.5f, 250f);
        }

        // ---- Catacumbas (0,0,16): Sigilo da Terra, infernais
        static void Catacumbas()
        {
            Vector3 c = new Vector3(0f, 0f, 16f);
            Sala("Catacumbas", c, 12f, Lado.N | Lado.S);
            Zona("Catacumbas", c, 12f, new Vector3(-5.2f, 6.8f, 5.4f), 58f);
            LuzSala(c, new Color(0.4f, 1f, 0.7f), 0.6f);

            Sarcofago(-3.8f, 13f); Sarcofago(3.8f, 13f); Sarcofago(-3.8f, 17.5f); Sarcofago(3.8f, 17.5f);
            Pilar(-2.2f, 19.5f); Pilar(2.2f, 19.5f);
            Tocha(new Vector3(-5.7f, 2.2f, 12f)); Tocha(new Vector3(5.7f, 2.2f, 12f)); Tocha(new Vector3(-5.7f, 2.2f, 19f)); Tocha(new Vector3(5.7f, 2.2f, 19f));

            Pedestal(-4.8f, 20.4f);
            Item(TipoColetavel.Chave, new Vector3(-4.8f, 1.25f, 20.4f), "sigilo_terra", "", "");
            Pedestal(4.8f, 20.4f);
            Item(TipoColetavel.Nota, new Vector3(4.8f, 1.1f, 20.4f), "", "Pergaminho Carbonizado", NotaCatacumbas);

            Essencia(-5f, 11.5f); Essencia(5f, 11.5f); Essencia(0f, 12f);
            Erva(5.2f, 15f);

            Mob(TipoInimigo.Infernal, -2.5f, 0.05f, 19.5f, 180f);
            Mob(TipoInimigo.Infernal, 2.5f, 0.05f, 18.5f, 180f);
            Mob(TipoInimigo.Espectro, 0f, 0.05f, 15.5f, 180f);
        }

        // ---- Santuário (0,0,34), 16x16: arena do chefe
        static void Santuario()
        {
            Vector3 c = new Vector3(0f, 0f, 34f);
            Sala("Santuario", c, 16f, Lado.S);
            Zona("Santuario", c, 16f, new Vector3(-7f, 8.8f, -7.2f), 60f);
            LuzSala(c, new Color(0.75f, 0.35f, 1f), 1.0f);

            // plataforma e glifo
            ProceduralModels.Prim(PrimitiveType.Cylinder, tProps, "Plataforma", new Vector3(0f, 0.125f, 39f), new Vector3(7f, 0.125f, 7f), "Obsidiana", true);
            ProceduralModels.Prim(PrimitiveType.Cylinder, tProps, "Glifo", new Vector3(0f, 0.262f, 39f), new Vector3(5.4f, 0.005f, 5.4f), "EmissaoRoxa");

            Pilar(-5.5f, 32f); Pilar(5.5f, 32f); Pilar(-5.5f, 38f); Pilar(5.5f, 38f);
            Braseiro(-5.5f, 35f); Braseiro(5.5f, 35f);
            Tocha(new Vector3(-7.7f, 2.2f, 30f)); Tocha(new Vector3(7.7f, 2.2f, 30f));

            Pedestal(-7f, 27.2f);
            Item(TipoColetavel.Nota, new Vector3(-7f, 1.1f, 27.2f), "", "Última Página", NotaSantuario);

            Essencia(-3.5f, 28.5f); Essencia(3.5f, 28.5f);
            Erva(-6.5f, 28.8f); Erva(6.5f, 28.8f);

            Mob(TipoInimigo.Arconte, 0f, 0.28f, 39f, 180f);
        }
    }
}
