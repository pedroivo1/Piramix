# Piramix — guia do código

## 1. Núcleo do projeto (explique com mais cuidado)

### `Assets/Scripts/Piramix.cs` — giro das camadas

Fica no objeto **Piramix**, que é o pai das 15 peças. É o arquivo mais importante.

#### a) Os 4 eixos de rotação (`Piramix.cs:12`)

Uma Pyraminx gira em torno de 4 eixos, cada um passando por um vértice e pelo centro da pirâmide. No espaço local da Piramix, com o centro na origem, os vértices estão nas direções:

```
(1, 1, 1)   (1, −1, −1)   (−1, 1, −1)   (−1, −1, 1)
```

Essas direções vêm de um fato geométrico: um tetraedro regular é formado por 4 vértices alternados de um cubo.

#### b) A que camada pertence cada peça (`CamadaDe`, `Piramix.cs:41`)

Para um eixo `ê`, a peça é classificada pelo **produto escalar** entre a posição dela e o eixo, `d = p · ê`. Esse valor é a "altura" da peça ao longo do eixo.

| Condição | Camada | Peças |
|---|---|---|
| `d > 0,9` | ponta | 1 |
| `0,3 < d ≤ 0,9` | meio | 4 |
| `d ≤ 0,3` | base | 10 |

Como a classificação usa a posição atual, ela continua correta depois de qualquer sequência de giros.

#### c) A rotação (`Girar`, `Piramix.cs:124`)

Cada peça da camada recebe o mesmo quatérnio `q = AngleAxis(±120°, eixo)`:

- posição nova: `p' = q · p`
- orientação nova: `r' = q · r`

Esse é o ponto central para explicar. Girar só a posição faria a peça "transladar" sem virar. Por isso a orientação também é multiplicada por `q`. Como o eixo passa pela origem do espaço local, não é preciso transladar para a origem antes de girar e voltar depois.

O giro é de 120° porque a pirâmide tem simetria de ordem 3 em torno de cada eixo de vértice.

#### d) Animação (`Piramix.cs:135`)

Durante 0,25 s, o ângulo cresce de 0 a 120° seguindo `SmoothStep`, uma curva que acelera e desacelera suavemente. A cada frame o código recalcula `q(t)` a partir da pose inicial da peça. Ele não soma pequenos incrementos, e isso evita acumular erro durante a animação.

#### e) Correção de erro numérico (`Aplicar`, `Piramix.cs:149`)

Depois de muitos giros, erros de ponto flutuante se acumulariam e as peças sairiam do lugar. Duas propriedades corrigem isso:

- **Posições:** todas as coordenadas são múltiplos de `s = 1/(2√2)`, então são arredondadas para o múltiplo mais próximo.
- **Rotações:** todas as orientações possíveis são **matrizes de permutação com sinal**, isto é, só têm entradas 0, +1 ou −1. Isso acontece porque as simetrias do tetraedro são um subgrupo das simetrias do cubo. O código arredonda os vetores "frente" e "cima" da peça para inteiros e reconstrói o quatérnio exato com `LookRotation`.

#### f) Picking: descobrir onde o usuário clicou (`Clicar`, `Piramix.cs:77`)

- `ScreenPointToRay` cria um **raio** que sai da câmera e passa pelo pixel clicado. É a inversa da projeção perspectiva.
- `Physics.Raycast` intersecta esse raio com os `MeshCollider` das peças, que são adicionados no `Awake`.
- A normal da face atingida é levada para o espaço local da Piramix. Comparando-a, por produto escalar, com as 4 normais das faces grandes, o código descobre qual face foi clicada.

#### g) Escolher o giro pelo arraste (`EscolherGiro`, `Piramix.cs:99`)

Para cada eixo candidato e cada sentido (+ ou −), o código:

1. gira o ponto clicado em 5° em torno do eixo;
2. **projeta** os pontos antes e depois na tela (`WorldToScreenPoint`, que aplica model → view → projeção → viewport);
3. calcula a direção em que o ponto andaria na tela.

Vence o eixo cuja direção tem o maior produto escalar com a direção do arraste. O eixo perpendicular à face clicada é ignorado, porque girar em torno dele moveria a face dentro do próprio plano.

### `Assets/Scripts/CameraOrbita.cs` — câmera orbital

Fica na **Main Camera**.

- **Coordenadas esféricas** (`Posicionar`, `CameraOrbita.cs:57`): a câmera é descrita por três valores, yaw (ângulo horizontal), pitch (ângulo vertical) e distância. A posição é `centro + Euler(pitch, yaw, 0) · (0, 0, −distância)`. Depois `LookAt(centro)` monta a matriz de visão apontando para o centro da pirâmide.
- **Alt + arrastar** (`CameraOrbita.cs:35`): o deslocamento do mouse em pixels vira graus. Na horizontal muda o yaw e na vertical muda o pitch.
- **Pitch limitado a ±80°:** em ±90° a câmera ficaria exatamente sobre o polo. Aí o vetor "cima" usado pelo `LookAt` fica paralelo à direção de visão, e a câmera giraria de forma descontrolada.
- **Zoom exponencial** (`CameraOrbita.cs:49`): `distância *= 0,9^scroll`. Cada clique da roda muda a distância em 10%, então o zoom tem a mesma sensação de perto e de longe. A distância fica limitada entre 3,5 e 20.
- O método `AltPressionado` também é usado pelo `Piramix.cs`, para não girar camada e câmera ao mesmo tempo.

