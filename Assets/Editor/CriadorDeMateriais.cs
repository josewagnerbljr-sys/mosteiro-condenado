using UnityEditor;
using UnityEngine;

namespace MongeArcano.EditorTools
{
    /// <summary>Cria os materiais (Standard) em Assets/Resources/Materiais. Idempotente: não sobrescreve os existentes.</summary>
    public static class CriadorDeMateriais
    {
        public const string Pasta = "Assets/Resources/Materiais";

        public static void Garantir()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder(Pasta)) AssetDatabase.CreateFolder("Assets/Resources", "Materiais");

            Solido("Pedra", new Color(0.20f, 0.20f, 0.23f), 0f, 0.15f);
            Solido("PedraClara", new Color(0.34f, 0.33f, 0.32f), 0f, 0.2f);
            Solido("Piso", new Color(0.12f, 0.11f, 0.11f), 0f, 0.3f);
            Solido("Madeira", new Color(0.20f, 0.12f, 0.07f), 0f, 0.15f);
            Solido("Tapete", new Color(0.28f, 0.03f, 0.05f), 0f, 0.05f);
            Solido("Tecido", new Color(0.78f, 0.40f, 0.08f), 0f, 0.1f);
            Solido("TecidoEscuro", new Color(0.30f, 0.08f, 0.02f), 0f, 0.1f);
            Solido("Pele", new Color(0.70f, 0.52f, 0.40f), 0f, 0.2f);
            Solido("Ouro", new Color(0.80f, 0.60f, 0.18f), 0.8f, 0.6f);
            Solido("CarnePodre", new Color(0.30f, 0.37f, 0.26f), 0f, 0.1f);
            Solido("PeleInfernal", new Color(0.38f, 0.04f, 0.04f), 0f, 0.35f);
            Solido("Osso", new Color(0.72f, 0.69f, 0.60f), 0f, 0.2f);
            Solido("Papel", new Color(0.85f, 0.80f, 0.62f), 0f, 0.05f);
            Solido("Terra", new Color(0.06f, 0.055f, 0.045f), 0f, 0.05f);
            Solido("Caminho", new Color(0.17f, 0.17f, 0.17f), 0f, 0.15f);
            Solido("PedraMusgo", new Color(0.17f, 0.21f, 0.17f), 0f, 0.1f);
            Solido("MadeiraMorta", new Color(0.12f, 0.09f, 0.07f), 0f, 0.05f);
            Solido("MongeTunica", new Color(0.70f, 0.38f, 0.10f), 0f, 0.1f);
            Solido("Armadura", new Color(0.26f, 0.27f, 0.32f), 0.8f, 0.5f);
            Solido("NecromanteRoupa", new Color(0.20f, 0.16f, 0.26f), 0f, 0.1f);
            Solido("BruxaVermelha", new Color(0.50f, 0.07f, 0.09f), 0f, 0.1f);
            Solido("BruxaNegra", new Color(0.07f, 0.06f, 0.09f), 0f, 0.1f);
            Solido("BruxaCinza", new Color(0.42f, 0.43f, 0.47f), 0f, 0.1f);
            Solido("Obsidiana", new Color(0.05f, 0.04f, 0.07f), 0.5f, 0.75f);

            Emissivo("EmissaoAzul", new Color(0.25f, 0.62f, 1f));
            Emissivo("EmissaoVermelha", new Color(1f, 0.12f, 0.06f));
            Emissivo("EmissaoLaranja", new Color(1f, 0.5f, 0.1f));
            Emissivo("EmissaoVerde", new Color(0.25f, 1f, 0.35f));
            Emissivo("EmissaoRoxa", new Color(0.65f, 0.25f, 1f));
            Emissivo("EmissaoBranca", new Color(1f, 1f, 1f));
            Emissivo("EmissaoDourada", new Color(1f, 0.8f, 0.25f));

            Fantasma("Fantasma", new Color(0.6f, 0.95f, 0.9f, 0.45f));
            Especiais();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        static void Especiais()
        {
            string p = Pasta + "/FxAditivo.mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(p) == null)
            {
                Shader sh = Shader.Find("Legacy Shaders/Particles/Additive");
                if (sh == null) sh = Shader.Find("Particles/Standard Unlit");
                Material m = new Material(sh);
                if (m.HasProperty("_TintColor")) m.SetColor("_TintColor", new Color(0.5f, 0.5f, 0.5f, 0.5f));
                AssetDatabase.CreateAsset(m, p);
            }
            p = Pasta + "/CeuTrevas.mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(p) == null)
            {
                Shader sh = Shader.Find("Skybox/Procedural");
                if (sh != null)
                {
                    Material m = new Material(sh);
                    m.SetFloat("_SunSize", 0f);
                    m.SetFloat("_AtmosphereThickness", 0.9f);
                    m.SetColor("_SkyTint", new Color(0.42f, 0.17f, 0.2f));
                    m.SetColor("_GroundColor", new Color(0.05f, 0.04f, 0.05f));
                    m.SetFloat("_Exposure", 0.7f);
                    m.EnableKeyword("_SUNDISK_NONE");
                    AssetDatabase.CreateAsset(m, p);
                }
            }
        }

        static Material Novo(string nome, out string caminho)
        {
            caminho = Pasta + "/" + nome + ".mat";
            Material existente = AssetDatabase.LoadAssetAtPath<Material>(caminho);
            if (existente != null) return existente;
            return new Material(Shader.Find("Standard"));
        }

        static void Solido(string nome, Color cor, float metal, float suave)
        {
            string p; Material m = Novo(nome, out p);
            if (m == null) return;
            m.color = cor;
            m.SetFloat("_Metallic", metal);
            m.SetFloat("_Glossiness", suave);
            if (AssetDatabase.LoadAssetAtPath<Material>(p) == null) AssetDatabase.CreateAsset(m, p); else EditorUtility.SetDirty(m);
        }

        static void Emissivo(string nome, Color cor)
        {
            if (AssetDatabase.LoadAssetAtPath<Material>(Pasta + "/" + nome + ".mat") != null) return;
            string p; Material m = Novo(nome, out p);
            m.color = cor;
            m.SetFloat("_Glossiness", 0.4f);
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", cor * 2f);
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            AssetDatabase.CreateAsset(m, p);
        }

        static void Fantasma(string nome, Color cor)
        {
            if (AssetDatabase.LoadAssetAtPath<Material>(Pasta + "/" + nome + ".mat") != null) return;
            string p; Material m = Novo(nome, out p);
            m.SetFloat("_Mode", 3f);
            m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetInt("_ZWrite", 0);
            m.DisableKeyword("_ALPHATEST_ON");
            m.EnableKeyword("_ALPHABLEND_ON");
            m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            m.renderQueue = 3000;
            m.color = cor;
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", new Color(0.2f, 0.45f, 0.4f));
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            AssetDatabase.CreateAsset(m, p);
        }
    }
}
