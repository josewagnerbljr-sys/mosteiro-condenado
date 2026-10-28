using System.Collections.Generic;
using UnityEngine;

namespace MongeArcano
{
    /// <summary>Visualização 3D do personagem girando devagar (câmera própria em camada separada, desenhada numa textura).</summary>
    public class PreviewPersonagem
    {
        GameObject raiz, clone;
        Camera cam;
        RenderTexture rt;
        float giro;
        public RenderTexture Textura { get { return rt; } }

        static void Camada(GameObject g, int camada)
        {
            g.layer = camada;
            for (int i = 0; i < g.transform.childCount; i++) Camada(g.transform.GetChild(i).gameObject, camada);
        }

        public PreviewPersonagem(PlayerController p)
        {
            raiz = new GameObject("PreviewPersonagem");
            raiz.transform.position = new Vector3(0f, -600f, 0f);
            if (p.visual != null)
            {
                clone = Object.Instantiate(p.visual.gameObject);
                clone.transform.SetParent(raiz.transform, false);
                clone.transform.localPosition = Vector3.zero;
                clone.transform.localRotation = Quaternion.identity;
                clone.transform.localScale = Vector3.one;
                Camada(clone, 30);
            }
            rt = new RenderTexture(512, 768, 16);

            GameObject cg = new GameObject("CamPreview");
            cg.transform.SetParent(raiz.transform, false);
            cg.transform.localPosition = new Vector3(0f, 1.15f, 4.4f);
            cg.transform.LookAt(raiz.transform.position + new Vector3(0f, 0.95f, 0f));
            cam = cg.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.06f, 0.045f, 0.075f, 1f);
            cam.cullingMask = 1 << 30;
            cam.fieldOfView = 30f;
            cam.targetTexture = rt;

            Camera principal = Camera.main;
            if (principal != null) principal.cullingMask &= ~(1 << 30);

            GameObject lg = new GameObject("LuzPreview");
            lg.transform.SetParent(raiz.transform, false);
            lg.transform.rotation = Quaternion.Euler(28f, 200f, 0f);
            Light l = lg.AddComponent<Light>();
            l.type = LightType.Directional;
            l.color = new Color(1f, 0.92f, 0.82f);
            l.intensity = 1.35f;
            l.cullingMask = 1 << 30;
            GameObject lg2 = new GameObject("LuzPreviewAro");
            lg2.transform.SetParent(raiz.transform, false);
            lg2.transform.rotation = Quaternion.Euler(-10f, 20f, 0f);
            Light l2 = lg2.AddComponent<Light>();
            l2.type = LightType.Directional;
            l2.color = new Color(0.4f, 0.55f, 1f);
            l2.intensity = 0.8f;
            l2.cullingMask = 1 << 30;
        }

