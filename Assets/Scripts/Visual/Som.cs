using System.Collections.Generic;
using UnityEngine;

namespace MongeArcano
{
    public enum TipoSom { Disparo, Impacto, Golpe, Explosao, Dano, Morte, Coleta, Porta, Trancada, Vazio, Altar, Grunhido, Cura, Nova, Leque, Escudo, Super, LevelUp, Missao, Bau, Passo, Encanto, Grito }

    /// <summary>
    /// Áudio 100% procedural: nenhum arquivo de som é necessário (zero risco de direitos autorais e build WebGL leve).
    /// </summary>
    public static class Som
    {
        const int Taxa = 22050;
        static readonly Dictionary<TipoSom, AudioClip> cache = new Dictionary<TipoSom, AudioClip>();
        static AudioClip ambiente;
        static readonly System.Random rnd = new System.Random(7);

        public static void Tocar(TipoSom tipo, Vector3 pos, float volume)
        {
            AudioClip clip;
            if (!cache.TryGetValue(tipo, out clip) || clip == null)
            {
                clip = Gerar(tipo);
                cache[tipo] = clip;
            }
            AudioSource.PlayClipAtPoint(clip, pos, Mathf.Clamp01(volume));
        }

        public static AudioClip Ambiente()
        {
            if (ambiente != null) return ambiente;
            const float dur = 8f;
            int n = (int)(Taxa * dur);
            float[] d = new float[n];
            float vento = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Taxa;
                float trem = 0.7f + 0.3f * Mathf.Sin(2f * Mathf.PI * 0.25f * t);
                float s = Mathf.Sin(2f * Mathf.PI * 55f * t) * 0.35f
                        + Mathf.Sin(2f * Mathf.PI * 82.5f * t) * 0.2f
                        + Mathf.Sin(2f * Mathf.PI * 110f * t) * 0.1f * Mathf.Sin(2f * Mathf.PI * 0.125f * t);
                vento += (Ruido() - vento) * 0.015f;
                d[i] = Mathf.Clamp((s * trem + vento * 0.5f) * 0.6f, -1f, 1f);
            }
            ambiente = AudioClip.Create("ambiente", n, 1, Taxa, false);
            ambiente.SetData(d, 0);
            return ambiente;
        }

        static float Ruido() { return (float)(rnd.NextDouble() * 2.0 - 1.0); }

        delegate float Gerador(float t, float p);

