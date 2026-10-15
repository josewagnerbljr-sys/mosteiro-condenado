using UnityEngine;

namespace MongeArcano
{
    /// <summary>Trilhas geradas por código: exploração (misteriosa), combate (pulsante) e chefe (intensa). Sem arquivos de áudio.</summary>
    public static class Musica
    {
        const int Taxa = 22050;
        const float PI2 = 6.2831853f;
        static readonly System.Random rnd = new System.Random(11);

        static float Hz(int midi) { return 440f * Mathf.Pow(2f, (midi - 69) / 12f); }

        static void Nota(float[] d, float inicio, float dur, float freq, float vol, int tipo)
        {
            int i0 = (int)(inicio * Taxa);
            int n = (int)(dur * Taxa);
            for (int k = 0; k < n; k++)
            {
                int idx = (i0 + k) % d.Length;
                float t = k / (float)Taxa;
                float p = k / (float)n;
                float env, s;
                switch (tipo)
                {
                    case 0: // pad
                        env = Mathf.Min(1f, t / 1.2f) * Mathf.Pow(1f - p, 1.2f);
                        s = Mathf.Sin(PI2 * freq * t) + 0.4f * Mathf.Sin(PI2 * freq * 2.003f * t) + 0.2f * Mathf.Sin(PI2 * freq * 0.5f * t);
                        break;
                    case 1: // sino
                        env = Mathf.Exp(-t * 2.2f);
                        s = Mathf.Sin(PI2 * freq * t) + 0.5f * Mathf.Sin(PI2 * freq * 2.76f * t) * Mathf.Exp(-t * 4f) + 0.25f * Mathf.Sin(PI2 * freq * 5.4f * t) * Mathf.Exp(-t * 6f);
                        break;
                    case 2: // baixo
                        env = Mathf.Min(1f, t / 0.01f) * Mathf.Exp(-t * 3f);
                        s = ((freq * t) % 1f < 0.5f ? 0.5f : -0.5f) + Mathf.Sin(PI2 * freq * t) * 0.5f;
                        break;
                    case 3: // golpe serrilhado
                        env = Mathf.Min(1f, t / 0.005f) * Mathf.Exp(-t * 7f);
                        s = (((freq * t) % 1f) * 2f - 1f) * 0.6f + (((freq * 1.01f * t) % 1f) * 2f - 1f) * 0.4f;
                        break;
                    case 4: // bumbo
                        env = Mathf.Exp(-t * 9f);
                        s = Mathf.Sin(PI2 * (40f * t + (110f / 25f) * (1f - Mathf.Exp(-25f * t))));
                        break;
                    default: // chimbal
                        env = Mathf.Exp(-t * 40f);
                        s = (float)(rnd.NextDouble() * 2.0 - 1.0);
                        break;
                }
                d[idx] += s * env * vol;
            }
        }

        static AudioClip Finalizar(string nome, float[] d)
        {
            for (int i = 0; i < d.Length; i++) d[i] = (float)System.Math.Tanh(d[i]);
            AudioClip c = AudioClip.Create(nome, d.Length, 1, Taxa, false);
            c.SetData(d, 0);
            return c;
        }

        public static AudioClip Explorar()
        {
            float dur = 16f;
            float[] d = new float[(int)(Taxa * dur)];
            int[][] acordes = { new[] { 45, 52, 57, 60 }, new[] { 41, 48, 53, 57 }, new[] { 38, 45, 50, 53 }, new[] { 40, 47, 50, 56 } };
            for (int c = 0; c < 4; c++)
                for (int k = 0; k < 4; k++) Nota(d, c * 4f, 5.5f, Hz(acordes[c][k]), 0.09f, 0);
            for (int i = 0; i < d.Length; i++)
            {
                float t = i / (float)Taxa;
                d[i] += Mathf.Sin(PI2 * 55f * t) * 0.10f * (0.7f + 0.3f * Mathf.Sin(PI2 * t / 8f));
            }
            int[] penta = { 69, 72, 74, 76, 79, 81 };
            for (int b = 0; b < 10; b++)
            {
                float quando = b * 1.6f + (float)rnd.NextDouble() * 0.8f;
                Nota(d, quando, 3f, Hz(penta[rnd.Next(penta.Length)]), 0.07f, 1);
            }
            return Finalizar("musica_explorar", d);
        }

        public static AudioClip Combate()
        {
            float dur = 8f;
            float[] d = new float[(int)(Taxa * dur)];
            for (int b = 0; b < 16; b++) Nota(d, b * 0.5f, 0.4f, 0f, 0.5f, 4);
            int[] padrao = { 33, 33, 36, 33, 31, 33, 36, 38 };
            for (int j = 0; j < 32; j++) Nota(d, j * 0.25f, 0.24f, Hz(padrao[j % 8]), 0.2f, 2);
            for (int j = 0; j < 16; j++) Nota(d, j * 0.5f + 0.25f, 0.08f, 0f, 0.12f, 5);
            float[] stabs = { 0f, 1.5f, 3f, 4f, 5.5f, 7f };
            int[] acorde = { 57, 60, 63 };
            for (int s = 0; s < stabs.Length; s++)
                for (int k = 0; k < acorde.Length; k++) Nota(d, stabs[s], 0.5f, Hz(acorde[k]), 0.07f, 3);
            Nota(d, 0f, 8f, Hz(45), 0.05f, 0);
            Nota(d, 0f, 8f, Hz(51), 0.05f, 0);
            return Finalizar("musica_combate", d);
        }

        public static AudioClip Chefe()
        {
            float beat = 60f / 140f;
            float dur = 16f * beat;
            float[] d = new float[(int)(Taxa * dur)];
            for (int b = 0; b < 16; b++) Nota(d, b * beat, 0.4f, 0f, 0.55f, 4);
            int[] padrao = { 33, 33, 45, 33, 31, 43, 33, 36 };
            for (int j = 0; j < 32; j++) Nota(d, j * beat / 2f, beat / 2f - 0.02f, Hz(padrao[j % 8]), 0.22f, 2);
            for (int j = 0; j < 32; j++) Nota(d, j * beat / 2f + beat / 4f, 0.06f, 0f, 0.1f, 5);
            int[] acorde = { 45, 51, 57, 60 };
            for (int k = 0; k < acorde.Length; k++) Nota(d, 0f, dur, Hz(acorde[k]), 0.07f, 0);
            for (int s = 0; s < 8; s++)
                for (int k = 0; k < 3; k++) Nota(d, s * 2f * beat, 0.45f, Hz(57 + k * 3 + (s % 2)), 0.06f, 3);
            return Finalizar("musica_chefe", d);
        }
    }
}
