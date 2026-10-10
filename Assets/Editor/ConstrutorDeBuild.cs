using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace MongeArcano.EditorTools
{
    /// <summary>Build WebGL na pasta /docs (pronta para GitHub Pages). Também funciona em linha de comando.</summary>
    public static class ConstrutorDeBuild
    {
        [MenuItem("Monge Arcano/2 - Build WebGL (pasta docs)")]
        public static void BuildWebGLPeloMenu()
        {
            BuildWebGL();
        }

        [MenuItem("Monge Arcano/3 - Gerar cena + Build WebGL")]
        public static void TudoPeloMenu()
        {
            ConstrutorDeCena.Gerar();
            BuildWebGL();
        }

        // Linha de comando: Unity -batchmode -quit -projectPath . -executeMethod MongeArcano.EditorTools.ConstrutorDeBuild.BuildWebGL
        public static void BuildWebGL()
        {
            if (!File.Exists(ConstrutorDeCena.CaminhoCena)) ConstrutorDeCena.Gerar();
            ConstrutorDeCena.ConfigurarProjeto();

            var opcoes = new BuildPlayerOptions
            {
                scenes = new[] { ConstrutorDeCena.CaminhoCena },
                locationPathName = "docs",
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            BuildReport relatorio = BuildPipeline.BuildPlayer(opcoes);
            if (relatorio.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError("[Monge Arcano] Build WebGL falhou: " + relatorio.summary.result);
                if (Application.isBatchMode) EditorApplication.Exit(1);
                return;
            }

            File.WriteAllText(Path.Combine("docs", ".nojekyll"), "");
            Debug.Log("[Monge Arcano] Build WebGL pronto na pasta 'docs'. Faça commit e ative o GitHub Pages (branch main, pasta /docs).");
        }
    }
}
