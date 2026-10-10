namespace MongeArcano
{
    /// <summary>Portal de luz que surge quando o Arconte cai. Atravessá-lo encerra o jogo com vitória.</summary>
    public class Portal : Interagivel
    {
        public override string Rotulo { get { return "Atravessar o portal de luz"; } }

        public override void Interagir(PlayerController jogador)
        {
            GameManager.Instance.Vitoria();
        }
    }
}
