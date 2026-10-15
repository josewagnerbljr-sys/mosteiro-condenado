# Pacote 2 - Mundo e criaturas com modelos 3D reais

Requer o Pacote 1 e o Pacote 0 (modelos em `Assets/Modelos`) já aplicados.

## Passo a passo
1. Feche o Unity. Copie a pasta `Assets` deste pacote por cima da `Assets` do projeto.
2. Abra o projeto. No Package Manager, instale **com.unity.cloud.gltfast** (Add package by name). Sem ele os .glb não são importados e o jogo usa as formas simples.
3. Espere importar os 42 modelos (alguns minutos no HDD). Confira no painel Project se `Assets/Modelos/...` mostra os modelos (clique em um .glb para ver no Inspector).
4. Menu **Monge Arcano > 1 - Gerar cena do Mosteiro**. Play.

## O que muda
- **O jogo começa fora do mosteiro**: cemitério com caminho, lápides, criptas, árvores mortas, dançarinos fantasmagóricos, lampiões, carruagem, bandeiras e portão gótico na entrada, castelo na névoa ao fundo, luar. Altar de salvamento logo no início e o Mensageiro (NPC, conversa com C).
- **Portão principal** do Salão (antes selado) agora abre; missão nova "Chegada ao Mosteiro".
- **Personagens com modelos reais**: monge (Hooded Cleric), aparições (possuídos), espectro, demônio (criatura infernal), Morte (chefe), esqueletos guerreiros, **armaduras assombradas** que parecem decoração e ganham vida quando você passa perto, 3 bruxas, monstro minotauro.
- **Bruxas**: Mestra (vermelha, lança magia e invoca), Invocadora (negra, invoca esqueletos), Lançadora (cinza, atira projéteis). Elas recitam um encanto sintetizado; chefe e armaduras gritam.
- **Interior** decorado: tronos do chefe, altares de ossos, bandeiras.
- Animação do corpo inteiro: balanço, torção, respiração, golpe, reação a dano, passos.

## Se algo ficar estranho
- **Modelos rosa/magenta**: o glTFast usa shader próprio. Em Edit > Project Settings > Graphics confira o pipeline (deve estar vazio, o jogo usa o Built-in).
- **Modelo muito claro/escuro ou de tamanho errado**: me diga o nome; ajusto altura/material no código (ProceduralModels.CriarInimigo e ConstrutorMundo.Exterior).
- Modelos que vieram de 3MF não têm textura; são pintados por código (tons de pedra, tecido, bruxas vermelha/negra/cinza).
- A criatura "criatura_infernal.glb" veio quebrada do arquivo original e não é usada.

## Limitações conhecidas
- Os modelos não têm esqueleto: a animação é do corpo inteiro, não de braços e pernas.
- Peso no celular: o exterior tem cerca de 250 mil triângulos; a otimização para celular vem no Pacote 6.
- Não testei no Unity (apenas compilação e tipos); reporte o que aparecer no Console.
