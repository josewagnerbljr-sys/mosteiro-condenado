import zipfile, sys, os, re, subprocess, numpy as np, trimesh, fast_simplification
from lxml import etree

U = os.environ.get("ASSETS_ZIPS", "/mnt/user-data/uploads")
OUT = os.environ.get("ASSETS_OUT", "/tmp/opt")

import re
VRE = re.compile(rb'<vertex x="([^"]+)" y="([^"]+)" z="([^"]+)"')
TRE = re.compile(rb'<triangle v1="(\d+)" v2="(\d+)" v3="(\d+)"')

def parse(zf, name):
    data = zf.read(name)
    objs = []
    for p in re.split(rb'(?=<object )', data):
        if not p.startswith(b'<object '): continue
        m = re.match(rb'<object id="(\d+)"', p)
        oid = m.group(1).decode() if m else "0"
        V = VRE.findall(p)
        if not V: continue
        T = TRE.findall(p)
        if not T: continue
        objs.append((oid, np.array(V).astype(np.float32), np.array(T).astype(np.int32)))
    del data
    return objs

def zup_to_yup(p):
    return np.stack([p[:, 0], p[:, 2], -p[:, 1]], axis=1)

# slug, zip, padrão do arquivo, modo, altura/tamanho alvo (m) do maior objeto, triângulos por objeto, cor base, rugosidade
CONFIG = [
 ("monge_clerigo",      "1.zip", "Hooded+Cleric1",      "joined",   1.8, 12000, (0.55,0.5,0.45)),
 ("cavaleiro_sombrio",  "1.zip", "Knight-cavaleiro",    "joined",   2.0, 12000, (0.35,0.35,0.4)),
 ("necromante",         "1.zip", "BAMBU_Necromancer",   "joined",   1.9, 14000, (0.3,0.25,0.35)),
 ("bruxa",              "1.zip", "fantasy+witch",       "separate", 1.8, 10000, (0.5,0.5,0.5)),
 ("arvores_mortas",     "1.zip", "arboles_variados",    "separate", 4.0, 3500,  (0.25,0.2,0.16)),
 ("criptas",            "1.zip", "criptas que",         "separate", 3.0, 6000,  (0.5,0.52,0.48)),
 ("lapides",            "1.zip", "lapidas+dnd",         "separate", 1.1, 1800,  (0.5,0.52,0.5)),
 ("dancarinos_cemiterio","1.zip","Eternal+Halloween",   "separate", 1.8, 5000,  (0.6,0.6,0.62)),
 ("altar_ossos",        "1.zip", "cathedral_of_bones",  "joined",   3.0, 14000, (0.7,0.68,0.6)),
 ("monasterio_interior","1.zip", "Fortezza+Necromante", "joined",   28.0, 60000, (0.4,0.4,0.42)),
 ("jardim_sombrio",     "1.zip", "DarkGarden200x200",   "joined",   60.0, 50000, (0.35,0.4,0.33)),
 ("altar_foco_boss",    "2.zip", "ornate+stone+altar",  "joined",   2.5, 14000, (0.5,0.5,0.52)),
]

def main(only=None):
    for slug, zname, pat, modo, alvo, budget, cor in CONFIG:
        if only and slug not in only: continue
        work = "/tmp/work3"
        os.makedirs(work, exist_ok=True)
        subprocess.run("rm -rf %s/*" % work, shell=True)
        subprocess.run(["unzip", "-q", "-o", os.path.join(U, zname), "*%s*" % pat, "-d", work], stderr=subprocess.DEVNULL)
        found = []
        for r, d, fs in os.walk(work):
            for f in fs:
                if f.endswith(".3mf"): found.append(os.path.join(r, f))
        if not found:
            print(slug, "NAO ENCONTRADO", flush=True); continue
        zf = zipfile.ZipFile(found[0])
        objs = []
        for n in zf.namelist():
            if n.endswith(".model"): objs += parse(zf, n)
        objs = [o for o in objs if len(o[2]) > 150]
        if not objs:
            print(slug, "SEM MALHA", flush=True); continue
        # escala: maior dimensão do maior objeto -> alvo (metros)
        maior = max(objs, key=lambda o: len(o[2]))
        ext = (maior[1].max(0) - maior[1].min(0)).max()
        if modo == "joined":
            ext = max((o[1].max(0) - o[1].min(0)).max() for o in objs)
        esc = alvo / ext
        scene = trimesh.Scene()
        origem_total = np.concatenate([o[1] for o in objs]).min(0) if modo == "joined" else None
        total = 0
        for i, (oid, v, t) in enumerate(objs[:40]):
            if len(t) > budget:
                r = 1.0 - budget / float(len(t))
                v, t = fast_simplification.simplify(v, t, target_reduction=min(r, 0.995))
            v = v.astype(np.float64) * esc
            v = zup_to_yup(v)
            if modo == "separate":
                v[:, 0] -= (v[:, 0].max() + v[:, 0].min()) / 2
                v[:, 2] -= (v[:, 2].max() + v[:, 2].min()) / 2
                v[:, 1] -= v[:, 1].min()
            else:
                pass
            m = trimesh.Trimesh(vertices=v, faces=t, process=True)
            m.visual = trimesh.visual.TextureVisuals(material=trimesh.visual.material.PBRMaterial(
                baseColorFactor=[int(cor[0]*255), int(cor[1]*255), int(cor[2]*255), 255], metallicFactor=0.05, roughnessFactor=0.85))
            scene.add_geometry(m, node_name="%s_%d" % (slug, i), geom_name="%s_%d" % (slug, i))
            total += len(m.faces)
        if modo == "joined":
            # centraliza em XZ e apoia no chão
            b = scene.bounds
            T = np.eye(4); T[0, 3] = -(b[0][0] + b[1][0]) / 2; T[1, 3] = -b[0][1]; T[2, 3] = -(b[0][2] + b[1][2]) / 2
            scene.apply_transform(T)
        path = os.path.join(OUT, slug + ".glb")
        scene.export(path)
        print(slug, "objetos:", min(len(objs), 40), "tris:", total, "->", os.path.getsize(path) // 1024, "KB", flush=True)

if __name__ == "__main__":
    main(sys.argv[1:] or None)
