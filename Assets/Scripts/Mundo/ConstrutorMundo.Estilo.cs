using System.Collections.Generic;
using UnityEngine;

namespace MongeArcano
{
    /// <summary>Estilização do cenário: capim morto, névoa rasteira, ruínas, cruzes, velas, terreno distante e tempestade.</summary>
    public static partial class ConstrutorMundo
    {
        static void Capim(Transform pai, Vector3 min, Vector3 max, int n)
        {
            List<Vector3> v = new List<Vector3>();
            List<Vector3> nr = new List<Vector3>();
            List<Vector2> uv = new List<Vector2>();
            List<int> tri = new List<int>();
            for (int i = 0; i < n; i++)
            {
                float x = R(min.x, max.x), z = R(min.z, max.z);
                if (Mathf.Abs(x) < 2.8f) continue;
                float w = R(0.8f, 1.5f), h = R(0.5f, 1.2f), yaw = R(0f, 3.14f);
                for (int q = 0; q < 2; q++)
                {
                    float a = yaw + q * 1.5708f;
                    Vector3 d = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * w * 0.5f;
                    Vector3 p = new Vector3(x, 0f, z);
                    int b = v.Count;
                    v.Add(p - d); v.Add(p + d); v.Add(p + d + Vector3.up * h); v.Add(p - d + Vector3.up * h);
                    for (int k = 0; k < 4; k++) nr.Add(Vector3.up);
                    uv.Add(new Vector2(0f, 0f)); uv.Add(new Vector2(1f, 0f)); uv.Add(new Vector2(1f, 1f)); uv.Add(new Vector2(0f, 1f));
                    tri.Add(b); tri.Add(b + 2); tri.Add(b + 1); tri.Add(b); tri.Add(b + 3); tri.Add(b + 2);
                    tri.Add(b); tri.Add(b + 1); tri.Add(b + 2); tri.Add(b); tri.Add(b + 2); tri.Add(b + 3);
                }
            }
            Mesh m = new Mesh();
            m.name = "CapimCombinado";
            m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            m.SetVertices(v);
            m.SetNormals(nr);
            m.SetUVs(0, uv);
            m.SetTriangles(tri, 0);
            m.RecalculateBounds();
            GameObject go = new GameObject("Capim");
            go.transform.SetParent(pai, false);
            go.AddComponent<MeshFilter>().sharedMesh = m;
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = ProceduralModels.Mat("Capim");
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
        }

        static void Nevoa(Transform pai)
        {
            float[] ys = { 0.35f, 0.9f, 1.7f };
            for (int i = 0; i < ys.Length; i++)
            {
                GameObject q = ProceduralModels.Prim(PrimitiveType.Quad, pai, "Nevoa" + i, new Vector3(0f, ys[i], -47f), new Vector3(96f, 96f, 1f), "NevoaPlano", false, new Vector3(90f, i * 37f, 0f));
                NevoaDeriva nd = q.AddComponent<NevoaDeriva>();
                nd.velocidade = new Vector2(0.004f * (i + 1) * (i % 2 == 0 ? 1f : -1f), 0.0025f * (3 - i));
            }
        }

        static void Cruz(Transform pai, Vector3 p)
        {
            Transform t = Filho(pai, "CruzDeMadeira");
            t.position = p;
            t.rotation = Quaternion.Euler(R(-12f, 12f), R(0f, 360f), R(-14f, 14f));
            ProceduralModels.Prim(PrimitiveType.Cube, t, "Haste", new Vector3(0f, 0.6f, 0f), new Vector3(0.12f, 1.2f, 0.12f), "MadeiraMorta");
            ProceduralModels.Prim(PrimitiveType.Cube, t, "Travessa", new Vector3(0f, 0.9f, 0f), new Vector3(0.65f, 0.11f, 0.11f), "MadeiraMorta");
        }

        static void Coluna(Transform pai, Vector3 p)
        {
            float h = R(1f, 3.4f);
            ProceduralModels.Prim(PrimitiveType.Cylinder, pai, "ColunaQuebrada", p + new Vector3(0f, h * 0.5f, 0f), new Vector3(0.9f, h * 0.5f, 0.9f), "PedraMusgo", true);
            for (int i = 0; i < 4; i++)
            {
                Vector3 o = new Vector3(R(-1.4f, 1.4f), 0.2f, R(-1.4f, 1.4f));
                GameObject e = ProceduralModels.Prim(PrimitiveType.Cube, pai, "Entulho", p + o, new Vector3(R(0.3f, 0.8f), R(0.2f, 0.5f), R(0.3f, 0.8f)), "PedraMusgo");
                e.transform.localRotation = Quaternion.Euler(R(0f, 30f), R(0f, 360f), R(0f, 30f));
            }
        }

        static void Velas(Transform pai, Vector3 p)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector3 o = new Vector3(R(-0.4f, 0.4f), 0f, R(-0.4f, 0.4f));
                float h = R(0.12f, 0.3f);
                ProceduralModels.Prim(PrimitiveType.Cylinder, pai, "Vela", p + o + new Vector3(0f, h * 0.5f, 0f), new Vector3(0.07f, h * 0.5f, 0.07f), "Osso");
                ProceduralModels.Prim(PrimitiveType.Sphere, pai, "Chama", p + o + new Vector3(0f, h + 0.05f, 0f), new Vector3(0.06f, 0.1f, 0.06f), "EmissaoDourada");
            }
            ProceduralModels.Luz(pai, "LuzVelas", p + new Vector3(0f, 0.5f, 0f), new Color(1f, 0.7f, 0.35f), 0.8f, 4.5f, true);
        }

        static void Estilo()
        {
            Transform e = Filho(tEst, "Estilo");
            ProceduralModels.Prim(PrimitiveType.Cube, e, "TerrenoDistante", new Vector3(0f, -0.7f, 0f), new Vector3(700f, 0.2f, 700f), "Terra");
            Capim(e, new Vector3(-35f, 0f, -84f), new Vector3(35f, 0f, -12f), 2600);
            Nevoa(e);

            GameObject rel = new GameObject("Relampago");
            rel.transform.SetParent(e, false);
            Light rl = rel.AddComponent<Light>();
            rl.type = LightType.Directional;
            rl.color = new Color(0.7f, 0.78f, 1f);
            rl.intensity = 0f;
            rl.shadows = LightShadows.None;
            rel.AddComponent<Relampago>();

            for (int i = 0; i < 16; i++)
            {
                float lado = rnd.NextDouble() < 0.5 ? -1f : 1f;
                Cruz(e, new Vector3(lado * R(4.5f, 33f), 0f, R(-81f, -14f)));
            }
            for (int i = 0; i < 9; i++)
            {
                float lado = rnd.NextDouble() < 0.5 ? -1f : 1f;
                Coluna(e, new Vector3(lado * R(6f, 32f), 0f, R(-80f, -16f)));
            }
            for (int i = 0; i < 8; i++)
            {
                float lado = rnd.NextDouble() < 0.5 ? -1f : 1f;
                Velas(e, new Vector3(lado * R(4.5f, 20f), 0f, R(-78f, -16f)));
            }
        }
    }
}
