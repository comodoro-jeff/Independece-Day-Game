# Independence Day Game — Desenvolvimento de Jogo 3D na Unity

Prova de conceito desenvolvida para a disciplina de Computação Gráfica e Processamento de Sinais com o objetivo de explorar a criação de jogos 3D e a integração de sistemas em um motor gráfico.

## Sobre o projeto

O projeto consiste no desenvolvimento de uma prova de conceito de um jogo 3D utilizando a engine Unity como ambiente de simulação interativa.

A ideia surgiu da necessidade de aplicar na prática conceitos teóricos de computação gráfica, matrizes de transformação, iluminação, sistemas de física e processamento de sinais em um ambiente tridimensional.

O projeto foi desenvolvido academicamente como uma primeira experiência prática com motores gráficos e desenvolvimento de jogos em equipe.

## Objetivo

Explorar, de forma prática, o processo de construção e estruturação de uma aplicação 3D, passando pela configuração do ambiente na Unity, organização de assets, gerenciamento de pacotes, escrita de scripts em C# e integração de pipelines de renderização.

## Módulos e Recursos

A estrutura da aplicação foi organizada seguindo os padrões nativos da Unity para separação de dependências e recursos:

| Pasta / Módulo | Descrição |
| --- | --- |
| `Assets` | Contém os modelos 3D, materiais, texturas, áudios e scripts do jogo |
| `ProjectSettings` | Armazena as configurações globais de física, entrada, gráficos e URP |
| `Packages` | Gerencia os pacotes e dependências nativas mantidas pelo Unity Package Manager |

## Funcionamento

A aplicação carrega as configurações do projeto, inicializa a cena tridimensional e executa a lógica do jogo por meio de rotinas de atualização e renderização em tempo real.

De forma simplificada, o fluxo do projeto pode ser representado como:

```text
Configurações Globais (ProjectSettings)
        │
        ▼
Gerenciamento de Dependências (Packages)
        │
        ▼
Carregamento da Cena e Assets (Assets)
        │
        ▼
Loop de Execução da Engine (Unity Runtime)
        │
        ├── Entradas do Usuário (Input Manager)
        ├── Simulação de Física e Colisões
        └── Renderização Visual e Áudio
