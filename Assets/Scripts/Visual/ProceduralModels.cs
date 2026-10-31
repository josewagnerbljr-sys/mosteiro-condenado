using System.Collections.Generic;
using UnityEngine;

namespace MongeArcano
{
    /// <summary>
    /// Fábrica de modelos 3D feitos só com primitivas + materiais (Resources/Materiais).
    /// Resultado: projeto leve, 100% original (nenhum asset de terceiros) e fácil de trocar por modelos próprios depois.
    /// </summary>
    public static class ProceduralModels
    {
        static readonly Dictionary<string, Material> cache = new Dictionary<string, Material>();

        public static Material Mat(string nome)
        {
            Material m;
            if (cache.TryGetValue(nome, out m) && m != null) return m;
            m = Resources.Load<Material>("Materiais/" + nome);
            if (m == null)
            {
                Shader sh = Shader.Find("Standard");
                if (sh == null) sh = Shader.Find("Sprites/Default");
                m = new Material(sh);
                m.name = nome + " (reserva)";
                m.color = nome.StartsWith("Emissao") ? Efeitos.CorDe(nome) : Color.gray;
            }
            cache[nome] = m;
            return m;
        }

        public static void Remover(Object o)
        {
            if (o == null) return;
            if (Application.isPlaying) Object.Destroy(o);
            else Object.DestroyImmediate(o);
        }

        public static GameObject Prim(PrimitiveType tipo, Transform pai, string nome, Vector3 pos, Vector3 escala, string mat, bool colisor = false, Vector3? rot = null)
        {
            GameObject go = GameObject.CreatePrimitive(tipo);
            go.name = nome;
            go.transform.SetParent(pai, false);
            go.transform.localPosition = pos;
            go.transform.localScale = escala;
            if (rot.HasValue) go.transform.localRotation = Quaternion.Euler(rot.Value);
            Renderer r = go.GetComponent<Renderer>();
            r.sharedMaterial = Mat(mat);
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            Collider col = go.GetComponent<Collider>();
            if (!colisor && col != null)
            {
                col.enabled = false;
                Remover(col);
            }
            return go;
        }

        public static Light Luz(Transform pai, string nome, Vector3 pos, Color cor, float intensidade, float alcance, bool tremular)
        {
            GameObject go = new GameObject(nome);
            go.transform.SetParent(pai, false);
            go.transform.localPosition = pos;
            Light l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = cor;
            l.intensity = intensidade;
            l.range = alcance;
            l.shadows = LightShadows.None;
            if (tremular) go.AddComponent<Flicker>();
            return l;
        }

