#!/usr/bin/env bash
# Reduz um GLB pesado (ex.: 40 MB, 50 mil triângulos, 3 texturas 4K) para algo leve (ex.: 3 MB, 10 mil triângulos, texturas 1024).
# Requisitos: Node.js e  npm i -g @gltf-transform/cli
# Uso: bash otimizar-glb.sh entrada.glb saida.glb [tamanho_textura=1024] [proporcao_triangulos=0.2]
set -e
IN="$1"; OUT="$2"; TEX="${3:-1024}"; RATIO="${4:-0.2}"
gltf-transform optimize "$IN" /tmp/_a.glb --compress false --texture-compress false --simplify true --simplify-ratio "$RATIO" --simplify-error 0.01
gltf-transform resize /tmp/_a.glb "$OUT" --width "$TEX" --height "$TEX"
echo "Pronto: $OUT"
