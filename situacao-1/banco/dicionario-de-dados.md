# Dicionário de dados

> Gerado automaticamente a partir do catálogo do PostgreSQL 16 depois de rodar `01-schema.sql` (consultas em `information_schema` e `pg_constraint`). Ou seja: é exatamente o que está no banco.

**SGBD:** PostgreSQL · **Codificação:** UTF-8 · **Datas:** `timestamptz` gravadas em UTC (o sistema exibe em horário de Brasília) · **Dinheiro:** `numeric(10,2)` (nunca `float`, para não perder centavos).

## Visão geral

| Tabela | Descrição |
|---|---|
| `usuario` | Pessoas com acesso ao sistema: clientes (app/loja) e administradores (painel). |
| `cupcake` | Cardápio. Cupcakes não são excluídos (estão em pedidos antigos): são desativados. |
| `pedido` | Pedido feito por um cliente. Subtotal e total são calculados a partir dos itens (3FN). |
| `item_pedido` | Cupcakes de cada pedido. Um cupcake aparece no máximo uma vez por pedido. |
| `vw_pedido_resumo` (visão) | Pedido com subtotal, total e quantidade de itens calculados. |

## `usuario`

Pessoas com acesso ao sistema: clientes (app/loja) e administradores (painel).

| # | Coluna | Tipo | Nulo? | Padrão | Chave | Descrição |
|---|---|---|---|---|---|---|
| 1 | `id` | integer | não | `identity` | PK | Identificador do usuário (gerado automaticamente). |
| 2 | `nome` | varchar(100) | não |  |  | Nome completo. |
| 3 | `email` | varchar(150) | não |  | UNIQUE | E-mail de login, sempre em minúsculas. Único. |
| 4 | `senha_hash` | varchar(255) | não |  |  | Hash PBKDF2 (ASP.NET Identity) da senha. A senha nunca é gravada em texto puro. |
| 5 | `telefone` | varchar(11) | não |  |  | Telefone com DDD, somente dígitos (10 ou 11). |
| 6 | `endereco` | varchar(200) | sim |  |  | Endereço padrão de entrega (opcional no cadastro). |
| 7 | `perfil` | varchar(10) | não | `'cliente'` |  | Papel no sistema: cliente ou admin. |
| 8 | `criado_em` | timestamptz | não | `now()` |  | Data/hora do cadastro (UTC). |

**Restrições e índices**

- `ck_usuario_perfil`: `CHECK (((perfil)::text = ANY ((ARRAY['cliente'::character varying, 'admin'::character varying])::text[])))`
- `uq_usuario_email`: índice único em (email)

## `cupcake`

Cardápio. Cupcakes não são excluídos (estão em pedidos antigos): são desativados.

| # | Coluna | Tipo | Nulo? | Padrão | Chave | Descrição |
|---|---|---|---|---|---|---|
| 1 | `id` | integer | não | `identity` | PK | Identificador do cupcake. |
| 2 | `nome` | varchar(80) | não |  | UNIQUE | Nome exibido no cardápio. Único. |
| 3 | `descricao` | varchar(500) | não |  |  | Descrição de massa, recheio e cobertura. |
| 4 | `preco` | numeric(10,2) | não |  |  | Preço atual em reais. Maior que zero. |
| 5 | `imagem` | varchar(300) | não |  |  | Nome do recurso de imagem do app (ex.: chocolate) ou URL https. |
| 6 | `ativo` | boolean | não | `true` |  | Se false, não aparece no cardápio nem pode ser comprado. |
| 7 | `criado_em` | timestamptz | não | `now()` |  | Data/hora de inclusão no cardápio (UTC). |

**Restrições e índices**

- `ck_cupcake_preco`: `CHECK ((preco > (0)::numeric))`
- `uq_cupcake_nome`: índice único em (nome)

## `pedido`

Pedido feito por um cliente. Subtotal e total são calculados a partir dos itens (3FN).

| # | Coluna | Tipo | Nulo? | Padrão | Chave | Descrição |
|---|---|---|---|---|---|---|
| 1 | `id` | integer | não | `identity` | PK | Número do pedido. |
| 2 | `usuario_id` | integer | não |  | FK → usuario.id | Cliente que fez o pedido (FK usuario.id). |
| 3 | `status` | varchar(20) | não | `'Recebido'` |  | Recebido → EmPreparo → SaiuParaEntrega → Entregue; ou Cancelado. |
| 4 | `taxa_entrega` | numeric(10,2) | não |  |  | Taxa de entrega cobrada neste pedido (histórico). |
| 5 | `endereco_entrega` | varchar(200) | não |  |  | Endereço de entrega deste pedido (pode diferir do cadastro). |
| 6 | `criado_em` | timestamptz | não | `now()` |  | Data/hora do pedido (UTC). |
| 7 | `atualizado_em` | timestamptz | não | `now()` |  | Data/hora da última mudança de status (UTC). |

