using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MongeArcano.EditorTools
{
    /// <summary>Gera a cena "Mosteiro" a partir do ConstrutorMundo e configura Build Settings / WebGL.</summary>
    public static class ConstrutorDeCena
    {
        public const string CaminhoCena = "Assets/Scenes/Mosteiro.unity";

        [MenuItem("Monge Arcano/1 - Gerar cena do Mosteiro")]
        public static void GerarPeloMenu()
        {
            Gerar();
        }

        public static void Gerar()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            CriadorDeMateriais.Garantir();
            if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");

            var cena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Modelos.ProvedorEditor = nome =>
            {
                string[] guids = AssetDatabase.FindAssets(nome + " t:GameObject", new[] { "Assets/Modelos" });
                for (int i = 0; i < guids.Length; i++)
                {
                    string caminho = AssetDatabase.GUIDToAssetPath(guids[i]);
                    if (System.IO.Path.GetFileNameWithoutExtension(caminho) == nome) return AssetDatabase.LoadAssetAtPath<GameObject>(caminho);
                }
                return null;
            };
            try { ConstrutorMundo.Construir(); }
            finally { Modelos.ProvedorEditor = null; Modelos.Registro = null; }
            EditorSceneManager.SaveScene(cena, CaminhoCena);
            ConfigurarProjeto();
            AssetDatabase.SaveAssets();
            Debug.Log("[Monge Arcano] Cena gerada em " + CaminhoCena + ". Aperte Play para jogar!");
        }

        public static void ConfigurarProjeto()
        {
            PlayerSettings.productName = "Monge Arcano - O Mosteiro Condenado";
            PlayerSettings.companyName = "José Wagner Blanco Júnior";
            PlayerSettings.defaultWebScreenWidth = 1280;
            PlayerSettings.defaultWebScreenHeight = 720;
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(CaminhoCena, true) };
            GarantirEntradaLegada();
        }

        // O jogo usa o Input clássico (teclado). Em Unity 6 o padrão pode ser só o novo Input System.
        static void GarantirEntradaLegada()
        {
            try
            {
                Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
                if (assets == null || assets.Length == 0) return;
                SerializedObject so = new SerializedObject(assets[0]);
                SerializedProperty p = so.FindProperty("activeInputHandler");
                if (p != null && p.intValue == 1)
                {
                    p.intValue = 2; // 0 = antigo, 1 = novo, 2 = ambos
                    so.ApplyModifiedPropertiesWithoutUndo();
                    Debug.LogWarning("[Monge Arcano] Entrada de teclado ajustada para 'Ambos'. Se o Unity pedir, reinicie o editor.");
                }
            }
            catch (System.Exception e) { Debug.LogWarning("[Monge Arcano] Não foi possível ajustar a entrada: " + e.Message); }
        }
    }

    /// <summary>Na primeira abertura do projeto, gera a cena sozinho (se ainda não existir).</summary>
    [InitializeOnLoad]
    public static class AutoSetup
    {
        static AutoSetup()
        {
            EditorApplication.delayCall += Rodar;
        }

        static void Rodar()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
            if (!File.Exists(ConstrutorDeCena.CaminhoCena)) ConstrutorDeCena.Gerar();
        }
    }
}
