using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MongeArcano.EditorTools
{
    /// <summary>Relatório automático: confere se os GLB, o esqueleto e as animações UAL funcionam depois da importação (sem precisar jogar).</summary>
    public static class DiagnosticoUal
    {
        static readonly string[] modelos =
        {
            "Assets/Modelos/Personagens/monge_clerigo.glb", "Assets/Modelos/Personagens/bruxa.glb", "Assets/Modelos/Inimigos/esqueleto_guerreiro.glb",
            "Assets/Modelos/Inimigos/armadura_assombrada.glb", "Assets/Modelos/Inimigos/boss_morte.glb", "Assets/Modelos/Inimigos/imp.glb", "Assets/Modelos/Inimigos/puglin.glb", "Assets/Modelos/Props/prop_bau.glb",
            "Assets/Modelos/Inimigos/monstro.glb", "Assets/Modelos/Inimigos/mensageiro.glb", "Assets/Modelos/Cenario/portao_gotico.glb"
        };

        static Transform Achar(Transform t, string nome)
        {
            if (t.name == nome) return t;
            for (int i = 0; i < t.childCount; i++) { Transform r = Achar(t.GetChild(i), nome); if (r != null) return r; }
            return null;
        }

        [MenuItem("Monge Arcano/Diagnóstico UAL (relatório)")]
        public static void Rodar()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== DIAGNÓSTICO UAL " + System.DateTime.Now + " ===");
            string animPath = "Assets/Modelos/Animacoes/ual_animacoes.glb";
            List<AnimationClip> clips = new List<AnimationClip>();
            foreach (Object o in AssetDatabase.LoadAllAssetsAtPath(animPath))
            {
                AnimationClip c = o as AnimationClip;
                if (c != null && !c.name.StartsWith("__preview__")) clips.Add(c);
            }
            sb.AppendLine("ual_animacoes.glb: clipes=" + clips.Count + (clips.Count > 0 ? " legacy=" + clips[0].legacy + " exemplo=" + clips[0].name + " duração=" + clips[0].length.ToString("0.00") : "  (ARQUIVO AUSENTE OU SEM CLIPES)"));
            AnimationClip walk = clips.Find(c => c.name == "Walk_Loop");
            AnimationClip jog = clips.Find(c => c.name == "Jog_Fwd_Loop");

            foreach (string path in modelos)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                string nome = Path.GetFileNameWithoutExtension(path);
                if (prefab == null) { sb.AppendLine(nome + ": AUSENTE"); continue; }
                GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                SkinnedMeshRenderer[] smr = inst.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                MeshRenderer[] mr = inst.GetComponentsInChildren<MeshRenderer>(true);
                Transform arm = Achar(inst.transform, "Armature"), pe = Achar(inst.transform, "foot_l"), mao = Achar(inst.transform, "hand_r");
                string linha = nome + ": skinned=" + smr.Length + " meshes=" + mr.Length + " bones=" + (smr.Length > 0 ? smr[0].bones.Length : 0) + " Armature=" + (arm != null) + " Animation=" + (inst.GetComponentInChildren<Animation>(true) != null);
                if (walk != null && pe != null && mao != null)
                {
                    walk.SampleAnimation(inst, 0f);
                    Vector3 p0 = pe.position, m0 = mao.position;
                    walk.SampleAnimation(inst, walk.length * 0.25f);
                    float dp = Vector3.Distance(p0, pe.position), dm = Vector3.Distance(m0, mao.position);
                    linha += "  | Walk: pé moveu " + dp.ToString("0.000") + " m, mão moveu " + dm.ToString("0.000") + " m => " + (dp > 0.05f ? "ANIMA" : "NÃO ANIMA");
                }
                else if (arm != null) linha += "  | (sem pé/mão para testar)";
                sb.AppendLine(linha);
                Object.DestroyImmediate(inst);
            }

            string cenaPath = "Assets/Scenes/Mosteiro.unity";
            if (File.Exists(cenaPath))
            {
                EditorSceneManager.OpenScene(cenaPath, OpenSceneMode.Single);
                UalAnimador[] us = Object.FindObjectsByType<UalAnimador>(FindObjectsInactive.Include);
                int comArm = 0;
                Dictionary<string, int> perfis = new Dictionary<string, int>();
                foreach (UalAnimador u in us)
                {
                    if (Achar(u.transform, "Armature") != null) comArm++;
                    string p = u.perfil.ToString();
                    perfis[p] = perfis.ContainsKey(p) ? perfis[p] + 1 : 1;
                }
                sb.AppendLine("Cena Mosteiro: UalAnimador=" + us.Length + " (com Armature: " + comArm + ")");
                foreach (KeyValuePair<string, int> kv in perfis) sb.AppendLine("   perfil " + kv.Key + ": " + kv.Value);
                BibliotecaUal bib = Object.FindAnyObjectByType<BibliotecaUal>(FindObjectsInactive.Include);
                sb.AppendLine("BibliotecaUal na cena: " + (bib != null) + (bib != null && bib.clipes != null ? "  clipes=" + bib.clipes.Length : ""));
                PlayerController pc = Object.FindAnyObjectByType<PlayerController>(FindObjectsInactive.Include);
                sb.AppendLine("Jogador na cena: " + (pc != null) + (pc != null ? "  UalAnimador no jogador=" + (pc.GetComponent<UalAnimador>() != null) : ""));
                foreach (ValsaEsqueletos v in Object.FindObjectsByType<ValsaEsqueletos>(FindObjectsInactive.Include))
                {
                    Renderer[] rs = v.GetComponentsInChildren<Renderer>(true);
                    Bounds b = rs.Length > 0 ? rs[0].bounds : new Bounds();
                    for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
                    sb.AppendLine("Valsa " + v.name + ": ativo=" + v.gameObject.activeInHierarchy + " pos=" + v.transform.position.ToString("0.0") + " filhos=" + v.transform.childCount + " renderers=" + rs.Length + " tamanho=" + b.size.ToString("0.00") + " a=" + (v.a != null) + " b=" + (v.b != null));
                }
                sb.AppendLine("Portão com PortaoDuplo: " + (Object.FindAnyObjectByType<PortaoDuplo>(FindObjectsInactive.Include) != null));
            }
            else sb.AppendLine("Cena Mosteiro.unity ausente (gere a cena).");

            string saida = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "diagnostico-ual.txt"));
            File.WriteAllText(saida, sb.ToString());
            Debug.Log(sb.ToString());
            Debug.Log("[Monge Arcano] Relatório salvo em " + saida);
        }
    }
}
