#!/usr/bin/env bash
# Uso (Git Bash, na raiz do projeto):  bash scripts/publicar-no-github.sh
set -e
echo "=== Publicar Monge Arcano no GitHub ==="
read -r -p "URL do repositório vazio no GitHub (ex.: https://github.com/SEU_USUARIO/monge-arcano-mosteiro-condenado.git): " URL
[ -z "$URL" ] && { echo "URL obrigatória."; exit 1; }

if [ ! -d .git ]; then
  git init
fi
git checkout -B main
git add .
git commit -m "feat: Monge Arcano - O Mosteiro Condenado (desafio DIO Unity)" || echo "Nada novo para commitar."
git remote remove origin 2>/dev/null || true
git remote add origin "$URL"
git push -u origin main
echo ""
echo "Pronto! Para o link jogável: GitHub > Settings > Pages > Branch main, pasta /docs."