        // ============================================================ JOGADOR
        public static PlayerController CriarJogador(Vector3 pos, float rotY)
        {
            GameObject raiz = new GameObject("Jogador (Monge Elian)");
            raiz.tag = "Player";
            raiz.transform.position = pos;
            raiz.transform.rotation = Quaternion.Euler(0f, rotY, 0f);

            CharacterController cc = raiz.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.38f;
            cc.center = new Vector3(0f, 0.9f, 0f);
            cc.stepOffset = 0.3f;
            cc.skinWidth = 0.05f;

            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(raiz.transform, false);
            Transform v = visual.transform;

            GameObject malhaMonge = Modelos.Instanciar("monge_clerigo", v, 1.8f, 0f);
            if (malhaMonge == null)
            {
            Prim(PrimitiveType.Cylinder, v, "Manto", new Vector3(0f, 0.62f, 0f), new Vector3(0.78f, 0.62f, 0.78f), "Tecido");
            Prim(PrimitiveType.Cylinder, v, "Faixa", new Vector3(0f, 0.95f, 0f), new Vector3(0.66f, 0.07f, 0.66f), "TecidoEscuro");
            Prim(PrimitiveType.Sphere, v, "Peito", new Vector3(0f, 1.22f, 0f), new Vector3(0.62f, 0.42f, 0.46f), "Tecido");
            Prim(PrimitiveType.Sphere, v, "Cabeca", new Vector3(0f, 1.58f, 0f), new Vector3(0.34f, 0.36f, 0.34f), "Pele");
            Prim(PrimitiveType.Sphere, v, "TerceiroOlho", new Vector3(0f, 1.63f, 0.16f), Vector3.one * 0.06f, "EmissaoAzul");
            Prim(PrimitiveType.Capsule, v, "BracoE", new Vector3(-0.36f, 1.12f, 0.08f), new Vector3(0.15f, 0.28f, 0.15f), "Tecido", false, new Vector3(0f, 0f, 10f));
            Prim(PrimitiveType.Capsule, v, "BracoD", new Vector3(0.38f, 1.1f, 0.12f), new Vector3(0.15f, 0.28f, 0.15f), "Tecido", false, new Vector3(25f, 0f, -8f));
            for (int i = 0; i < 6; i++)
            {
                float a = i * Mathf.PI / 5f - Mathf.PI / 2.5f;
                Prim(PrimitiveType.Sphere, v, "Conta" + i, new Vector3(Mathf.Sin(a) * 0.17f, 1.36f - Mathf.Abs(Mathf.Sin(a)) * 0.05f, 0.2f), Vector3.one * 0.045f, "Ouro");
            }
            }

            GameObject pivo = new GameObject("PivoCajado");
            pivo.transform.SetParent(raiz.transform, false);
            pivo.transform.localPosition = new Vector3(0.42f, 1.05f, 0.2f);
            if (malhaMonge == null)
            {
                Prim(PrimitiveType.Cylinder, pivo.transform, "Cajado", new Vector3(0f, 0.1f, 0f), new Vector3(0.05f, 0.8f, 0.05f), "Madeira");
                Prim(PrimitiveType.Sphere, pivo.transform, "Orbe", new Vector3(0f, 0.95f, 0f), Vector3.one * 0.2f, "EmissaoAzul");
            }
            Luz(pivo.transform, "LuzOrbe", new Vector3(0f, 0.95f, 0f), new Color(0.35f, 0.7f, 1f), 1.6f, 7f, true);

            GameObject ponto = new GameObject("PontoDisparo");
            ponto.transform.SetParent(raiz.transform, false);
            ponto.transform.localPosition = new Vector3(0.1f, 1.35f, 0.6f);

            Luz(raiz.transform, "LuzJogador", new Vector3(0f, 1.8f, -0.2f), new Color(0.75f, 0.85f, 1f), 0.9f, 7f, false);

            GameObject escudo = Prim(PrimitiveType.Sphere, raiz.transform, "EscudoMistico", new Vector3(0f, 1f, 0f), Vector3.one * 2.3f, "Fantasma");
            FxMagia.EscudoHex(escudo, new Color(0.45f, 0.7f, 1f, 0.9f));
            escudo.SetActive(false);
            AnimadorProcedural an = raiz.AddComponent<AnimadorProcedural>();
            an.corpo = v;
            an.frequencia = 9f;
            an.inclinacao = 7f;
            an.balancoCorpo = 0.06f;
            an.rolagem = 2f;
            an.passos = true;

            UalAnimador ualJ = UalAnimador.Montar(raiz, malhaMonge, UalAnimador.Perfil.Monge);
            if (ualJ != null) an.ual = ualJ;

            PlayerController pc = raiz.AddComponent<PlayerController>();
            pc.escudoVisual = escudo;
            pc.anim = an;
            pc.visual = v;
            pc.pivoCajado = pivo.transform;
            pc.pontoDisparo = ponto.transform;
            return pc;
        }

