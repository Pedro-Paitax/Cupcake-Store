# Plano e resultado dos testes

O sistema é testado em três níveis:

| Nível | Ferramenta | O que cobre | Como rodar |
|---|---|---|---|
| Unidade | xUnit | Regras de negócio dos serviços (usuário, cupcake, pedido) e utilitários | `dotnet test` |
| Integração | xUnit + `WebApplicationFactory` | A aplicação inteira em memória: rotas, JSON, JWT, códigos HTTP, banco | `dotnet test` |
| Sistema / aceitação | Roteiro manual + testadores | Jornada completa no app, na loja e no painel | Seção 3 abaixo |
| Banco | psql | Restrições do PostgreSQL (únicos, checks, chaves estrangeiras) | `situacao-1/banco/04-teste-restricoes.sql` |

## 1. Testes automatizados (verificação)

São 70 casos ao todo, contando cada linha de `[InlineData]`. Eles rodam a cada push no GitHub Actions; o relatório fica em *Actions → API - build e testes → resultados-testes*.

### Usuário (`UsuarioServiceTests`): 18 casos
| Caso | Esperado |
|---|---|
| Cadastro válido | Grava com e-mail em minúsculas e telefone só com dígitos |
| Senha | Gravada como hash, nunca o texto digitado |
| E-mail repetido (mesmo com maiúsculas) | Conflito: "Este e-mail já está em uso." |
| E-mail vazio, sem @ ou sem domínio | Validação: "O formato do e-mail é inválido." |
| Telefone com 7, 9 ou 15 dígitos | Validação |
| Senha com 7 caracteres | Validação: "mínimo 8 caracteres" |
| Nome em branco | Validação |
| Login correto (e-mail com maiúsculas) | Autentica |
| Senha errada / e-mail inexistente | A mesma mensagem nos dois casos (não revela se o e-mail existe) |
| Login sem dados | Validação |
| Atualizar perfil | Dados alterados |
| Atualizar para e-mail de outra pessoa | Conflito |
| Atualizar usuário inexistente | Não encontrado |

### Pedido (`PedidoServiceTests`): 25 casos
| Caso | Esperado |
|---|---|
| 2× R$ 8,00 + 1× R$ 9,50 + entrega R$ 5,00 | Subtotal R$ 25,50 e total R$ 30,50 |
| Reajuste de preço depois da compra | O pedido mantém o preço antigo |
| Mesmo cupcake enviado duas vezes | Vira uma linha com a soma |
| Carrinho vazio ou nulo | "O carrinho está vazio." |
| Quantidade 0, −3 ou 51 | Validação |
| Cupcake inexistente | Validação |
| Cupcake desativado | "não está mais disponível" |
| Sem endereço no pedido | Usa o endereço do cadastro |
| Sem endereço em lugar nenhum | "Informe o endereço de entrega." |
| Dois clientes | Cada um vê só os próprios pedidos |
| Fluxo de status (8 combinações) | Só avança na ordem; finalizado não muda |
| Status inválido pelo admin | Recusado e o pedido não muda |
| Cliente cancela "Recebido" | Cancelado |
| Cliente cancela "Em preparo" | Recusado |
| Cliente cancela pedido de outro | Não encontrado |
| Painel | O faturamento ignora pedidos cancelados |

### Cardápio (`CupcakeServiceTests`): 7 casos
Criar válido · preço 0 ou negativo recusado · nome duplicado recusado (ignora maiúsculas) · editar mantendo o nome · desativar some do cardápio público · cardápio inicial com os 8 sabores do app.

### Utilitários (`UtilitariosTests`): 6 casos
Formatação em reais (R$ 1.234,50) · conversão da URL do Neon em connection string · caminho da imagem.

