using System.Collections.Generic;
using UnityEngine;

namespace MongeArcano
{
    public enum Colisor { Nenhum, Caixa, Tronco }

    /// <summary>Guarda no cenário as referências dos modelos 3D usados, para instanciá-los também durante o jogo (ex.: invocações).</summary>
    public class ModelosRegistro : MonoBehaviour
    {
        public static ModelosRegistro Instancia;

        [System.Serializable]
        public class Entrada { public string nome; public GameObject prefab; }
        public List<Entrada> entradas = new List<Entrada>();

        void Awake() { Instancia = this; }

        public GameObject Prefab(string nome)
        {
            for (int i = 0; i < entradas.Count; i++) if (entradas[i].nome == nome) return entradas[i].prefab;
            return null;
        }

        public void Registrar(string nome, GameObject prefab)
        {
            if (prefab == null || Prefab(nome) != null) return;
            entradas.Add(new Entrada { nome = nome, prefab = prefab });
        }
    }

    /// <summary>
    /// Carrega modelos .glb importados (Assets/Modelos), normaliza altura e pivô (pés no chão, centrado), pinta com materiais próprios
    /// quando o modelo não tem textura (os vindos de 3MF) e adiciona colisores. Se o modelo não existir, devolve null e o jogo usa a forma simples.
    /// </summary>
    public static class Modelos
    {
        /// <summary>Definido pelo editor ao gerar a cena: procura o asset pelo nome.</summary>
        public static System.Func<string, GameObject> ProvedorEditor;
        public static ModelosRegistro Registro;

        static readonly Dictionary<string, string> pinturaPadrao = new Dictionary<string, string>
        {
            { "monge_clerigo", "MongeTunica" }, { "cavaleiro_sombrio", "Armadura" }, { "necromante", "NecromanteRoupa" },
            { "bruxa", "BruxaNegra" }, { "criptas", "PedraMusgo" }, { "lapides", "PedraMusgo" }, { "arvores_mortas", "MadeiraMorta" },
            { "dancarinos_cemiterio", "Fantasma" }, { "altar_ossos", "Osso" }, { "altar_foco_boss", "PedraClara" },
            { "monasterio_exterior", "Pedra" }, { "monasterio_interior", "Pedra" }, { "jardim_sombrio", "Terra" }
        };

        public static GameObject Prefab(string nome)
        {
            GameObject p = null;
            if (ProvedorEditor != null)
            {
                p = ProvedorEditor(nome);
                if (p != null && Registro != null) Registro.Registrar(nome, p);
            }
            else if (ModelosRegistro.Instancia != null) p = ModelosRegistro.Instancia.Prefab(nome);
            return p;
        }

        static void Destruir(Object o)
        {
            if (o == null) return;
            if (Application.isPlaying) Object.Destroy(o); else Object.DestroyImmediate(o);
        }

        static Bounds Limites(GameObject go)
        {
            Renderer[] rs = go.GetComponentsInChildren<Renderer>();
            if (rs.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
            Bounds b = rs[0].bounds;
            for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
            return b;
        }

        public static void Pintar(GameObject go, string material)
        {
            if (string.IsNullOrEmpty(material)) return;
            Material m = ProceduralModels.Mat(material);
            Renderer[] rs = go.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < rs.Length; i++)
            {
                Material[] arr = new Material[Mathf.Max(1, rs[i].sharedMaterials.Length)];
                for (int k = 0; k < arr.Length; k++) arr[k] = m;
                rs[i].sharedMaterials = arr;
            }
        }

        static void Sombras(GameObject go)
        {
            Renderer[] rs = go.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < rs.Length; i++)
            {
                rs[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                rs[i].receiveShadows = false;
            }
        }

        /// <summary>Cria uma instância normalizada: altura (ou maior dimensão) = alvo, pés em y=0, centrada. Devolve o objeto-pai já posicionado em "pai".</summary>
        public static GameObject Instanciar(string nome, Transform pai, float alvo, float rotY, string pintura = null, Colisor colisor = Colisor.Nenhum, bool pelaMaiorDimensao = false)
        {
            GameObject prefab = Prefab(nome);
            if (prefab == null) return null;
            GameObject inst = Object.Instantiate(prefab);
            inst.name = nome + "_modelo";
            string tinta = pintura;
            if (tinta == null) pinturaPadrao.TryGetValue(nome, out tinta);
            return Finalizar(inst, nome, pai, alvo, rotY, tinta, colisor, pelaMaiorDimensao);
        }