        // ============================================================ INIMIGOS
        public static Inimigo CriarInimigo(TipoInimigo tipo, Vector3 pos, float rotY)
        {
            GameObject go = new GameObject(tipo.ToString());
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(0f, rotY, 0f);
            Transform t = go.transform;

            Rigidbody rb = go.AddComponent<Rigidbody>();
            rb.mass = 1f;
            CapsuleCollider cap = go.AddComponent<CapsuleCollider>();
            Inimigo ini = go.AddComponent<Inimigo>();
            ini.tipo = tipo;

            GameObject modeloGo = new GameObject("Modelo");
            modeloGo.transform.SetParent(t, false);
            Transform m = modeloGo.transform;
            AnimadorProcedural an = go.AddComponent<AnimadorProcedural>();
            an.corpo = m;
            ini.anim = an;

            GameObject mira = new GameObject("PontoMira");
            mira.transform.SetParent(t, false);
            ini.pontoMira = mira.transform;

            // modelo 3D (nome, altura em metros, raio do colisor, pintura opcional)
            string nomeModelo = "aparicao"; float altura = 2f, raio = 0.45f; string pintura = null;
            switch (tipo)
            {
                case TipoInimigo.Possuido: nomeModelo = "aparicao"; altura = 2.0f; raio = 0.45f; break;
                case TipoInimigo.Espectro: nomeModelo = "espectro"; altura = 2.0f; raio = 0.4f; break;
                case TipoInimigo.Infernal: nomeModelo = "demonio"; altura = 2.6f; raio = 0.55f; break;
                case TipoInimigo.Arconte: nomeModelo = "boss_morte"; altura = 3.6f; raio = 0.8f; break;
                case TipoInimigo.Esqueleto: nomeModelo = "esqueleto_guerreiro"; altura = 1.8f; raio = 0.4f; break;
                case TipoInimigo.Armadura: nomeModelo = "armadura_assombrada"; altura = 2.1f; raio = 0.5f; break;
                case TipoInimigo.BruxaVermelha: nomeModelo = "bruxa"; altura = 1.9f; raio = 0.42f; pintura = "BruxaVermelha"; break;
                case TipoInimigo.BruxaNegra: nomeModelo = "bruxa"; altura = 1.8f; raio = 0.42f; pintura = "BruxaNegra"; break;
                case TipoInimigo.BruxaCinza: nomeModelo = "bruxa"; altura = 1.8f; raio = 0.42f; pintura = "BruxaCinza"; break;
                case TipoInimigo.Monstro: nomeModelo = "monstro"; altura = 3.4f; raio = 0.8f; break;
            }

            GameObject malha = Modelos.Instanciar(nomeModelo, m, altura, 0f, pintura);
            UalAnimador ual = UalAnimador.Montar(go, malha, PerfilUal(tipo));
            if (ual != null) an.ual = ual;
            if (malha == null)
            {
                // sem o modelo importado: usa as formas simples do protótipo
                altura = 1.9f; raio = 0.4f;
                switch (tipo)
                {
                    case TipoInimigo.Espectro: ConstruirEspectro(m); break;
                    case TipoInimigo.Infernal: ConstruirInfernal(m); go.transform.localScale = Vector3.one * 1.25f; break;
                    case TipoInimigo.Arconte: ConstruirArconte(m); go.transform.localScale = Vector3.one * 1.6f; break;
                    default: ConstruirPossuido(m); break;
                }
            }
            cap.height = altura;
            cap.radius = raio;
            cap.center = new Vector3(0f, altura * 0.5f, 0f);
            mira.transform.localPosition = new Vector3(0f, altura * 0.62f, 0f);

            switch (tipo)
            {
                case TipoInimigo.Possuido:
                    ini.nomeExibido = "Possuído"; ini.vidaMax = 60f; ini.velocidade = 1.5f; ini.dano = 15f; ini.alcanceAtaque = 1.5f;
                    ini.deteccao = 8f; ini.intervaloAtaque = 1.4f; ini.chanceDrop = 0.35f; ini.xp = 18f;
                    an.frequencia = 4f; an.inclinacao = 10f; an.balancoCorpo = 0.05f;
                    break;
                case TipoInimigo.Espectro:
                    ini.nomeExibido = "Espectro"; ini.vidaMax = 40f; ini.velocidade = 3.3f; ini.dano = 9f; ini.alcanceAtaque = 1.4f;
                    ini.deteccao = 9f; ini.intervaloAtaque = 1.0f; ini.chanceDrop = 0.3f; ini.xp = 14f;
                    ini.flutua = true; ini.alturaFlutuar = 0.7f;
                    an.frequencia = 3f; an.rolagem = 5f; an.oscilacaoOciosa = 0.08f;
                    break;
                case TipoInimigo.Infernal:
                    ini.nomeExibido = "Criatura Infernal"; ini.vidaMax = 120f; ini.velocidade = 2.4f; ini.dano = 22f; ini.alcanceAtaque = 1.9f;
                    ini.deteccao = 10f; ini.intervaloAtaque = 1.3f; ini.chanceDrop = 0.6f; ini.xp = 45f;
                    an.frequencia = 6f; an.inclinacao = 8f; an.balancoCorpo = 0.06f;
                    break;
                case TipoInimigo.Arconte:
                    ini.nomeExibido = "Arconte das Cinzas"; ini.vidaMax = 400f; ini.velocidade = 1.9f; ini.dano = 30f; ini.alcanceAtaque = 2.8f;
                    ini.deteccao = 14f; ini.intervaloAtaque = 1.6f; ini.chanceDrop = 0f; ini.xp = 300f;
                    ini.chefe = true; ini.ataqueADistancia = true; ini.danoDistancia = 18f; ini.intervaloDisparo = 3f;
                    mira.transform.localPosition = new Vector3(0f, altura * 0.55f, 0.4f);
                    an.frequencia = 2.2f; an.rolagem = 3f; an.oscilacaoOciosa = 0.08f;
                    break;
                case TipoInimigo.Esqueleto:
                    ini.nomeExibido = "Esqueleto Guerreiro"; ini.vidaMax = 70f; ini.velocidade = 2.3f; ini.dano = 14f; ini.alcanceAtaque = 1.6f;
                    ini.deteccao = 9f; ini.intervaloAtaque = 1.2f; ini.chanceDrop = 0.3f; ini.xp = 20f;
                    an.frequencia = 7f; an.inclinacao = 8f; an.balancoCorpo = 0.06f;
                    break;
                case TipoInimigo.Armadura:
                    ini.nomeExibido = "Armadura Assombrada"; ini.vidaMax = 150f; ini.velocidade = 1.7f; ini.dano = 24f; ini.alcanceAtaque = 1.8f;
                    ini.deteccao = 3.6f; ini.intervaloAtaque = 1.5f; ini.chanceDrop = 0.5f; ini.xp = 50f;
                    an.frequencia = 4.5f; an.inclinacao = 6f; an.balancoCorpo = 0.04f;
                    break;
                case TipoInimigo.BruxaVermelha:
                    ini.nomeExibido = "Bruxa Mestra"; ini.vidaMax = 170f; ini.velocidade = 1.4f; ini.dano = 12f; ini.alcanceAtaque = 1.6f;
                    ini.deteccao = 13f; ini.intervaloAtaque = 1.6f; ini.chanceDrop = 0.7f; ini.xp = 90f;
                    ini.ataqueADistancia = true; ini.danoDistancia = 16f; ini.intervaloDisparo = 2.2f;
                    ini.invocadora = true; ini.intervaloInvocacao = 14f;
                    an.frequencia = 5f; an.oscilacaoOciosa = 0.03f;
                    break;
                case TipoInimigo.BruxaNegra:
                    ini.nomeExibido = "Bruxa Invocadora"; ini.vidaMax = 110f; ini.velocidade = 1.5f; ini.dano = 10f; ini.alcanceAtaque = 1.6f;
                    ini.deteccao = 12f; ini.intervaloAtaque = 1.6f; ini.chanceDrop = 0.6f; ini.xp = 60f;
                    ini.invocadora = true; ini.intervaloInvocacao = 11f;
                    an.frequencia = 5f; an.oscilacaoOciosa = 0.03f;
                    break;
                case TipoInimigo.BruxaCinza:
                    ini.nomeExibido = "Bruxa Lançadora"; ini.vidaMax = 95f; ini.velocidade = 1.9f; ini.dano = 10f; ini.alcanceAtaque = 1.6f;
                    ini.deteccao = 12f; ini.intervaloAtaque = 1.4f; ini.chanceDrop = 0.5f; ini.xp = 50f;
                    ini.ataqueADistancia = true; ini.danoDistancia = 10f; ini.intervaloDisparo = 1.3f;
                    an.frequencia = 5.5f; an.oscilacaoOciosa = 0.03f;
                    break;
                default:
                    ini.nomeExibido = "Monstro"; ini.vidaMax = 280f; ini.velocidade = 2.0f; ini.dano = 34f; ini.alcanceAtaque = 2.6f;
                    ini.deteccao = 10f; ini.intervaloAtaque = 1.6f; ini.chanceDrop = 0.8f; ini.xp = 100f;
                    an.frequencia = 3.5f; an.inclinacao = 5f; an.balancoCorpo = 0.08f;
                    break;
            }
            if (ini.xp > 40f) ini.chanceItem = 0.5f;
            if (tipo == TipoInimigo.BruxaCinza || tipo == TipoInimigo.BruxaNegra || tipo == TipoInimigo.BruxaVermelha) ini.chanceItem = 0.55f;
            ini.vida = ini.vidaMax;
            return ini;
        }

