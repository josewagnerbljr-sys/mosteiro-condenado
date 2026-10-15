using UnityEngine;

namespace MongeArcano
{
    /// <summary>Troca a trilha conforme a situação: exploração, combate (inimigo desperto por perto) e chefe.</summary>
    public class MusicaDinamica : MonoBehaviour
    {
        readonly AudioSource[] fontes = new AudioSource[3];
        readonly float[] vol = new float[3];
        bool iniciado;
        float proxCheque;
        int alvo;

        public void Iniciar()
        {
            if (iniciado) return;
            iniciado = true;
            AudioClip[] clips = { Musica.Explorar(), Musica.Combate(), Musica.Chefe() };
            for (int i = 0; i < 3; i++)
            {
                fontes[i] = gameObject.AddComponent<AudioSource>();
                fontes[i].clip = clips[i];
                fontes[i].loop = true;
                fontes[i].spatialBlend = 0f;
                fontes[i].volume = 0f;
                fontes[i].Play();
            }
        }

        void Update()
        {
            if (!iniciado) return;
            if (Time.unscaledTime >= proxCheque)
            {
                proxCheque = Time.unscaledTime + 0.3f;
                alvo = Escolher();
            }
            for (int i = 0; i < 3; i++)
            {
                float v = i == alvo ? (i == 0 ? 0.45f : 0.55f) : 0f;
                vol[i] = Mathf.MoveTowards(vol[i], v, 0.7f * Time.unscaledDeltaTime);
                fontes[i].volume = vol[i];
            }
        }

        int Escolher()
        {
            GameManager gm = GameManager.Instance;
            if (gm == null || gm.jogador == null) return 0;
            bool chefe = false, combate = false;
            for (int i = 0; i < Inimigo.Ativos.Count; i++)
            {
                Inimigo e = Inimigo.Ativos[i];
                if (e == null || !e.Ativo) continue;
                float d = Vector3.Distance(e.transform.position, gm.jogador.transform.position);
                if (e.chefe && d < 30f) chefe = true;
                else if (d < 14f) combate = true;
            }
            return chefe ? 2 : (combate ? 1 : 0);
        }
    }
}
