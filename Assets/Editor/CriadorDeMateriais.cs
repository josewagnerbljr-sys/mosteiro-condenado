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

            Solido("Pedra", new Color(0.30f, 0.29f, 0.32f), 0f, 0.15f);
            Solido("PedraClara", new Color(0.52f, 0.50f, 0.46f), 0f, 0.2f);
            Solido("Piso", new Color(0.20f, 0.18f, 0.17f), 0f, 0.3f);
            Solido("Madeira", new Color(0.30f, 0.17f, 0.09f), 0f, 0.15f);
            Solido("Tapete", new Color(0.35f, 0.04f, 0.06f), 0f, 0.05f);
            Solido("Tecido", new Color(0.90f, 0.45f, 0.06f), 0f, 0.1f);
            Solido("TecidoEscuro", new Color(0.40f, 0.11f, 0.03f), 0f, 0.1f);
            Solido("Pele", new Color(0.76f, 0.56f, 0.42f), 0f, 0.2f);
            Solido("Ouro", new Color(0.90f, 0.70f, 0.20f), 0.8f, 0.6f);
            Solido("CarnePodre", new Color(0.35f, 0.42f, 0.30f), 0f, 0.1f);
            Solido("PeleInfernal", new Color(0.45f, 0.05f, 0.05f), 0f, 0.35f);
            Solido("Osso", new Color(0.82f, 0.79f, 0.68f), 0f, 0.2f);
            Solido("Papel", new Color(0.88f, 0.82f, 0.64f), 0f, 0.05f);
            Solido("Obsidiana", new Color(0.05f, 0.04f, 0.07f), 0.5f, 0.75f);

            Emissivo("EmissaoAzul", new Color(0.25f, 0.62f, 1f));
            Emissivo("EmissaoVermelha", new Color(1f, 0.12f, 0.06f));
            Emissivo("EmissaoLaranja", new Color(1f, 0.5f, 0.1f));
            Emissivo("EmissaoVerde", new Color(0.25f, 1f, 0.35f));
            Emissivo("EmissaoRoxa", new Color(0.65f, 0.25f, 1f));
            Emissivo("EmissaoBranca", new Color(1f, 1f, 1f));
            Emissivo("EmissaoDourada", new Color(1f, 0.8f, 0.25f));

            Fantasma("Fantasma", new Color(0.6f, 0.95f, 0.9f, 0.45f));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        static Material Novo(string nome, out string caminho)
        {
            caminho = Pasta + "/" + nome + ".mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(caminho) != null) return null;
            return new Material(Shader.Find("Standard"));
        }

        static void Solido(string nome, Color cor, float metal, float suave)
        {
            string p; Material m = Novo(nome, out p);
            if (m == null) return;
            m.color = cor;
            m.SetFloat("_Metallic", metal);
            m.SetFloat("_Glossiness", suave);
            AssetDatabase.CreateAsset(m, p);
        }

        static void Emissivo(string nome, Color cor)
        {
            string p; Material m = Novo(nome, out p);
            if (m == null) return;
            m.color = cor;
            m.SetFloat("_Glossiness", 0.4f);
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", cor * 2f);
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            AssetDatabase.CreateAsset(m, p);
        }

        static void Fantasma(string nome, Color cor)
        {
            string p; Material m = Novo(nome, out p);
            if (m == null) return;
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
