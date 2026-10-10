using System.Collections.Generic;
using UnityEngine;

namespace MongeArcano
{
    /// <summary>
    /// Câmera fixa por sala (marca registrada do survival horror clássico).
    /// Quando o jogador entra na zona, a câmera "corta" para o ângulo da sala e passa a girar acompanhando o monge.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class ZonaCamera : MonoBehaviour
    {
        public Vector3 posicaoCamera;
        public float campoDeVisao = 55f;

        static readonly List<ZonaCamera> todas = new List<ZonaCamera>();
        static ZonaCamera atual;

        void OnEnable() { if (!todas.Contains(this)) todas.Add(this); }
        void OnDisable() { todas.Remove(this); if (atual == this) atual = null; }

        public static void Resetar() { atual = null; }

        public static void AtivarPara(Vector3 p)
        {
            for (int i = 0; i < todas.Count; i++)
            {
                ZonaCamera z = todas[i];
                if (z == null) continue;
                BoxCollider c = z.GetComponent<BoxCollider>();
                if (c != null && c.bounds.Contains(p))
                {
                    if (z != atual)
                    {
                        atual = z;
                        if (CameraDirector.Instance != null) CameraDirector.Instance.Cortar(z.posicaoCamera, z.campoDeVisao);
                    }
                    return;
                }
            }
        }
    }
}