        static UalAnimador.Perfil PerfilUal(TipoInimigo tipo)
        {
            switch (tipo)
            {
                case TipoInimigo.Possuido: return UalAnimador.Perfil.Zumbi;
                case TipoInimigo.Infernal: return UalAnimador.Perfil.Besta;
                case TipoInimigo.Monstro: return UalAnimador.Perfil.Besta;
                case TipoInimigo.Arconte: return UalAnimador.Perfil.Chefe;
                case TipoInimigo.BruxaVermelha:
                case TipoInimigo.BruxaNegra:
                case TipoInimigo.BruxaCinza: return UalAnimador.Perfil.Bruxa;
                default: return UalAnimador.Perfil.Guerreiro;
            }
        }

        static void ConstruirPossuido(Transform t)
        {
            Prim(PrimitiveType.Capsule, t, "PernaE", new Vector3(-0.14f, 0.4f, 0f), new Vector3(0.2f, 0.4f, 0.2f), "TecidoEscuro");
            Prim(PrimitiveType.Capsule, t, "PernaD", new Vector3(0.14f, 0.4f, 0f), new Vector3(0.2f, 0.4f, 0.2f), "TecidoEscuro");
            Prim(PrimitiveType.Capsule, t, "Torso", new Vector3(0f, 1.2f, 0f), new Vector3(0.55f, 0.42f, 0.38f), "CarnePodre");
            Prim(PrimitiveType.Sphere, t, "Cabeca", new Vector3(0f, 1.7f, 0.06f), new Vector3(0.3f, 0.32f, 0.3f), "CarnePodre", false, new Vector3(20f, 0f, 12f));
            Prim(PrimitiveType.Sphere, t, "OlhoE", new Vector3(-0.08f, 1.72f, 0.2f), Vector3.one * 0.07f, "EmissaoVermelha");
            Prim(PrimitiveType.Sphere, t, "OlhoD", new Vector3(0.08f, 1.72f, 0.2f), Vector3.one * 0.07f, "EmissaoVermelha");
            Prim(PrimitiveType.Capsule, t, "BracoE", new Vector3(-0.34f, 1.3f, 0.36f), new Vector3(0.13f, 0.35f, 0.13f), "CarnePodre", false, new Vector3(80f, 0f, 0f));
            Prim(PrimitiveType.Capsule, t, "BracoD", new Vector3(0.34f, 1.3f, 0.36f), new Vector3(0.13f, 0.35f, 0.13f), "CarnePodre", false, new Vector3(80f, 0f, 0f));
            Prim(PrimitiveType.Cube, t, "Trapo", new Vector3(0f, 0.85f, 0.2f), new Vector3(0.5f, 0.5f, 0.05f), "TecidoEscuro");
        }

