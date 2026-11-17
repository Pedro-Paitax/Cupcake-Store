# Registro de correções e melhorias

## A. Encontradas na revisão do código do PI I (Situação 1)

| # | Problema | Correção |
|---|---|---|
| 1 | `BaseViewModel` redeclarava `PropertyChanged`, então propriedades geradas pelo `[ObservableProperty]` não atualizavam a tela | Herda direto de `ObservableObject` |
| 2 | Botão "Adicionar ao carrinho" não tinha comando | `Command="{Binding AddToCartCommand}"` |
| 3 | O carrinho nunca recebia itens (cada tela tinha a própria lista) | `CarrinhoService` único |
| 4 | O total do carrinho era sempre R$ 0,00 e a tela usava uma propriedade inexistente | Totais calculados a partir dos itens |
| 5 | Serviços atribuídos a si mesmos (`x = x`), então ficavam nulos e o app travava | Instâncias criadas |
| 6 | Perfil validava a senha como CPF, então salvar sempre falhava | Validação corrigida |
| 7 | Login fixo `admin@teste.com` / `123456` | Login real pela API |
| 8 | Cadastro sempre "com sucesso" sem gravar | Gravação na API e e-mail único |
| 9 | Senha iria em texto puro para o banco | Hash PBKDF2 no servidor |
| 10 | Acentos quebrados em `CarrinhoPage.xaml.cs` | Arquivo em UTF-8 |

## B. Encontradas nos testes do sistema (Situação 3)

| # | Origem | Problema | Correção |
|---|---|---|---|
| 11 | Teste S3 (Playwright) | O contador do carrinho mostrava "0" em vez de sumir | Regra `[hidden] { display: none !important; }` |

## C. Sugestões dos testadores

| # | Testador | Sugestão / bug | Pertinente? | Correção (commit) |
|---|---|---|---|---|
| | 1 | | | |
| | 2 | | | |
| | 3 | | | |
| | 4 | | | |
| | 5 | | | |
