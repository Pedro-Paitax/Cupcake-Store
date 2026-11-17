-- =====================================================================
--  Cupcake Store — Consultas usadas no painel e nos relatórios
-- =====================================================================

-- Faturamento (pedidos não cancelados)
SELECT SUM(total) AS faturamento
FROM vw_pedido_resumo
WHERE status <> 'Cancelado';

-- Pedidos por status
SELECT status, COUNT(*) AS quantidade
FROM pedido
GROUP BY status
ORDER BY quantidade DESC;

-- Top 5 cupcakes mais vendidos
SELECT c.nome, SUM(i.quantidade) AS unidades
FROM item_pedido i
JOIN cupcake c ON c.id = i.cupcake_id
JOIN pedido  p ON p.id = i.pedido_id
WHERE p.status <> 'Cancelado'
GROUP BY c.nome
ORDER BY unidades DESC
LIMIT 5;

-- Histórico de um cliente
SELECT id, status, criado_em, total
FROM vw_pedido_resumo
WHERE usuario_id = 2
ORDER BY criado_em DESC;
