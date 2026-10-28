using System.Collections.Generic;
using UnityEngine;

namespace MongeArcano
{
    public enum ModoCamera { Terceira, Fixa }

    /// <summary>
    /// Câmera em terceira pessoa (padrão): atrás do monge, ligeiramente sobre o ombro, inclinação baixa e cinematográfica.
    /// Botão direito do mouse + arrastar gira; roda do mouse dá zoom; recentraliza sozinha atrás do personagem ao andar; trava no alvo quando há lock-on.
    /// Colide com cenário; paredes que bloqueiam a visão somem. Tecla Y alterna para câmera fixa por sala. Também faz voo de abertura, tremor e foco em itens.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraDirector : MonoBehaviour
    {
        public static CameraDirector Instance { get; private set; }

        public Transform alvo;
        public ModoCamera modo = ModoCamera.Terceira;
        public float distancia = 6.5f;
        public float inclinacao = 14f;
        public float giro;
        public float alturaFoco = 1.55f;
        public float ombro = 0.45f;
        public float suavizacao = 7f;

        Vector3 pos, focoPos, focoCentro;
        Camera cam;
        float tremor, fovFixo = 52f;
        Transform foco;
        PlayerController jogador;
        List<ParedeOcultavel> ocultas = new List<ParedeOcultavel>();
        List<ParedeOcultavel> novas = new List<ParedeOcultavel>();
        readonly RaycastHit[] hits = new RaycastHit[24];

        bool voando;
        Vector3[] vooPos, vooOlhar;
        float vooT, vooDur;
        System.Action vooFim;

        public Vector3 Frente { get { return transform.forward; } }

        void Awake()
        {
            Instance = this;
            cam = GetComponent<Camera>();
            pos = transform.position;
            fovFixo = cam.fieldOfView;
        }

        void Start()
        {
            if (alvo != null) jogador = alvo.GetComponent<PlayerController>();
            if (alvo != null) giro = alvo.eulerAngles.y;
        }

        public void DefinirZona(Vector3 novaPos, float fov)
        {
            pos = novaPos;
            fovFixo = fov;
            if (modo == ModoCamera.Fixa) Cortar(novaPos, fov);
        }

        public void Cortar(Vector3 novaPos, float fov)
        {
            pos = novaPos;
            fovFixo = fov;
            transform.position = novaPos;
            if (cam != null) cam.fieldOfView = fov;
            if (alvo != null) transform.rotation = Quaternion.LookRotation((alvo.position + Vector3.up) - novaPos);
        }

        public void Tremer(float forca) { tremor = Mathf.Max(tremor, forca); }

        public void IniciarFoco(Transform t)
        {
            if (t == null) return;
            foco = t;
            Vector3 centro = t.position;
            float tam = 0.6f;
            Renderer[] rs = t.GetComponentsInChildren<Renderer>();
            if (rs.Length > 0)
            {
                Bounds b = rs[0].bounds;
                for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
                centro = b.center;
                tam = b.size.magnitude;
            }
            focoCentro = centro;
            Vector3 dir = centro - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.01f) dir = Vector3.forward;
            dir.Normalize();
            focoPos = centro - dir * Mathf.Max(1.5f, tam * 1.1f) + Vector3.up * 0.2f;
        }

        public void FimFoco() { foco = null; }

        public void IniciarVoo(Vector3[] posicoes, Vector3[] olhar, float duracao, System.Action aoFim)
        {
            vooPos = posicoes; vooOlhar = olhar; vooDur = duracao; vooT = 0f; vooFim = aoFim; voando = true;
        }

        public void PararVoo()
        {
            if (!voando) return;
            voando = false;
            if (vooFim != null) { System.Action f = vooFim; vooFim = null; f(); }
        }

        static Vector3 Curva(Vector3[] pts, float t)
        {
            float e = t * (pts.Length - 1);
            int i = Mathf.Min(pts.Length - 2, (int)e);
            float f = e - i;
            f = f * f * (3f - 2f * f);
            return Vector3.Lerp(pts[i], pts[i + 1], f);
        }

        void LateUpdate()
        {
            float dt = Time.unscaledDeltaTime;

            if (voando)
            {
                vooT += dt / vooDur;
                float t = Mathf.Clamp01(vooT);
                Vector3 p = Curva(vooPos, t), o = Curva(vooOlhar, t);
                transform.position = p;
                transform.rotation = Quaternion.LookRotation(o - p);
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 50f, 1f - Mathf.Exp(-3f * dt));
                if (t >= 1f || Input.GetKeyDown(KeyCode.Return)) PararVoo();
                return;
            }

