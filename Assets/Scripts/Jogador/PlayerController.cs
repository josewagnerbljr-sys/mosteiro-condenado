using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MongeArcano
{
    /// <summary>
    /// Monge arcano. Movimento direto (relativo à câmera, com giro suave) ou de tanque (tecla T).
    /// Magias: ESPAÇO Raio, F Nova, G Leque, B Escudo, R Invocação Celeste (precisa do Poder Arcano cheio).
    /// C coleta, V cura, X cajado, 1-6 atalhos de itens, Shift corre. Tem XP e níveis.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour, IDanificavel
    {
        public static readonly string[] NomesMagias = { "Raio Arcano", "Nova Sagrada", "Leque Arcano", "Escudo Místico", "Invocação Celeste" };
        public static readonly string[] TeclasMagias = { "ESPAÇO", "F", "G", "B", "R" };
        public readonly float[] custoMagia = { 1f, 5f, 3f, 4f, 0f };
        public readonly float[] recargaMagia = { 0.42f, 2f, 1.2f, 8f, 1f };
        readonly float[] proxMagia = new float[5];

        [Header("Atributos")]
        public float vidaMax = 100f;
        public float vida = 100f;
        public float manaMax = 30f;
        public float mana = 16f;
        public int ervas = 1;
        public int frascos = 1;
        public int nivel = 1;
        public float xp;
        public float poder;
        public float poderMax = 100f;
        public float bonusDano;

        [Header("Progressão (pontos, inventário e equipamento)")]
        public int forca, destreza, inteligencia, sabedoria, espectro, pontosLivres;
        public int[] nivelMagia = { 1, 1, 1, 1, 1 };
        public List<Item> inventario = new List<Item>();
        public Item armaEq, armaduraEq, amuletoEq;
        public const int Capacidade = 20;
        public const int NivelMax = 50;

        [Header("Movimento")]
        public float velAndar = 1.7f;
        public float velCorrer = 5f;
        public float velRe = 2f;
        public float velGiro = 135f;
        public bool modoTanque;

        [Header("Referências (preenchidas pelo construtor)")]
        public Transform visual;
        public Transform pivoCajado;
        public Transform pontoDisparo;
        public GameObject escudoVisual;
        public AnimadorProcedural anim;

        CharacterController cc;
        float velY, giroRapidoRestante, velAtual, yawEntrada;
        float travadoAte, proxGolpe, invulAte, proxZona, escudoAte;
        float golpeInicio = -10f;
        Vector3 empurrao, ultimaDir = Vector3.forward;
        bool morto;
        Interagivel alvoInteracao;

        static readonly KeyCode[] teclasSlot = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6 };

        public bool Morto { get { return morto; } }
        public float FracaoVida { get { return vida / vidaMax; } }
        public float XpProximo { get { return 60f + (nivel - 1) * 45f; } }
        public float DanoMult { get { return 1f + (nivel - 1) * 0.08f + 0.02f * TotInt + bonusDano + (armaEq != null ? armaEq.bonusDano : 0f); } }
        public int TotInt { get { return inteligencia + (armaEq != null ? armaEq.bInt : 0) + (armaduraEq != null ? armaduraEq.bInt : 0) + (amuletoEq != null ? amuletoEq.bInt : 0); } }
        public int TotWis { get { return sabedoria + (armaEq != null ? armaEq.bWis : 0) + (armaduraEq != null ? armaduraEq.bWis : 0) + (amuletoEq != null ? amuletoEq.bWis : 0); } }
        public int TotStr { get { return forca + (armaEq != null ? armaEq.bStr : 0) + (armaduraEq != null ? armaduraEq.bStr : 0) + (amuletoEq != null ? amuletoEq.bStr : 0); } }
        public int TotDex { get { return destreza + (armaEq != null ? armaEq.bDex : 0) + (armaduraEq != null ? armaduraEq.bDex : 0) + (amuletoEq != null ? amuletoEq.bDex : 0); } }
        public int TotEsp { get { return espectro + (armaEq != null ? armaEq.bEsp : 0) + (armaduraEq != null ? armaduraEq.bEsp : 0) + (amuletoEq != null ? amuletoEq.bEsp : 0); } }
        public int SlotsUsados { get { int t = 0; for (int i = 0; i < inventario.Count; i++) t += inventario[i].Slots; return t; } }

        public float DanoMagia(int i, float baseDano) { return baseDano * DanoMult * (1f + 0.3f * (nivelMagia[i] - 1)); }
        public bool PoderCheio { get { return poder >= poderMax; } }
        public bool EscudoAtivo { get { return Time.time < escudoAte; } }

        public float RecargaRestante(int i)
        {
            return Mathf.Clamp01((proxMagia[i] - Time.time) / Mathf.Max(0.01f, recargaMagia[i]));
        }

        void Awake()
        {
            cc = GetComponent<CharacterController>();
            cc.stepOffset = 0.5f;
            RecalcularStatus();
            vida = vidaMax;
            ultimaDir = transform.forward;
        }

        void Update()
        {
            GameManager gm = GameManager.Instance;
            if (gm == null) return;
            if (gm.Estado != EstadoJogo.Jogando) { gm.Dica = ""; return; }
            if (morto) return;

            if (escudoVisual != null && escudoVisual.activeSelf && Time.time >= escudoAte) escudoVisual.SetActive(false);

            if (Input.GetKeyDown(KeyCode.T))
            {
                modoTanque = !modoTanque;
                gm.Mostrar(modoTanque ? "Movimento: TANQUE (A/D giram, W/S andam)" : "Movimento: DIRETO (relativo à câmera)", 2.5f);
            }

            if (Input.GetKeyDown(KeyCode.Z) || Input.GetMouseButtonDown(2)) AlternarTravamento();
            if (alvoTravado != null && (alvoTravado.Morto || (alvoTravado.transform.position - transform.position).sqrMagnitude > 900f)) alvoTravado = null;

            Movimento();

            if (Time.time >= proxZona)
            {
                proxZona = Time.time + 0.15f;
                ZonaCamera.AtivarPara(transform.position);
            }

            ProcurarInteracao(gm);
            if (!gm.InteracaoBloqueada && (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.E)) && alvoInteracao != null) alvoInteracao.Interagir(this);

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) Magia(0);
            if (Input.GetKeyDown(KeyCode.F)) Magia(1);
            if (Input.GetKeyDown(KeyCode.G)) Magia(2);
            if (Input.GetKeyDown(KeyCode.B)) Magia(3);
            if (Input.GetKeyDown(KeyCode.R)) Magia(4);
            if (Input.GetKeyDown(KeyCode.X)) Golpe();
            if (Input.GetKeyDown(KeyCode.V)) UsarErva();
            for (int i = 0; i < teclasSlot.Length; i++) if (Input.GetKeyDown(teclasSlot[i])) UsarSlot(i);
        }

        // ------------------------------------------------------------------ movimento
        void Movimento()
        {
            bool preso = Time.time < travadoAte;
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            bool corre = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            Vector3 mov;

            if (modoTanque)
            {
                if (Input.GetKeyDown(KeyCode.Q) && giroRapidoRestante <= 0f && !preso) giroRapidoRestante = 180f;
                if (giroRapidoRestante > 0f)
                {
                    float passo = Mathf.Min(giroRapidoRestante, 780f * Time.deltaTime);
                    transform.Rotate(0f, passo, 0f);
                    giroRapidoRestante -= passo;
                }
                else transform.Rotate(0f, h * velGiro * Time.deltaTime, 0f);

                float vel = 0f;
                if (!preso && giroRapidoRestante <= 0f)
                {
                    if (v > 0.1f) vel = corre ? velCorrer : velAndar;
                    else if (v < -0.1f) vel = -velRe;
                }
                velAtual = Mathf.MoveTowards(velAtual, Mathf.Abs(vel), 18f * Time.deltaTime);
                mov = transform.forward * (vel < 0f ? -velAtual : velAtual);
            }
            else
            {
                Vector2 inp = new Vector2(h, v);
                if (inp.sqrMagnitude > 1f) inp.Normalize();
                bool tem = inp.sqrMagnitude > 0.01f;
                float camYaw = Camera.main != null ? Camera.main.transform.eulerAngles.y : 0f;
                if (!tem) yawEntrada = camYaw; // mantém a direção durante cortes de câmera

                float alvoVel = (tem && !preso) ? (corre ? velCorrer : velAndar) * inp.magnitude * (1f + 0.003f * TotDex) : 0f;
                velAtual = Mathf.MoveTowards(velAtual, alvoVel, 16f * Time.deltaTime);

                if (tem && !preso)
                {
                    Vector3 dir = Quaternion.Euler(0f, yawEntrada, 0f) * new Vector3(inp.x, 0f, inp.y);
                    if (dir.sqrMagnitude > 0.001f)
                    {
                        ultimaDir = dir.normalized;
                        Quaternion alvoRot = Quaternion.LookRotation(ultimaDir);
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, alvoRot, 620f * Time.deltaTime);
                    }
                }
                mov = ultimaDir * velAtual;
            }

            if (cc.isGrounded && velY < 0f) velY = -2f;
            else velY -= 25f * Time.deltaTime;

            cc.Move((mov + Vector3.up * velY + empurrao) * Time.deltaTime);
            empurrao = Vector3.Lerp(empurrao, Vector3.zero, 8f * Time.deltaTime);

            if (pivoCajado != null)
            {
                float t = (Time.time - golpeInicio) / 0.35f;
                if (t >= 0f && t <= 1f) pivoCajado.localRotation = Quaternion.Euler(Mathf.Lerp(-40f, 100f, t), 0f, 0f);
                else pivoCajado.localRotation = Quaternion.Euler(Mathf.Sin(Time.time * 1.5f) * 3f, 0f, 0f);
            }
        }

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody corpo = hit.collider.attachedRigidbody;
            if (corpo == null || corpo.isKinematic || hit.moveDirection.y < -0.3f) return;
            if (hit.collider.GetComponentInParent<Inimigo>() != null) return;
            Vector3 dir = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z);
            corpo.SetVel(dir * 2.2f);
        }

        // ------------------------------------------------------------------ interação
        void ProcurarInteracao(GameManager gm)
        {
            alvoInteracao = null;
            float melhor = float.MaxValue;
            Vector3 centro = transform.position + transform.forward * 0.6f + Vector3.up * 0.9f;
            Vector3 corpo = transform.position + Vector3.up * 0.9f;
            Collider[] cols = Physics.OverlapSphere(centro, 1.5f, ~0, QueryTriggerInteraction.Collide);
            for (int i = 0; i < cols.Length; i++)
            {
                Interagivel it = cols[i].GetComponentInParent<Interagivel>();
                if (it == null || !it.Disponivel) continue;

                Coletavel col = it as Coletavel;
                if (col != null && col.automatico)
                {
                    if ((cols[i].ClosestPoint(corpo) - corpo).sqrMagnitude < 1.1f) col.Interagir(this);
                    continue;
                }
                float d = (cols[i].ClosestPoint(centro) - centro).sqrMagnitude;
                if (d < melhor) { melhor = d; alvoInteracao = it; }
            }
            gm.Dica = alvoInteracao != null ? "[C] " + alvoInteracao.Rotulo : "";
        }

        // ------------------------------------------------------------------ magias
        Inimigo alvoTravado;
        public Inimigo AlvoTravado { get { return alvoTravado; } }
        public float VelocidadeAtual { get { return velAtual; } }

        Inimigo EscolherAlvo()
        {
            Camera cam = Camera.main;
            Vector3 cf = cam != null ? cam.transform.forward : transform.forward;
            cf.y = 0f;
            Inimigo melhor = null;
            float melhorNota = float.MaxValue;
            for (int i = 0; i < Inimigo.Ativos.Count; i++)
            {
                Inimigo e = Inimigo.Ativos[i];
                if (e == null || e.Morto) continue;
                Vector3 para = e.PontoMira - transform.position;
                float dist = para.magnitude;
                if (dist > 24f) continue;
                float ang = Vector3.Angle(cf, new Vector3(para.x, 0f, para.z));
                if (ang > 75f) continue;
                float nota = ang * 0.5f + dist * 0.6f;
                if (nota < melhorNota) { melhorNota = nota; melhor = e; }
            }
            return melhor;
        }

        void AlternarTravamento()
        {
            GameManager gm = GameManager.Instance;
            if (alvoTravado != null) { alvoTravado = null; gm.Mostrar("Alvo destravado", 1.5f); return; }
            alvoTravado = EscolherAlvo();
            gm.Mostrar(alvoTravado != null ? "Alvo travado: " + alvoTravado.nomeExibido + "  ([Z] destrava)" : "Nenhum inimigo por perto", 2f);
        }

        /// <summary>Direção do disparo: no alvo travado/automático; sem alvo, no ponto sob a mira (centro da tela). O monge vira para a direção do disparo.</summary>
        Vector3 DirecaoMira(Vector3 origem, out Inimigo alvo)
        {
            alvo = alvoTravado != null ? alvoTravado : EscolherAlvo();
            Vector3 dir;
            if (alvo != null) dir = (alvo.PontoMira - origem).normalized;
            else
            {
                Camera cam = Camera.main;
                Vector3 destino = transform.position + transform.forward * 30f + Vector3.up * 1.3f;
                if (cam != null)
                {
                    destino = cam.transform.position + cam.transform.forward * 40f;
                    RaycastHit h;
                    if (Physics.Raycast(cam.transform.position + cam.transform.forward * 1.5f, cam.transform.forward, out h, 60f, ~0, QueryTriggerInteraction.Ignore) && h.collider.GetComponentInParent<PlayerController>() == null)
                        destino = h.point;
                }
                dir = (destino - origem).normalized;
            }
            Vector3 plano = new Vector3(dir.x, 0f, dir.z);
            if (plano.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(plano);
            return dir;
        }

        void Magia(int i)
        {
            if (Time.time < proxMagia[i]) return;
            GameManager gm = GameManager.Instance;
            if (i == 4)
            {
                if (!PoderCheio) { gm.Mostrar("Poder Arcano incompleto. Ataque inimigos para enchê-lo.", 2.5f); proxMagia[i] = Time.time + 0.5f; return; }
            }
            else if (mana < custoMagia[i])
            {
                gm.Mostrar("Sem essência! Use o cajado [X] ou um frasco [2].", 2.5f);
                proxMagia[i] = Time.time + 0.4f;
                Som.Tocar(TipoSom.Vazio, transform.position, 0.6f);
                return;
            }
            mana -= custoMagia[i];
            proxMagia[i] = Time.time + recargaMagia[i] / (1f + 0.005f * TotDex);
            if (anim != null) anim.Conjurar();

            switch (i)
            {
                case 0: Raio(); break;
                case 1: Nova(); break;
                case 2: Leque(); break;
                case 3: Escudo(); break;
                default: StartCoroutine(InvocacaoRotina()); break;
            }
            gm.Evento("magia", NomesMagias[i]);
        }

        void Raio()
        {
            travadoAte = Time.time + 0.2f;
            Inimigo alvo;
            Vector3 origem = pontoDisparo.position;
            Vector3 dir = DirecaoMira(origem, out alvo);
            ProceduralModels.CriarProjetil(origem, dir, true, DanoMagia(0, 25f), 18f, cc, alvo);
            Som.Tocar(TipoSom.Disparo, origem, 0.7f);
        }

        void Leque()
        {
            travadoAte = Time.time + 0.3f;
            Inimigo alvo;
            Vector3 origem = pontoDisparo.position;
            Vector3 baseDir = DirecaoMira(origem, out alvo);
            float[] angulos = { -18f, 0f, 18f };
            for (int i = 0; i < angulos.Length; i++)
            {
                Vector3 d = Quaternion.Euler(0f, angulos[i], 0f) * baseDir;
                ProceduralModels.CriarProjetil(origem, d, true, DanoMagia(2, 18f), 17f, cc, alvo);
            }
            Som.Tocar(TipoSom.Leque, origem, 0.8f);
        }

        void Nova()
        {
            travadoAte = Time.time + 0.5f;
            Vector3 c = transform.position + Vector3.up;
            DanoArea(c, 5.5f, DanoMagia(1, 45f), 6f, 14f);
            Efeitos.Explosao(c, "EmissaoAzul", 11f, 0.55f);
            FxMagia.NovaRunica(c, new Color(0.55f, 0.75f, 1f), 5.5f);
            GameManager.Instance.Tremer(0.35f);
            Som.Tocar(TipoSom.Nova, c, 1f);
        }

        void Escudo()
        {
            escudoAte = Time.time + 4f + nivelMagia[3];
            if (escudoVisual != null) escudoVisual.SetActive(true);
            Efeitos.Explosao(transform.position + Vector3.up, "EmissaoAzul", 3.5f, 0.5f);
            Som.Tocar(TipoSom.Escudo, transform.position, 0.9f);
            GameManager.Instance.Mostrar("Escudo Místico: invulnerável por 5 segundos", 2f);
        }

        IEnumerator InvocacaoRotina()
        {
            GameManager gm = GameManager.Instance;
            poder = 0f;
            travadoAte = Time.time + 3.2f;
            invulAte = Time.time + 3.6f;
            gm.Tremer(0.8f);
            gm.MostrarBanner("INVOCAÇÃO CELESTE!", new Color(0.8f, 0.5f, 1f), 2.5f);
            Som.Tocar(TipoSom.Super, transform.position, 1f);
            Vector3 c = transform.position;
            Efeitos.Explosao(c + Vector3.up, "EmissaoRoxa", 14f, 1f);
            DanoArea(c + Vector3.up, 12f, DanoMagia(4, 40f), 9f, 16f);
            for (int i = 0; i < 14; i++)
            {
                Vector2 r = Random.insideUnitCircle * 10f;
                Vector3 p = c + new Vector3(r.x, 0.1f, r.y);
                Efeitos.Explosao(p, "EmissaoVermelha", 1.4f, 0.45f);
                yield return new WaitForSeconds(0.12f);
                Efeitos.Explosao(p + Vector3.up * 0.5f, "EmissaoRoxa", 6f, 0.5f);
                DanoArea(p, 3.8f, DanoMagia(4, 55f), 5f, 10f);
                gm.Tremer(0.3f);
                Som.Tocar(TipoSom.Explosao, p, 0.5f);
            }
        }

        void DanoArea(Vector3 c, float raio, float dano, float empurraoInimigo, float forcaFisica)
        {
            var vistos = new HashSet<object>();
            Collider[] cols = Physics.OverlapSphere(c, raio, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < cols.Length; i++)
            {
                Rigidbody rb = cols[i].attachedRigidbody;
                bool ehInimigo = cols[i].GetComponentInParent<Inimigo>() != null;
                if (rb != null && !ehInimigo && !rb.isKinematic) rb.AddExplosionForce(forcaFisica, c, raio, 0.6f, ForceMode.Impulse);
                IDanificavel d = cols[i].GetComponentInParent<IDanificavel>();
                if (d != null && !(d is PlayerController) && vistos.Add(d)) d.ReceberDano(dano, c, empurraoInimigo);
            }
        }

        void Golpe()
        {
            if (Time.time < proxGolpe) return;
            proxGolpe = Time.time + 0.7f;
            golpeInicio = Time.time;
            if (anim != null) anim.Atacar();
            travadoAte = Time.time + 0.3f;
            Vector3 c = transform.position + transform.forward * 1.2f + Vector3.up;
            var vistos = new HashSet<object>();
            Collider[] cols = Physics.OverlapSphere(c, 1.3f, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < cols.Length; i++)
            {
                Rigidbody rb = cols[i].attachedRigidbody;
                bool ehInimigo = cols[i].GetComponentInParent<Inimigo>() != null;
                if (rb != null && !ehInimigo && !rb.isKinematic) rb.AddForce(transform.forward * 4f, ForceMode.Impulse);
                IDanificavel d = cols[i].GetComponentInParent<IDanificavel>();
                if (d != null && !(d is PlayerController) && vistos.Add(d)) d.ReceberDano(12f * DanoMult * (1f + 0.03f * TotStr), transform.position, 5f);
            }
            Som.Tocar(TipoSom.Golpe, c, 0.7f);
        }

        // ------------------------------------------------------------------ itens / progressão
        void UsarSlot(int i)
        {
            GameManager gm = GameManager.Instance;
            if (i == 0) UsarErva();
            else if (i == 1) UsarFrasco();
            else
            {
                int idx = i - 2;
                List<Item> pocoes = Pocoes();
                if (idx < pocoes.Count) UsarItem(pocoes[idx]);
            }
        }

        void UsarErva()
        {
            GameManager gm = GameManager.Instance;
            if (ervas <= 0) { gm.Mostrar("Você não tem Ervas Sagradas.", 2f); return; }
            if (vida >= vidaMax) { gm.Mostrar("Sua vida já está cheia.", 2f); return; }
            ervas--;
            Curar(45f);
            gm.Mostrar("Erva Sagrada usada (+45 de vida).", 2f);
            Som.Tocar(TipoSom.Cura, transform.position, 0.8f);
            gm.Evento("erva", "");
        }

        void UsarFrasco()
        {
            GameManager gm = GameManager.Instance;
            if (frascos <= 0) { gm.Mostrar("Você não tem Frascos de Essência.", 2f); return; }
            if (mana >= manaMax) { gm.Mostrar("Sua essência já está cheia.", 2f); return; }
            frascos--;
            AddMana(12f);
            gm.Mostrar("Frasco de Essência usado (+12).", 2f);
            Som.Tocar(TipoSom.Cura, transform.position, 0.8f);
        }

        public void Curar(float q) { vida = Mathf.Min(vidaMax, vida + q); }
        public void AddMana(float q) { mana = Mathf.Min(manaMax, mana + q); }
        public void AddErva() { ervas = Mathf.Min(9, ervas + 1); }
        public void AddFrasco() { frascos = Mathf.Min(9, frascos + 1); }

        public void AddPoder(float q)
        {
            if (poder >= poderMax) return;
            poder = Mathf.Min(poderMax, poder + q * (1f + 0.02f * TotEsp));
            if (poder >= poderMax) GameManager.Instance.Mostrar("Poder Arcano pronto! Pressione [R] para a Invocação Celeste.", 4f);
        }

        public List<Item> Pocoes()
        {
            List<Item> l = new List<Item>();
            for (int i = 0; i < inventario.Count && l.Count < 4; i++) if (inventario[i].tipo == TipoItem.Pocao) l.Add(inventario[i]);
            return l;
        }

        public void RecalcularStatus()
        {
            float fv = vidaMax > 0f ? vida / vidaMax : 1f;
            float fm = manaMax > 0f ? mana / manaMax : 1f;
            vidaMax = 100f + 10f * (nivel - 1) + 4f * TotStr + (armaduraEq != null ? armaduraEq.vida : 0f);
            manaMax = 30f + 3f * (nivel - 1) + 2f * TotWis;
            vida = Mathf.Clamp(fv * vidaMax, 1f, vidaMax);
            mana = Mathf.Clamp(fm * manaMax, 0f, manaMax);
        }

        public void GastarPonto(int atributo)
        {
            if (pontosLivres <= 0) return;
            switch (atributo)
            {
                case 0: inteligencia++; break;
                case 1: sabedoria++; break;
                case 2: forca++; break;
                case 3: destreza++; break;
                default: espectro++; break;
            }
            pontosLivres--;
            RecalcularStatus();
        }

        public void UparMagia(int i)
        {
            if (pontosLivres <= 0 || nivelMagia[i] >= 5) return;
            nivelMagia[i]++;
            pontosLivres--;
            GameManager.Instance.Mostrar(NomesMagias[i] + " evoluiu para o nível " + nivelMagia[i] + "!", 3f);
            Som.Tocar(TipoSom.LevelUp, transform.position, 0.6f);
        }

        public void Teleportar(Vector3 destino)
        {
            cc.enabled = false;
            transform.position = destino;
            cc.enabled = true;
            velY = 0f;
            alvoTravado = null;
            ZonaCamera.Resetar();
        }

        public bool AdicionarItem(Item it)
        {
            if (SlotsUsados + it.Slots > Capacidade) return false;
            inventario.Add(it);
            return true;
        }

        public void UsarItem(Item it)
        {
            GameManager gm = GameManager.Instance;
            if (it.tipo == TipoItem.Pocao)
            {
                if (it.vida > 0f && it.mana <= 0f && vida >= vidaMax) { gm.Mostrar("Sua vida já está cheia.", 2f); return; }
                if (it.mana > 0f && it.vida <= 0f && mana >= manaMax) { gm.Mostrar("Sua essência já está cheia.", 2f); return; }
                Curar(it.vida);
                AddMana(it.mana);
                inventario.Remove(it);
                gm.Mostrar(it.nome + " usado.", 2f);
                Som.Tocar(TipoSom.Cura, transform.position, 0.8f);
            }
            else if (it.tipo == TipoItem.Pergaminho)
            {
                if (nivelMagia[it.magia] >= 5) { gm.Mostrar(NomesMagias[it.magia] + " já está no nível máximo.", 2.5f); return; }
                nivelMagia[it.magia]++;
                inventario.Remove(it);
                gm.MostrarBanner(NomesMagias[it.magia] + " evoluiu: nível " + nivelMagia[it.magia], new Color(0.75f, 0.5f, 1f), 3f);
                Som.Tocar(TipoSom.LevelUp, transform.position, 0.8f);
            }
            else EquiparItem(it);
        }

        public void EquiparItem(Item it)
        {
            GameManager gm = GameManager.Instance;
            Item antigo;
            switch (it.tipo)
            {
                case TipoItem.Arma: antigo = armaEq; break;
                case TipoItem.Armadura: antigo = armaduraEq; break;
                case TipoItem.Amuleto: antigo = amuletoEq; break;
                default: return;
            }
            inventario.Remove(it);
            if (antigo != null && !AdicionarItem(antigo)) { inventario.Add(it); gm.Mostrar("Sem espaço para guardar o item que está equipado.", 2.5f); return; }
            if (it.tipo == TipoItem.Arma) armaEq = it; else if (it.tipo == TipoItem.Armadura) armaduraEq = it; else amuletoEq = it;
            RecalcularStatus();
            gm.Mostrar("Equipado: " + it.nome, 2f);
            Som.Tocar(TipoSom.Coleta, transform.position, 0.7f);
        }

        public void Desequipar(TipoItem tipo)
        {
            Item it = tipo == TipoItem.Arma ? armaEq : (tipo == TipoItem.Armadura ? armaduraEq : amuletoEq);
            if (it == null) return;
            if (!AdicionarItem(it)) { GameManager.Instance.Mostrar("Inventário cheio.", 2f); return; }
            if (tipo == TipoItem.Arma) armaEq = null; else if (tipo == TipoItem.Armadura) armaduraEq = null; else amuletoEq = null;
            RecalcularStatus();
        }

        public void DescartarItem(Item it)
        {
            inventario.Remove(it);
            GameManager.Instance.Mostrar(it.nome + " descartado.", 2f);
        }

        public void GanharXp(float q)
        {
            if (nivel >= NivelMax) return;
            xp += q;
            while (xp >= XpProximo && nivel < NivelMax)
            {
                xp -= XpProximo;
                nivel++;
                pontosLivres += 7;
                RecalcularStatus();
                vida = vidaMax;
                mana = manaMax;
                GameManager.Instance.MostrarBanner("NÍVEL " + nivel + "!  +7 pontos de habilidade  (tecla I)", new Color(0.4f, 1f, 0.6f), 4f);
                Som.Tocar(TipoSom.LevelUp, transform.position, 0.9f);
                Efeitos.Explosao(transform.position + Vector3.up, "EmissaoDourada", 6f, 0.9f);
            }
        }

        public void ReceberDano(float dano, Vector3 origem, float forca)
        {
            if (morto || Time.time < invulAte) return;
            if (EscudoAtivo)
            {
                invulAte = Time.time + 0.3f;
                Efeitos.Explosao(transform.position + Vector3.up, "EmissaoAzul", 3f, 0.25f);
                return;
            }
            vida -= dano;
            invulAte = Time.time + 0.6f;
            Vector3 dir = transform.position - origem;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f) empurrao = dir.normalized * forca;
            GameManager gm = GameManager.Instance;
            gm.PiscarDano();
            gm.Tremer(0.25f);
            if (anim != null) anim.Atingido();
            Fx.Faiscas(transform.position + Vector3.up, new Color(0.8f, 0.08f, 0.08f), 16, 3f, 0.1f, 0.6f);
            Som.Tocar(TipoSom.Dano, transform.position, 0.9f);
            if (vida <= 0f)
            {
                vida = 0f;
                Morrer();
            }
        }

        void Morrer()
        {
            morto = true;
            if (anim != null && anim.EmUal) anim.Morrer();
            else
            {
                if (anim != null) anim.enabled = false;
                if (visual != null)
                {
                    visual.localRotation = Quaternion.Euler(-90f, 0f, 0f);
                    visual.localPosition = new Vector3(0f, 0.25f, 0f);
                }
            }
            Som.Tocar(TipoSom.Morte, transform.position, 1f);
            GameManager.Instance.JogadorMorreu();
        }

        public void Reviver(Vector3 pos, float rotY)
        {
            cc.enabled = false;
            transform.position = pos;
            transform.rotation = Quaternion.Euler(0f, rotY, 0f);
            cc.enabled = true;
            vida = vidaMax;
            mana = Mathf.Max(mana, 8f);
            morto = false;
            velY = 0f;
            velAtual = 0f;
            empurrao = Vector3.zero;
            invulAte = Time.time + 2f;
            if (escudoVisual != null) escudoVisual.SetActive(false);
            if (anim != null) { anim.enabled = true; anim.Reviver(); }
            if (visual != null)
            {
                visual.localRotation = Quaternion.identity;
                visual.localPosition = Vector3.zero;
            }
        }
    }
}
