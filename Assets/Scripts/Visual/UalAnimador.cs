using System.Collections.Generic;
using UnityEngine;

namespace MongeArcano
{
    /// <summary>
    /// Toca as animações da Universal Animation Library (Quaternius, CC0) nos personagens: parado, andar, correr, atacar, conjurar, dano e morte.
    /// Os personagens são os modelos do projeto vestidos no esqueleto UAL; os clipes ficam num arquivo compartilhado (ual_animacoes.glb)
    /// e são anexados a cada personagem na partida. A velocidade de cada ciclo acompanha a velocidade real de deslocamento (sem patinar).
    /// </summary>
    public class UalAnimador : MonoBehaviour
    {
        public enum Perfil { Monge, Guerreiro, Bruxa, Zumbi, Besta, Chefe, Npc }

        public Perfil perfil;
        public string loopInicial = "";
        public float velocidadeLoop = 1f;
        public bool Pronto { get; private set; }
        public bool Morto { get { return morto; } }

        Animation anim;
        Transform raizModelo;
        string idle, andar, correr, sprint, ataque, conjurar, golpe, morte;
        float refAndar = 1.45f, refCorrer = 3.7f, refSprint = 6.2f;
        string atual;
        float bloqueioAte, velSuave;
        bool morto;
        Vector3 posAnt;
        float escala = 1f;

        static List<AnimationClip> clipes;
        static bool carregado;

        public static UalAnimador Montar(GameObject raiz, GameObject malha, Perfil perfil)
        {
            if (malha == null) return null;
            Modelos.Prefab("ual_animacoes"); // garante o registro do arquivo de clipes na geração da cena
            Transform arm = Achar(malha.transform, "Armature");
            if (arm == null || malha.GetComponentInChildren<SkinnedMeshRenderer>(true) == null) return null;
            UalAnimador u = raiz.GetComponent<UalAnimador>();
            if (u == null) u = raiz.AddComponent<UalAnimador>();
            u.perfil = perfil;
            u.raizModelo = arm.parent;
            return u;
        }

        static Transform Achar(Transform t, string nome)
        {
            if (t.name == nome) return t;
            for (int i = 0; i < t.childCount; i++)
            {
                Transform r = Achar(t.GetChild(i), nome);
                if (r != null) return r;
            }
            return null;
        }

        static void CarregarClipes()
        {
            if (carregado) return;
            carregado = true;
            clipes = new List<AnimationClip>();
            GameObject prefab = Modelos.Prefab("ual_animacoes");
            if (prefab == null) return;
            GameObject temp = Object.Instantiate(prefab);
            Animation a = temp.GetComponentInChildren<Animation>(true);
            if (a != null) foreach (AnimationState st in a) if (st.clip != null) clipes.Add(st.clip);
            Object.Destroy(temp);
        }

        void Perfis()
        {
            switch (perfil)
            {
                case Perfil.Monge:
                    idle = "Idle_Loop"; andar = "Walk_Loop"; correr = "Jog_Fwd_Loop"; sprint = "Sprint_Loop";
                    ataque = "Sword_Attack"; conjurar = "Spell_Simple_Shoot"; golpe = "Hit_Chest"; morte = "Death01"; break;
                case Perfil.Guerreiro:
                    idle = "Sword_Idle"; andar = "Walk_Loop"; correr = "Jog_Fwd_Loop"; sprint = "Sprint_Loop";
                    ataque = "Sword_Regular_A"; conjurar = "Spell_Simple_Shoot"; golpe = "Hit_Chest"; morte = "Death01"; break;
                case Perfil.Bruxa:
                    idle = "Idle_Talking_Loop"; andar = "Walk_Formal_Loop"; correr = "Jog_Fwd_Loop"; sprint = "Sprint_Loop";
                    ataque = "Spell_Simple_Shoot"; conjurar = "Spell_Simple_Shoot"; golpe = "Hit_Head"; morte = "Death01"; break;
                case Perfil.Zumbi:
                    idle = "Zombie_Idle_Loop"; andar = "Zombie_Walk_Fwd_Loop"; correr = "Zombie_Walk_Fwd_Loop"; sprint = "Zombie_Walk_Fwd_Loop";
                    ataque = "Zombie_Scratch"; conjurar = "Zombie_Scratch"; golpe = "Hit_Knockback"; morte = "Death01"; refAndar = 1.0f; refCorrer = 1.6f; refSprint = 2.2f; break;
                case Perfil.Besta:
                    idle = "Zombie_Idle_Loop"; andar = "Zombie_Walk_Fwd_Loop"; correr = "Jog_Fwd_Loop"; sprint = "Sprint_Loop";
                    ataque = "Melee_Hook"; conjurar = "Melee_Hook"; golpe = "Hit_Knockback"; morte = "Death01"; refAndar = 1.0f; break;
                case Perfil.Chefe:
                    idle = "Idle_Loop"; andar = "Walk_Formal_Loop"; correr = "Jog_Fwd_Loop"; sprint = "Sprint_Loop";
                    ataque = "Sword_Regular_Combo"; conjurar = "Spell_Simple_Shoot"; golpe = "Hit_Chest"; morte = "Death01"; break;
                default:
                    idle = "Idle_Talking_Loop"; andar = "Walk_Formal_Loop"; correr = "Jog_Fwd_Loop"; sprint = "Sprint_Loop";
                    ataque = "Interact"; conjurar = "Interact"; golpe = "Hit_Chest"; morte = "Death01"; break;
            }
        }

