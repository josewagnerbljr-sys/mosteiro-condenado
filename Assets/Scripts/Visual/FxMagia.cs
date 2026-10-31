using UnityEngine;

namespace MongeArcano
{
    /// <summary>
    /// Efeitos de magia mais ricos que bolas de luz: estrela de energia, anel rúnico giratório, chama, fumaça mágica, escudo hexagonal e círculo de runas no chão.
    /// Todas as texturas são geradas por código (sem arquivos extras).
    /// </summary>
    public static class FxMagia
    {
        static Texture2D texEstrela, texRuna, texChama, texFumaca, texHex, texFaisca;

        static float Ruido(float x, float y)
        {
            return Mathf.PerlinNoise(x, y);
        }

        static Texture2D Criar(int tam, System.Func<float, float, float> f)
        {
            Color32[] px = new Color32[tam * tam];
            for (int j = 0; j < tam; j++)
                for (int i = 0; i < tam; i++)
                {
                    float x = (i + 0.5f) / tam * 2f - 1f, y = (j + 0.5f) / tam * 2f - 1f;
                    byte a = (byte)(Mathf.Clamp01(f(x, y)) * 255f);
                    px[j * tam + i] = new Color32(255, 255, 255, a);
                }
            Texture2D t = new Texture2D(tam, tam, TextureFormat.RGBA32, false);
            t.SetPixels32(px);
            t.Apply();
            t.wrapMode = TextureWrapMode.Clamp;
            t.hideFlags = HideFlags.DontSave;
            return t;
        }

        /// <summary>Estrela de 4 pontas com núcleo brilhante (brilho de energia).</summary>
        public static Texture2D Estrela()
        {
            if (texEstrela == null) texEstrela = Criar(128, (x, y) =>
            {
                float d = Mathf.Sqrt(x * x + y * y);
                float nucleo = Mathf.Pow(Mathf.Clamp01(1f - d * 1.6f), 2f);
                float cruz = Mathf.Pow(Mathf.Clamp01(1f - Mathf.Abs(x) * 7f), 2f) * Mathf.Clamp01(1f - Mathf.Abs(y)) + Mathf.Pow(Mathf.Clamp01(1f - Mathf.Abs(y) * 7f), 2f) * Mathf.Clamp01(1f - Mathf.Abs(x));
                float halo = Mathf.Pow(Mathf.Clamp01(1f - d), 2.6f) * 0.55f;
                return nucleo + cruz * 0.8f + halo;
            });
            return texEstrela;
        }

        /// <summary>Círculo mágico: aro duplo, marcas de runa e um triângulo central.</summary>
        public static Texture2D Runa()
        {
            if (texRuna == null) texRuna = Criar(256, (x, y) =>
            {
                float d = Mathf.Sqrt(x * x + y * y), a = Mathf.Atan2(y, x);
                float aro1 = Mathf.Clamp01(1f - Mathf.Abs(d - 0.92f) / 0.025f);
                float aro2 = Mathf.Clamp01(1f - Mathf.Abs(d - 0.80f) / 0.015f);
                float marcas = 0f;
                if (d > 0.82f && d < 0.90f) { float s = Mathf.Abs(Mathf.Sin(a * 12f)); marcas = s > 0.82f ? 1f : 0f; }
                float tri = 0f;
                for (int k = 0; k < 3; k++)
                {
                    float ang = a - k * 2.0944f - 1.5708f;
                    float r = 0.55f / Mathf.Cos(Mathf.Repeat(ang + 1.0472f, 2.0944f) - 1.0472f);
                    tri = Mathf.Max(tri, Mathf.Clamp01(1f - Mathf.Abs(d - r * 0.5f) / 0.02f) * (d < 0.6f ? 1f : 0f));
                }
                float glifos = (d > 0.62f && d < 0.78f && Mathf.Abs(Mathf.Sin(a * 6f + d * 9f)) > 0.9f) ? 0.8f : 0f;
                float miolo = Mathf.Pow(Mathf.Clamp01(1f - d), 3f) * 0.35f;
                return aro1 + aro2 * 0.8f + marcas * 0.9f + tri * 0.7f + glifos + miolo;
            });
            return texRuna;
        }

        /// <summary>Chama em forma de gota com bordas irregulares.</summary>
        public static Texture2D Chama()
        {
            if (texChama == null) texChama = Criar(128, (x, y) =>
            {
                float yy = (y + 1f) * 0.5f;
                float larg = Mathf.Lerp(0.55f, 0.02f, Mathf.Pow(yy, 0.8f)) * (yy < 0.1f ? yy * 10f : 1f);
                float n = Ruido(x * 3.2f + 5f, y * 3.2f) * 0.35f;
                float d = Mathf.Abs(x) / Mathf.Max(0.02f, larg + n * 0.2f);
                return Mathf.Pow(Mathf.Clamp01(1f - d), 1.4f) * Mathf.Clamp01(1.15f - yy) * (0.7f + 0.5f * n * 2f);
            });
            return texChama;
        }