### API completa (`ApiIntegracaoTests`): 14 casos
| Requisição | Esperado |
|---|---|
| `GET /health` | 200 |
| `GET /api/cupcakes` | 200 com os 8 sabores iniciais |
| `GET /api/cupcakes/99999` | 404 |
| Cadastro → login → pedido → meus pedidos | 201 → 200 → 201 (total correto) → pedido na lista |
| Cadastro com e-mail repetido | 409 com a mensagem em `detail` |
| Cadastro com dados inválidos | 400 com a mensagem em português |
| Login com senha errada | 401 |
| Pedido sem token | 401 |
| Token adulterado | 401 |
| Pedido com carrinho vazio | 400 |
| Cliente B abre pedido do cliente A | 404 |
| `PUT /api/usuarios/me` | 200 com os dados novos |
| `/Painel` sem login | Redireciona para `/Conta/Login` |
| `/`, `/Conta/Login`, `/loja/` | 200 |

## 2. Banco de dados

O script `04-teste-restricoes.sql` tenta gravar 7 coisas proibidas e o PostgreSQL recusa todas: e-mail duplicado, preço zero, status inexistente, quantidade 51, cupcake repetido no pedido, apagar cupcake já vendido e pedido de usuário inexistente. A saída real está em [`04-teste-restricoes.resultado.txt`](../situacao-1/banco/04-teste-restricoes.resultado.txt).

## 3. Testes de sistema (validação)

Roteiro executado na loja web com navegador automatizado (Playwright). Os prints estão em [`evidencias/`](evidencias):

| # | Passo | Esperado | Resultado |
|---|---|---|---|
| S1 | Abrir `/loja/` | 8 cupcakes com foto e preço | ✅ [print](evidencias/loja-01-cardapio.png) |
| S2 | Buscar "limão" | 1 resultado | ✅ |
| S3 | Adicionar 2× Red Velvet e 1× Chocolate | Contador = 3; total = R$ 33,00 (20 + 8 + 5) | ✅ [print](evidencias/loja-02-carrinho.png) |
| S4 | Finalizar sem login | Abre "Entrar / Criar conta" | ✅ |
| S5 | Cadastro com senhas diferentes | "As senhas não são iguais." | ✅ [print](evidencias/loja-03-cadastro-erro.png) |
| S6 | Cadastro correto | Entra sozinho e volta ao carrinho | ✅ |
| S7 | Finalizar | "Pedido #N recebido!", carrinho zerado | ✅ [print](evidencias/loja-04-pedido-confirmado.png) |
| S8 | Admin muda para "Em preparo" → cliente abre Meus pedidos | Status e barra de progresso atualizados | ✅ [print](evidencias/loja-05-meus-pedidos.png) |
| S9 | Abrir em celular (390 px) | Sem rolagem lateral | ✅ [print](evidencias/loja-06-celular.png) |

**Bug encontrado nesta rodada:** o contador do carrinho mostrava "0" em vez de sumir, porque o CSS `display` sobrescrevia o atributo `hidden`. Corrigido com a regra `[hidden] { display: none !important; }` e registrado no [registro de correções](registro-de-correcoes.md).

Repita o roteiro no **app Android** (emulador ou celular) e no **painel** e marque abaixo:

| # | Passo (app) | Esperado | Resultado |
|---|---|---|---|
| A1 | Login com senha errada | Alerta "E-mail ou senha inválidos." | ☐ |
| A2 | Cadastro → login | Cardápio com "Olá, <nome>" | ☐ |
| A3 | Detalhes → + + → Adicionar → Ver carrinho | 3 unidades no carrinho, total certo | ☐ |
| A4 | Finalizar pedido | Alerta "Pedido #N recebido!" e tela Meus pedidos | ☐ |
| A5 | Cancelar pedido "Recebido" | Status "Cancelado" | ☐ |
| A6 | Perfil → alterar telefone → Salvar | "Perfil atualizado com sucesso." | ☐ |
| A7 | Modo avião → puxar o cardápio | "Não foi possível conectar ao servidor." | ☐ |
| P1 | Painel → Pedidos → avançar até "Entregue" | Botões somem no final | ☐ |
| P2 | Painel → Cardápio → desativar sabor | Some da loja e do app | ☐ |
| P3 | Painel → Novo cupcake com preço 0 | Mensagem de erro no formulário | ☐ |
