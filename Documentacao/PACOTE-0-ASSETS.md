# Pacote 0 - Modelos 3D otimizados (42 arquivos, cerca de 95 MB)

Origem: seus três zips (1,3 GB). Cada modelo foi reduzido para uso em jogo (PC, celular e WebGL):

| Antes | Depois |
|---|---|
| GLB de 15 a 47 MB, cerca de 50 mil triângulos, 3 texturas 4096x4096 (cerca de 270 MB de memória de vídeo por modelo) | cerca de 0,2 a 7 MB, de 2 a 12 mil triângulos, texturas 512 ou 1024 |
| 3MF de impressão 3D (60 a 260 MB de XML, 230 mil a 500 mil triângulos, sem cor e sem textura) | GLB de 0,1 a 1 MB, de 7 a 60 mil triângulos, cor neutra para pintar no Unity |

## Como importar no Unity 6
1. Window > Package Manager > botão "+" > **Add package by name** > `com.unity.cloud.gltfast` (o Unity importa .glb nativamente só com esse pacote oficial).
2. Copie a pasta `Assets/Modelos` para dentro do projeto.
3. Os .glb aparecem como prefabs arrastáveis. Os modelos vêm em metros, de pé e centralizados.

## Mapa de papéis (suas anotações)
| Arquivo | Papel no jogo |
|---|---|
| Personagens/monge_clerigo | Monge (jogador), versão 3MF sem cor |
| Personagens/necromante | Segunda classe, liberada ao derrotar o último chefe |
| Personagens/cavaleiro_sombrio | Cavaleiro sombrio (NPC) |
| Personagens/bruxa | Bruxas: vermelha (mestra), negra (invocadora), cinza (lança armas). Pintar por cor no Unity |
| Inimigos/boss_morte | Guardião do chefe (Morte) |
| Inimigos/esqueleto_guerreiro | Guerreiros combatentes |
| Inimigos/armadura_assombrada | Armaduras decorativas que ganham vida e atacam |
| Inimigos/espectro, aparicao | Espectros e aparições |
| Inimigos/demonio, criatura_infernal, monstro | Demônios e criaturas infernais |
| Inimigos/criatura_invocada | Criatura que as bruxas invocam |
| Inimigos/mensageiro | Aparição mensageira que narra a história |
| Armas/cajado_monge, cajado_elevado | Cajado inicial e cajado de nível elevado |
| Armas/faca_osso | Arma da classe Necromante |
| Armas/ferramentas_osso | Armas arremessadas pelas bruxas |
| Armas/machado_boss | Machado do chefe |
| Armas/escudo_osso | Decoração |
| Itens/frasco_cura, frasco_mana, elixir_alma | Elixir de cura, poção de mana, elixir da alma (status por 1 minuto) |
| Itens/pergaminho | Pistas e pergaminhos |
| Itens/pedra_invocacao | Cai quando o chefe morre e transforma o monge em necromante |
| Itens/bolsa | Bolsa do inventário na cintura |
| Itens/suporte_chaves | Suporte das chaves |
| Cenario/monasterio_exterior, monasterio_interior, jardim_sombrio | Mosteiro por fora, por dentro (modelo mais adequado) e jardim sombrio |
| Cenario/criptas, lapides, arvores_mortas, dancarinos_cemiterio | Cemitério externo com caminho até a entrada |
| Cenario/altar_ossos, altar_foco_boss | Altar da sala antes do chefe e altar do poço do chefe |
| Cenario/trono_boss, trono_lateral | Trono central do chefe e tronos laterais |
| Cenario/portao_gotico, carruagem, bandeira, lanterna | Decoração (portão, carruagem, bandeiras da entrada, lanterna) |

## O que estes modelos NÃO têm (importante)
- **Nenhum modelo tem esqueleto nem animação.** São malhas estáticas. Para andar de verdade, o caminho é enviar o personagem ao Mixamo (gratuito, exige login) para rigging automático e baixar o FBX com animações. Enquanto isso, o jogo usa animação procedural do corpo inteiro (balanço, inclinação, golpe, reação a dano).
- **Os modelos vindos de 3MF não têm cor nem textura** (são miniaturas para impressão 3D). Eles recebem material no Unity (pedra, tecido, osso etc.).

## Licenças (leia antes de publicar no GitHub)
Os arquivos 3MF trazem nos metadados licença do tipo "Standard Digital File License" (MakerWorld). Esse tipo costuma permitir uso pessoal e **não** a redistribuição dos arquivos. Os GLB têm nomes de origem diferentes. Como o repositório será público e open source, confirme a licença de cada modelo antes de commitar os .glb, ou mantenha `Assets/Modelos` fora do Git (adicione ao `.gitignore`) e distribua só o código. Registre a autoria de cada asset em um arquivo CREDITOS.md.

## Ferramentas (pasta tools)
- `otimizar-glb.sh`: reduz qualquer GLB novo.
- `converter-3mf.py`: converte 3MF em GLB (edite a lista CONFIG no topo).
- `webp-para-png.mjs`: converte texturas WebP para PNG (o glTFast não lê WebP).
Cada GLB original foi mantido no seu computador; só os otimizados entram no projeto.
