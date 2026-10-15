using System.Collections.Generic;
using UnityEngine;

namespace MongeArcano
{
    public enum ModoCamera { Seguir, Fixa }

    /// <summary>
    /// Câmera. Modo SEGUIR (padrão, estilo Diablo): inclinada, acompanha o jogador, roda do mouse dá zoom e paredes que atrapalham somem.
    /// Modo FIXA (tecla Y): câmeras por sala, estilo survival horror clássico. Também faz tremor e aproximação (foco) em itens e documentos.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraDirector : MonoBehaviour
    {
        public static CameraDirector Instance { get; private set; }

        public Transform alvo;
        public ModoCamera modo = ModoCamera.Seguir;
        public float suavizacao = 7f;
        public float distancia = 13f;
        public float inclinacao = 52f;
        public float giro = 35f;

        Vector3 pos, focoPos, focoCentro;
        Camera cam;
        float tremor, fovFixo = 52f;
        Transform foco;
        List<ParedeOcultavel> ocultas = new List<ParedeOcultavel>();
        List<ParedeOcultavel> novas = new List<ParedeOcultavel>();
        readonly RaycastHit[] hits = new RaycastHit[24];

        void Awake()
        {
            Instance = this;
            cam = GetComponent<Camera>();
            pos = transform.position;
            fovFixo = cam.fieldOfView;
        }

        /// <summary>Chamado pelas zonas de sala. Só afeta o modo fixo.</summary>
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

        void LateUpdate()
        {
            float dt = Time.unscaledDeltaTime;
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
                modo = modo == ModoCamera.Seguir ? ModoCamera.Fixa : ModoCamera.Seguir;
                gm.Mostrar(modo == ModoCamera.Seguir ? "Câmera: SEGUIR (estilo Diablo). Roda do mouse = zoom. [Y] troca." : "Câmera: FIXA por sala (estilo clássico). [Y] troca.", 3f);
                if (modo == ModoCamera.Fixa) { ZonaCamera.Resetar(); ZonaCamera.AtivarPara(alvo.position); }
            }
            if (jogando && modo == ModoCamera.Seguir)
            {
                float roda = Input.GetAxis("Mouse ScrollWheel");
                if (roda != 0f) distancia = Mathf.Clamp(distancia - roda * 8f, 7f, 20f);
            }
            if (alvo == null) return;

            Vector3 p;
            if (modo == ModoCamera.Seguir)
            {
                Vector3 f = alvo.position + Vector3.up * 1.0f;
                Quaternion rot = Quaternion.Euler(inclinacao, giro, 0f);
                Vector3 desejado = f - rot * Vector3.forward * distancia;
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, 1f - Mathf.Exp(-8f * dt));
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 42f, 1f - Mathf.Exp(-6f * dt));
                p = Vector3.Lerp(transform.position, desejado, 1f - Exp8(dt));
                OcultarParedes(f);
            }
            else
            {
                Vector3 f = alvo.position + Vector3.up * 1.1f;
                Quaternion rot = Quaternion.LookRotation(f - pos);
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, 1f - Mathf.Exp(-suavizacao * dt));
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, fovFixo, 1f - Mathf.Exp(-6f * dt));
                p = Vector3.Lerp(transform.position, pos, 1f - Mathf.Exp(-10f * dt));
                RestaurarParedes();
            }

            if (tremor > 0f)
            {
                p += Random.insideUnitSphere * tremor * 0.3f;
                tremor = Mathf.MoveTowards(tremor, 0f, dt * 1.5f);
            }
            transform.position = p;
        }

        static float Exp8(float dt) { return Mathf.Exp(-8f * dt); }

        void OcultarParedes(Vector3 foco)
        {
            Vector3 de = transform.position;
            Vector3 dir = foco - de;
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
