using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MongeArcano
{
    public enum TipoInimigo { Possuido, Espectro, Infernal, Arconte }

    /// <summary>
    /// IA simples e robusta (sem NavMesh): desperta por proximidade/visão, persegue, ataca corpo a corpo
    /// e leva empurrões pela física. O espectro flutua; o Arconte (chefe) dispara bolas infernais e invoca espíritos.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Inimigo : MonoBehaviour, IDanificavel
    {
        public static readonly List<Inimigo> Ativos = new List<Inimigo>();

        public TipoInimigo tipo;
        public string nomeExibido = "Inimigo";
        public float vidaMax = 60f;
        public float vida = 60f;
        public float velocidade = 1.5f;
        public float dano = 15f;
        public float alcanceAtaque = 1.5f;
        public float deteccao = 8f;
        public float intervaloAtaque = 1.4f;
        public float chanceDrop = 0.3f;
        public bool flutua;
        public float alturaFlutuar = 0.7f;
        public bool chefe;
        public bool ataqueADistancia;
        public float danoDistancia = 18f;
        public float intervaloDisparo = 3f;
        public Transform pontoMira;

        Rigidbody rb;
        PlayerController jogador;
        Renderer[] rends;
        MaterialPropertyBlock mpb;
        float proxAtaque, proxDisparo, atordoadoAte, baseY, fase, proxGrunhido;
        bool ativo, morto, fase2;

        public bool Ativo { get { return ativo && !morto; } }
        public bool Morto { get { return morto; } }
        public float FracaoVida { get { return Mathf.Clamp01(vida / vidaMax); } }
        public Vector3 PontoMira { get { return pontoMira != null ? pontoMira.position : transform.position + Vector3.up; } }

        void OnEnable() { if (!Ativos.Contains(this)) Ativos.Add(this); }
        void OnDisable() { Ativos.Remove(this); }

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.useGravity = !flutua;
            vida = vidaMax;
            baseY = transform.position.y;
            fase = Random.value * 10f;
            jogador = GameManager.Instance != null ? GameManager.Instance.jogador : null;
            rends = GetComponentsInChildren<Renderer>();
            mpb = new MaterialPropertyBlock();
            proxGrunhido = Time.time + Random.Range(3f, 8f);
        }

        public void Despertar() { ativo = true; }

        bool TemVisao()
        {
            Vector3 de = PontoMira;
            Vector3 para = jogador.transform.position + Vector3.up;
            RaycastHit h;
            if (Physics.Linecast(de, para, out h, ~0, QueryTriggerInteraction.Ignore))
                return h.collider.GetComponentInParent<PlayerController>() != null;
            return true;
        }

        void FixedUpdate()
        {
            if (morto || rb == null || jogador == null) return;

            Vector3 para = jogador.transform.position - transform.position;
            Vector3 plano = new Vector3(para.x, 0f, para.z);
            float dist = plano.magnitude;
            Vector3 v = rb.GetVel();

            if (flutua)
            {
                float yAlvo = baseY + alturaFlutuar + Mathf.Sin(Time.time * 2f + fase) * 0.2f;
                v.y = (yAlvo - rb.position.y) * 3f;
            }

            if (Time.time < atordoadoAte)
            {
                v.x *= 0.9f; v.z *= 0.9f;
                rb.SetVel(v);
                return;
            }

            if (!ativo)
            {
                if (!jogador.Morto && dist < deteccao && (dist < 3.5f || TemVisao())) Ativar();
                else
                {
                    v.x = 0f; v.z = 0f;
                    rb.SetVel(v);
                    return;
                }
            }

            if (jogador.Morto)
            {
                v.x = 0f; v.z = 0f;
                rb.SetVel(v);
                return;
            }

            if (dist > 0.05f)
            {
                Quaternion alvoRot = Quaternion.LookRotation(plano.normalized);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, alvoRot, 6f * Time.fixedDeltaTime));
            }

            float vel = velocidade;
            if (tipo == TipoInimigo.Possuido) vel *= 0.75f + 0.25f * Mathf.Sin(Time.time * 2.5f + fase);

            if (dist > alcanceAtaque * 0.85f)
            {
                Vector3 d = plano.normalized * vel;
                v.x = d.x; v.z = d.z;
            }
            else { v.x = 0f; v.z = 0f; }
            rb.SetVel(v);

            if (dist <= alcanceAtaque && Mathf.Abs(para.y) < 2.4f && Time.time >= proxAtaque)
            {
                proxAtaque = Time.time + intervaloAtaque;
                jogador.ReceberDano(dano, transform.position, 4f);
            }

            if (ataqueADistancia && dist > 3.5f && dist < 18f && Time.time >= proxDisparo)
            {
                proxDisparo = Time.time + intervaloDisparo;
                Vector3 origem = PontoMira;
                Vector3 dir = (jogador.transform.position + Vector3.up * 1.1f - origem).normalized;
                ProceduralModels.CriarProjetil(origem + dir * 1.2f, dir, false, danoDistancia, 9f, GetComponent<Collider>());
                Som.Tocar(TipoSom.Grunhido, origem, 0.9f);
            }

            if (Time.time >= proxGrunhido)
            {
                proxGrunhido = Time.time + Random.Range(4f, 9f);
                Som.Tocar(TipoSom.Grunhido, transform.position, 0.45f);
            }
        }

        void Ativar()
        {
            ativo = true;
            Som.Tocar(TipoSom.Grunhido, transform.position, 0.8f);
            if (chefe)
            {
                GameManager.Instance.chefe = this;
                GameManager.Instance.Mostrar("O Arconte das Cinzas desperta!", 4f);
                GameManager.Instance.Tremer(0.6f);
            }
        }

        public void ReceberDano(float quantidade, Vector3 origem, float empurrao)
        {
            if (morto) return;
            if (!ativo) { ativo = true; if (chefe) { GameManager.Instance.chefe = this; } }
            vida -= quantidade;

            if (empurrao > 0f && rb != null && !chefe)
            {
                Vector3 dir = transform.position - origem;
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.001f) rb.AddForce(dir.normalized * empurrao, ForceMode.Impulse);
                atordoadoAte = Time.time + 0.25f;
            }

            Piscar();
            Som.Tocar(TipoSom.Impacto, transform.position, 0.6f);

            if (chefe && !fase2 && vida < vidaMax * 0.5f) EntrarFase2();
            if (vida <= 0f) Morrer();
        }

        void EntrarFase2()
        {
            fase2 = true;
            velocidade *= 1.35f;
            intervaloDisparo *= 0.6f;
            GameManager.Instance.Mostrar("O Arconte invoca espíritos das cinzas!", 4f);
            GameManager.Instance.Tremer(0.7f);
            for (int i = 0; i < 2; i++)
            {
                Vector3 p = transform.position + new Vector3(i == 0 ? -3.5f : 3.5f, 0f, 0f);
                Inimigo e = ProceduralModels.CriarInimigo(TipoInimigo.Espectro, p, 0f);
                e.deteccao = 40f;
                e.Despertar();
            }
        }

        void Piscar()
        {
            if (rends == null) return;
            mpb.SetColor("_Color", new Color(3f, 3f, 3f, 1f));
            for (int i = 0; i < rends.Length; i++) if (rends[i] != null) rends[i].SetPropertyBlock(mpb);
            CancelInvoke("LimparPisca");
            Invoke("LimparPisca", 0.08f);
        }

        void LimparPisca()
        {
            if (rends == null) return;
            for (int i = 0; i < rends.Length; i++) if (rends[i] != null) rends[i].SetPropertyBlock(null);
        }

        void Morrer()
        {
            morto = true;
            ativo = false;
            Ativos.Remove(this);
            GameManager gm = GameManager.Instance;
            gm.InimigosDerrotados++;

            foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = false;
            foreach (Light l in GetComponentsInChildren<Light>()) l.enabled = false;
            rb.SetVel(Vector3.zero);
            rb.isKinematic = true;
            Som.Tocar(TipoSom.Morte, transform.position, 0.9f);

            if (Random.value < chanceDrop)
            {
                TipoColetavel t = Random.value < 0.25f ? TipoColetavel.Erva : TipoColetavel.Essencia;
                Vector3 p = transform.position;
                ProceduralModels.CriarColetavel(t, new Vector3(p.x, 0.7f, p.z));
            }

            if (chefe) gm.ChefeDerrotado();
            StartCoroutine(Desabar());
        }

        IEnumerator Desabar()
        {
            Quaternion r0 = transform.rotation;
            Quaternion r1 = flutua ? r0 : r0 * Quaternion.Euler(-90f, 0f, 0f);
            Vector3 s0 = transform.localScale;
            float t = 0f;
            while (t < 0.5f)
            {
                t += Time.deltaTime;
                transform.rotation = Quaternion.Slerp(r0, r1, Mathf.Clamp01(t / 0.5f));
                yield return null;
            }
            yield return new WaitForSeconds(chefe ? 6f : 3f);
            t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime;
                transform.localScale = s0 * Mathf.Max(0.001f, 1f - t);
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