        static GameObject Finalizar(GameObject inst, string nome, Transform pai, float alvo, float rotY, string tinta, Colisor colisor, bool maior)
        {
            GameObject w = new GameObject(nome);
            w.transform.position = Vector3.zero;
            w.transform.rotation = Quaternion.identity;
            inst.transform.SetParent(w.transform, true);

            Pintar(w, tinta);
            Sombras(w);

            Bounds b = Limites(w);
            float dim = maior ? Mathf.Max(b.size.x, Mathf.Max(b.size.y, b.size.z)) : b.size.y;
            float s = (alvo > 0f && dim > 0.0001f) ? alvo / dim : 1f;
            inst.transform.position -= new Vector3(b.center.x, b.min.y, b.center.z);
            w.transform.localScale = Vector3.one * s;

            if (colisor != Colisor.Nenhum)
            {
                Bounds b2 = Limites(w);
                if (colisor == Colisor.Caixa)
                {
                    BoxCollider bc = w.AddComponent<BoxCollider>();
                    bc.center = b2.center / s;
                    bc.size = b2.size / s;
                }
                else
                {
                    CapsuleCollider cc = w.AddComponent<CapsuleCollider>();
                    cc.center = new Vector3(0f, b2.size.y * 0.5f / s, 0f);
                    cc.height = b2.size.y / s;
                    cc.radius = Mathf.Min(b2.size.x, b2.size.z) * 0.12f / s;
                }
            }

            w.transform.SetParent(pai, false);
            w.transform.localPosition = Vector3.zero;
            w.transform.localRotation = Quaternion.Euler(0f, rotY, 0f);
            return w;
        }

        /// <summary>Para arquivos com vários objetos (lápides, criptas, árvores): devolve um molde por objeto (desativados). Mantém a proporção entre os objetos e descarta lascas.</summary>
        public static List<GameObject> Variantes(string nome, float alvo, string pintura = null, Colisor colisor = Colisor.Nenhum)
        {
            List<GameObject> lista = new List<GameObject>();
            GameObject prefab = Prefab(nome);
            if (prefab == null) return lista;
            GameObject temp = Object.Instantiate(prefab);
            MeshFilter[] mfs = temp.GetComponentsInChildren<MeshFilter>();
            string tinta = pintura;
            if (tinta == null) pinturaPadrao.TryGetValue(nome, out tinta);

            float[] alturas = new float[mfs.Length];
            float maior = 0.0001f;
            for (int i = 0; i < mfs.Length; i++)
            {
                Renderer r = mfs[i].GetComponent<Renderer>();
                alturas[i] = r != null ? r.bounds.size.y : 0f;
                if (alturas[i] > maior) maior = alturas[i];
            }
            for (int i = 0; i < mfs.Length; i++)
            {
                if (alturas[i] < maior * 0.25f) continue; // lasca ou base solta
                float alvoI = alvo * alturas[i] / maior;
                GameObject w = Finalizar(mfs[i].gameObject, nome + "_v" + i, null, alvoI, 0f, tinta ?? "", colisor, false);
                w.SetActive(false);
                lista.Add(w);
            }
            Destruir(temp);
            return lista;
        }

        /// <summary>Gatilho esférico com raio em metros reais (ignora a escala do pai).</summary>
        public static GameObject Gatilho(string nome, Transform pai, Vector3 posMundo, float raio)
        {
            GameObject g = new GameObject(nome);
            g.transform.SetParent(pai, false);
            g.transform.position = posMundo;
            SphereCollider sc = g.AddComponent<SphereCollider>();
            sc.isTrigger = true;
            sc.radius = raio;
            return g;
        }

        public static GameObject Clonar(List<GameObject> moldes, System.Random rnd, Transform pai, Vector3 pos, float rotY, float escalaMin, float escalaMax)
        {
            GameObject m = moldes[rnd.Next(moldes.Count)];
            GameObject c = Object.Instantiate(m);
            c.name = m.name;
            c.SetActive(true);
            c.transform.SetParent(pai, false);
            float e = escalaMin + (float)rnd.NextDouble() * (escalaMax - escalaMin);
            c.transform.localScale = m.transform.localScale * e;
            c.transform.localPosition = pos;
            c.transform.localRotation = Quaternion.Euler(0f, rotY, 0f);
            return c;
        }

        public static void Liberar(List<GameObject> moldes)
        {
            for (int i = 0; i < moldes.Count; i++) Destruir(moldes[i]);
            moldes.Clear();
        }
    }
}
