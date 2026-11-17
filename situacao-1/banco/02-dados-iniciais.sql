-- =====================================================================
--  Cupcake Store — Dados iniciais (cardápio do app do PI I)
--  O usuário administrador é criado pela API na primeira execução,
--  porque a senha precisa ser gravada como hash pelo próprio sistema.
-- =====================================================================
INSERT INTO cupcake (nome, descricao, preco, imagem) VALUES
 ('Cupcake de Chocolate', 'Um bolo fofo de chocolate amargo, coberto com ganache de chocolate cremoso e raspas de cacau.', 8.00, 'chocolate'),
 ('Cupcake de Morango', 'Massa de baunilha com pedaços de morango, recheado com geleia de morango caseira e cobertura de chantilly de morango.', 9.00, 'morango'),
 ('Cupcake de Baunilha', 'A receita original, com massa aerada de baunilha e uma suave cobertura de buttercream de baunilha.', 7.00, 'baunilha'),
 ('Cupcake Red Velvet', 'O famoso bolo aveludado vermelho, com um toque suave de cacau, e a tradicional cobertura de cream cheese frosting.', 10.00, 'velvet'),
 ('Cupcake de Limão Siciliano', 'Bolo com raspas e sumo de limão siciliano, com recheio ácido e doce, e cobertura de merengue suíço tostado.', 9.00, 'limao'),
 ('Cupcake de Doce de Leite', 'Massa úmida de coco, recheada com doce de leite argentino e finalizada com flocos de coco queimado.', 11.00, 'doce'),
 ('Cupcake de Café e Caramelo Salgado', 'Bolo de café intenso, coberto com um buttercream de caramelo salgado e um grão de café coberto com chocolate.', 10.00, 'caramelo'),
 ('Cupcake de Cenoura', 'Massa de cenoura com canela e nozes picadas, com cobertura densa e cremosa de cream cheese e um toque de especiarias.', 9.00, 'cenoura')
ON CONFLICT (nome) DO NOTHING;