        static void ConstruirEspectro(Transform t)
        {
            Prim(PrimitiveType.Capsule, t, "Corpo", new Vector3(0f, 1.0f, 0f), new Vector3(0.55f, 0.75f, 0.55f), "Fantasma");
            Prim(PrimitiveType.Capsule, t, "Cauda", new Vector3(0f, 0.45f, -0.05f), new Vector3(0.35f, 0.5f, 0.35f), "Fantasma");
            Prim(PrimitiveType.Sphere, t, "Cabeca", new Vector3(0f, 1.7f, 0f), Vector3.one * 0.45f, "Fantasma");
            Prim(PrimitiveType.Sphere, t, "OlhoE", new Vector3(-0.1f, 1.74f, 0.2f), Vector3.one * 0.09f, "EmissaoVerde");
            Prim(PrimitiveType.Sphere, t, "OlhoD", new Vector3(0.1f, 1.74f, 0.2f), Vector3.one * 0.09f, "EmissaoVerde");
            Prim(PrimitiveType.Capsule, t, "Boca", new Vector3(0f, 1.58f, 0.2f), new Vector3(0.1f, 0.12f, 0.05f), "Obsidiana");
            Prim(PrimitiveType.Capsule, t, "BracoE", new Vector3(-0.5f, 1.2f, 0.1f), new Vector3(0.1f, 0.45f, 0.1f), "Fantasma", false, new Vector3(0f, 0f, 30f));
            Prim(PrimitiveType.Capsule, t, "BracoD", new Vector3(0.5f, 1.2f, 0.1f), new Vector3(0.1f, 0.45f, 0.1f), "Fantasma", false, new Vector3(0f, 0f, -30f));
            Luz(t, "Luz", new Vector3(0f, 1.3f, 0f), new Color(0.4f, 1f, 0.8f), 1.1f, 5f, true);
        }

