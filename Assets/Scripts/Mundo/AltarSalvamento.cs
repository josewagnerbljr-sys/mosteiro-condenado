using UnityEngine;

namespace MongeArcano
{
    /// <summary>Sala segura: restaura a vida e define o ponto de retorno (checkpoint). A essência extra só vem uma vez.</summary>
    public class AltarSalvamento : Interagivel
    {
        bool essenciaConcedida;

        public override string Rotulo { get { return "Rezar no altar (salvar e restaurar vida)"; } }

        public override void Interagir(PlayerController p)
        {
            GameManager gm = GameManager.Instance;
            p.Curar(p.vidaMax);
            gm.DefinirCheckpoint(p.transform.position, p.transform.eulerAngles.y);
            if (!essenciaConcedida)
            {
                essenciaConcedida = true;
                p.AddMana(8f);
                gm.Mostrar("Checkpoint salvo. Vida restaurada e +8 de essência.", 4f);
            }
            else gm.Mostrar("Checkpoint salvo. Vida restaurada.", 3f);

            Efeitos.Explosao(p.transform.position + Vector3.up, "EmissaoAzul", 4f, 0.8f);
            Som.Tocar(TipoSom.Altar, transform.position, 1f);
        }
    }
}