        void Start()
        {
            if (!Application.isPlaying || raizModelo == null) return;
            Perfis();
            CarregarClipes();
            if (clipes == null || clipes.Count == 0)
            {
                GameManager gm = GameManager.Instance;
                if (gm != null && !avisou) { avisou = true; gm.Mostrar("Animações UAL não encontradas: confira Assets/Modelos/Animacoes/ual_animacoes.glb", 8f); }
                return;
            }
            anim = raizModelo.GetComponent<Animation>();
            if (anim == null) anim = raizModelo.gameObject.AddComponent<Animation>();
            anim.playAutomatically = false;
            anim.cullingType = AnimationCullingType.AlwaysAnimate;
            for (int i = 0; i < clipes.Count; i++)
            {
                AnimationClip c = clipes[i];
                if (anim[c.name] == null) anim.AddClip(c, c.name);
                AnimationState st = anim[c.name];
                if (st != null) st.wrapMode = c.name.EndsWith("_Loop") ? WrapMode.Loop : WrapMode.Once;
            }
            anim.Stop();
            escala = Mathf.Max(0.05f, raizModelo.lossyScale.y);
            posAnt = transform.position;
            Tocar(idle, 0f, 1f);
            Pronto = true;
            if (!string.IsNullOrEmpty(loopInicial)) TocarLoop(loopInicial, velocidadeLoop);
        }

        static bool avisou;

        void Tocar(string nome, float fade, float velocidade)
        {
            if (anim == null || string.IsNullOrEmpty(nome)) return;
            AnimationState st = anim[nome];
            if (st == null) return;
            st.speed = velocidade;
            if (atual != nome)
            {
                if (st.wrapMode != WrapMode.Loop) st.time = 0f;
                anim.CrossFade(nome, fade);
                atual = nome;
            }
        }

        void Acao(string nome, float velocidade, float fade)
        {
            if (!Pronto || morto || anim == null) return;
            AnimationState st = anim[nome];
            if (st == null) return;
            st.wrapMode = WrapMode.Once;
            st.speed = velocidade;
            st.time = 0f;
            anim.CrossFade(nome, fade);
            atual = nome;
            bloqueioAte = Time.time + st.length / Mathf.Max(0.1f, velocidade) * 0.92f;
        }

        public void Atacar() { Acao(ataque, 1.35f, 0.08f); }
        public void Conjurar() { Acao(conjurar, 1.4f, 0.08f); }
        public void Atingido() { if (Time.time < bloqueioAte - 0.05f && atual == ataque) return; Acao(golpe, 1.3f, 0.05f); }
        public void Interagir() { Acao("Interact", 1f, 0.15f); }

        public void Morrer()
        {
            if (!Pronto || morto) return;
            morto = true;
            AnimationState st = anim[morte];
            if (st == null) return;
            st.wrapMode = WrapMode.ClampForever;
            st.speed = 1f;
            st.time = 0f;
            anim.CrossFade(morte, 0.1f);
            atual = morte;
        }

        public void Reviver()
        {
            morto = false;
            bloqueioAte = 0f;
            atual = null;
            if (Pronto) { anim.Stop(); Tocar(idle, 0f, 1f); }
        }

        /// <summary>Toca um clipe qualquer em loop (dançarinos, NPC falando).</summary>
        public void TocarLoop(string nome, float velocidade)
        {
            if (!Pronto || anim == null) return;
            AnimationState st = anim[nome];
            if (st == null) return;
            st.wrapMode = WrapMode.Loop;
            bloqueioAte = float.MaxValue;
            atual = null;
            Tocar(nome, 0.2f, velocidade);
        }

        void Update()
        {
            if (!Pronto || morto) return;
            float dt = Time.deltaTime;
            if (dt <= 0f) return;
            Vector3 d = transform.position - posAnt;
            d.y = 0f;
            posAnt = transform.position;
            velSuave = Mathf.Lerp(velSuave, d.magnitude / dt, 1f - Mathf.Exp(-12f * dt));
            if (Time.time < bloqueioAte) return;

            float v = velSuave / escala;
            if (v < 0.3f) Tocar(idle, 0.25f, 1f);
            else if (v < 2.4f) Tocar(andar, 0.2f, Mathf.Clamp(v / refAndar, 0.6f, 1.7f));
            else if (v < 5.2f) Tocar(correr, 0.2f, Mathf.Clamp(v / refCorrer, 0.7f, 1.6f));
            else Tocar(sprint, 0.2f, Mathf.Clamp(v / refSprint, 0.8f, 1.5f));
        }
    }
}
