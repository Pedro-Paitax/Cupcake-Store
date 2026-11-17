# Situação 3: testes, validação com pares e entrega

| Entrega pedida | Onde está |
|---|---|
| Testes de verificação documentados | [Plano e resultado dos testes](plano-de-testes.md) e testes automatizados em [`/CupcakeStore.Tests`](../CupcakeStore.Tests) |
| Formulário para 5 testadores | [Perguntas do formulário](formulario-testadores.md), para montar no Google Forms |
| Laudos dos testadores | [`laudos/`](laudos), um arquivo por testador |
| Correções a partir das sugestões | [Registro de correções](registro-de-correcoes.md) |
| Vídeo de até 5 minutos | [Roteiro](roteiro-video.md) e link do vídeo abaixo |
| Evidências (prints) | [`evidencias/`](evidencias) |

**Vídeo da solução:** _cole aqui o link do YouTube (não listado) ou do Google Drive_

## Como a validação com os testadores funciona (tudo remoto)

1. **Publicar:** a API, a loja e o painel ficam no Render, e o APK numa Release do GitHub (veja a [Situação 2](../situacao-2/README.md)).
2. **Convidar 5 pessoas:** colegas da turma (vale propor troca: "testo o seu, você testa o meu") ou profissionais da área. Mensagem pronta:

   > Oi! Estou fazendo o Projeto Integrador e preciso que 5 pessoas testem meu sistema. Leva uns 10 minutos.
   > 🛍️ Loja web: https://SEU-SITE.onrender.com/loja/
   > 📱 App Android (opcional): https://github.com/Pedro-Paitax/Cupcake-Store/releases
   > 📝 Formulário com o roteiro: LINK_DO_FORMS
   > O primeiro acesso pode demorar ~1 min (servidor gratuito acordando). Obrigado!

3. **Receber os laudos:** as respostas caem na planilha do Forms. Exporte cada resposta para `laudos/testador-N.md` (há um [modelo](laudos/modelo-laudo.md)) e salve os prints recebidos em `laudos/prints/`.
4. **Corrigir:** para cada bug ou sugestão pertinente, faça a correção num commit que cite o testador (ex.: `fix: mensagem de telefone mais clara (testador 3)`) e registre em [registro-de-correcoes.md](registro-de-correcoes.md).
