# Laudos dos testadores — 17/11/2026

Ambiente testado: https://cupcake.techstartsummit.com.br (loja `/loja/` e painel `/Painel`).
Observação: o endereço `cupkake.techstartsummit.com.br` (com "k") não responde.

---

**Nome:** Mariana Costa (cliente nova, acessa pelo celular)	**Data do teste:** 17/11/2026

**O que testou e funcionou:**
- Página inicial com atalhos para a loja e para o painel.
- Cardápio carrega os 8 sabores com foto, descrição e preço.
- Busca por sabor e por ingrediente ("chocolate", "CREAM CHEESE"), sem diferenciar maiúsculas.
- Busca sem resultado mostra "Nenhum sabor encontrado para “pizza”."
- Layout no celular (375 px) sem rolagem lateral.
- Carrinho continua salvo depois de recarregar a página.

**O que testou e não funcionou – O que deve ser corrigido:**
- O endereço divulgado `cupkake.techstartsummit.com.br` não abre. Corrigir o link divulgado ou criar o DNS/redirect para `cupcake...`.
- A descrição do "Cupcake de Doce de Leite" fala em "massa úmida de coco". Revisar o texto.
- Os botões 👤 e 🛒 do topo não têm `aria-label`, então o leitor de tela não diz o que fazem.

**Funcionalidade não testada (faltou ou não foi implementada):**
- App Android (APK MAUI): não foi instalado neste teste.
- Página de detalhes de um cupcake na loja web (só existe o card na lista).

---

**Nome:** Ricardo Alves (cliente frequente, faz pedidos)	**Data do teste:** 17/11/2026

**O que testou e funcionou:**
- Seletor de quantidade fica entre 1 e 50.
- Somar e remover itens no carrinho. Subtotal, entrega de R$ 5,00 e total estão corretos.
- Sem login, o botão vira "Entrar para finalizar" e abre a tela de login.
- Login vazio mostra "Preencha o e-mail e a senha."
- Finalizar sem endereço mostra "Informe o endereço de entrega."
- Pedido #1 criado (3× Chocolate, R$ 29,00). A tela de sucesso tem "Acompanhar pedido" e o carrinho é zerado.
- Mesmo sabor enviado duas vezes vira uma linha só, com a soma.

**O que testou e não funcionou – O que deve ser corrigido:**
- Com 50 unidades no carrinho, adicionar mais mostra "1× … no carrinho", mas a quantidade continua 50. Avisar que o limite foi atingido.
- O endereço de entrega vem preenchido com o do cadastro, sem opção de escolher outro endereço salvo. É só uma sugestão de melhoria.

**Funcionalidade não testada (faltou ou não foi implementada):**
- Pagamento online: não implementado, o pedido é fechado sem pagamento.
- Cadastro de conta nova pela loja: o teste usou uma conta já existente.
- Notificação ao cliente (e-mail/push) quando o status muda: não implementada.

---

**Nome:** Juliana Ferreira (cliente que muda de ideia)	**Data do teste:** 17/11/2026

**O que testou e funcionou:**
- "Meus pedidos" lista status, data, itens e total.
- Cancelar um pedido "Recebido" (Pedido #2) mostra "Pedido cancelado." e o status muda para "Cancelado".
- O botão Cancelar só aparece em pedidos que ainda podem ser cancelados.
- Em "Meus dados", e-mail inválido mostra "O formato do e-mail é inválido.".
- Telefone inválido mostra "Telefone inválido. Use DDD + número (10 ou 11 dígitos)."
- Alteração de endereço salva com "Dados atualizados ✔" e aparece em `/api/usuarios/me`.

**O que testou e não funcionou – O que deve ser corrigido:**
- Cancelar de novo um pedido já cancelado mostra "Só é possível cancelar pedidos que ainda não entraram em preparo". A mensagem deveria dizer que o pedido já está cancelado.

**Funcionalidade não testada (faltou ou não foi implementada):**
- Trocar a senha pelo perfil: não implementado.
- Recuperar senha ("esqueci minha senha"): não implementado.
- Botão "Sair da conta" da loja: não foi testado.

---

**Nome:** Carlos Mendes (dono da loja / administrador)	**Data do teste:** 17/11/2026

**O que testou e funcionou:**
- `/Painel` sem login redireciona para a tela de login.
- Visão geral: faturamento (R$ 29,00) e "Mais vendidos" ignoram o pedido cancelado, e a contagem por status está certa.
- Lista de pedidos com filtro por status.
- Detalhe do pedido mostra itens, endereço, telefone e e-mail do cliente.
- Status do Pedido #1: Recebido → Em preparo → Saiu para entrega → Entregue. No fim aparece "Pedido finalizado. Não há mais mudanças possíveis."
- Voltar de "Em preparo" para "Recebido" é bloqueado com mensagem.
- Cardápio: criou o "Cupcake Teste UAT" e editou o preço para R$ 13,90 (refletiu na API).
- Ao desativar, ele sumiu da loja e a API retorna 404.
- Validações do cardápio: preço 0, descrição curta, imagem vazia e nome repetido ("Já existe um cupcake com esse nome.").

**O que testou e não funcionou – O que deve ser corrigido:**
- Preço vazio no cadastro de cupcake mostra a mensagem em inglês "The value '' is invalid.". Traduzir.
- O card "Clientes" mostra 0 mesmo com pedidos feitos (pela conta admin). Deixar claro o que é contado.
- Status inválido enviado ao servidor (ex.: 99) é lido como "Recebido" e gera uma mensagem enganosa. Validar o enum antes.

**Funcionalidade não testada (faltou ou não foi implementada):**
- Excluir um cupcake de vez: só existe desativar (o "Cupcake Teste UAT" ficou desativado).
- Upload de foto do cupcake: só aceita nome de recurso ou URL.
- Relatórios por período ou exportação: não implementados.

---

**Nome:** Fernanda Lima (analista de QA / segurança)	**Data do teste:** 17/11/2026

**O que testou e funcionou:**
- `GET /health` retorna 200 com `{"status":"ok","banco":"postgresql"}`.
- `/api/cupcakes` retorna 200. Cupcake inexistente retorna 404 "Cupcake não encontrado.".
- Pedido sem token e `/api/pedidos/meus` sem token retornam 401.
- Login com senha errada retorna 401 com a mesma mensagem para e-mail inexistente, sem revelar se o e-mail existe.
- Cadastro inválido retorna 400 com mensagem em português.
- Pedidos com quantidade 0 ou 51, cupcake inexistente e carrinho vazio retornam 400 com mensagem clara.
- Pedido de outra pessoa retorna 404.
- HTML digitado na busca é escapado (sem XSS).
- Página 404 personalizada.

**O que testou e não funcionou – O que deve ser corrigido:**
- **Grave:** `/Painel` redireciona para `http://cupcake.../Conta/Login`, sem HTTPS. A senha e o cookie do admin trafegam sem criptografia. Configurar `UseForwardedHeaders` (X-Forwarded-Proto do Cloudflare/proxy) e `UseHttpsRedirection`/HSTS.
- Não há cabeçalhos de segurança (HSTS, X-Content-Type-Options, X-Frame-Options/CSP).

**Funcionalidade não testada (faltou ou não foi implementada):**
- Limite de tentativas de login (rate limiting / bloqueio por força bruta).
- Expiração do token JWT durante o uso da loja.
- Teste de carga.

---

### Dados criados em produção durante o teste
- Pedido #1 (Entregue) e Pedido #2 (Cancelado), conta admin, endereço marcado "(teste UAT)".
- Cupcake #9 "Cupcake Teste UAT", desativado.