        public static Texture2D Fumaca()
        {
            if (texFumaca == null) texFumaca = Criar(128, (x, y) =>
            {
                float d = Mathf.Sqrt(x * x + y * y);
                float n = Ruido(x * 2.4f + 3f, y * 2.4f + 3f) * 0.6f + Ruido(x * 5f, y * 5f) * 0.4f;
                return Mathf.Clamp01(1f - d) * Mathf.Clamp01(n * 1.5f);
            });
            return texFumaca;
        }

        /// <summary>Faísca alongada (traço de luz).</summary>
        public static Texture2D Faisca()
        {
            if (texFaisca == null) texFaisca = Criar(64, (x, y) => Mathf.Pow(Mathf.Clamp01(1f - Mathf.Abs(x)), 3f) * Mathf.Pow(Mathf.Clamp01(1f - Mathf.Abs(y) * 5f), 2f));
            return texFaisca;
        }

        /// <summary>Malha hexagonal para o escudo místico.</summary>
        public static Texture2D Hex()
        {
            if (texHex == null)
            {
                int tam = 128;
                Color32[] px = new Color32[tam * tam];
                for (int j = 0; j < tam; j++)
                    for (int i = 0; i < tam; i++)
                    {
                        float x = i / (float)tam * 4f, y = j / (float)tam * 4f * 1.1547f;
                        float row = Mathf.Floor(y);
                        float xo = x + (((int)row % 2) == 0 ? 0f : 0.5f);
                        float fx = Mathf.Abs(Mathf.Repeat(xo, 1f) - 0.5f) * 2f, fy = Mathf.Abs(Mathf.Repeat(y, 1f) - 0.5f) * 2f;
                        float borda = Mathf.Max(fx, fy * 0.9f + fx * 0.3f);
                        byte a = (byte)(Mathf.Clamp01((borda - 0.82f) / 0.14f) * 255f);
                        px[j * tam + i] = new Color32(255, 255, 255, a);
                    }
                texHex = new Texture2D(tam, tam, TextureFormat.RGBA32, false);
                texHex.SetPixels32(px);
                texHex.Apply();
                texHex.wrapMode = TextureWrapMode.Repeat;
                texHex.hideFlags = HideFlags.DontSave;
            }
            return texHex;
        }

        static GameObject Camada(Transform pai, Texture2D tex, Color cor, float tam, float giro, float pulso)
        {
            GameObject g = Fx.Quad(pai.position, tex, cor, tam, tam, 1f, true, false);
            g.transform.SetParent(pai, true);
            FxQuad q = g.GetComponent<FxQuad>();
            q.perene = true;
            q.pulso = pulso;
            q.giro = giro;
            return g;
        }

        /// <summary>Núcleo de projétil: estrela branca, brilho colorido e aro rúnico girando.</summary>
        public static void Nucleo(Transform pai, Color cor, float tam)
        {
            Camada(pai, Estrela(), Color.Lerp(cor, Color.white, 0.75f), tam * 1.0f, 70f, 0.18f);
            Camada(pai, Fx.Brilho(), cor, tam * 1.9f, 0f, 0.12f);
            Camada(pai, Runa(), cor, tam * 1.5f, -160f, 0.05f);
        }

        /// <summary>Impacto: clarão em estrela, onda de choque, traços de faísca e fumaça mágica.</summary>
        public static void Impacto(Vector3 pos, Color cor, float tam)
        {
            GameObject f = Fx.Quad(pos, Estrela(), Color.Lerp(cor, Color.white, 0.6f), tam * 0.5f, tam * 1.7f, 0.28f, true, true);
            f.GetComponent<FxQuad>().giro = 90f;
            Fx.Quad(pos, Fx.Anel(), cor, tam * 0.2f, tam * 1.6f, 0.4f, true, false);
            Fx.Quad(new Vector3(pos.x, Mathf.Max(0.06f, pos.y * 0.2f), pos.z), Runa(), cor, tam * 0.3f, tam * 1.4f, 0.6f, false, false);
            Fumar(pos, cor, tam);
            Traços(pos, cor, tam);
        }