        static void ConstruirInfernal(Transform t)
        {
            Prim(PrimitiveType.Capsule, t, "PernaE", new Vector3(-0.2f, 0.45f, 0f), new Vector3(0.25f, 0.45f, 0.25f), "PeleInfernal");
            Prim(PrimitiveType.Capsule, t, "PernaD", new Vector3(0.2f, 0.45f, 0f), new Vector3(0.25f, 0.45f, 0.25f), "PeleInfernal");
            Prim(PrimitiveType.Capsule, t, "Torso", new Vector3(0f, 1.2f, 0f), new Vector3(0.8f, 0.65f, 0.6f), "PeleInfernal");
            Prim(PrimitiveType.Sphere, t, "Cabeca", new Vector3(0f, 1.95f, 0.1f), new Vector3(0.4f, 0.38f, 0.4f), "PeleInfernal");
            Prim(PrimitiveType.Capsule, t, "ChifreE", new Vector3(-0.2f, 2.28f, 0.05f), new Vector3(0.07f, 0.22f, 0.07f), "Osso", false, new Vector3(0f, 0f, 25f));
            Prim(PrimitiveType.Capsule, t, "ChifreD", new Vector3(0.2f, 2.28f, 0.05f), new Vector3(0.07f, 0.22f, 0.07f), "Osso", false, new Vector3(0f, 0f, -25f));
            Prim(PrimitiveType.Sphere, t, "OlhoE", new Vector3(-0.12f, 1.98f, 0.3f), Vector3.one * 0.08f, "EmissaoLaranja");
            Prim(PrimitiveType.Sphere, t, "OlhoD", new Vector3(0.12f, 1.98f, 0.3f), Vector3.one * 0.08f, "EmissaoLaranja");
            Prim(PrimitiveType.Capsule, t, "BracoE", new Vector3(-0.6f, 1.15f, 0.15f), new Vector3(0.2f, 0.55f, 0.2f), "PeleInfernal", false, new Vector3(-20f, 0f, 10f));
            Prim(PrimitiveType.Capsule, t, "BracoD", new Vector3(0.6f, 1.15f, 0.15f), new Vector3(0.2f, 0.55f, 0.2f), "PeleInfernal", false, new Vector3(-20f, 0f, -10f));
            Prim(PrimitiveType.Capsule, t, "GarraE", new Vector3(-0.66f, 0.55f, 0.3f), new Vector3(0.08f, 0.16f, 0.08f), "Osso", false, new Vector3(-20f, 0f, 0f));
            Prim(PrimitiveType.Capsule, t, "GarraD", new Vector3(0.66f, 0.55f, 0.3f), new Vector3(0.08f, 0.16f, 0.08f), "Osso", false, new Vector3(-20f, 0f, 0f));
            Prim(PrimitiveType.Sphere, t, "Chama", new Vector3(0f, 1.45f, -0.42f), Vector3.one * 0.3f, "EmissaoLaranja");
            Luz(t, "Luz", new Vector3(0f, 1.5f, -0.4f), new Color(1f, 0.45f, 0.15f), 1.3f, 6f, true);
        }

        static void ConstruirArconte(Transform t)
        {
            Prim(PrimitiveType.Cylinder, t, "Manto", new Vector3(0f, 0.9f, 0f), new Vector3(1.1f, 0.9f, 1.0f), "Obsidiana");
            Prim(PrimitiveType.Sphere, t, "Torso", new Vector3(0f, 1.85f, 0f), new Vector3(0.9f, 0.8f, 0.7f), "Obsidiana");
            Prim(PrimitiveType.Sphere, t, "Cranio", new Vector3(0f, 2.5f, 0.05f), Vector3.one * 0.5f, "Osso");
            Prim(PrimitiveType.Sphere, t, "OlhoE", new Vector3(-0.13f, 2.55f, 0.3f), Vector3.one * 0.11f, "EmissaoVermelha");
            Prim(PrimitiveType.Sphere, t, "OlhoD", new Vector3(0.13f, 2.55f, 0.3f), Vector3.one * 0.11f, "EmissaoVermelha");
            Prim(PrimitiveType.Sphere, t, "OlhoCentral", new Vector3(0f, 2.68f, 0.34f), Vector3.one * 0.09f, "EmissaoRoxa");
            for (int i = 0; i < 6; i++)
            {
                float a = i * Mathf.PI * 2f / 6f;
                Vector3 fora = new Vector3(Mathf.Sin(a), 0f, Mathf.Cos(a));
                Vector3 pos = new Vector3(fora.x * 0.3f, 2.85f, fora.z * 0.3f);
                Vector3 eul = Quaternion.FromToRotation(Vector3.up, (fora * 0.6f + Vector3.up).normalized).eulerAngles;
                Prim(PrimitiveType.Capsule, t, "Chifre" + i, pos, new Vector3(0.09f, 0.3f, 0.09f), "Osso", false, eul);
            }
            Prim(PrimitiveType.Sphere, t, "Coracao", new Vector3(0f, 1.85f, 0.32f), Vector3.one * 0.32f, "EmissaoRoxa");
            Prim(PrimitiveType.Capsule, t, "BracoE", new Vector3(-0.85f, 1.7f, 0.2f), new Vector3(0.22f, 0.7f, 0.22f), "Obsidiana", false, new Vector3(-30f, 0f, 25f));
            Prim(PrimitiveType.Capsule, t, "BracoD", new Vector3(0.85f, 1.7f, 0.2f), new Vector3(0.22f, 0.7f, 0.22f), "Obsidiana", false, new Vector3(-30f, 0f, -25f));
            Prim(PrimitiveType.Sphere, t, "MaoE", new Vector3(-1.2f, 1.05f, 0.55f), Vector3.one * 0.3f, "EmissaoVermelha");
            Prim(PrimitiveType.Sphere, t, "MaoD", new Vector3(1.2f, 1.05f, 0.55f), Vector3.one * 0.3f, "EmissaoVermelha");
            Luz(t, "LuzCoracao", new Vector3(0f, 1.9f, 0.5f), new Color(0.7f, 0.25f, 1f), 2.2f, 12f, true);
            Luz(t, "LuzMaos", new Vector3(0f, 1.1f, 0.8f), new Color(1f, 0.2f, 0.1f), 1.2f, 9f, true);
        }

