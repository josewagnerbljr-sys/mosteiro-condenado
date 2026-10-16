using UnityEngine;

namespace MongeArcano
{
    /// <summary>Efeitos visuais estilo ARPG sombrio: brilho aditivo, rastros, faíscas, anéis no chão e brasas. Texturas geradas por código.</summary>
    public static class Fx
    {
        static Texture2D texBrilho, texAnel;
        static Material baseAditiva;

        static Texture2D Gerar(int tam, System.Func<float, float> f)
        {
            Color32[] px = new Color32[tam * tam];
            for (int j = 0; j < tam; j++)
                for (int i = 0; i < tam; i++)
                {
                    float x = (i + 0.5f) / tam * 2f - 1f, y = (j + 0.5f) / tam * 2f - 1f;
                    float d = Mathf.Sqrt(x * x + y * y);
                    byte a = (byte)(Mathf.Clamp01(f(d)) * 255f);
                    px[j * tam + i] = new Color32(255, 255, 255, a);
                }
            Texture2D t = new Texture2D(tam, tam, TextureFormat.RGBA32, false);
            t.SetPixels32(px);
            t.Apply();
            t.wrapMode = TextureWrapMode.Clamp;
            t.hideFlags = HideFlags.DontSave;
            return t;
        }

        public static Texture2D Brilho()
        {
            if (texBrilho == null) texBrilho = Gerar(64, d => Mathf.Pow(Mathf.Clamp01(1f - d), 2.2f));
            return texBrilho;
        }

        public static Texture2D Anel()
        {
            if (texAnel == null) texAnel = Gerar(96, d => Mathf.Clamp01(1f - Mathf.Abs(d - 0.82f) / 0.14f) + 0.18f * Mathf.Clamp01(1f - d));
            return texAnel;
        }

        static Material Aditivo()
        {
            if (baseAditiva == null)
            {
                baseAditiva = new Material(ProceduralModels.Mat("FxAditivo"));
                baseAditiva.mainTexture = Brilho();
                baseAditiva.hideFlags = HideFlags.DontSave;
            }
            return baseAditiva;
        }

        public static Material NovoMaterial(Texture2D tex, Color cor)
        {
            Material m = new Material(Aditivo());
            m.mainTexture = tex;
            SetCor(m, cor);
            return m;
        }

        public static void SetCor(Material m, Color c)
        {
            Color t = new Color(c.r * 0.5f, c.g * 0.5f, c.b * 0.5f, c.a * 0.5f);
            if (m.HasProperty("_TintColor")) m.SetColor("_TintColor", t);
            else m.color = c;
        }

