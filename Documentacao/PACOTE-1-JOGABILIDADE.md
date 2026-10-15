# Pacote 1 - Jogabilidade e interface (versão 2.0)

## Como aplicar
1. Feche o Unity.
2. Copie a pasta `Assets` deste pacote por cima da pasta `Assets` do projeto (aceite substituir tudo).
   Se existir `Assets/Scripts/monge-arcano` sobrando de uma cópia anterior, apague.
3. Abra o projeto no Unity. Quando compilar sem erros, rode **Monge Arcano > 1 - Gerar cena do Mosteiro** (a cena precisa ser gerada de novo).
4. Play.

## O que mudou
- **Movimento**: direto, relativo à câmera, com aceleração e giro suave. A direção é mantida durante cortes de câmera. Tecla T alterna para o modo tanque.
- **Controles**: W A S D mover, Shift correr, **ESPAÇO** Raio Arcano, F Nova Sagrada, G Leque Arcano, B Escudo Místico, **R** Invocação Celeste, **C** coletar/interagir, **V** curar, X golpe de cajado, 1 a 6 atalhos de itens.
- **Barra de atalhos (embaixo)**: 1 Erva Sagrada, 2 Frasco de Essência, 3 a 6 chaves e sigilos.
- **Barra de magias (direita)**: 5 ícones com a tecla embaixo, custo e recarga. Na pausa (TAB) dá para arrastar as duas barras; a posição é salva.
- **Poder Arcano**: barra que enche ao causar dano. Cheia, a tecla R lança a Invocação Celeste: explosão grande mais 14 meteoros em área.
- **XP e níveis**: cada nível aumenta vida, essência e dano das magias.
- **Missões**: 7 principais em sequência e 4 secundárias, com progresso por eventos (vale mesmo fora de ordem) e XP de recompensa.
- **Aproximação de câmera**: ao pegar chave ou sigilo e ao abrir um documento a câmera se aproxima do objeto, com faixas de cinema.
- **Música dinâmica** (gerada por código): exploração, combate e chefe, com transição suave. Novos efeitos sonoros para cada magia, passos, subir de nível e missão.
- **Animação procedural**: balanço do corpo, inclinação, golpe, conjuração e reação a dano para o monge e os monstros.

## Limitações desta etapa
Os personagens ainda são as formas simples do protótipo; os modelos 3D do Pacote 0 entram no Pacote 2 e 3. Não testei no Unity (só a compilação e os tipos foram verificados), então reporte qualquer erro do Console.
