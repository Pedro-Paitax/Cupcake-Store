-- =====================================================================
--  Cupcake Store — Teste das regras do banco
--  Cada bloco tenta gravar algo proibido e confirma que o PostgreSQL recusa.
--  Rode com: psql -v ON_ERROR_STOP=0 -f 04-teste-restricoes.sql
-- =====================================================================
BEGIN;
INSERT INTO usuario (nome, email, senha_hash, telefone, endereco)
VALUES ('Cliente Teste', 'teste@email.com', 'hash-fake', '41999990000', 'Rua A, 1');
INSERT INTO pedido (usuario_id, taxa_entrega, endereco_entrega)
VALUES ((SELECT id FROM usuario WHERE email = 'teste@email.com'), 5.00, 'Rua A, 1');
INSERT INTO item_pedido (pedido_id, cupcake_id, quantidade, preco_unitario)
VALUES ((SELECT max(id) FROM pedido), (SELECT id FROM cupcake WHERE nome = 'Cupcake de Chocolate'), 2, 8.00);
SELECT id, cliente, status, quantidade_itens, subtotal, taxa_entrega, total FROM vw_pedido_resumo;

SAVEPOINT t1;
INSERT INTO usuario (nome, email, senha_hash, telefone) VALUES ('Dup', 'teste@email.com', 'x', '41999990000');
ROLLBACK TO t1;   -- esperado: uq_usuario_email

SAVEPOINT t2;
INSERT INTO cupcake (nome, descricao, preco, imagem) VALUES ('Grátis', 'Preço zero', 0, 'x');
ROLLBACK TO t2;   -- esperado: ck_cupcake_preco

SAVEPOINT t3;
UPDATE pedido SET status = 'Perdido';
ROLLBACK TO t3;   -- esperado: ck_pedido_status

SAVEPOINT t4;
INSERT INTO item_pedido (pedido_id, cupcake_id, quantidade, preco_unitario)
VALUES ((SELECT max(id) FROM pedido), (SELECT id FROM cupcake WHERE nome = 'Cupcake de Morango'), 51, 9.00);
ROLLBACK TO t4;   -- esperado: ck_item_quantidade

SAVEPOINT t5;
INSERT INTO item_pedido (pedido_id, cupcake_id, quantidade, preco_unitario)
VALUES ((SELECT max(id) FROM pedido), (SELECT id FROM cupcake WHERE nome = 'Cupcake de Chocolate'), 1, 8.00);
ROLLBACK TO t5;   -- esperado: uq_item_pedido_cupcake

SAVEPOINT t6;
DELETE FROM cupcake WHERE nome = 'Cupcake de Chocolate';
ROLLBACK TO t6;   -- esperado: fk_item_pedido_cupcake (cupcake com venda não pode ser apagado)

SAVEPOINT t7;
INSERT INTO pedido (usuario_id, taxa_entrega, endereco_entrega) VALUES (9999, 5, 'Rua X');
ROLLBACK TO t7;   -- esperado: fk_pedido_usuario

ROLLBACK;         -- desfaz os dados de teste