        static void Fumar(Vector3 pos, Color cor, float tam)
        {
            GameObject go = new GameObject("FxFumaca");
            go.transform.position = pos;
            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.MainModule m = ps.main;
            m.loop = false;
            m.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.1f);
            m.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 1.1f * Mathf.Min(tam, 3f));
            m.startSize = new ParticleSystem.MinMaxCurve(tam * 0.35f, tam * 0.7f);
            m.startColor = new Color(cor.r * 0.8f, cor.g * 0.8f, cor.b * 0.9f, 0.55f);
            m.gravityModifier = -0.05f;
            m.stopAction = ParticleSystemStopAction.Destroy;
            ParticleSystem.EmissionModule e = ps.emission;
            e.rateOverTime = 0f;
            e.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, (short)Mathf.Clamp(Mathf.RoundToInt(tam * 3f), 4, 14)) });
            ParticleSystem.ShapeModule sh = ps.shape;
            sh.shapeType = ParticleSystemShapeType.Sphere;
            sh.radius = 0.15f * tam;
            ParticleSystem.ColorOverLifetimeModule cl = ps.colorOverLifetime;
            cl.enabled = true;
            Gradient g = new Gradient();
            g.SetKeys(new GradientColorKey[] { new GradientColorKey(Color.Lerp(cor, Color.white, 0.4f), 0f), new GradientColorKey(cor * 0.5f, 1f) },
                      new GradientAlphaKey[] { new GradientAlphaKey(0.8f, 0f), new GradientAlphaKey(0f, 1f) });
            cl.color = new ParticleSystem.MinMaxGradient(g);
            ParticleSystem.SizeOverLifetimeModule sz = ps.sizeOverLifetime;
            sz.enabled = true;
            sz.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.4f), new Keyframe(1f, 1.4f)));
            ParticleSystemRenderer r = go.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = Fx.NovoMaterial(Fumaca(), Color.white);
            ps.Play();
            Object.Destroy(go, 3f);
        }

        static void Traços(Vector3 pos, Color cor, float tam)
        {
            GameObject go = new GameObject("FxTracos");
            go.transform.position = pos;
            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.MainModule m = ps.main;
            m.loop = false;
            m.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.55f);
            m.startSpeed = new ParticleSystem.MinMaxCurve(3f, 8f + tam * 2f);
            m.startSize = new ParticleSystem.MinMaxCurve(0.18f, 0.4f);
            m.startColor = Color.Lerp(cor, Color.white, 0.55f);
            m.gravityModifier = 0.8f;
            m.stopAction = ParticleSystemStopAction.Destroy;
            ParticleSystem.EmissionModule e = ps.emission;
            e.rateOverTime = 0f;
            e.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, (short)Mathf.Clamp(Mathf.RoundToInt(10f + tam * 6f), 10, 60)) });
            ParticleSystem.ShapeModule sh = ps.shape;
            sh.shapeType = ParticleSystemShapeType.Sphere;
            sh.radius = 0.1f;
            ParticleSystem.SizeOverLifetimeModule sz = ps.sizeOverLifetime;
            sz.enabled = true;
            sz.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f)));
            ParticleSystemRenderer r = go.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = Fx.NovoMaterial(Faisca(), Color.white);
            r.renderMode = ParticleSystemRenderMode.Stretch;
            r.velocityScale = 0.12f;
            r.lengthScale = 2.2f;
            ps.Play();
            Object.Destroy(go, 2f);
        }

        /// <summary>Nova Sagrada: círculo de runas que se expande no chão, colunas de luz e uma onda de choque.</summary>
        public static void NovaRunica(Vector3 centro, Color cor, float raio)
        {
            Vector3 chao = new Vector3(centro.x, 0.07f, centro.z);
            GameObject r1 = Fx.Quad(chao, Runa(), cor, raio * 0.2f, raio * 1.9f, 1.1f, false, false);
            r1.GetComponent<FxQuad>().giro = 80f;
            GameObject r2 = Fx.Quad(chao + Vector3.up * 0.01f, Runa(), Color.Lerp(cor, Color.white, 0.5f), raio * 0.15f, raio * 1.2f, 0.9f, false, false);
            r2.GetComponent<FxQuad>().giro = -140f;
            Fx.Quad(chao + Vector3.up * 0.02f, Fx.Anel(), Color.Lerp(cor, Color.white, 0.3f), 0.5f, raio * 2.1f, 0.55f, false, false);
            GameObject f = Fx.Quad(centro, Estrela(), Color.white, raio * 0.4f, raio * 1.2f, 0.4f, true, true);
            f.GetComponent<FxQuad>().giro = 120f;
            for (int i = 0; i < 8; i++)
            {
                float a = i * Mathf.PI * 2f / 8f;
                Vector3 p = chao + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * raio * 0.55f;
                Fx.Faiscas(p + Vector3.up * 0.4f, cor, 14, 3.5f, 0.2f, 1.2f);
            }
            Fumar(chao + Vector3.up * 0.3f, cor, raio * 0.3f);
        }

        /// <summary>Aplica a malha hexagonal giratória à esfera do escudo.</summary>
        public static void EscudoHex(GameObject esfera, Color cor)
        {
            Renderer r = esfera.GetComponent<Renderer>();
            Material m = Fx.NovoMaterial(Hex(), cor);
            m.mainTextureScale = new Vector2(3f, 2f);
            r.sharedMaterial = m;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            EscudoGira g = esfera.GetComponent<EscudoGira>();
            if (g == null) esfera.AddComponent<EscudoGira>();
        }
    }

    /// <summary>Faz a malha do escudo deslizar e pulsar.</summary>
    public class EscudoGira : MonoBehaviour
    {
        Material m;
        void Start() { m = GetComponent<Renderer>().sharedMaterial; }
        void Update()
        {
            if (m == null) return;
            m.mainTextureOffset = new Vector2(Time.time * 0.12f, Time.time * 0.05f);
            float p = 1f + 0.03f * Mathf.Sin(Time.time * 6f);
            transform.localScale = Vector3.one * 2.3f * p;
        }
    }
}
