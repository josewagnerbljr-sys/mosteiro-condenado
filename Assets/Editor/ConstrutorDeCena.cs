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
            ConstrutorMundo.Construir();
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
