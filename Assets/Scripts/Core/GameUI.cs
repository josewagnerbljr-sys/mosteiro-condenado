using System.Collections.Generic;
using UnityEngine;

namespace MongeArcano
{
    /// <summary>Interface inteira em IMGUI (OnGUI): HUD, título, pausa/inventário, notas, derrota e vitória. Sem dependências externas.</summary>
    public class GameUI : MonoBehaviour
    {
        GUIStyle titulo, grande, normal, pequeno, quebra, centro;
        float s = 1f;

        void CriarEstilos()
        {
            titulo = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            grande = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            normal = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
            pequeno = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold };
            quebra = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.UpperLeft, wordWrap = true };
            centro = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.UpperCenter, wordWrap = true };
            titulo.normal.textColor = grande.normal.textColor = normal.normal.textColor = Color.white;
            pequeno.normal.textColor = quebra.normal.textColor = centro.normal.textColor = Color.white;
        }

        void Retangulo(Rect r, Color c)
        {
            Color o = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = o;
        }

        void Rotulo(Rect r, string t, GUIStyle st, Color cor)
        {
            Color o = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.85f);
            GUI.Label(new Rect(r.x + 2f * s, r.y + 2f * s, r.width, r.height), t, st);
            GUI.color = cor;
            GUI.Label(r, t, st);
            GUI.color = o;
        }

        void Barra(Rect r, float frac, Color cor)
        {
            Retangulo(new Rect(r.x - 2f, r.y - 2f, r.width + 4f, r.height + 4f), new Color(0f, 0f, 0f, 0.7f));
            Retangulo(r, new Color(0.15f, 0.15f, 0.15f, 0.9f));
            Retangulo(new Rect(r.x, r.y, r.width * Mathf.Clamp01(frac), r.height), cor);
        }

        void OnGUI()
        {
            GameManager gm = GameManager.Instance;
            if (gm == null || gm.jogador == null) return;
            if (titulo == null) CriarEstilos();

            s = Screen.height / 720f;
            titulo.fontSize = Mathf.RoundToInt(64f * s);
            grande.fontSize = Mathf.RoundToInt(34f * s);
            normal.fontSize = Mathf.RoundToInt(22f * s);
            pequeno.fontSize = Mathf.RoundToInt(17f * s);
            quebra.fontSize = Mathf.RoundToInt(18f * s);
            centro.fontSize = Mathf.RoundToInt(20f * s);

            float W = Screen.width, H = Screen.height;

            if (gm.Estado != EstadoJogo.Titulo) DesenharHUD(gm, W, H);

            switch (gm.Estado)
            {
                case EstadoJogo.Titulo: Titulo(W, H); break;
                case EstadoJogo.Pausado: Pausa(gm, W, H); break;
                case EstadoJogo.Lendo: Nota(gm, W, H); break;
                case EstadoJogo.FimDeJogo: FimDeJogo(W, H); break;
                case EstadoJogo.Vitoria: Vitoria(gm, W, H); break;
            }
        }

        void DesenharHUD(GameManager gm, float W, float H)
        {
            PlayerController p = gm.jogador;
            float x = 20f * s, y = 14f * s;
            float f = p.FracaoVida;
            string cond; Color cor;
            if (f > 0.6f) { cond = "BOM"; cor = new Color(0.3f, 1f, 0.4f); }
            else if (f > 0.3f) { cond = "CUIDADO"; cor = new Color(1f, 0.85f, 0.2f); }
            else { cond = "PERIGO"; cor = new Color(1f, 0.25f, 0.2f); }

            Rotulo(new Rect(x, y, 320f * s, 26f * s), "CONDIÇÃO: " + cond, pequeno, cor);
            y += 30f * s;
            Barra(new Rect(x, y, 260f * s, 14f * s), f, cor);
            Rotulo(new Rect(x + 272f * s, y - 6f * s, 220f * s, 24f * s), "VIDA " + Mathf.CeilToInt(p.vida) + "/" + Mathf.CeilToInt(p.vidaMax), pequeno, Color.white);
            y += 26f * s;
            Barra(new Rect(x, y, 260f * s, 14f * s), p.mana / p.manaMax, new Color(0.3f, 0.65f, 1f));
            Rotulo(new Rect(x + 272f * s, y - 6f * s, 260f * s, 24f * s), "ESSÊNCIA " + Mathf.FloorToInt(p.mana) + "/" + Mathf.FloorToInt(p.manaMax), pequeno, new Color(0.6f, 0.85f, 1f));
            y += 26f * s;
            Rotulo(new Rect(x, y, 320f * s, 24f * s), "ERVAS SAGRADAS: " + p.ervas + "   [H] usar", pequeno, new Color(0.5f, 1f, 0.5f));
            y += 24f * s;
            if (gm.Chaves.Count > 0)
            {
                var lista = new List<string>();
                foreach (string c in gm.Chaves) lista.Add(GameManager.NomeChave(c));
                Rotulo(new Rect(x, y, 600f * s, 24f * s), "ITENS: " + string.Join(", ", lista.ToArray()), pequeno, new Color(1f, 0.85f, 0.4f));
            }

            // objetivo
            float bw = 360f * s, bx = W - bw - 20f * s;
            Retangulo(new Rect(bx, 14f * s, bw, 86f * s), new Color(0f, 0f, 0f, 0.55f));
            Rotulo(new Rect(bx + 10f * s, 16f * s, bw, 22f * s), "OBJETIVO", pequeno, new Color(1f, 0.8f, 0.3f));
            GUI.Label(new Rect(bx + 10f * s, 40f * s, bw - 20f * s, 60f * s), gm.Objetivo, quebra);

            // chefe
            if (gm.chefe != null && gm.chefe.Ativo)
            {
                float cw = 520f * s;
                Rotulo(new Rect((W - cw) / 2f, 8f * s, cw, 26f * s), "ARCONTE DAS CINZAS", new GUIStyle(pequeno) { alignment = TextAnchor.MiddleCenter }, new Color(1f, 0.4f, 0.4f));
                Barra(new Rect((W - cw) / 2f, 38f * s, cw, 14f * s), gm.chefe.FracaoVida, new Color(0.8f, 0.1f, 0.15f));
            }

            if (gm.Estado == EstadoJogo.Jogando)
            {
                if (Time.unscaledTime < gm.MensagemAte && !string.IsNullOrEmpty(gm.Mensagem))
                    Rotulo(new Rect(0f, H - 150f * s, W, 40f * s), gm.Mensagem, normal, Color.white);
                if (!string.IsNullOrEmpty(gm.Dica))
                    Rotulo(new Rect(0f, H - 100f * s, W, 34f * s), gm.Dica, normal, new Color(1f, 0.9f, 0.5f));
            }

            float flash = gm.FlashAte - Time.unscaledTime;
            if (flash > 0f) Retangulo(new Rect(0f, 0f, W, H), new Color(0.8f, 0f, 0f, Mathf.Clamp01(flash / 0.4f) * 0.35f));
            if (f < 0.3f && p.vida > 0f)
                Retangulo(new Rect(0f, 0f, W, H), new Color(0.5f, 0f, 0f, 0.1f + 0.08f * Mathf.Sin(Time.unscaledTime * 6f)));
        }

        void Titulo(float W, float H)
        {
            Retangulo(new Rect(0f, 0f, W, H), new Color(0f, 0f, 0f, 0.72f));
            Rotulo(new Rect(0f, H * 0.10f, W, 90f * s), "MONGE ARCANO", titulo, new Color(1f, 0.75f, 0.25f));
            Rotulo(new Rect(0f, H * 0.10f + 80f * s, W, 50f * s), "O MOSTEIRO CONDENADO", grande, new Color(0.85f, 0.2f, 0.2f));
            string ajuda =
                "W/S ou setas: andar e recuar (controles de tanque)    A/D: girar\n" +
                "SHIFT: correr     Q: giro rápido de 180°\n" +
                "ESPAÇO ou clique: Raio Arcano (1 essência, mira automática)\n" +
                "F: Explosão Sagrada (5 essência, empurra tudo)     V: golpe de cajado\n" +
                "E: interagir, pegar e ler     H: usar Erva Sagrada     TAB ou P: pausa";
            GUI.Label(new Rect(W * 0.15f, H * 0.38f, W * 0.7f, 200f * s), ajuda, centro);
            float pulso = 0.6f + 0.4f * Mathf.Sin(Time.unscaledTime * 3f);
            Rotulo(new Rect(0f, H * 0.78f, W, 40f * s), "Pressione ENTER para começar", normal, new Color(1f, 1f, 1f, pulso));
            Rotulo(new Rect(0f, H - 40f * s, W, 30f * s), "Desafio DIO - Trilha Unity  |  Survival horror 3D com câmeras fixas", new GUIStyle(pequeno) { alignment = TextAnchor.MiddleCenter }, new Color(0.7f, 0.7f, 0.7f));
        }

        void Pausa(GameManager gm, float W, float H)
        {
            Retangulo(new Rect(0f, 0f, W, H), new Color(0f, 0f, 0f, 0.8f));
            Rotulo(new Rect(0f, H * 0.08f, W, 60f * s), "PAUSA / INVENTÁRIO", grande, Color.white);
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("ESSÊNCIA: " + Mathf.FloorToInt(gm.jogador.mana) + "/" + Mathf.FloorToInt(gm.jogador.manaMax) + "     ERVAS: " + gm.jogador.ervas);
            sb.AppendLine();
            sb.AppendLine("ITENS CHAVE:");
            if (gm.Chaves.Count == 0) sb.AppendLine("  (nenhum)");
            foreach (string c in gm.Chaves) sb.AppendLine("  - " + GameManager.NomeChave(c));
            sb.AppendLine();
            sb.AppendLine("ARQUIVOS LIDOS (" + gm.NotasLidas.Count + "/" + GameManager.TotalNotas + "):");
            if (gm.NotasLidas.Count == 0) sb.AppendLine("  (nenhum)");
            foreach (string n in gm.NotasLidas) sb.AppendLine("  - " + n);
            sb.AppendLine();
            sb.AppendLine("OBJETIVO: " + gm.Objetivo);
            GUI.Label(new Rect(W * 0.2f, H * 0.2f, W * 0.6f, H * 0.6f), sb.ToString(), quebra);
            Rotulo(new Rect(0f, H * 0.88f, W, 34f * s), "TAB / P / ENTER: continuar      R: reiniciar o jogo", normal, new Color(1f, 0.9f, 0.5f));
        }

        void Nota(GameManager gm, float W, float H)
        {
            Retangulo(new Rect(0f, 0f, W, H), new Color(0f, 0f, 0f, 0.6f));
            float bw = W * 0.62f, bh = H * 0.62f, bx = (W - bw) / 2f, by = (H - bh) / 2f;
            Retangulo(new Rect(bx - 4f, by - 4f, bw + 8f, bh + 8f), new Color(0.25f, 0.15f, 0.05f, 1f));
            Retangulo(new Rect(bx, by, bw, bh), new Color(0.85f, 0.78f, 0.6f, 1f));
            GUIStyle t = new GUIStyle(grande);
            t.normal.textColor = new Color(0.25f, 0.1f, 0.05f);
            GUI.Label(new Rect(bx, by + 14f * s, bw, 50f * s), gm.NotaTitulo, t);
            GUIStyle q = new GUIStyle(quebra);
            q.normal.textColor = new Color(0.15f, 0.08f, 0.04f);
            q.fontSize = Mathf.RoundToInt(21f * s);
            GUI.Label(new Rect(bx + 30f * s, by + 80f * s, bw - 60f * s, bh - 130f * s), gm.NotaTexto, q);
            GUIStyle f = new GUIStyle(pequeno);
            f.alignment = TextAnchor.MiddleCenter;
            f.normal.textColor = new Color(0.3f, 0.1f, 0.05f);
            GUI.Label(new Rect(bx, by + bh - 40f * s, bw, 30f * s), "[E] ou [ENTER] para fechar", f);
        }

        void FimDeJogo(float W, float H)
        {
            Retangulo(new Rect(0f, 0f, W, H), new Color(0.35f, 0f, 0f, 0.85f));
            Rotulo(new Rect(0f, H * 0.30f, W, 90f * s), "VOCÊ FOI CONSUMIDO", titulo, new Color(1f, 0.2f, 0.2f));
            Rotulo(new Rect(0f, H * 0.55f, W, 40f * s), "ENTER: continuar do último altar", normal, Color.white);
            Rotulo(new Rect(0f, H * 0.62f, W, 40f * s), "R: reiniciar o jogo", normal, new Color(0.8f, 0.8f, 0.8f));
        }

        void Vitoria(GameManager gm, float W, float H)
        {
            Retangulo(new Rect(0f, 0f, W, H), new Color(0.95f, 0.85f, 0.5f, 0.85f));
            GUIStyle t = new GUIStyle(titulo);
            t.normal.textColor = new Color(0.25f, 0.1f, 0f);
            GUI.Label(new Rect(0f, H * 0.18f, W, 90f * s), "O MAL FOI SELADO", t);
            int min = Mathf.FloorToInt(gm.TempoDeJogo / 60f);
            int seg = Mathf.FloorToInt(gm.TempoDeJogo % 60f);
            GUIStyle n = new GUIStyle(normal);
            n.normal.textColor = new Color(0.2f, 0.08f, 0f);
            GUI.Label(new Rect(0f, H * 0.40f, W, 40f * s), "Tempo: " + min.ToString("00") + ":" + seg.ToString("00"), n);
            GUI.Label(new Rect(0f, H * 0.47f, W, 40f * s), "Criaturas banidas: " + gm.InimigosDerrotados, n);
            GUI.Label(new Rect(0f, H * 0.54f, W, 40f * s), "Arquivos encontrados: " + gm.NotasLidas.Count + "/" + GameManager.TotalNotas, n);
            GUI.Label(new Rect(0f, H * 0.68f, W, 40f * s), "Pressione R ou ENTER para jogar de novo", n);
        }
    }
}