            if (foco != null)
            {
                transform.position = Vector3.Lerp(transform.position, focoPos, 1f - Mathf.Exp(-6f * dt));
                Quaternion r = Quaternion.LookRotation(focoCentro - transform.position);
                transform.rotation = Quaternion.Slerp(transform.rotation, r, 1f - Mathf.Exp(-8f * dt));
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 34f, 1f - Mathf.Exp(-5f * dt));
                return;
            }

            GameManager gm = GameManager.Instance;
            bool jogando = gm != null && gm.Estado == EstadoJogo.Jogando;
            if (jogando && Input.GetKeyDown(KeyCode.Y))
            {
                modo = modo == ModoCamera.Terceira ? ModoCamera.Fixa : ModoCamera.Terceira;
                gm.Mostrar(modo == ModoCamera.Terceira ? "Câmera: TERCEIRA PESSOA. Botão direito + mouse gira, roda dá zoom. [Y] troca." : "Câmera: FIXA por sala. [Y] troca.", 3f);
                if (modo == ModoCamera.Fixa) { ZonaCamera.Resetar(); ZonaCamera.AtivarPara(alvo.position); }
            }
            if (alvo == null) return;
            if (jogador == null) jogador = alvo.GetComponent<PlayerController>();

            Vector3 p2;
            if (modo == ModoCamera.Terceira)
            {
                if (jogando)
                {
                    bool orbitando = Input.GetMouseButton(1);
                    if (orbitando)
                    {
                        giro += Input.GetAxis("Mouse X") * 3.2f;
                        inclinacao = Mathf.Clamp(inclinacao - Input.GetAxis("Mouse Y") * 2.2f, 3f, 48f);
                    }
                    float roda = Input.GetAxis("Mouse ScrollWheel");
                    if (roda != 0f) distancia = Mathf.Clamp(distancia - roda * 6f, 3.5f, 14f);

                    Inimigo travado = jogador != null ? jogador.AlvoTravado : null;
                    if (travado != null && !orbitando)
                    {
                        Vector3 para = travado.transform.position - alvo.position;
                        float yawAlvo = Mathf.Atan2(para.x, para.z) * Mathf.Rad2Deg;
                        giro = Mathf.LerpAngle(giro, yawAlvo, 1f - Mathf.Exp(-2.5f * dt));
                    }
                    else if (!orbitando && jogador != null && jogador.VelocidadeAtual > 0.6f)
                    {
                        float yawAlvo = alvo.eulerAngles.y;
                        float dif = Mathf.DeltaAngle(giro, yawAlvo);
                        if (Mathf.Abs(dif) < 115f) giro += dif * (1f - Mathf.Exp(-1.1f * dt));
                    }
                }

                Quaternion rot = Quaternion.Euler(inclinacao, giro, 0f);
                Vector3 f = alvo.position + Vector3.up * alturaFoco + rot * Vector3.right * ombro;
                Vector3 desejado = f - rot * Vector3.forward * distancia;
                Vector3 dir = desejado - f;
                float d = dir.magnitude;
                float livre = d;
                if (d > 0.1f)
                {
                    int n = Physics.SphereCastNonAlloc(f, 0.25f, dir / d, hits, d, ~0, QueryTriggerInteraction.Ignore);
                    for (int i = 0; i < n; i++)
                    {
                        Collider c = hits[i].collider;
                        if (c.transform.IsChildOf(alvo) || c.GetComponent<ParedeOcultavel>() != null || c.GetComponentInParent<Inimigo>() != null) continue;
                        if (hits[i].distance < livre) livre = Mathf.Max(0.8f, hits[i].distance - 0.1f);
                    }
                    desejado = f + dir / d * livre;
                }
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, 1f - Mathf.Exp(-12f * dt));
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 50f, 1f - Mathf.Exp(-6f * dt));
                p2 = Vector3.Lerp(transform.position, desejado, 1f - Mathf.Exp(-14f * dt));
                OcultarParedes(f);
            }
            else
            {
                Vector3 f = alvo.position + Vector3.up * 1.1f;
                Quaternion rot = Quaternion.LookRotation(f - pos);
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, 1f - Mathf.Exp(-suavizacao * dt));
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, fovFixo, 1f - Mathf.Exp(-6f * dt));
                p2 = Vector3.Lerp(transform.position, pos, 1f - Mathf.Exp(-10f * dt));
                RestaurarParedes();
            }

            if (tremor > 0f)
            {
                p2 += Random.insideUnitSphere * tremor * 0.3f;
                tremor = Mathf.MoveTowards(tremor, 0f, dt * 1.5f);
            }
            transform.position = p2;
        }

        void OcultarParedes(Vector3 focoPonto)
        {
            Vector3 de = transform.position;
            Vector3 dir = focoPonto - de;
            float d = dir.magnitude;
            novas.Clear();
            if (d > 0.1f)
            {
                int n = Physics.SphereCastNonAlloc(de, 0.7f, dir / d, hits, d, ~0, QueryTriggerInteraction.Ignore);
                for (int i = 0; i < n; i++)
                {
                    ParedeOcultavel p = hits[i].collider.GetComponent<ParedeOcultavel>();
                    if (p != null) { p.Ocultar(true); novas.Add(p); }
                }
            }
            for (int i = 0; i < ocultas.Count; i++) if (ocultas[i] != null && !novas.Contains(ocultas[i])) ocultas[i].Ocultar(false);
            List<ParedeOcultavel> tmp = ocultas; ocultas = novas; novas = tmp;
        }

        void RestaurarParedes()
        {
            for (int i = 0; i < ocultas.Count; i++) if (ocultas[i] != null) ocultas[i].Ocultar(false);
            ocultas.Clear();
        }
    }
}