        // ============================================================ COLETÁVEIS
        public static Coletavel CriarColetavel(TipoColetavel tipo, Vector3 pos, string chaveId = "", string titulo = "", string texto = "")
        {
            GameObject go = new GameObject("Item_" + tipo + (chaveId.Length > 0 ? "_" + chaveId : ""));
            go.transform.position = pos;
            Transform t = go.transform;

            SphereCollider sc = go.AddComponent<SphereCollider>();
            sc.isTrigger = true;
            sc.radius = 0.8f;

            Coletavel c = go.AddComponent<Coletavel>();
            c.tipo = tipo;
            c.chaveId = chaveId;
            c.titulo = titulo;
            c.texto = texto;

            switch (tipo)
            {
                case TipoColetavel.Essencia:
                    Prim(PrimitiveType.Sphere, t, "Essencia", Vector3.zero, new Vector3(0.26f, 0.34f, 0.26f), "EmissaoAzul");
                    Luz(t, "Luz", Vector3.zero, new Color(0.3f, 0.65f, 1f), 0.9f, 3.5f, false);
                    c.valor = 5f;
                    c.automatico = true;
                    break;

                case TipoColetavel.Erva:
                    Prim(PrimitiveType.Sphere, t, "Folha1", Vector3.zero, new Vector3(0.4f, 0.1f, 0.22f), "EmissaoVerde", false, new Vector3(0f, 0f, 20f));
                    Prim(PrimitiveType.Sphere, t, "Folha2", Vector3.zero, new Vector3(0.4f, 0.1f, 0.22f), "EmissaoVerde", false, new Vector3(0f, 60f, -20f));
                    Prim(PrimitiveType.Sphere, t, "Folha3", Vector3.zero, new Vector3(0.4f, 0.1f, 0.22f), "EmissaoVerde", false, new Vector3(0f, 120f, 20f));
                    Luz(t, "Luz", Vector3.zero, new Color(0.3f, 1f, 0.4f), 0.7f, 3f, false);
                    c.automatico = true;
                    break;

                case TipoColetavel.Frasco:
                    Prim(PrimitiveType.Sphere, t, "Corpo", new Vector3(0f, -0.05f, 0f), new Vector3(0.3f, 0.34f, 0.3f), "EmissaoAzul");
                    Prim(PrimitiveType.Cylinder, t, "Gargalo", new Vector3(0f, 0.17f, 0f), new Vector3(0.1f, 0.08f, 0.1f), "Osso");
                    Luz(t, "Luz", Vector3.zero, new Color(0.3f, 0.65f, 1f), 0.9f, 3.5f, false);
                    c.valor = 12f;
                    c.automatico = true;
                    break;

                case TipoColetavel.Chave:
                    ConstruirChave(t, chaveId);
                    c.automatico = false;
                    break;

                default:
                    Prim(PrimitiveType.Cube, t, "Papel", Vector3.zero, new Vector3(0.36f, 0.03f, 0.46f), "Papel", false, new Vector3(0f, 0f, 0f));
                    Prim(PrimitiveType.Cube, t, "Selo", new Vector3(0f, 0.03f, 0.1f), new Vector3(0.1f, 0.03f, 0.1f), "EmissaoVermelha");
                    Luz(t, "Luz", Vector3.up * 0.3f, new Color(1f, 0.9f, 0.6f), 0.7f, 3f, false);
                    c.automatico = false;
                    break;
            }
            return c;
        }

