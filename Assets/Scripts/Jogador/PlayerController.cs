using System.Collections.Generic;
using UnityEngine;

namespace MongeArcano
{
    /// <summary>
    /// Monge arcano com controles de TANQUE (estilo survival horror clássico):
    /// girar com A/D, andar com W/S, correr com Shift, giro rápido de 180° com Q.
    /// Ataques: Raio Arcano (mira automática), Explosão Sagrada (física) e golpe de cajado.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour, IDanificavel
    {
        [Header("Atributos")]
        public float vidaMax = 100f;
        public float vida = 100f;
        public float manaMax = 30f;
        public float mana = 14f;
        public int ervas = 1;

        [Header("Movimento")]
        public float velAndar = 3.2f;
        public float velCorrer = 5.6f;
        public float velRe = 2.0f;
        public float velGiro = 135f;

        [Header("Referências (preenchidas pelo construtor)")]
        public Transform visual;
        public Transform pivoCajado;
        public Transform pontoDisparo;

        CharacterController cc;
        float velY, giroRapidoRestante, bob;
        float travadoAte, proxDisparo, proxGolpe, proxExplosao, invulAte, proxZona;
        float golpeInicio = -10f;
        Vector3 empurrao;
        bool morto;
        Interagivel alvoInteracao;

        public bool Morto { get { return morto; } }
        public float FracaoVida { get { return vida / vidaMax; } }

        void Awake()
        {
            cc = GetComponent<CharacterController>();
            vida = vidaMax;
        }

        void Update()
        {
            GameManager gm = GameManager.Instance;
            if (gm == null) return;

            if (gm.Estado != EstadoJogo.Jogando)
            {
                gm.Dica = "";
                return;
            }
            if (morto) return;

            Movimento();

            if (Time.time >= proxZona)
            {
                proxZona = Time.time + 0.15f;
                ZonaCamera.AtivarPara(transform.position);
            }

            ProcurarInteracao(gm);

            if (!gm.InteracaoBloqueada && Input.GetKeyDown(KeyCode.E) && alvoInteracao != null) alvoInteracao.Interagir(this);
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) Disparar();
            if (Input.GetKeyDown(KeyCode.F)) Explosao();
            if (Input.GetKeyDown(KeyCode.V)) Golpe();
            if (Input.GetKeyDown(KeyCode.H)) UsarErva();
        }

