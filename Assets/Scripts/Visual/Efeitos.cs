using UnityEngine;

namespace MongeArcano
{
    public static class Efeitos
    {
        public static void Explosao(Vector3 pos, string material, float tamanhoFinal, float duracao)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Efeito";
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * 0.2f;
            Collider col = go.GetComponent<Collider>();
            col.enabled = false;
            Object.Destroy(col);
            go.GetComponent<Renderer>().sharedMaterial = ProceduralModels.Mat(material);

            Light luz = go.AddComponent<Light>();
            luz.type = LightType.Point;
            luz.range = Mathf.Max(4f, tamanhoFinal * 1.3f);
            luz.intensity = 2.5f;
            luz.shadows = LightShadows.None;
            luz.color = CorDe(material);

            EfeitoTemporario e = go.AddComponent<EfeitoTemporario>();
            e.duracao = duracao;
            e.tamanhoFinal = tamanhoFinal;
            e.luz = luz;
        }

        public static Color CorDe(string material)
        {
            switch (material)
            {
                case "EmissaoVermelha": return new Color(1f, 0.2f, 0.1f);
                case "EmissaoLaranja": return new Color(1f, 0.55f, 0.2f);
                case "EmissaoVerde": return new Color(0.3f, 1f, 0.4f);
                case "EmissaoRoxa": return new Color(0.65f, 0.3f, 1f);
                case "EmissaoDourada": return new Color(1f, 0.8f, 0.3f);
                case "EmissaoBranca": return Color.white;
                default: return new Color(0.3f, 0.65f, 1f);
            }
        }
    }
}
