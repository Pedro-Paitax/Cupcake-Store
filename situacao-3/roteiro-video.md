# Roteiro do vídeo (até 5 minutos)

Grave com o OBS Studio: tela inteira e microfone, sem câmera. Deixe abertos antes de começar: o emulador Android com o app, a loja em uma aba e o painel em outra (já logado). Faça um pedido de teste antes, para o servidor estar "acordado".

| Tempo | Tela | O que mostrar | Fala sugerida |
|---|---|---|---|
| 0:00–0:25 | README no GitHub | Nome do projeto e estrutura de pastas | "Este é o Cupcake Store, meu Projeto Integrador. No PI I eu fiz o protótipo do app; no PI II construí o sistema completo: API, banco, painel e loja web." |
| 0:25–0:50 | Diagrama de arquitetura | Os três clientes ligados à API e ao PostgreSQL | "A API em ASP.NET Core segue o padrão MVC e atende o app Android, a loja web e o painel administrativo, todos usando o mesmo banco PostgreSQL." |
| 0:50–1:50 | App no emulador | Cadastro → login → detalhes → adicionar 2 → carrinho → finalizar | "Crio uma conta, escolho um cupcake, aumento a quantidade e finalizo. O pedido vai para o servidor, que recalcula o preço." |
| 1:50–2:20 | App: Meus pedidos | Pedido com status "Recebido" | "Aqui o cliente acompanha o status." |
| 2:20–3:10 | Painel | Visão geral → abrir o pedido → "Em preparo" | "No painel, a confeitaria vê o pedido que acabou de chegar e avança o status." |
| 3:10–3:25 | App | Puxar para atualizar | "E o status já aparece atualizado no app." |
| 3:25–3:50 | Painel: Cardápio | Desativar um sabor; mostrar que sumiu da loja | "Quando um sabor acaba, basta desativar." |
| 3:50–4:15 | Loja web no celular | Cardápio responsivo e carrinho | "Quem não tem Android usa a loja pelo navegador." |
| 4:15–4:40 | GitHub Actions | Execução verde com os 70 testes | "São 70 testes automatizados que rodam a cada commit." |
| 4:40–5:00 | Pasta situacao-3 | Laudos e registro de correções | "Cinco pessoas testaram, e as correções estão documentadas aqui. Obrigado!" |