        // ------------------------------------------------------------------ movimento
        void Movimento()
        {
            bool preso = Time.time < travadoAte;
            float giro = Input.GetAxisRaw("Horizontal");
            float avanco = Input.GetAxisRaw("Vertical");

            if (Input.GetKeyDown(KeyCode.Q) && giroRapidoRestante <= 0f && !preso) giroRapidoRestante = 180f;

            if (giroRapidoRestante > 0f)
            {
                float passo = Mathf.Min(giroRapidoRestante, 780f * Time.deltaTime);
                transform.Rotate(0f, passo, 0f);
                giroRapidoRestante -= passo;
            }
            else
            {
                transform.Rotate(0f, giro * velGiro * Time.deltaTime, 0f);
            }

            float vel = 0f;
            if (!preso && giroRapidoRestante <= 0f)
            {
                if (avanco > 0.1f) vel = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? velCorrer : velAndar;
                else if (avanco < -0.1f) vel = -velRe;
            }

            if (cc.isGrounded && velY < 0f) velY = -2f;
            else velY -= 25f * Time.deltaTime;

            Vector3 mov = transform.forward * vel + Vector3.up * velY + empurrao;
            cc.Move(mov * Time.deltaTime);
            empurrao = Vector3.Lerp(empurrao, Vector3.zero, 8f * Time.deltaTime);

            // animação procedural
            if (visual != null)
            {
                bob += Time.deltaTime * Mathf.Abs(vel) * 2.4f;
                float balanco = Mathf.Abs(vel) > 0.1f ? Mathf.Abs(Mathf.Sin(bob)) * 0.06f : 0f;
                visual.localPosition = new Vector3(0f, balanco, 0f);
                visual.localRotation = Quaternion.Euler(vel > velAndar + 0.1f ? 9f : 0f, 0f, 0f);
            }
            if (pivoCajado != null)
            {
                float t = (Time.time - golpeInicio) / 0.35f;
                if (t >= 0f && t <= 1f) pivoCajado.localRotation = Quaternion.Euler(Mathf.Lerp(-40f, 100f, t), 0f, 0f);
                else pivoCajado.localRotation = Quaternion.Euler(Mathf.Sin(Time.time * 1.5f) * 3f, 0f, 0f);
            }
        }

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            // empurra caixas e barris (física)
            Rigidbody corpo = hit.collider.attachedRigidbody;
            if (corpo == null || corpo.isKinematic || hit.moveDirection.y < -0.3f) return;
            if (hit.collider.GetComponentInParent<Inimigo>() != null) return;
            Vector3 dir = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z);
            corpo.velocity = dir * 2.2f;
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
            gm.Dica = alvoInteracao != null ? "[E] " + alvoInteracao.Rotulo : "";
        }

        // ------------------------------------------------------------------ combate
        Inimigo MelhorAlvo(Vector3 origem)
        {
            Inimigo melhor = null;
            float melhorAng = 36f;
            for (int i = 0; i < Inimigo.Ativos.Count; i++)
            {
                Inimigo e = Inimigo.Ativos[i];
                if (e == null || e.Morto) continue;
                Vector3 para = e.PontoMira - origem;
                if (para.magnitude > 15f) continue;
                Vector3 plano = new Vector3(para.x, 0f, para.z);
                float ang = Vector3.Angle(transform.forward, plano);
                if (ang >= melhorAng) continue;
                RaycastHit h;
                if (Physics.Linecast(origem, e.PontoMira, out h, ~0, QueryTriggerInteraction.Ignore))
                {
                    if (h.collider.GetComponentInParent<Inimigo>() != e) continue;
                }
                melhorAng = ang;
                melhor = e;
            }
            return melhor;
        }

        void Disparar()
        {
            if (Time.time < proxDisparo) return;
            GameManager gm = GameManager.Instance;
            if (mana < 1f)
            {
                gm.Mostrar("Sem essência! Use o cajado [V] ou procure mais.", 2.5f);
                proxDisparo = Time.time + 0.5f;
                Som.Tocar(TipoSom.Vazio, transform.position, 0.6f);
                return;
            }
            mana -= 1f;
            proxDisparo = Time.time + 0.42f;
            travadoAte = Time.time + 0.22f;

            Vector3 origem = pontoDisparo.position;
            Vector3 dir = transform.forward;
            Inimigo alvo = MelhorAlvo(origem);
            if (alvo != null)
            {
                dir = (alvo.PontoMira - origem).normalized;
                Vector3 plano = alvo.transform.position - transform.position;
                plano.y = 0f;
                if (plano.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(plano);
            }
            ProceduralModels.CriarProjetil(origem, dir, true, 25f, 17f, cc);
            Som.Tocar(TipoSom.Disparo, origem, 0.7f);
        }

        void Explosao()
        {
            if (Time.time < proxExplosao) return;
            GameManager gm = GameManager.Instance;
            if (mana < 5f)
            {
                gm.Mostrar("Essência insuficiente para a Explosão Sagrada (5).", 2.5f);
                proxExplosao = Time.time + 0.5f;
                return;
            }
            mana -= 5f;
            proxExplosao = Time.time + 2f;
            travadoAte = Time.time + 0.5f;

            Vector3 c = transform.position + Vector3.up;
            var vistos = new HashSet<object>();
            Collider[] cols = Physics.OverlapSphere(c, 5.5f, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < cols.Length; i++)
            {
                Rigidbody rb = cols[i].attachedRigidbody;
                bool ehInimigo = cols[i].GetComponentInParent<Inimigo>() != null;
                if (rb != null && !ehInimigo && !rb.isKinematic) rb.AddExplosionForce(14f, c, 5.5f, 0.6f, ForceMode.Impulse);
                IDanificavel d = cols[i].GetComponentInParent<IDanificavel>();
                if (d != null && !(d is PlayerController) && vistos.Add(d)) d.ReceberDano(45f, c, 6f);
            }
            Efeitos.Explosao(c, "EmissaoAzul", 11f, 0.55f);
            gm.Tremer(0.35f);
            Som.Tocar(TipoSom.Explosao, c, 1f);
        }

        void Golpe()
        {
            if (Time.time < proxGolpe) return;
            proxGolpe = Time.time + 0.7f;
            golpeInicio = Time.time;
            travadoAte = Time.time + 0.3f;

            Vector3 c = transform.position + transform.forward * 1.2f + Vector3.up;
            var vistos = new HashSet<object>();
            Collider[] cols = Physics.OverlapSphere(c, 1.2f, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < cols.Length; i++)
            {
                Rigidbody rb = cols[i].attachedRigidbody;
                bool ehInimigo = cols[i].GetComponentInParent<Inimigo>() != null;
                if (rb != null && !ehInimigo && !rb.isKinematic) rb.AddForce(transform.forward * 4f, ForceMode.Impulse);
                IDanificavel d = cols[i].GetComponentInParent<IDanificavel>();
                if (d != null && !(d is PlayerController) && vistos.Add(d)) d.ReceberDano(12f, transform.position, 5f);
            }
            Som.Tocar(TipoSom.Golpe, c, 0.7f);
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
        }

        // ------------------------------------------------------------------ estado
        public void Curar(float quantidade) { vida = Mathf.Min(vidaMax, vida + quantidade); }
        public void AddMana(float quantidade) { mana = Mathf.Min(manaMax, mana + quantidade); }
        public void AddErva() { ervas = Mathf.Min(9, ervas + 1); }

        public void ReceberDano(float dano, Vector3 origem, float forca)
        {
            if (morto || Time.time < invulAte) return;
            vida -= dano;
            invulAte = Time.time + 0.6f;
            Vector3 dir = transform.position - origem;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f) empurrao = dir.normalized * forca;
            GameManager gm = GameManager.Instance;
            gm.PiscarDano();
            gm.Tremer(0.25f);
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
            if (visual != null)
            {
                visual.localRotation = Quaternion.Euler(-90f, 0f, 0f);
                visual.localPosition = new Vector3(0f, 0.25f, 0f);
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
            empurrao = Vector3.zero;
            invulAte = Time.time + 2f;
            if (visual != null)
            {
                visual.localRotation = Quaternion.identity;
                visual.localPosition = Vector3.zero;
            }
        }
    }
}