        static void ConstruirChave(Transform t, string id)
        {
            switch (id)
            {
                case "sigilo_fogo": Sigilo(t, "EmissaoLaranja", new Color(1f, 0.5f, 0.15f)); break;
                case "sigilo_lua": Sigilo(t, "EmissaoAzul", new Color(0.4f, 0.7f, 1f)); break;
                case "sigilo_terra": Sigilo(t, "EmissaoVerde", new Color(0.3f, 1f, 0.4f)); break;
                default:
                    string m = id == "osso" ? "Osso" : "PedraClara";
                    Prim(PrimitiveType.Cylinder, t, "Haste", Vector3.zero, new Vector3(0.05f, 0.2f, 0.05f), m, false, new Vector3(0f, 0f, 90f));
                    Prim(PrimitiveType.Sphere, t, "Argola", new Vector3(-0.22f, 0f, 0f), new Vector3(0.18f, 0.18f, 0.05f), m);
                    Prim(PrimitiveType.Cube, t, "Dente1", new Vector3(0.16f, -0.07f, 0f), new Vector3(0.05f, 0.1f, 0.04f), m);
                    Prim(PrimitiveType.Cube, t, "Dente2", new Vector3(0.08f, -0.06f, 0f), new Vector3(0.05f, 0.08f, 0.04f), m);
                    Luz(t, "Luz", Vector3.zero, new Color(1f, 0.95f, 0.8f), 0.8f, 3f, false);
                    break;
            }
        }

        static void Sigilo(Transform t, string matEmissivo, Color luz)
        {
            Prim(PrimitiveType.Cylinder, t, "Disco", Vector3.zero, new Vector3(0.5f, 0.03f, 0.5f), "Obsidiana", false, new Vector3(90f, 0f, 0f));
            Prim(PrimitiveType.Cylinder, t, "Runa", new Vector3(0f, 0f, 0.025f), new Vector3(0.36f, 0.02f, 0.36f), matEmissivo, false, new Vector3(90f, 0f, 0f));
            Prim(PrimitiveType.Cylinder, t, "RunaTras", new Vector3(0f, 0f, -0.025f), new Vector3(0.36f, 0.02f, 0.36f), matEmissivo, false, new Vector3(90f, 0f, 0f));
            Luz(t, "Luz", Vector3.zero, luz, 1.4f, 5f, true);
        }

        // ============================================================ PROJÉTEIS
        public static Projetil CriarProjetil(Vector3 pos, Vector3 dir, bool amigavel, float dano, float vel, Collider ignorar)
        {
            return CriarProjetil(pos, dir, amigavel, dano, vel, ignorar, null);
        }

        public static Projetil CriarProjetil(Vector3 pos, Vector3 dir, bool amigavel, float dano, float vel, Collider ignorar, Inimigo alvo)
        {
            GameObject go = new GameObject(amigavel ? "RaioArcano" : "BolaInfernal");
            go.transform.position = pos;
            string mat = amigavel ? "EmissaoAzul" : "EmissaoVermelha";
            float r = amigavel ? 0.14f : 0.3f;
            Color cf = amigavel ? new Color(0.45f, 0.6f, 1f) : new Color(1f, 0.35f, 0.12f);
            FxMagia.Nucleo(go.transform, cf, r * (amigavel ? 8f : 6.5f));
            Fx.Rastro(go, cf, r * 1.7f);
            Fx.FaiscasContinuas(go.transform, cf);

            SphereCollider sc = go.AddComponent<SphereCollider>();
            sc.radius = r;
            Rigidbody rb = go.AddComponent<Rigidbody>();
            rb.mass = 0.2f;
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            Luz(go.transform, "Luz", Vector3.zero, amigavel ? new Color(0.3f, 0.7f, 1f) : new Color(1f, 0.2f, 0.1f), 1.6f, 6f, false);

            Projetil p = go.AddComponent<Projetil>();
            p.amigavel = amigavel;
            p.dano = dano;
            p.velocidade = vel;
            p.efeitoMaterial = mat;
            p.alvo = alvo;
            if (ignorar != null) Physics.IgnoreCollision(sc, ignorar);
            p.Lancar(dir);
            return p;
        }
    }
}
