using UnityEngine;

namespace MongeArcano
{
    /// <summary>Pós-processamento cinematográfico (pipeline padrão): bloom leve, vinheta, grão, contraste, dessaturação e tonalização fria nas sombras e quente nas luzes.</summary>
    [RequireComponent(typeof(Camera))]
    public class Cinema : MonoBehaviour
    {
        public float vinheta = 1.3f, saturacao = 0.82f, contraste = 1.14f, bloom = 0.55f, grao = 0.03f;
        public Color sombra = new Color(0.82f, 0.95f, 1.08f), realce = new Color(1.14f, 1.0f, 0.86f);
        public bool ativo = true;

        Material mat;

        void Start() { mat = Resources.Load<Material>("Materiais/CinemaMat"); }

        void OnRenderImage(RenderTexture src, RenderTexture dst)
        {
            if (!ativo || mat == null || mat.shader == null || !mat.shader.isSupported)
            {
                Graphics.Blit(src, dst);
                return;
            }
            mat.SetFloat("_Vinheta", vinheta);
            mat.SetFloat("_Saturacao", saturacao);
            mat.SetFloat("_Contraste", contraste);
            mat.SetFloat("_Bloom", bloom);
            mat.SetFloat("_Grao", grao);
            mat.SetFloat("_Tempo", Time.unscaledTime % 10f);
            mat.SetColor("_Sombra", sombra);
            mat.SetColor("_Realce", realce);
            Graphics.Blit(src, dst, mat);
        }
    }
}