        static Gradient Desvanecer(Color c)
        {
            Gradient g = new Gradient();
            g.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.Lerp(c, Color.white, 0.5f), 0f), new GradientColorKey(c, 0.4f), new GradientColorKey(c * 0.3f, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.8f, 0.5f), new GradientAlphaKey(0f, 1f) });
            return g;
        }

        // ------------------------------------------------------------ quads (clarão, anel, halo)
        public static GameObject Quad(Vector3 pos, Texture2D tex, Color cor, float ini, float fim, float dur, bool billboard, bool comLuz)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = "FxQuad";
            Collider c = go.GetComponent<Collider>();
            c.enabled = false;
            Object.Destroy(c);
            go.transform.position = pos;
            if (!billboard) go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            Renderer r = go.GetComponent<Renderer>();
            r.sharedMaterial = NovoMaterial(tex, cor);
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            FxQuad q = go.AddComponent<FxQuad>();
            q.billboard = billboard;
            q.tamanhoInicial = ini;
            q.tamanhoFinal = fim;
            q.duracao = Mathf.Max(0.05f, dur);
            q.cor = cor;
            if (comLuz)
            {
                Light l = go.AddComponent<Light>();
                l.type = LightType.Point;
                l.color = cor;
                l.range = Mathf.Max(4f, fim * 2f);
                l.intensity = 2.5f;
                l.shadows = LightShadows.None;
                q.luz = l;
            }
            return go;
        }

        public static void Halo(Transform pai, Color cor, float tam)
        {
            GameObject g = Quad(pai.position, Brilho(), cor, tam, tam, 1f, true, false);
            g.transform.SetParent(pai, true);
            FxQuad q = g.GetComponent<FxQuad>();
            q.perene = true;
            q.pulso = 0.12f;
        }

        public static void Explodir(Vector3 pos, Color cor, float tam, float dur)
        {
            Quad(pos, Brilho(), cor, tam * 0.4f, tam * 1.5f, dur * 0.7f, true, true);
            Quad(new Vector3(pos.x, 0.08f, pos.z), Anel(), cor, 0.3f, tam, dur, false, false);
            Faiscas(pos, cor, Mathf.Clamp(Mathf.RoundToInt(tam * 7f), 10, 100), 2.5f + tam * 0.7f, 0.1f + tam * 0.015f, dur + 0.6f);
        }

        // ------------------------------------------------------------ partículas
        public static ParticleSystem Faiscas(Vector3 pos, Color cor, int quantidade, float vel, float tamanho, float vida)
        {
            GameObject go = new GameObject("FxFaiscas");
            go.transform.position = pos;
            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.MainModule main = ps.main;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(vida * 0.6f, vida);
            main.startSpeed = new ParticleSystem.MinMaxCurve(vel * 0.4f, vel);
            main.startSize = new ParticleSystem.MinMaxCurve(tamanho * 0.5f, tamanho);
            main.startColor = cor;
            main.gravityModifier = 0.6f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.stopAction = ParticleSystemStopAction.Destroy;
            ParticleSystem.EmissionModule em = ps.emission;
            em.rateOverTime = 0f;
            em.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, (short)quantidade) });
            ParticleSystem.ShapeModule sh = ps.shape;
            sh.shapeType = ParticleSystemShapeType.Sphere;
            sh.radius = 0.15f;
            ParticleSystem.ColorOverLifetimeModule cl = ps.colorOverLifetime;
            cl.enabled = true;
            cl.color = new ParticleSystem.MinMaxGradient(Desvanecer(cor));
            ParticleSystem.SizeOverLifetimeModule sz = ps.sizeOverLifetime;
            sz.enabled = true;
            sz.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f)));
            go.GetComponent<ParticleSystemRenderer>().sharedMaterial = Aditivo();
            ps.Play();
            Object.Destroy(go, vida + 1.5f);
            return ps;
        }

        public static void FaiscasContinuas(Transform pai, Color cor)
        {
            GameObject go = new GameObject("FxRastroFaiscas");
            go.transform.SetParent(pai, false);
            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.MainModule main = ps.main;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.6f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 1.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.12f);
            main.startColor = cor;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            ParticleSystem.EmissionModule em = ps.emission;
            em.rateOverTime = 45f;
            ParticleSystem.ShapeModule sh = ps.shape;
            sh.shapeType = ParticleSystemShapeType.Sphere;
            sh.radius = 0.1f;
            ParticleSystem.ColorOverLifetimeModule cl = ps.colorOverLifetime;
            cl.enabled = true;
            cl.color = new ParticleSystem.MinMaxGradient(Desvanecer(cor));
            ParticleSystem.SizeOverLifetimeModule sz = ps.sizeOverLifetime;
            sz.enabled = true;
            sz.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f)));
            go.GetComponent<ParticleSystemRenderer>().sharedMaterial = Aditivo();
            ps.Play();
        }

        public static void Rastro(GameObject go, Color cor, float largura)
        {
            TrailRenderer tr = go.AddComponent<TrailRenderer>();
            tr.time = 0.4f;
            tr.startWidth = largura;
            tr.endWidth = 0f;
            tr.sharedMaterial = NovoMaterial(Brilho(), Color.white);
            tr.colorGradient = Desvanecer(cor);
            tr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        /// <summary>Brasas flutuando no ar (ambiente). Emissor em caixa.</summary>
        public static void Brasas(Transform pai, Vector3 centro, Vector3 tamanho, Color cor)
        {
            GameObject go = new GameObject("FxBrasas");
            go.transform.SetParent(pai, false);
            go.transform.position = centro;
            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.MainModule main = ps.main;
            main.loop = true;
            main.prewarm = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(6f, 9f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.15f, 0.6f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.12f);
            main.startColor = cor;
            main.gravityModifier = -0.02f;
            main.maxParticles = 700;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            ParticleSystem.EmissionModule em = ps.emission;
            em.rateOverTime = 40f;
            ParticleSystem.ShapeModule sh = ps.shape;
            sh.shapeType = ParticleSystemShapeType.Box;
            sh.scale = tamanho;
            ParticleSystem.ColorOverLifetimeModule cl = ps.colorOverLifetime;
            cl.enabled = true;
            cl.color = new ParticleSystem.MinMaxGradient(Desvanecer(cor));
            go.GetComponent<ParticleSystemRenderer>().sharedMaterial = Aditivo();
            ps.Play();
        }
    }
}
