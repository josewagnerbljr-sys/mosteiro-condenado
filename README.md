# 🕯️ Mosteiro Condenado: Monge Arcano

> Desafio **DIO - Trilha Unity**: criar uma cena 3D no Unity com boa composição, aplicar física e publicar via WebGL.
> Repositório de referência: [digitalinnovationone/trilha-unity](https://github.com/digitalinnovationone/trilha-unity)

Survival horror 3D com **câmeras fixas por sala** e **controles de tanque**, inspirado na estrutura clássica do gênero
(mansão trancada, puzzles de chaves, recursos escassos, salas seguras), com **história, personagens, criaturas e código 100% originais**.
Em vez de zumbis, você enfrenta **possuídos, espectros, criaturas infernais e o Arconte das Cinzas**.
Você é o **Irmão Elian**, um monge com poderes arcanos.

## ▶️ Jogar online
**https://josewagnerbljr-sys.github.io/mosteiro-condenado/**

(Hospedado no GitHub Pages a partir da pasta `docs/`. Se a página ainda mostrar a tela de aviso, o build WebGL ainda não foi publicado.)

## 🎮 Controles
| Tecla | Ação |
|---|---|
| **W / S** (ou setas) | Andar / recuar (estilo tanque) |
| **A / D** (ou setas) | Girar |
| **Shift** | Correr |
| **Q** | Giro rápido de 180° |
| **Espaço** ou clique | Raio Arcano (1 essência, mira automática) |
| **F** | Explosão Sagrada (5 essência, dano em área + empurra objetos) |
| **V** | Golpe de cajado (grátis, evita ficar sem defesa) |
| **E** | Interagir, pegar itens, ler arquivos, abrir portões, rezar no altar |
| **H** | Usar Erva Sagrada (+45 de vida) |
| **Tab / P / Esc** | Pausa e inventário |

## 🧩 Progressão (cena completa de evolução)
1. **Salão de Entrada**: leia o diário do Abade.
2. **Capela** (oeste): altar de salvamento, **Chave de Ferro** e **Sigilo da Lua**, guardado por **espectros**.
3. **Biblioteca** (leste, exige a Chave de Ferro): **Sigilo do Fogo**, **Chave de Osso** e **possuídos**.
4. **Catacumbas** (norte, exige a Chave de Osso): **Sigilo da Terra** e **criaturas infernais**.
5. **Santuário** (exige os 3 sigilos): batalha contra o **Arconte das Cinzas** (2 fases, invoca espíritos), depois o **portal de luz** para vencer.

Recursos escassos (essência e ervas), checkpoints no altar, 5 arquivos para colecionar, tela de derrota/vitória com estatísticas.

## 🧠 Conceitos aplicados no desafio
- **Composição de cena**: salas, corredores, pilares, tochas, sarcófagos, estantes, iluminação por sala, névoa e câmeras fixas (`ZonaCamera` + `CameraDirector`).
- **Física**: `CharacterController` (jogador), `Rigidbody` (inimigos, caixas, barris, projéteis), colisões, `AddExplosionForce`, empurrões, projéteis com detecção contínua.
- **Scripts C#**: máquina de estados (`GameManager`), IA (`Inimigo`), interfaces (`IDanificavel`), classes abstratas (`Interagivel`), coroutines, construtor procedural de mundo.
- **Publicação WebGL**: build automatizado em `docs/` pronto para GitHub Pages.
- **Zero assets externos**: modelos feitos com primitivas + materiais criados por código; áudio sintetizado por código (sem direitos autorais).

## 🛠️ Como abrir no Unity Hub
1. Instale o **Unity 6 (6000.6.4f1)** com o módulo **WebGL Build Support**. O projeto foi criado no 2022.3 LTS e atualizado para o Unity 6.
2. Unity Hub → **Add → Add project from disk** → selecione esta pasta.
3. Ao abrir, o projeto **gera sozinho** a cena `Assets/Scenes/Mosteiro.unity` e os materiais (aguarde ~10 s). Se não gerar, use o menu **Monge Arcano → 1 - Gerar cena do Mosteiro**.
4. Aperte **Play** e jogue.

## 🌐 Gerar o link jogável (WebGL + GitHub Pages)
1. No Unity: **Monge Arcano → 3 - Gerar cena + Build WebGL** (saída em `docs/`).
2. Commit e push da pasta `docs` (`bash scripts/atualizar-build-webgl.sh`).
3. GitHub → **Settings → Pages → Branch `main`, pasta `/docs`**. O link aparece em 1 a 3 minutos.

Build por linha de comando (opcional):
```bash
Unity -batchmode -quit -projectPath . -executeMethod MongeArcano.EditorTools.ConstrutorDeBuild.BuildWebGL -logFile -
```

## 📤 Subir para o GitHub (Git Bash)
1. Repositório: `https://github.com/josewagnerbljr-sys/mosteiro-condenado` (criado vazio).
2. Na raiz do projeto, no Git Bash: `bash scripts/publicar-no-github.sh`
3. Aperte ENTER para aceitar a URL padrão.

> Dica: abra o projeto no Unity **antes** de publicar para que `Assets/Scenes`, `Resources/Materiais` e os arquivos `.meta` sejam gerados e versionados. Nunca ignore `.meta` no `.gitignore`.

## 📁 Estrutura
```
Assets/
  Editor/            Gerador de cena, materiais e build WebGL (menu "Monge Arcano")
  Scripts/
    Core/            GameManager, GameUI, contratos (IDanificavel, Interagivel)
    Jogador/         PlayerController (tanque, magias, física), Projetil
    Inimigos/        Inimigo (IA, 4 tipos, chefe em 2 fases)
    Mundo/           ConstrutorMundo, Porta, Coletavel, Altar, Portal, câmeras fixas, Flicker
    Visual/          ProceduralModels (modelos 3D), Efeitos, Som (áudio procedural)
docs/                Build WebGL (GitHub Pages)
scripts/             Automação de publicação (Git Bash)
```

## 🔧 Ajustes rápidos
- Dificuldade: vida/dano/velocidade dos inimigos em `ProceduralModels.CriarInimigo`.
- Mapa e itens: `ConstrutorMundo`. Depois de editar, regenere a cena pelo menu (ou edite a cena direto no Unity depois de gerada).
- Para trocar por modelos próprios: substitua os filhos visuais dos prefabs/objetos mantendo os componentes.

## 👤 Autor
**José Wagner Blanco Júnior**, Maringá/PR

- GitHub: [josewagnerbljr-sys](https://github.com/josewagnerbljr-sys)
- DIO: [consultoriablanco8](https://web.dio.me/users/consultoriablanco8)
- LinkedIn: [blancoconsultoria](https://www.linkedin.com/in/blancoconsultoria)

Projeto de portfólio do desafio DIO (Trilha Unity). Licença MIT.
