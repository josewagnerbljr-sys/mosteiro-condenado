using System.Collections.Generic;

namespace MongeArcano
{
    public class ObjetivoMissao
    {
        public string descricao, evento, alvo;
        public int necessario = 1, atual;
        public bool Completo { get { return atual >= necessario; } }
        public ObjetivoMissao(string d, string ev, string al, int n) { descricao = d; evento = ev; alvo = al; necessario = n; }
    }

    public class Missao
    {
        public string id, titulo, descricao;
        public int xp;
        public bool principal, concluida;
        public List<ObjetivoMissao> objetivos = new List<ObjetivoMissao>();

        public Missao(string id, string titulo, string descricao, int xp, bool principal)
        {
            this.id = id; this.titulo = titulo; this.descricao = descricao; this.xp = xp; this.principal = principal;
        }

        public Missao Obj(string desc, string evento, string alvo, int n = 1)
        {
            objetivos.Add(new ObjetivoMissao(desc, evento, alvo, n));
            return this;
        }

        public bool Cumprida
        {
            get
            {
                for (int i = 0; i < objetivos.Count; i++) if (!objetivos[i].Completo) return false;
                return true;
            }
        }
    }

    /// <summary>Missões principais (em sequência) e secundárias (sempre ativas). Progresso vem de eventos do jogo e vale mesmo fora de ordem.</summary>
    public static class MissoesDoJogo
    {
        public static List<Missao> Criar()
        {
            var l = new List<Missao>();

            l.Add(new Missao("m0", "Chegada ao Mosteiro", "Siga o caminho do cemitério até o portão.", 40, true)
                .Obj("Siga o caminho até o portão", "sala", "Portao")
                .Obj("Abra o portão principal", "marco", "porta_principal"));

            l.Add(new Missao("m1", "Despertar no Mosteiro", "O mosteiro guarda segredos. Comece lendo o diário do Abade.", 30, true)
                .Obj("Leia o Diário do Abade", "nota", "Diário do Abade"));

            l.Add(new Missao("m2", "A Capela Assombrada", "Os irmãos mortos ainda rondam a Capela.", 70, true)
                .Obj("Entre na Capela", "sala", "Capela")
                .Obj("Derrote os espectros", "matar", "Espectro", 2)
                .Obj("Pegue a Chave de Ferro", "item", "ferro"));

            l.Add(new Missao("m3", "Silêncio na Biblioteca", "Possuídos vagam entre as estantes.", 100, true)
                .Obj("Abra a Biblioteca", "marco", "porta_biblioteca")
                .Obj("Derrote os possuídos", "matar", "Possuido", 3)
                .Obj("Pegue a Chave de Osso", "item", "osso")
                .Obj("Recupere o Sigilo do Fogo", "item", "sigilo_fogo"));

            l.Add(new Missao("m4", "Catacumbas Profanas", "Criaturas infernais guardam o Sigilo da Terra.", 130, true)
                .Obj("Abra as Catacumbas", "marco", "porta_catacumbas")
                .Obj("Derrote as criaturas infernais", "matar", "Infernal", 2)
                .Obj("Recupere o Sigilo da Terra", "item", "sigilo_terra"));

            l.Add(new Missao("m5", "Os Três Sigilos", "Reúna Lua, Fogo e Terra.", 80, true)
                .Obj("Reúna os 3 sigilos", "item", "sigilo_*", 3));

            l.Add(new Missao("m6", "O Arconte das Cinzas", "Abra o Santuário e enfrente o mal.", 250, true)
                .Obj("Abra o portão do Santuário", "marco", "porta_santuario")
                .Obj("Derrote o Arconte", "matar", "Arconte"));

            l.Add(new Missao("m7", "O Portal de Luz", "Atravesse o portal e sele o mal.", 100, true)
                .Obj("Atravesse o portal de luz", "vitoria", ""));

            l.Add(new Missao("s1", "Arquivista", "Leia todos os arquivos do mosteiro.", 100, false)
                .Obj("Arquivos lidos", "nota", "*", 5));
            l.Add(new Missao("s2", "Caçador de Sombras", "Banir criaturas das trevas.", 150, false)
                .Obj("Criaturas banidas", "matar", "*", 12));
            l.Add(new Missao("s3", "Mestre Arcano", "Pratique as artes arcanas.", 120, false)
                .Obj("Magias lançadas", "magia", "*", 25));
            l.Add(new Missao("s4", "Fôlego Sagrado", "Use ervas para se curar.", 60, false)
                .Obj("Ervas usadas", "erva", "", 3));
            l.Add(new Missao("s5", "Caçador de Tesouros", "Abra baús espalhados pelo mosteiro.", 140, false)
                .Obj("Baús abertos", "bau", "", 4));
            l.Add(new Missao("s6", "Eco da Masmorra", "Conclua a masmorra de ondas pelo portal do cemitério.", 220, false)
                .Obj("Masmorra concluída", "arena", ""));
            return l;
        }
    }
}
