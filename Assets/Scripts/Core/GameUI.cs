using System.Collections.Generic;
using UnityEngine;

namespace MongeArcano
{
    /// <summary>
    /// Interface em IMGUI: condição/vida/essência/poder/XP, atalhos 1-6 (embaixo), barra de magias (direita, arrastável na pausa),
    /// painel de missões, banners, aproximação cinematográfica (faixas pretas), título, pausa/inventário, notas, derrota e vitória.
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        GUIStyle titulo, grande, normal, pequeno, quebra, centro, minusculo;
        float s = 1f;
        readonly List<SlotInfo> slots = new List<SlotInfo>();
        Vector2 offHotbar, offMagias;
        bool prefsCarregadas;
        int arrastando;

        void CriarEstilos()
        {
            titulo = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            grande = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            normal = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
            pequeno = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold };
            minusculo = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            quebra = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.UpperLeft, wordWrap = true };
            centro = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.UpperCenter, wordWrap = true };
            titulo.normal.textColor = grande.normal.textColor = normal.normal.textColor = Color.white;
            pequeno.normal.textColor = quebra.normal.textColor = centro.normal.textColor = minusculo.normal.textColor = Color.white;
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

        void Icone(Rect r, Texture2D tex, Color cor)
        {
            Color o = GUI.color;
            GUI.color = cor;
            GUI.DrawTexture(r, tex);
            GUI.color = o;
        }

        void OnGUI()
        {
            GameManager gm = GameManager.Instance;
            if (gm == null || gm.jogador == null) return;
            if (titulo == null) CriarEstilos();
            if (!prefsCarregadas)
            {
                prefsCarregadas = true;
                offHotbar = new Vector2(PlayerPrefs.GetFloat("ui_hx", 0f), PlayerPrefs.GetFloat("ui_hy", 0f));
                offMagias = new Vector2(PlayerPrefs.GetFloat("ui_mx", 0f), PlayerPrefs.GetFloat("ui_my", 0f));
            }

            s = Screen.height / 720f;
            titulo.fontSize = Mathf.RoundToInt(64f * s);
            grande.fontSize = Mathf.RoundToInt(34f * s);
            normal.fontSize = Mathf.RoundToInt(22f * s);
            pequeno.fontSize = Mathf.RoundToInt(16f * s);
            minusculo.fontSize = Mathf.RoundToInt(12f * s);
            quebra.fontSize = Mathf.RoundToInt(17f * s);
            centro.fontSize = Mathf.RoundToInt(20f * s);

            float W = Screen.width, H = Screen.height;

            if (gm.Estado != EstadoJogo.Titulo && gm.Estado != EstadoJogo.Inventario)
            {
                DesenharHUD(gm, W, H);
                Hotbar(gm, W, H);
                BarraMagias(gm, W, H);
                Missoes(gm, W, H);
                BannerUI(gm, W, H);
                if (gm.Estado == EstadoJogo.Jogando) Mira(gm, W, H);
            }

            if (gm.Estado == EstadoJogo.Cinematica || gm.Estado == EstadoJogo.Lendo) Faixas(gm, W, H);

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
            float x = 20f * s, y = 12f * s;
            float f = p.FracaoVida;
            string cond; Color cor;
            if (f > 0.6f) { cond = "BOM"; cor = new Color(0.3f, 1f, 0.4f); }
            else if (f > 0.3f) { cond = "CUIDADO"; cor = new Color(1f, 0.85f, 0.2f); }
            else { cond = "PERIGO"; cor = new Color(1f, 0.25f, 0.2f); }

            Rotulo(new Rect(x, y, 360f * s, 24f * s), "NÍVEL " + p.nivel + "   CONDIÇÃO: " + cond, pequeno, cor);
            y += 28f * s;
            Barra(new Rect(x, y, 250f * s, 13f * s), f, cor);
            Rotulo(new Rect(x + 262f * s, y - 7f * s, 200f * s, 24f * s), "VIDA " + Mathf.CeilToInt(p.vida) + "/" + Mathf.CeilToInt(p.vidaMax), pequeno, Color.white);
            y += 24f * s;
            Barra(new Rect(x, y, 250f * s, 13f * s), p.mana / p.manaMax, new Color(0.3f, 0.65f, 1f));
            Rotulo(new Rect(x + 262f * s, y - 7f * s, 260f * s, 24f * s), "ESSÊNCIA " + Mathf.FloorToInt(p.mana) + "/" + Mathf.FloorToInt(p.manaMax), pequeno, new Color(0.6f, 0.85f, 1f));
            y += 24f * s;
            bool cheio = p.PoderCheio;
            Color corPoder = cheio ? Color.Lerp(new Color(1f, 0.7f, 0.2f), Color.white, 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 8f)) : new Color(0.9f, 0.65f, 0.15f);
            Barra(new Rect(x, y, 250f * s, 13f * s), p.poder / p.poderMax, corPoder);
            Rotulo(new Rect(x + 262f * s, y - 7f * s, 300f * s, 24f * s), cheio ? "PODER ARCANO PRONTO [R]" : "PODER ARCANO", pequeno, new Color(1f, 0.85f, 0.4f));
            y += 24f * s;
            Barra(new Rect(x, y, 250f * s, 8f * s), p.xp / p.XpProximo, new Color(0.5f, 1f, 0.6f));
            Rotulo(new Rect(x + 262f * s, y - 9f * s, 300f * s, 24f * s), "XP " + Mathf.FloorToInt(p.xp) + "/" + Mathf.FloorToInt(p.XpProximo), pequeno, new Color(0.7f, 1f, 0.75f));

            if (gm.chefe != null && gm.chefe.Ativo)
            {
                float cw = 520f * s;
                Rotulo(new Rect((W - cw) / 2f, 8f * s, cw, 26f * s), gm.chefe.nomeExibido.ToUpper(), new GUIStyle(pequeno) { alignment = TextAnchor.MiddleCenter }, new Color(1f, 0.4f, 0.4f));
                Barra(new Rect((W - cw) / 2f, 38f * s, cw, 14f * s), gm.chefe.FracaoVida, new Color(0.8f, 0.1f, 0.15f));
            }

            if (gm.Estado == EstadoJogo.Jogando)
            {
                if (Time.unscaledTime < gm.MensagemAte && !string.IsNullOrEmpty(gm.Mensagem))
                    Rotulo(new Rect(0f, H - 210f * s, W, 36f * s), gm.Mensagem, normal, Color.white);
                if (!string.IsNullOrEmpty(gm.Dica))
                    Rotulo(new Rect(0f, H - 165f * s, W, 34f * s), gm.Dica, normal, new Color(1f, 0.9f, 0.5f));
            }

            float flash = gm.FlashAte - Time.unscaledTime;
            if (flash > 0f) Retangulo(new Rect(0f, 0f, W, H), new Color(0.8f, 0f, 0f, Mathf.Clamp01(flash / 0.4f) * 0.35f));
            if (f < 0.3f && p.vida > 0f)
                Retangulo(new Rect(0f, 0f, W, H), new Color(0.5f, 0f, 0f, 0.1f + 0.08f * Mathf.Sin(Time.unscaledTime * 6f)));
            if (p.EscudoAtivo) Retangulo(new Rect(0f, 0f, W, H), new Color(0.3f, 0.8f, 1f, 0.06f));
        }

        bool Arrastar(Rect r, ref Vector2 off, int id, GameManager gm)
        {
            if (gm.Estado != EstadoJogo.Pausado) return false;
            Retangulo(new Rect(r.x - 5f, r.y - 5f, r.width + 10f, r.height + 10f), new Color(1f, 0.9f, 0.3f, 0.18f));
            Event e = Event.current;
            if (e.type == EventType.MouseDown && r.Contains(e.mousePosition)) { arrastando = id; e.Use(); }
            else if (e.type == EventType.MouseDrag && arrastando == id) { off += e.delta; e.Use(); }
            else if (e.type == EventType.MouseUp && arrastando == id)
            {
                arrastando = 0;
                PlayerPrefs.SetFloat("ui_hx", offHotbar.x); PlayerPrefs.SetFloat("ui_hy", offHotbar.y);
                PlayerPrefs.SetFloat("ui_mx", offMagias.x); PlayerPrefs.SetFloat("ui_my", offMagias.y);
                e.Use();
            }
            return true;
        }

        void Hotbar(GameManager gm, float W, float H)
        {
            gm.MontarSlots(slots);
            float tam = 56f * s, gap = 6f * s;
            float total = slots.Count * tam + (slots.Count - 1) * gap;
            Rect area = new Rect((W - total) / 2f + offHotbar.x, H - 82f * s + offHotbar.y, total, tam);
            Arrastar(area, ref offHotbar, 1, gm);
            for (int i = 0; i < slots.Count; i++)
            {
                Rect r = new Rect(area.x + i * (tam + gap), area.y, tam, tam);
                Retangulo(new Rect(r.x - 2f, r.y - 2f, r.width + 4f, r.height + 4f), new Color(0f, 0f, 0f, 0.75f));
                Retangulo(r, new Color(0.12f, 0.1f, 0.14f, 0.92f));
                SlotInfo sl = slots[i];
                if (sl.icone.Length > 0)
                {
                    bool vazio = sl.usavel && sl.qtd <= 0;
                    Icone(new Rect(r.x + 6f * s, r.y + 6f * s, tam - 12f * s, tam - 12f * s), Icones.Item(sl.icone), vazio ? new Color(1f, 1f, 1f, 0.25f) : Color.white);
                    if (sl.usavel) Rotulo(new Rect(r.x, r.y + tam - 18f * s, tam - 4f * s, 16f * s), "x" + sl.qtd, new GUIStyle(minusculo) { alignment = TextAnchor.MiddleRight }, Color.white);
                }
                Rotulo(new Rect(r.x + 3f * s, r.y, 20f * s, 16f * s), (i + 1).ToString(), new GUIStyle(minusculo) { alignment = TextAnchor.MiddleLeft }, new Color(1f, 0.9f, 0.5f));
            }
        }

        void BarraMagias(GameManager gm, float W, float H)
        {
            PlayerController p = gm.jogador;
            float tam = 52f * s, gap = 8f * s, rotuloH = 18f * s;
            float alturaTotal = 5f * (tam + rotuloH) + 4f * gap;
            Rect area = new Rect(W - 82f * s + offMagias.x, H * 0.5f - alturaTotal / 2f + offMagias.y, 62f * s, alturaTotal);
            Arrastar(area, ref offMagias, 2, gm);
            for (int i = 0; i < 5; i++)
            {
                float y = area.y + i * (tam + rotuloH + gap);
                Rect r = new Rect(area.x + 5f * s, y, tam, tam);
                bool pronto = i == 4 ? p.PoderCheio : p.mana >= p.custoMagia[i];
                Retangulo(new Rect(r.x - 2f, r.y - 2f, r.width + 4f, r.height + 4f), (i == 4 && p.PoderCheio) ? new Color(1f, 0.85f, 0.3f, 0.9f) : new Color(0f, 0f, 0f, 0.75f));
                Retangulo(r, new Color(0.1f, 0.08f, 0.14f, 0.92f));
                Icone(new Rect(r.x + 4f * s, r.y + 4f * s, tam - 8f * s, tam - 8f * s), Icones.Magia(i), pronto ? Color.white : new Color(1f, 1f, 1f, 0.3f));
                float rec = p.RecargaRestante(i);
                if (rec > 0f) Retangulo(new Rect(r.x, r.y + tam * (1f - rec), tam, tam * rec), new Color(0f, 0f, 0f, 0.6f));
                string custo = i == 4 ? "PODER" : (p.custoMagia[i] > 0f ? p.custoMagia[i].ToString("0") : "");
                Rotulo(new Rect(r.x, r.y + tam - 16f * s, tam - 3f * s, 16f * s), custo, new GUIStyle(minusculo) { alignment = TextAnchor.MiddleRight }, new Color(0.7f, 0.9f, 1f));
                Rotulo(new Rect(area.x - 4f * s, y + tam, area.width + 8f * s, rotuloH), PlayerController.TeclasMagias[i], minusculo, new Color(1f, 0.9f, 0.5f));
            }
        }

        void Missoes(GameManager gm, float W, float H)
        {
            float bw = 330f * s, bx = W - bw - 100f * s;
            float y = 12f * s;
            Missao p = gm.PrincipalAtual();
            int linhas = 1;
            if (p != null) linhas += 1 + p.objetivos.Count;
            int secundarias = 0;
            for (int i = 0; i < gm.Missoes.Count; i++) if (!gm.Missoes[i].principal && !gm.Missoes[i].concluida) secundarias++;
            linhas += secundarias > 0 ? secundarias + 1 : 0;
            float altura = (linhas + 0.5f) * 21f * s;
            Retangulo(new Rect(bx, y, bw, altura), new Color(0f, 0f, 0f, 0.5f));
            float ly = y + 4f * s;
            if (p != null)
            {
                Rotulo(new Rect(bx + 8f * s, ly, bw, 21f * s), "MISSÃO: " + p.titulo, pequeno, new Color(1f, 0.85f, 0.35f));
                ly += 21f * s;
                for (int j = 0; j < p.objetivos.Count; j++)
                {
                    ObjetivoMissao o = p.objetivos[j];
                    string cont = o.necessario > 1 ? "  " + o.atual + "/" + o.necessario : "";
                    Rotulo(new Rect(bx + 16f * s, ly, bw, 21f * s), (o.Completo ? "[x] " : "[ ] ") + o.descricao + cont, pequeno, o.Completo ? new Color(0.5f, 1f, 0.6f) : Color.white);
                    ly += 21f * s;
                }
            }
            else Rotulo(new Rect(bx + 8f * s, ly, bw, 21f * s), "Missões principais concluídas!", pequeno, new Color(0.5f, 1f, 0.6f));
            if (secundarias > 0)
            {
                ly += 4f * s;
                Rotulo(new Rect(bx + 8f * s, ly, bw, 21f * s), "SECUNDÁRIAS", pequeno, new Color(0.7f, 0.8f, 1f));
                ly += 21f * s;
                for (int i = 0; i < gm.Missoes.Count; i++)
                {
                    Missao m = gm.Missoes[i];
                    if (m.principal || m.concluida) continue;
                    ObjetivoMissao o = m.objetivos[0];
                    Rotulo(new Rect(bx + 16f * s, ly, bw, 21f * s), m.titulo + "  " + o.atual + "/" + o.necessario, pequeno, new Color(0.85f, 0.85f, 0.9f));
                    ly += 21f * s;
                }
            }
        }

        void Mira(GameManager gm, float W, float H)
        {
            float cx = W * 0.5f, cy = H * 0.5f;
            Color c = new Color(1f, 1f, 1f, 0.55f);
            Retangulo(new Rect(cx - 1f * s, cy - 8f * s, 2f * s, 6f * s), c);
            Retangulo(new Rect(cx - 1f * s, cy + 2f * s, 2f * s, 6f * s), c);
            Retangulo(new Rect(cx - 8f * s, cy - 1f * s, 6f * s, 2f * s), c);
            Retangulo(new Rect(cx + 2f * s, cy - 1f * s, 6f * s, 2f * s), c);

            Inimigo alvo = gm.jogador.AlvoTravado;
            Camera cam = Camera.main;
            if (alvo == null || cam == null) return;
            Vector3 sp = cam.WorldToScreenPoint(alvo.PontoMira);
            if (sp.z <= 0f) return;
            float x = sp.x, y = H - sp.y, r = 26f * s;
            Color v = new Color(1f, 0.25f, 0.2f, 0.9f);
            float t = 3f * s;
            Retangulo(new Rect(x - r, y - r, r * 0.6f, t), v); Retangulo(new Rect(x - r, y - r, t, r * 0.6f), v);
            Retangulo(new Rect(x + r - r * 0.6f, y - r, r * 0.6f, t), v); Retangulo(new Rect(x + r - t, y - r, t, r * 0.6f), v);
            Retangulo(new Rect(x - r, y + r - t, r * 0.6f, t), v); Retangulo(new Rect(x - r, y + r - r * 0.6f, t, r * 0.6f), v);
            Retangulo(new Rect(x + r - r * 0.6f, y + r - t, r * 0.6f, t), v); Retangulo(new Rect(x + r - t, y + r - r * 0.6f, t, r * 0.6f), v);
            Barra(new Rect(x - 40f * s, y - r - 14f * s, 80f * s, 6f * s), alvo.FracaoVida, new Color(0.85f, 0.15f, 0.15f));
        }

        void BannerUI(GameManager gm, float W, float H)
        {
            float restante = gm.BannerAte - Time.unscaledTime;
            if (restante <= 0f || string.IsNullOrEmpty(gm.Banner)) return;
            float a = Mathf.Clamp01(restante / 0.6f);
            Color c = gm.BannerCor; c.a = a;
            Retangulo(new Rect(0f, H * 0.22f - 6f * s, W, 52f * s), new Color(0f, 0f, 0f, 0.5f * a));
            Rotulo(new Rect(0f, H * 0.22f, W, 44f * s), gm.Banner, grande, c);
        }

        void Faixas(GameManager gm, float W, float H)
        {
            Retangulo(new Rect(0f, 0f, W, H * 0.12f), new Color(0f, 0f, 0f, 0.92f));
            Retangulo(new Rect(0f, H * 0.88f, W, H * 0.12f), new Color(0f, 0f, 0f, 0.92f));
            if (!string.IsNullOrEmpty(gm.TituloCena))
            {
                Rotulo(new Rect(0f, H * 0.88f + 4f * s, W, 40f * s), gm.TituloCena, grande, new Color(1f, 0.9f, 0.5f));
                Rotulo(new Rect(0f, H * 0.88f + 44f * s, W, 28f * s), gm.SubtituloCena, normal, new Color(0.8f, 0.8f, 0.85f));
            }
        }

        void Titulo(float W, float H)
        {
            Retangulo(new Rect(0f, 0f, W, H), new Color(0f, 0f, 0f, 0.74f));
            Rotulo(new Rect(0f, H * 0.08f, W, 90f * s), "MONGE ARCANO", titulo, new Color(1f, 0.75f, 0.25f));
            Rotulo(new Rect(0f, H * 0.08f + 80f * s, W, 50f * s), "O MOSTEIRO CONDENADO", grande, new Color(0.85f, 0.2f, 0.2f));
            string ajuda =
                "W A S D: mover     SHIFT: correr     MOUSE: gira a câmera     Botão direito: mirar (zoom)     Roda: zoom     Y: câmera fixa\n" +
                "ESPAÇO / clique: Raio Arcano (mira no alvo ou no centro da tela)     Z ou botão do meio: travar/destravar alvo\n" +
                "F: Nova Sagrada   G: Leque Arcano   B: Escudo Místico   R: Invocação Celeste (Poder Arcano cheio)\n" +
                "C: coletar / interagir / ler     V: curar     X: golpe de cajado     1 a 6: atalhos de itens     I: inventário e habilidades     TAB: pausa";
            GUI.Label(new Rect(W * 0.1f, H * 0.36f, W * 0.8f, 220f * s), ajuda, centro);
            float pulso = 0.6f + 0.4f * Mathf.Sin(Time.unscaledTime * 3f);
            Rotulo(new Rect(0f, H * 0.78f, W, 40f * s), "Pressione ENTER para começar", normal, new Color(1f, 1f, 1f, pulso));
            Rotulo(new Rect(0f, H - 40f * s, W, 30f * s), "Desafio DIO - Trilha Unity  |  Survival horror 3D  |  Código aberto", new GUIStyle(pequeno) { alignment = TextAnchor.MiddleCenter }, new Color(0.7f, 0.7f, 0.7f));
        }

        void Pausa(GameManager gm, float W, float H)
        {
            Retangulo(new Rect(0f, 0f, W, H), new Color(0f, 0f, 0f, 0.82f));
            Rotulo(new Rect(0f, H * 0.04f, W, 50f * s), "PAUSA / INVENTÁRIO", grande, Color.white);
            PlayerController p = gm.jogador;
            var a = new System.Text.StringBuilder();
            a.AppendLine("NÍVEL " + p.nivel + "     XP " + Mathf.FloorToInt(p.xp) + "/" + Mathf.FloorToInt(p.XpProximo));
            a.AppendLine("Vida " + Mathf.CeilToInt(p.vida) + "/" + Mathf.CeilToInt(p.vidaMax) + "     Essência " + Mathf.FloorToInt(p.mana) + "/" + Mathf.FloorToInt(p.manaMax));
            a.AppendLine("Dano das magias: x" + p.DanoMult.ToString("0.00"));
            a.AppendLine();
            a.AppendLine("ITENS:");
            a.AppendLine("  Ervas Sagradas: " + p.ervas + "     Frascos de Essência: " + p.frascos);
            if (gm.Chaves.Count == 0) a.AppendLine("  (sem chaves)");
            foreach (string c in gm.ChavesOrdem) a.AppendLine("  - " + GameManager.NomeChave(c));
            a.AppendLine();
            a.AppendLine("ARQUIVOS LIDOS (" + gm.NotasLidas.Count + "/" + GameManager.TotalNotas + "):");
            foreach (string n in gm.NotasLidas) a.AppendLine("  - " + n);
            GUI.Label(new Rect(W * 0.07f, H * 0.14f, W * 0.4f, H * 0.7f), a.ToString(), quebra);

            var b = new System.Text.StringBuilder();
            b.AppendLine("MISSÕES:");
            for (int i = 0; i < gm.Missoes.Count; i++)
            {
                Missao m = gm.Missoes[i];
                if (m.principal && !m.concluida && m != gm.PrincipalAtual()) continue;
                b.AppendLine((m.concluida ? "[x] " : "[ ] ") + (m.principal ? "" : "(sec.) ") + m.titulo);
                if (!m.concluida) for (int j = 0; j < m.objetivos.Count; j++) b.AppendLine("      " + (m.objetivos[j].Completo ? "ok  " : "..  ") + m.objetivos[j].descricao + (m.objetivos[j].necessario > 1 ? "  " + m.objetivos[j].atual + "/" + m.objetivos[j].necessario : ""));
            }
            GUI.Label(new Rect(W * 0.52f, H * 0.14f, W * 0.43f, H * 0.7f), b.ToString(), quebra);
            Rotulo(new Rect(0f, H * 0.86f, W, 30f * s), "Arraste as barras de atalhos e de magias para reposicionar.   TAB / P / ENTER: continuar   R: reiniciar", pequeno, new Color(1f, 0.9f, 0.5f));
        }

        void Nota(GameManager gm, float W, float H)
        {
            float bw = W * 0.58f, bh = H * 0.58f, bx = (W - bw) / 2f, by = (H - bh) / 2f;
            Retangulo(new Rect(bx - 4f, by - 4f, bw + 8f, bh + 8f), new Color(0.25f, 0.15f, 0.05f, 1f));
            Retangulo(new Rect(bx, by, bw, bh), new Color(0.85f, 0.78f, 0.6f, 1f));
            GUIStyle t = new GUIStyle(grande);
            t.normal.textColor = new Color(0.25f, 0.1f, 0.05f);
            GUI.Label(new Rect(bx, by + 12f * s, bw, 50f * s), gm.NotaTitulo, t);
            GUIStyle q = new GUIStyle(quebra);
            q.normal.textColor = new Color(0.15f, 0.08f, 0.04f);
            q.fontSize = Mathf.RoundToInt(20f * s);
            GUI.Label(new Rect(bx + 28f * s, by + 76f * s, bw - 56f * s, bh - 120f * s), gm.NotaTexto, q);
            GUIStyle f = new GUIStyle(pequeno);
            f.alignment = TextAnchor.MiddleCenter;
            f.normal.textColor = new Color(0.3f, 0.1f, 0.05f);
            GUI.Label(new Rect(bx, by + bh - 36f * s, bw, 28f * s), "[C] ou [ENTER] para fechar", f);
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
            Retangulo(new Rect(0f, 0f, W, H), new Color(0.95f, 0.85f, 0.5f, 0.88f));
            GUIStyle t = new GUIStyle(titulo);
            t.normal.textColor = new Color(0.25f, 0.1f, 0f);
            GUI.Label(new Rect(0f, H * 0.16f, W, 90f * s), "O MAL FOI SELADO", t);
            int min = Mathf.FloorToInt(gm.TempoDeJogo / 60f);
            int seg = Mathf.FloorToInt(gm.TempoDeJogo % 60f);
            GUIStyle n = new GUIStyle(normal);
            n.normal.textColor = new Color(0.2f, 0.08f, 0f);
            GUI.Label(new Rect(0f, H * 0.38f, W, 40f * s), "Tempo: " + min.ToString("00") + ":" + seg.ToString("00") + "     Nível final: " + gm.jogador.nivel, n);
            GUI.Label(new Rect(0f, H * 0.45f, W, 40f * s), "Criaturas banidas: " + gm.InimigosDerrotados, n);
            GUI.Label(new Rect(0f, H * 0.52f, W, 40f * s), "Arquivos encontrados: " + gm.NotasLidas.Count + "/" + GameManager.TotalNotas, n);
            GUI.Label(new Rect(0f, H * 0.66f, W, 40f * s), "Pressione R ou ENTER para jogar de novo", n);
        }
    }
}
