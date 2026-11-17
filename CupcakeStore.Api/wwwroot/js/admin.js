// Cupcake Store — interações do painel administrativo
(function () {
  "use strict";

  // Confirmação antes de ações destrutivas (botões com data-confirmar preenchido).
  document.addEventListener("submit", function (evento) {
    var botao = evento.submitter || evento.target.querySelector("[data-confirmar]");
    var mensagem = botao && botao.getAttribute("data-confirmar");
    if (mensagem && !window.confirm(mensagem)) {
      evento.preventDefault();
    }
  });

  // Busca instantânea no cardápio.
  document.querySelectorAll("[data-filtro]").forEach(function (campo) {
    var seletor = campo.getAttribute("data-filtro");
    campo.addEventListener("input", function () {
      var termo = campo.value.trim().toLowerCase();
      document.querySelectorAll(seletor).forEach(function (item) {
        var texto = item.getAttribute("data-texto") || item.textContent.toLowerCase();
        item.hidden = termo !== "" && texto.indexOf(termo) === -1;
      });
    });
  });

  // Prévia da imagem no formulário de cupcake.
  document.querySelectorAll("[data-previa]").forEach(function (campo) {
    var imagem = document.querySelector(campo.getAttribute("data-previa"));
    if (!imagem) return;
    var atualizar = function () {
      var valor = campo.value.trim();
      if (!valor) { imagem.src = "/img/logo_cupcake.png"; return; }
      imagem.src = /^https?:\/\//i.test(valor) ? valor : "/img/" + encodeURIComponent(valor) + ".png";
    };
    imagem.addEventListener("error", function () { imagem.src = "/img/logo_cupcake.png"; });
    campo.addEventListener("change", atualizar);
    campo.addEventListener("blur", atualizar);
  });

  // Avisos de sucesso somem sozinhos.
  document.querySelectorAll(".aviso.sucesso").forEach(function (aviso) {
    setTimeout(function () {
      aviso.style.transition = "opacity .4s";
      aviso.style.opacity = "0";
      setTimeout(function () { aviso.remove(); }, 400);
    }, 4000);
  });
})();
