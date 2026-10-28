using UnityEngine;

namespace MongeArcano
{
    /// <summary>Névoa rasteira: plano translúcido cuja textura desliza devagar.</summary>
    public class NevoaDeriva : MonoBehaviour
    {
        public Vector2 velocidade = new Vector2(0.004f, 0.002f);
        Material mat;
        Vector2 off;

        void Start()
        {
            Renderer r = GetComponent<Renderer>();
            mat = new Material(r.sharedMaterial);
            r.sharedMaterial = mat;
        }

        void Update()
        {
            off += velocidade * Time.deltaTime;
            if (mat != null) mat.mainTextureOffset = off;
        }
    }
}