---

## 2. Geometria e montagem da cena

### `Assets/Scenes/SampleScene.unity` — hierarquia (grafo de cena)

```
Piramix                  ← pai: girado para a ponta ficar em +Y, base em y=0
├── Tetraedro 1..10      ← peças "em pé"
├── Octaedro 1..4        ← preenchem os vãos
├── Tetraedro Invertido  ← centro (interno, fecha o sólido)
│    └── Adesivo <Cor>   ← adesivos são filhos das peças
```

- **Posição de cada peça**, chamando de `Vₘ` os vértices de um tetraedro de aresta 1:
  - 10 tetraedros em pé em `Va + Vb`, todas as combinações de pares com repetição;
  - 4 octaedros em `Vₘ`;
  - 1 tetraedro invertido na origem, girado 90° em Z, o que transforma `V` em `−V`.
- **Conferência de volume:** um tetraedro de lado 3 tem volume 27 vezes o de lado 1. Um octaedro de aresta 1 vale 4 tetraedros. Então `10·1 + 4·4 + 1 = 27`, e não sobra buraco.
- **Pai rotacionado:** o objeto Piramix aplica uma única rotação que leva a direção `(−1, 1, −1)` para `+Y`. Assim as peças trabalham em coordenadas locais simples, e a matriz do pai coloca tudo em pé (`mundo = M_pai · local`).
- **Adesivos como filhos:** é por isso que eles giram junto com as peças sem nenhum código extra. Quem faz isso é a composição de transformações do grafo de cena.

### Malhas (`.asset`)

| Arquivo | O que é | Detalhe para explicar |
|---|---|---|
| `Assets/Octaedro.asset` (novo) | octaedro de aresta 1 | vértices em `±a` nos eixos (`a = 1/√2`); 8 faces × 3 = **24 vértices**, para que cada face tenha a própria normal (sombreamento plano) |
| `Assets/Adesivo.asset` (novo) | prisma triangular fino | lado 0,9 e altura 0,02; 18 vértices (2 triângulos + 3 laterais); é 90% da face, por isso sobra uma borda preta |
| `Assets/Tetraedro.asset` (corrigido) | tetraedro de aresta 1 | estava **do avesso**: as normais apontavam para dentro. A ordem dos vértices dos triângulos foi invertida |

**Sobre a ordem dos vértices (winding):** no Unity, a face visível é aquela cujos vértices aparecem em **sentido horário** para quem olha. A normal é o produto vetorial `(b − a) × (c − a)`. Com a ordem invertida, a GPU descarta pelo *back-face culling* exatamente as faces que deveriam aparecer.

### Materiais (`Assets/Materials/`)

`Vermelho.mat`, `Verde.mat`, `Azul.mat` e `Amarelo.mat` são cópias do `Preto.mat` (shader URP Lit) com outra cor base. Há uma cor por face grande.

---

## 3. Arquivos de apoio (menos importantes)

- `Assets/Scripts/TetraedroRegular.cs`: só a ordem dos triângulos foi corrigida (linhas 33–36), igual à correção do `Tetraedro.asset`.
- `Assets/Scripts/Giro.cs`: removido. Era um método solto, fora de qualquer classe, e quebrava a compilação do projeto todo.
- `.gitignore`: tira do repositório o que o Unity gera sozinho (`Library/`, `Temp/`, `Logs/`, `*.csproj`) e os arquivos do build.
- Os arquivos `.meta` dos itens novos guardam o identificador (GUID) que a cena usa para referenciá-los.

---

## 4. O que não está no projeto, mas o professor pode perguntar

- **Como as posições foram calculadas:** com scripts Python temporários. Eles calcularam as fórmulas acima, geraram os meshes e editaram o arquivo da cena. Também validaram a geometria: sortearam 20 mil pontos dentro da pirâmide e confirmaram que cada um cai em exatamente uma peça.
- **Testes:** o Unity rodou em modo batch numa cópia do projeto. Os testes verificaram que 3 giros iguais voltam ao início, que depois de 300 giros aleatórios o sólido continua fechado e com 9 adesivos por face, e que o clique e a câmera funcionam com um mouse e um teclado simulados.

Esses scripts ficaram numa pasta temporária e não fazem parte do repositório.

---

## Ordem sugerida de estudo

1. Grafo de cena e transformações locais.
2. `CamadaDe`, que é um produto escalar.
3. `Girar`, onde estão `p' = q · p` e `r' = q · r`.
4. `Aplicar`, com o arredondamento.
5. Picking, com o raycast e a projeção.
6. Câmera em coordenadas esféricas.
