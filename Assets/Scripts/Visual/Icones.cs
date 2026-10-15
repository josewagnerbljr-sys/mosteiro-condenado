using System.Collections.Generic;
using UnityEngine;

namespace MongeArcano
{
    /// <summary>Ícones desenhados por código (sem arquivos de imagem): 5 magias e itens (erva, frasco, chaves, sigilos).</summary>
    public static class Icones
    {
        const int T = 48;
        static readonly Dictionary<string, Texture2D> cache = new Dictionary<string, Texture2D>();

        public static Texture2D Magia(int i) { return Obter("magia" + i); }
        public static Texture2D Item(string id) { return Obter("item_" + id); }

        static Texture2D Obter(string chave)
        {
            Texture2D t;
            if (cache.TryGetValue(chave, out t) && t != null) return t;
            t = Gerar(chave);
            cache[chave] = t;
            return t;
        }

        // coberturas suavizadas (coordenadas de -1 a 1)
        static float Disco(float x, float y, float cx, float cy, float r)
        {
            float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
            return Mathf.Clamp01((r - d) * T * 0.5f);
        }

        static float Anel(float x, float y, float r, float w)
        {
            float d = Mathf.Sqrt(x * x + y * y);
            return Mathf.Clamp01((w - Mathf.Abs(d - r)) * T * 0.5f);
        }

        static float Retang(float x, float y, float cx, float cy, float hw, float hh)
        {
            float d = Mathf.Max(Mathf.Abs(x - cx) - hw, Mathf.Abs(y - cy) - hh);
            return Mathf.Clamp01(0.5f - d * T * 0.5f);
        }

        static float Estrela(float x, float y, float pontas, float rMin, float rMax, float agudez)
        {
            float d = Mathf.Sqrt(x * x + y * y);
            float a = Mathf.Atan2(y, x);
            float r = rMin + rMax * Mathf.Pow(Mathf.Abs(Mathf.Cos(pontas * a * 0.5f)), agudez);
            return Mathf.Clamp01((r - d) * T * 0.5f);
        }

        static Texture2D Gerar(string chave)
        {
            Color cor = Color.white;
            switch (chave)
            {
                case "magia0": cor = new Color(0.35f, 0.7f, 1f); break;
                case "magia1": cor = new Color(1f, 0.85f, 0.3f); break;
                case "magia2": cor = new Color(0.4f, 1f, 0.55f); break;
                case "magia3": cor = new Color(0.4f, 0.95f, 1f); break;
                case "magia4": cor = new Color(0.75f, 0.4f, 1f); break;
                case "item_erva": cor = new Color(0.4f, 0.95f, 0.4f); break;
                case "item_frasco": cor = new Color(0.4f, 0.7f, 1f); break;
                case "item_ferro": cor = new Color(0.78f, 0.8f, 0.9f); break;
                case "item_osso": cor = new Color(0.92f, 0.88f, 0.75f); break;
                case "item_sigilo_fogo": cor = new Color(1f, 0.5f, 0.15f); break;
                case "item_sigilo_lua": cor = new Color(0.6f, 0.8f, 1f); break;
                case "item_sigilo_terra": cor = new Color(0.4f, 0.9f, 0.4f); break;
            }

            Color32[] px = new Color32[T * T];
            for (int j = 0; j < T; j++)
            {
                for (int i = 0; i < T; i++)
                {
                    float x = (i + 0.5f) / T * 2f - 1f;
                    float y = (j + 0.5f) / T * 2f - 1f;
                    float a = Cobertura(chave, x, y);
                    Color c = cor;
                    c.a = a;
                    px[j * T + i] = c;
                }
            }
            Texture2D tex = new Texture2D(T, T, TextureFormat.RGBA32, false);
            tex.SetPixels32(px);
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            tex.hideFlags = HideFlags.DontSave;
            return tex;
        }

        static float Cobertura(string chave, float x, float y)
        {
            switch (chave)
            {
                case "magia0": return Mathf.Max(Anel(x, y, 0.62f, 0.08f), Disco(x, y, 0f, 0f, 0.28f));
                case "magia1": return Mathf.Max(Estrela(x, y, 8f, 0.22f, 0.65f, 6f), Disco(x, y, 0f, 0f, 0.2f));
                case "magia2":
                    return Mathf.Max(Disco(x, y, -0.55f, 0.35f, 0.2f), Disco(x, y, 0f, 0.55f, 0.2f), Mathf.Max(Disco(x, y, 0.55f, 0.35f, 0.2f), Disco(x, y, 0f, -0.55f, 0.14f)));
                case "magia3":
                    {
                        float hex = Mathf.Max(Mathf.Abs(x) * 0.866f + Mathf.Abs(y) * 0.5f, Mathf.Abs(y));
                        float anel = Mathf.Clamp01((0.09f - Mathf.Abs(hex - 0.72f)) * T * 0.5f);
                        return Mathf.Max(anel, Disco(x, y, 0f, 0f, 0.2f));
                    }
                case "magia4": return Mathf.Max(Anel(x, y, 0.85f, 0.07f), Estrela(x, y, 5f, 0.12f, 0.6f, 3f) * 0.9f, Disco(x, y, 0f, 0f, 0.12f));
                case "item_erva":
                    {
                        float u = (x + y) * 0.707f, v = (x - y) * 0.707f;
                        float e = 1f - ((u / 0.75f) * (u / 0.75f) + (v / 0.3f) * (v / 0.3f));
                        return Mathf.Max(Mathf.Clamp01(e * 6f), Retang(x, y, -0.5f, -0.5f, 0.05f, 0.25f));
                    }
                case "item_frasco":
                    return Mathf.Max(Disco(x, y, 0f, -0.2f, 0.5f), Retang(x, y, 0f, 0.45f, 0.15f, 0.25f), Retang(x, y, 0f, 0.8f, 0.22f, 0.08f));
                case "item_ferro":
                case "item_osso":
                    return Mathf.Max(Anel(x + 0.45f, y, 0.28f, 0.1f), Retang(x, y, 0.15f, 0f, 0.55f, 0.08f), Retang(x, y, 0.58f, -0.2f, 0.07f, 0.2f), Retang(x, y, 0.38f, -0.15f, 0.07f, 0.15f));
                case "item_sigilo_fogo":
                case "item_sigilo_lua":
                case "item_sigilo_terra":
                    return Mathf.Max(Anel(x, y, 0.7f, 0.1f), Disco(x, y, 0f, 0f, 0.42f) * 0.75f);
                default:
                    return Retang(x, y, 0f, 0f, 0.6f, 0.6f);
            }
        }
    }
}
