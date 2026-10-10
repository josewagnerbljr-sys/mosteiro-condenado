using UnityEngine;

namespace MongeArcano
{
    /// <summary>Controla a câmera principal: posição fixa por sala, rotação suave seguindo o alvo e tremor.</summary>
    [RequireComponent(typeof(Camera))]
    public class CameraDirector : MonoBehaviour
    {
        public static CameraDirector Instance { get; private set; }

        public Transform alvo;
        public float suavizacao = 7f;

        Vector3 pos;
        Camera cam;
        float tremor;

        void Awake()
        {
            Instance = this;
            cam = GetComponent<Camera>();
            pos = transform.position;
        }

        public void Cortar(Vector3 novaPos, float fov)
        {
            pos = novaPos;
            transform.position = novaPos;
            if (cam != null) cam.fieldOfView = fov;
            if (alvo != null) transform.rotation = Quaternion.LookRotation((alvo.position + Vector3.up) - novaPos);
        }

        public void Tremer(float forca) { tremor = Mathf.Max(tremor, forca); }

        void LateUpdate()
        {
            if (alvo == null) return;
            Vector3 foco = alvo.position + Vector3.up * 1.1f;
            Quaternion rot = Quaternion.LookRotation(foco - pos);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, 1f - Mathf.Exp(-suavizacao * Time.unscaledDeltaTime));

            Vector3 p = pos;
            if (tremor > 0f)
            {
                p += Random.insideUnitSphere * tremor * 0.3f;
                tremor = Mathf.MoveTowards(tremor, 0f, Time.unscaledDeltaTime * 1.5f);
            }
            transform.position = p;
        }
    }
}
