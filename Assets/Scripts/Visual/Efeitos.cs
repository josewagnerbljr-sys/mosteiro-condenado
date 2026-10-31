using UnityEngine;

namespace MongeArcano
{
    public static class Efeitos
    {
        /// <summary>Explosão mágica: clarão com luz, anel no chão e faíscas (sem esferas sólidas).</summary>
        public static void Explosao(Vector3 pos, string material, float tamanhoFinal, float duracao)
        {
            Fx.Explodir(pos, CorDe(material), tamanhoFinal, duracao);
            if (tamanhoFinal <= 4f) FxMagia.Impacto(pos, CorDe(material), tamanhoFinal);
        }

        public static Color CorDe(string material)
        {
            switch (material)
            {
                case "EmissaoVermelha": return new Color(1f, 0.25f, 0.12f);
                case "EmissaoLaranja": return new Color(1f, 0.55f, 0.2f);
                case "EmissaoVerde": return new Color(0.3f, 1f, 0.4f);
                case "EmissaoRoxa": return new Color(0.7f, 0.35f, 1f);
                case "EmissaoDourada": return new Color(1f, 0.8f, 0.3f);
                case "EmissaoBranca": return Color.white;
                default: return new Color(0.4f, 0.6f, 1f);
            }
        }
    }
}