        public void Atualizar(PlayerController p)
        {
            if (clone == null) return;
            giro += 26f * Time.unscaledDeltaTime;
            clone.transform.localRotation = Quaternion.Euler(0f, giro, 0f);
            Color tinta = p.armaduraEq != null ? Color.Lerp(Color.white, p.armaduraEq.Cor, 0.35f) : Color.white;
            Renderer[] rs = clone.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < rs.Length; i++)
                if (rs[i].material.HasProperty("_Color")) rs[i].material.color = tinta;
        }

        public void Destruir()
        {
            if (rt != null) { rt.Release(); Object.Destroy(rt); }
            if (raiz != null) Object.Destroy(raiz);
        }
    }

    /// <summary>Tela do inventário (tecla I): 20 espaços, equipamento, visual 3D, distribuição de pontos e evolução das magias.</summary>
    public class InventarioUI : MonoBehaviour
    {
        GUIStyle titulo, normal, pequeno, botao, quebra;
        float s = 1f;
        Item selecionado, sobre;
        PreviewPersonagem preview;

        void Estilos()
        {
            titulo = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            normal = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft };
            pequeno = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            quebra = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.UpperLeft, wordWrap = true };
            botao = new GUIStyle(GUI.skin.button) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            titulo.normal.textColor = normal.normal.textColor = pequeno.normal.textColor = quebra.normal.textColor = Color.white;
        }

        void Ret(Rect r, Color c)
        {
            Color o = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = o;
        }

        void Texto(Rect r, string t, GUIStyle st, Color c)
        {
            Color o = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.8f);
            GUI.Label(new Rect(r.x + 1.5f, r.y + 1.5f, r.width, r.height), t, st);
            GUI.color = c;
            GUI.Label(r, t, st);
            GUI.color = o;
        }

        void OnGUI()
        {
            GameManager gm = GameManager.Instance;
            if (gm == null || gm.jogador == null) return;
            if (gm.Estado != EstadoJogo.Inventario)
            {
                if (preview != null) { preview.Destruir(); preview = null; }
                selecionado = null;
                return;
            }
            if (titulo == null) Estilos();
            PlayerController p = gm.jogador;
            if (preview == null) preview = new PreviewPersonagem(p);
            preview.Atualizar(p);

            s = Screen.height / 720f;
            titulo.fontSize = Mathf.RoundToInt(28f * s);
            normal.fontSize = Mathf.RoundToInt(16f * s);
            pequeno.fontSize = Mathf.RoundToInt(13f * s);
            quebra.fontSize = Mathf.RoundToInt(15f * s);
            botao.fontSize = Mathf.RoundToInt(15f * s);
            float W = Screen.width, H = Screen.height;
            sobre = null;

            Ret(new Rect(0f, 0f, W, H), new Color(0.02f, 0.015f, 0.03f, 0.95f));
            Texto(new Rect(0f, 8f * s, W, 40f * s), "INVENTÁRIO E HABILIDADES", titulo, new Color(1f, 0.8f, 0.4f));

            ColunaInventario(p, W * 0.03f, H * 0.10f, W * 0.33f, H * 0.80f);
            ColunaPersonagem(p, W * 0.38f, H * 0.10f, W * 0.25f, H * 0.80f);
            ColunaHabilidades(p, W * 0.65f, H * 0.10f, W * 0.32f, H * 0.80f);

            Texto(new Rect(0f, H - 30f * s, W, 26f * s), "[I] ou [ESC] fecha     Clique em um item para selecionar     Clique em uma barra do equipamento para desequipar", pequeno, new Color(0.8f, 0.8f, 0.85f));
        }

        void ColunaInventario(PlayerController p, float x, float y, float w, float h)
        {
            Ret(new Rect(x, y, w, h), new Color(0.09f, 0.07f, 0.11f, 0.9f));
            Texto(new Rect(x, y + 4f * s, w, 26f * s), "MOCHILA  " + p.SlotsUsados + "/" + PlayerController.Capacidade + " espaços", pequeno, new Color(1f, 0.85f, 0.5f));

            Item[] celulas = new Item[PlayerController.Capacidade];
            int c = 0;
            for (int i = 0; i < p.inventario.Count && c < celulas.Length; i++)
                for (int k = 0; k < p.inventario[i].Slots && c < celulas.Length; k++) celulas[c++] = p.inventario[i];

            int colunas = 5;
            float tam = Mathf.Min((w - 24f * s) / colunas, 64f * s);
            float gx = x + (w - tam * colunas) / 2f, gy = y + 36f * s;
            for (int i = 0; i < celulas.Length; i++)
            {
                Rect r = new Rect(gx + (i % colunas) * tam, gy + (i / colunas) * tam, tam - 3f, tam - 3f);
                Item it = celulas[i];
                Ret(r, it == null ? new Color(0.14f, 0.12f, 0.16f, 1f) : new Color(it.Cor.r * 0.35f, it.Cor.g * 0.35f, it.Cor.b * 0.35f, 1f));
                if (it != null)
                {
                    if (it == selecionado) Ret(new Rect(r.x - 2f, r.y - 2f, r.width + 4f, r.height + 4f), new Color(1f, 0.9f, 0.4f, 0.55f));
                    bool primeira = i == 0 || celulas[i - 1] != it;
                    if (primeira) Texto(r, Abrev(it), pequeno, it.Cor);
                    if (r.Contains(Event.current.mousePosition)) sobre = it;
                    if (GUI.Button(r, GUIContent.none, GUIStyle.none)) selecionado = it;
                }
            }

            float by = gy + 4f * tam + 10f * s;
            Item ver = sobre != null ? sobre : selecionado;
            if (ver != null)
            {
                Texto(new Rect(x + 10f * s, by, w - 20f * s, 22f * s), ver.nome, normal, ver.Cor);
                GUI.Label(new Rect(x + 10f * s, by + 24f * s, w - 20f * s, 140f * s), ver.Resumo(), quebra);
            }
            if (selecionado != null)
            {
                string rot = selecionado.tipo == TipoItem.Pocao ? "USAR" : (selecionado.tipo == TipoItem.Pergaminho ? "LER" : "EQUIPAR");
                if (GUI.Button(new Rect(x + 10f * s, y + h - 76f * s, w * 0.45f, 32f * s), rot, botao)) { p.UsarItem(selecionado); selecionado = null; }
                if (GUI.Button(new Rect(x + w * 0.5f, y + h - 76f * s, w * 0.45f, 32f * s), "DESCARTAR", botao)) { p.DescartarItem(selecionado); selecionado = null; }
            }
            GameManager gm = GameManager.Instance;
            string chaves = "Chaves: ";
            if (gm.ChavesOrdem.Count == 0) chaves += "nenhuma";
            for (int i = 0; i < gm.ChavesOrdem.Count; i++) chaves += (i > 0 ? ", " : "") + GameManager.NomeChave(gm.ChavesOrdem[i]);
            GUI.Label(new Rect(x + 10f * s, y + h - 36f * s, w - 20f * s, 32f * s), chaves, quebra);
        }

        string Abrev(Item it)
        {
            return it.tipo == TipoItem.Arma ? "ARM" : it.tipo == TipoItem.Armadura ? "VES" : it.tipo == TipoItem.Amuleto ? "AMU" : it.tipo == TipoItem.Pocao ? "POC" : "PER";
        }

        void Caixa(PlayerController p, TipoItem tipo, Item atual, string rotulo, Rect r)
        {
            Ret(r, new Color(0.12f, 0.1f, 0.15f, 1f));
            Texto(new Rect(r.x, r.y, r.width, 16f * s), rotulo, pequeno, new Color(0.7f, 0.7f, 0.8f));
            Texto(new Rect(r.x, r.y + 14f * s, r.width, r.height - 14f * s), atual != null ? atual.nome : "(vazio)", pequeno, atual != null ? atual.Cor : new Color(0.5f, 0.5f, 0.55f));
            if (atual != null)
            {
                if (r.Contains(Event.current.mousePosition)) sobre = atual;
                if (GUI.Button(r, GUIContent.none, GUIStyle.none)) p.Desequipar(tipo);
            }
        }

        void ColunaPersonagem(PlayerController p, float x, float y, float w, float h)
        {
            Ret(new Rect(x, y, w, h), new Color(0.09f, 0.07f, 0.11f, 0.9f));
            Texto(new Rect(x, y + 4f * s, w, 26f * s), "MONGE   NÍVEL " + p.nivel, pequeno, new Color(1f, 0.85f, 0.5f));
            Rect pr = new Rect(x + 8f * s, y + 34f * s, w - 16f * s, h * 0.50f);
            if (preview != null && preview.Textura != null) GUI.DrawTexture(pr, preview.Textura, ScaleMode.ScaleToFit, false);
            float ey = pr.yMax + 6f * s;
            float lw = (w - 24f * s) / 3f;
            Caixa(p, TipoItem.Arma, p.armaEq, "ARMA", new Rect(x + 8f * s, ey, lw, 44f * s));
            Caixa(p, TipoItem.Armadura, p.armaduraEq, "VESTE", new Rect(x + 12f * s + lw, ey, lw, 44f * s));
            Caixa(p, TipoItem.Amuleto, p.amuletoEq, "AMULETO", new Rect(x + 16f * s + lw * 2f, ey, lw, 44f * s));

            string st = "Vida " + Mathf.CeilToInt(p.vida) + "/" + Mathf.CeilToInt(p.vidaMax) + "     Essência " + Mathf.FloorToInt(p.mana) + "/" + Mathf.FloorToInt(p.manaMax) +
                        "\nDano das magias x" + p.DanoMult.ToString("0.00") + "     XP " + Mathf.FloorToInt(p.xp) + "/" + Mathf.FloorToInt(p.XpProximo);
            GUI.Label(new Rect(x + 10f * s, ey + 52f * s, w - 20f * s, 60f * s), st, quebra);
        }

        void Linha(PlayerController p, float x, float y, float w, string nome, string desc, int valor, int bonus, int id, bool magia)
        {
            Texto(new Rect(x + 8f * s, y, w * 0.62f, 22f * s), nome + (magia ? "  Nv " + valor + "/5" : "  " + valor + (bonus > 0 ? " (+" + bonus + ")" : "")), normal, Color.white);
            GUI.Label(new Rect(x + 8f * s, y + 20f * s, w * 0.72f, 20f * s), desc, pequeno);
            if (p.pontosLivres > 0 && (!magia || valor < 5))
                if (GUI.Button(new Rect(x + w - 48f * s, y + 2f * s, 38f * s, 30f * s), "+", botao))
                {
                    if (magia) p.UparMagia(id); else p.GastarPonto(id);
                }
        }

        void ColunaHabilidades(PlayerController p, float x, float y, float w, float h)
        {
            Ret(new Rect(x, y, w, h), new Color(0.09f, 0.07f, 0.11f, 0.9f));
            Texto(new Rect(x, y + 4f * s, w, 26f * s), "PONTOS LIVRES: " + p.pontosLivres + "   (7 por nível)", pequeno, p.pontosLivres > 0 ? new Color(0.5f, 1f, 0.6f) : new Color(0.8f, 0.8f, 0.8f));
            float ly = y + 36f * s, passo = 40f * s;
            Linha(p, x, ly, w, "INTELIGÊNCIA", "+2% de dano das magias", p.inteligencia, p.TotInt - p.inteligencia, 0, false); ly += passo;
            Linha(p, x, ly, w, "SABEDORIA", "+2 de essência máxima", p.sabedoria, p.TotWis - p.sabedoria, 1, false); ly += passo;
            Linha(p, x, ly, w, "FORÇA", "+4 de vida máxima e golpe de cajado", p.forca, p.TotStr - p.forca, 2, false); ly += passo;
            Linha(p, x, ly, w, "DESTREZA", "recarga e velocidade de movimento", p.destreza, p.TotDex - p.destreza, 3, false); ly += passo;
            Linha(p, x, ly, w, "ESPECTRO AMALDIÇOADO", "Poder Arcano enche mais rápido", p.espectro, p.TotEsp - p.espectro, 4, false); ly += passo + 8f * s;

            Texto(new Rect(x, ly, w, 22f * s), "ENCANTAMENTOS (1 ponto = +1 nível, +30% de dano)", pequeno, new Color(1f, 0.85f, 0.5f));
            ly += 26f * s;
            for (int i = 0; i < 5; i++)
            {
                Linha(p, x, ly, w, PlayerController.NomesMagias[i] + " [" + PlayerController.TeclasMagias[i] + "]", "Dano atual x" + (1f + 0.3f * (p.nivelMagia[i] - 1)).ToString("0.0"), p.nivelMagia[i], 0, i, true);
                ly += passo;
            }
        }
    }
}