        static AudioClip Sintetizar(string nome, float dur, Gerador g)
        {
            int n = (int)(Taxa * dur);
            float[] d = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Taxa;
                float p = i / (float)n;
                d[i] = Mathf.Clamp(g(t, p), -1f, 1f);
            }
            AudioClip c = AudioClip.Create(nome, n, 1, Taxa, false);
            c.SetData(d, 0);
            return c;
        }

        static float Varredura(float t, float dur, float f0, float f1)
        {
            float k = (f1 - f0) / dur;
            return Mathf.Sin(2f * Mathf.PI * (f0 * t + 0.5f * k * t * t));
        }

        static AudioClip Gerar(TipoSom tipo)
        {
            switch (tipo)
            {
                case TipoSom.Disparo:
                    return Sintetizar("disparo", 0.35f, (t, p) => Varredura(t, 0.35f, 1000f, 220f) * (1f - p) * (1f - p) * 0.5f + Ruido() * 0.08f * (1f - p));
                case TipoSom.Impacto:
                    return Sintetizar("impacto", 0.25f, (t, p) => (Ruido() * 0.5f + Mathf.Sin(2f * Mathf.PI * 120f * t) * 0.5f) * Mathf.Pow(1f - p, 3f) * 0.6f);
                case TipoSom.Golpe:
                    return Sintetizar("golpe", 0.2f, (t, p) => Ruido() * Mathf.Pow(1f - p, 2f) * 0.5f * Mathf.Sin(p * Mathf.PI));
                case TipoSom.Explosao:
                    return Sintetizar("explosao", 0.9f, (t, p) => (Ruido() * 0.6f + Mathf.Sin(2f * Mathf.PI * 60f * t) * 0.6f) * Mathf.Pow(1f - p, 2f) * 0.8f);
                case TipoSom.Dano:
                    return Sintetizar("dano", 0.3f, (t, p) => (Varredura(t, 0.3f, 190f, 80f) * 0.6f + Ruido() * 0.25f) * (1f - p));
                case TipoSom.Morte:
                    return Sintetizar("morte", 0.8f, (t, p) => (Varredura(t, 0.8f, 240f, 40f) * 0.6f + Ruido() * 0.15f) * (1f - p));
                case TipoSom.Coleta:
                    return Sintetizar("coleta", 0.35f, (t, p) => (Mathf.Sin(2f * Mathf.PI * 880f * t) + Mathf.Sin(2f * Mathf.PI * 1320f * t) * 0.6f) * 0.25f * (1f - p));
                case TipoSom.Porta:
                    return Sintetizar("porta", 1.3f, (t, p) => (Ruido() * 0.35f + Mathf.Sin(2f * Mathf.PI * 48f * t) * 0.5f) * Mathf.Sin(p * Mathf.PI) * 0.7f);
                case TipoSom.Trancada:
                    return Sintetizar("trancada", 0.3f, (t, p) => Mathf.Sin(2f * Mathf.PI * 150f * t) * (p < 0.4f ? 1f - p / 0.4f : (p > 0.5f && p < 0.9f ? 1f - (p - 0.5f) / 0.4f : 0f)) * 0.6f);
                case TipoSom.Vazio:
                    return Sintetizar("vazio", 0.12f, (t, p) => Ruido() * (1f - p) * 0.3f);
                case TipoSom.Altar:
                    return Sintetizar("altar", 1.3f, (t, p) => (Mathf.Sin(2f * Mathf.PI * 261.6f * t) + Mathf.Sin(2f * Mathf.PI * 329.6f * t) + Mathf.Sin(2f * Mathf.PI * 392f * t)) * 0.18f * (1f - p));
                case TipoSom.Grunhido:
                    return Sintetizar("grunhido", 0.7f, (t, p) => (Mathf.Sin(2f * Mathf.PI * (70f + 20f * Mathf.Sin(2f * Mathf.PI * 6f * t)) * t) * 0.6f + Ruido() * 0.2f) * Mathf.Sin(p * Mathf.PI) * 0.7f);
                case TipoSom.Nova:
                    return Sintetizar("nova", 1.0f, (t, p) => p < 0.5f
                        ? Varredura(t, 1f, 100f, 700f) * Mathf.Sin(p * Mathf.PI) * 0.4f + Ruido() * 0.05f * p
                        : (Mathf.Sin(2f * Mathf.PI * 60f * t) * 0.7f + Ruido() * 0.4f) * Mathf.Exp(-(t - 0.5f) * 6f));
                case TipoSom.Leque:
                    return Sintetizar("leque", 0.35f, (t, p) => Varredura(t, 0.35f, 1200f, 400f) * (0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * 40f * t)) * (1f - p) * 0.5f);
                case TipoSom.Escudo:
                    return Sintetizar("escudo", 0.8f, (t, p) => (Mathf.Sin(2f * Mathf.PI * 523.25f * t) + Mathf.Sin(2f * Mathf.PI * 659.25f * t) + Mathf.Sin(2f * Mathf.PI * 130f * t) * 0.6f) * 0.2f * (1f - p));
                case TipoSom.Super:
                    return Sintetizar("super", 2.0f, (t, p) => p < 0.5f
                        ? Varredura(t, 1f, 80f, 1200f) * p * 1.2f + Ruido() * 0.1f * p
                        : (Mathf.Sin(2f * Mathf.PI * 50f * t) * 0.8f + Ruido() * 0.6f) * Mathf.Exp(-(t - 1f) * 3f));
                case TipoSom.LevelUp:
                    return Sintetizar("levelup", 1.0f, (t, p) => Mathf.Sin(2f * Mathf.PI * new float[] { 523.25f, 659.25f, 783.99f, 1046.5f }[Mathf.Min(3, (int)(t / 0.15f))] * t) * 0.3f * (1f - p));
                case TipoSom.Missao:
                    return Sintetizar("missao", 0.9f, (t, p) => t < 0.25f
                        ? Mathf.Sin(2f * Mathf.PI * 392f * t) * 0.3f
                        : (Mathf.Sin(2f * Mathf.PI * 523.25f * t) + Mathf.Sin(2f * Mathf.PI * 659.25f * t)) * 0.2f * (1f - p));
                case TipoSom.Bau:
                    return Sintetizar("bau", 0.7f, (t, p) => p < 0.7f
                        ? (Varredura(t, 0.5f, 120f, 240f) * 0.4f + Ruido() * 0.15f) * (0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * 25f * t))
                        : Mathf.Sin(2f * Mathf.PI * 1318f * t) * 0.25f * (1f - p));
                case TipoSom.Passo:
                    return Sintetizar("passo", 0.12f, (t, p) => Mathf.Sin(2f * Mathf.PI * 70f * t) * Mathf.Exp(-t * 40f) * 0.5f + Ruido() * Mathf.Exp(-t * 60f) * 0.2f);
                case TipoSom.Encanto:
                    return Sintetizar("encanto", 3.2f, (t, p) =>
                    {
                        int sil = Mathf.Min(9, (int)(t * 3.1f));
                        float local = t * 3.1f - sil;
                        float[] fm = sil % 3 == 0 ? new float[] { 800f, 1200f } : (sil % 3 == 1 ? new float[] { 350f, 2300f } : new float[] { 450f, 800f });
                        float f0 = 175f + 25f * Mathf.Sin(sil * 1.7f) + 10f * Mathf.Sin(2f * Mathf.PI * 5f * t);
                        float v = 0f;
                        for (int k = 1; k <= 14; k++)
                        {
                            float fk = f0 * k;
                            float g = Mathf.Exp(-Mathf.Pow((fk - fm[0]) / 220f, 2f)) + 0.7f * Mathf.Exp(-Mathf.Pow((fk - fm[1]) / 320f, 2f));
                            v += g * Mathf.Sin(2f * Mathf.PI * fk * t) / Mathf.Pow(k, 0.3f);
                        }
                        float env = Mathf.Sin(Mathf.Clamp01(local) * Mathf.PI) * (1f - p * 0.3f);
                        return (v * 0.12f + Ruido() * 0.03f) * env;
                    });
                case TipoSom.Grito:
                    return Sintetizar("grito", 1.1f, (t, p) => (Varredura(t, 1.1f, 320f, 90f) * 0.45f + Ruido() * 0.35f * (1f - p)) * Mathf.Sin(Mathf.Clamp01(p * 1.15f) * Mathf.PI) * 0.8f);
                default:
                    return Sintetizar("cura", 0.5f, (t, p) => Varredura(t, 0.5f, 440f, 880f) * 0.3f * (1f - p));
            }
        }
    }
}