**Restrições e índices**

- `ck_pedido_status`: `CHECK (((status)::text = ANY ((ARRAY['Recebido'::character varying, 'EmPreparo'::character varying, 'SaiuParaEntrega'::character varying, 'Entregue'::character varying, 'Cancelado'::character varying])::text[])))`
- `ck_pedido_taxa`: `CHECK ((taxa_entrega >= (0)::numeric))`
- `fk_pedido_usuario`: `FOREIGN KEY (usuario_id) REFERENCES usuario(id) ON DELETE RESTRICT`
- `ix_pedido_status`: índice em (status)
- `ix_pedido_usuario`: índice em (usuario_id)

## `item_pedido`

Cupcakes de cada pedido. Um cupcake aparece no máximo uma vez por pedido.

| # | Coluna | Tipo | Nulo? | Padrão | Chave | Descrição |
|---|---|---|---|---|---|---|
| 1 | `id` | integer | não | `identity` | PK | Identificador do item. |
| 2 | `pedido_id` | integer | não |  | FK → pedido.id | Pedido ao qual o item pertence (FK pedido.id). |
| 3 | `cupcake_id` | integer | não |  | FK → cupcake.id | Cupcake comprado (FK cupcake.id). |
| 4 | `quantidade` | integer | não |  |  | Quantidade (1 a 50). |
| 5 | `preco_unitario` | numeric(10,2) | não |  |  | Preço do cupcake no momento da compra (não muda se o cardápio for reajustado). |

**Restrições e índices**

- `ck_item_preco`: `CHECK ((preco_unitario > (0)::numeric))`
- `ck_item_quantidade`: `CHECK (((quantidade >= 1) AND (quantidade <= 50)))`
- `fk_item_pedido_cupcake`: `FOREIGN KEY (cupcake_id) REFERENCES cupcake(id) ON DELETE RESTRICT`
- `fk_item_pedido_pedido`: `FOREIGN KEY (pedido_id) REFERENCES pedido(id) ON DELETE CASCADE`
- `ix_item_pedido_cupcake`: índice em (cupcake_id)
- `uq_item_pedido_cupcake`: índice único em (pedido_id, cupcake_id)

## Regras de negócio garantidas pelo banco

| Regra | Como o banco garante |
|---|---|
| Um e-mail por conta | `uq_usuario_email` |
| Perfil só pode ser cliente ou admin | `ck_usuario_perfil` |
| Preço sempre positivo | `ck_cupcake_preco`, `ck_item_preco` |
| Status só assume os 5 valores do fluxo | `ck_pedido_status` |
| Quantidade entre 1 e 50 por cupcake | `ck_item_quantidade` |
| Cupcake não se repete no mesmo pedido | `uq_item_pedido_cupcake` |
| Cupcake vendido não pode ser apagado (só desativado) | `fk_item_pedido_cupcake` com `ON DELETE RESTRICT` |
| Apagar um pedido apaga seus itens | `fk_item_pedido_pedido` com `ON DELETE CASCADE` |
| Pedido sempre pertence a um usuário existente | `fk_pedido_usuario` |

Todas essas regras foram testadas: veja `04-teste-restricoes.sql` e a saída real em `04-teste-restricoes.resultado.txt`.

## Normalização (até a 3ª forma normal)

- **1FN:** todos os atributos são atômicos. O endereço é um texto único porque o sistema não faz consultas por bairro ou CEP; separar em colunas seria complexidade sem uso.
- **2FN:** as tabelas têm chave primária simples (`id`), então não existe dependência parcial. Em `item_pedido`, o par (`pedido_id`, `cupcake_id`) é chave candidata (índice único) e `quantidade`/`preco_unitario` dependem do par inteiro.
- **3FN:** nenhum atributo depende de outro atributo não chave. Por isso **subtotal e total não são gravados**: eles são calculados a partir dos itens (na aplicação e na visão `vw_pedido_resumo`).
- **Decisões conscientes:** `item_pedido.preco_unitario` e `pedido.taxa_entrega` parecem repetir dados, mas são **fatos históricos** (o preço e a taxa no dia da compra). Sem eles, um reajuste no cardápio mudaria o valor de pedidos antigos. O mesmo vale para `pedido.endereco_entrega`, que pode ser diferente do endereço do cadastro.
