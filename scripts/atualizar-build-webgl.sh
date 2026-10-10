#!/usr/bin/env bash
# Depois de gerar o build WebGL no Unity (menu Monge Arcano > 2 ou 3), publique a atualização:
set -e
git add docs
git commit -m "build: atualiza build WebGL" || echo "Nada para commitar."
git push
