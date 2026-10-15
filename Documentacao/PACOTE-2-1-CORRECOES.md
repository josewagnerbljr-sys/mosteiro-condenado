# Pacote 2.1 - Correções, câmera estilo Diablo e efeitos

Aplicar por cima do Pacote 2 (mesma rotina). Substitua também os modelos pelo `pacote-0-assets.zip` atualizado (criptas, casal dançante e lápides corrigidos).

## Passos
1. Unity fechado. Copie `Assets` deste pacote por cima de `Assets` do projeto, e `Assets/Modelos` do Pacote 0 novo.
2. Abra o Unity, espere compilar e importar. 
3. **Monge Arcano > 1 - Gerar cena** (isso também cria os materiais novos: FxAditivo e CeuTrevas, e escurece a paleta).
4. Play.

## O que foi corrigido (causas encontradas)
- **Blocos azuis translúcidos gigantes**: o arquivo dos dançarinos tinha uma lasca de 0,08 m. Ao normalizar a altura ela virou uma laje de 48 m, pintada de fantasma. Agora lascas são descartadas e todos os objetos de um arquivo mantêm a proporção entre si.
- **Criptas desalinhadas**: o arquivo trazia 6 criptas juntas num bloco só. Foram separadas em 6 objetos.
- **Interação com o Mensageiro de longe**: o gatilho de conversa herdava a escala do modelo (raio de 14 m). Agora é de 1,4 m reais.
- **Paredes**: sem câmera fixa, as paredes que ficam entre a câmera e o monge desaparecem (só visual, a colisão continua).
- **Altar do Santuário**: reduzido e agora dá para subir nele (degrau do jogador subiu para 0,5 m).

## Novidades
- **Câmera estilo Diablo** (padrão): inclinada, segue o monge, roda do mouse dá zoom. Tecla **Y** alterna para a câmera fixa por sala.
- **Casal esquelético dançando valsa** entre os túmulos: gira, orbita e sobe e desce em compasso de 3 tempos.
- **Céu de tempestade avermelhada**, névoa e luz rubra distante, brasas flutuando no cemitério e no Santuário, paleta mais escura.
- **Efeitos de magia**: projéteis com halo luminoso, rastro e faíscas; explosões com clarão, anel no chão e faíscas; faíscas ao acertar e ao matar; sangue ao levar dano. Fim das bolas brancas sólidas.
- **Marcha estilizada**: saltitos a cada passo, balanço lateral, torção, esticar e achatar, e ataque com antecipação e investida.

## Ainda não está neste pacote
Distribuição de pontos de status, inventário (I), drops de armas e árvore de habilidades: Pacote 4. Animação de membros (braços e pernas) depende de modelos com esqueleto.
