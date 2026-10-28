using UnityEngine;

namespace MongeArcano
{
    /// <summary>Tempestade distante: clarões de relâmpago no céu avermelhado seguidos de trovão.</summary>
    public class Relampago : MonoBehaviour
    {
        Light luz;
        float proximo, fimClarao;
        int fase;

        void Start()
        {
            luz = GetComponent<Light>();
            proximo = Time.time + Random.Range(6f, 14f);
        }

        void Update()
        {
            if (luz == null) return;
            if (Time.time >= proximo && fase == 0)
            {
                transform.rotation = Quaternion.Euler(Random.Range(25f, 60f), Random.Range(0f, 360f), 0f);
                luz.intensity = Random.Range(1.6f, 2.6f);
                fimClarao = Time.time + 0.09f;
                fase = 1;
                Invoke("Trovao", Random.Range(0.7f, 2.2f));
            }
            else if (fase == 1 && Time.time >= fimClarao) { luz.intensity = 0f; fimClarao = Time.time + 0.08f; fase = 2; }
            else if (fase == 2 && Time.time >= fimClarao) { luz.intensity = Random.Range(1.2f, 2.0f); fimClarao = Time.time + 0.12f; fase = 3; }
            else if (fase == 3 && Time.time >= fimClarao) { luz.intensity = 0f; fase = 0; proximo = Time.time + Random.Range(9f, 24f); }
        }

        void Trovao()
        {
            GameManager gm = GameManager.Instance;
            Vector3 p = gm != null && gm.jogador != null ? gm.jogador.transform.position : transform.position;
            Som.Tocar(TipoSom.Trovao, p, 0.55f);
        }
    }
}
