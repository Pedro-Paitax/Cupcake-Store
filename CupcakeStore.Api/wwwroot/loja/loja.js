// Cupcake Store — loja web (HTML + CSS + JavaScript puro consumindo a API REST)
(function () {
  "use strict";

  var TAXA_ENTREGA = 5.0; // exibição; o valor oficial é calculado pela API
  var CHAVE_TOKEN = "cupcake.token";
  var CHAVE_USUARIO = "cupcake.usuario";
  var CHAVE_CARRINHO = "cupcake.carrinho";
  var ETAPAS = ["Recebido", "EmPreparo", "SaiuParaEntrega", "Entregue"];

  var estado = {
    cardapio: [],
    carrinho: ler(CHAVE_CARRINHO, []), // [{ id, quantidade }]
    token: ler(CHAVE_TOKEN, null),
    usuario: ler(CHAVE_USUARIO, null),
    aposLogin: null
  };

  // ---------- Utilidades ----------
  function $(seletor) { return document.querySelector(seletor); }

  function ler(chave, padrao) {
    try {
      var valor = localStorage.getItem(chave);
      return valor ? JSON.parse(valor) : padrao;
    } catch (e) { return padrao; }
  }

  function gravar(chave, valor) {
    try {
      if (valor === null) localStorage.removeItem(chave);
      else localStorage.setItem(chave, JSON.stringify(valor));
    } catch (e) { /* armazenamento indisponível: segue só em memória */ }
  }

  function esc(texto) {
    return String(texto == null ? "" : texto)
      .replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;").replace(/'/g, "&#39;");
  }

  function brl(valor) {
    return "R$ " + Number(valor).toFixed(2).replace(".", ",").replace(/\B(?=(\d{3})+(?!\d))/g, ".");
  }

  function urlImagem(imagem) {
    return /^https?:\/\//i.test(imagem || "") ? imagem : "/img/" + encodeURIComponent(imagem || "logo_cupcake") + ".png";
  }

  function dataBr(iso) {
    var d = new Date(iso);
    return d.toLocaleDateString("pt-BR") + " às " + d.toLocaleTimeString("pt-BR", { hour: "2-digit", minute: "2-digit" });
  }

  var timerToast;
  function toast(mensagem) {
    var el = $("#toast");
    el.textContent = mensagem;
    el.hidden = false;
    clearTimeout(timerToast);
    timerToast = setTimeout(function () { el.hidden = true; }, 3200);
  }

  function mostrarErro(form, mensagem) {
    var el = form.querySelector(".erro-form");
    el.textContent = mensagem || "";
    el.hidden = !mensagem;
  }

  // Chamada à API com tratamento de erros padronizado (ProblemDetails).
  function api(caminho, opcoes) {
    opcoes = opcoes || {};
    var cabecalhos = { "Accept": "application/json" };
    if (opcoes.corpo !== undefined) cabecalhos["Content-Type"] = "application/json";
    if (estado.token) cabecalhos["Authorization"] = "Bearer " + estado.token;

    return fetch(caminho, {
      method: opcoes.metodo || "GET",
      headers: cabecalhos,
      body: opcoes.corpo !== undefined ? JSON.stringify(opcoes.corpo) : undefined
    }).then(function (resp) {
      if (resp.status === 401 && estado.token) {
        sair(true);
        throw new Error("Sua sessão expirou. Entre novamente.");
      }
      return resp.text().then(function (texto) {
        var dados = null;
        try { dados = texto ? JSON.parse(texto) : null; } catch (e) { dados = null; }
        if (!resp.ok) {
          throw new Error((dados && (dados.detail || dados.title)) || "Erro " + resp.status + ". Tente novamente.");
        }
        return dados;
      });
    }, function () {
      throw new Error("Sem conexão com o servidor. Verifique sua internet.");
    });
  }

  // ---------- Cardápio ----------
  function carregarCardapio() {
    api("/api/cupcakes").then(function (lista) {
      estado.cardapio = lista || [];
      // Remove do carrinho itens que saíram do cardápio.
      estado.carrinho = estado.carrinho.filter(function (i) { return buscarCupcake(i.id); });
      salvarCarrinho();
      desenharCardapio();
    }).catch(function (erro) {
      $("#cardapio").innerHTML = '<p class="vazio">Não foi possível carregar o cardápio. ' + esc(erro.message) +
        ' <button type="button" class="botao secundario" id="tentar">Tentar de novo</button></p>';
      $("#tentar").addEventListener("click", carregarCardapio);
    });
  }

  function buscarCupcake(id) {
    for (var i = 0; i < estado.cardapio.length; i++) if (estado.cardapio[i].id === id) return estado.cardapio[i];
    return null;
  }

  function desenharCardapio() {
    var termo = $("#busca").value.trim().toLowerCase();
    var lista = estado.cardapio.filter(function (c) {
      return !termo || (c.nome + " " + c.descricao).toLowerCase().indexOf(termo) !== -1;
    });
    if (lista.length === 0) {
      $("#cardapio").innerHTML = '<p class="vazio">' + (termo ? "Nenhum sabor encontrado para “" + esc(termo) + "”." : "O cardápio está vazio no momento.") + "</p>";
      return;
    }
    $("#cardapio").innerHTML = lista.map(function (c) {
      return '<article class="produto" data-id="' + c.id + '">' +
        '<img src="' + esc(urlImagem(c.imagem)) + '" alt="' + esc(c.nome) + '" loading="lazy" />' +
        '<div class="info"><h3>' + esc(c.nome) + "</h3><p>" + esc(c.descricao) + '</p><span class="preco">' + brl(c.preco) + "</span></div>" +
        '<div class="comprar">' +
        '<div class="stepper"><button type="button" data-acao="menos" aria-label="Diminuir">−</button><span data-qtd>1</span><button type="button" data-acao="mais" aria-label="Aumentar">+</button></div>' +
        '<button type="button" class="botao" data-acao="adicionar">Adicionar</button>' +
        "</div></article>";
    }).join("");
  }

  $("#cardapio").addEventListener("click", function (e) {
    var botao = e.target.closest("[data-acao]");
    if (!botao) return;
    var card = botao.closest(".produto");
    var qtdEl = card.querySelector("[data-qtd]");
    var qtd = parseInt(qtdEl.textContent, 10);
    var acao = botao.getAttribute("data-acao");
    if (acao === "menos") qtdEl.textContent = Math.max(1, qtd - 1);
    if (acao === "mais") {
      qtdEl.textContent = Math.min(50, qtd + 1);
      if (qtd >= 50) toast("O limite é de 50 unidades por cupcake.");
    }
    if (acao === "adicionar") {
      var id = parseInt(card.getAttribute("data-id"), 10);
      adicionarAoCarrinho(id, qtd);
      qtdEl.textContent = "1";
    }
  });

  $("#busca").addEventListener("input", desenharCardapio);

  // ---------- Carrinho ----------
  function salvarCarrinho() {
    gravar(CHAVE_CARRINHO, estado.carrinho);
    var total = estado.carrinho.reduce(function (s, i) { return s + i.quantidade; }, 0);
    var contador = $("#contador");
    contador.textContent = total;
    contador.hidden = total === 0;
  }

  function adicionarAoCarrinho(id, quantidade) {
    var item = estado.carrinho.filter(function (i) { return i.id === id; })[0];
    var c = buscarCupcake(id);
    var nome = c ? c.nome : "cupcake";
    var atual = item ? item.quantidade : 0;
    var nova = Math.min(50, atual + quantidade);
    var adicionadas = nova - atual;
    if (item) item.quantidade = nova;
    else estado.carrinho.push({ id: id, quantidade: nova });
    salvarCarrinho();
    if (adicionadas === 0) toast("Você já tem 50 unidades de " + nome + " no carrinho (limite por pedido).");
    else if (adicionadas < quantidade) toast("Adicionamos " + adicionadas + "× " + nome + ": o limite é de 50 por pedido.");
    else toast(quantidade + "× " + nome + " no carrinho 🧁");
  }

  function subtotalCarrinho() {
    return estado.carrinho.reduce(function (s, i) {
      var c = buscarCupcake(i.id);
      return s + (c ? c.preco * i.quantidade : 0);
    }, 0);
  }

  function desenharCarrinho() {
    var alvo = $("#carrinho-conteudo");
    if (estado.carrinho.length === 0) {
      alvo.innerHTML = '<p class="vazio">Seu carrinho está vazio. Escolha um sabor no cardápio!</p>' +
        '<button type="button" class="botao largo" data-fechar>Ver cardápio</button>';
      return;
    }
    var subtotal = subtotalCarrinho();
    var endereco = estado.usuario && estado.usuario.endereco ? estado.usuario.endereco : "";
    alvo.innerHTML =
      estado.carrinho.map(function (i) {
        var c = buscarCupcake(i.id);
        if (!c) return "";
        return '<div class="item-carrinho" data-id="' + c.id + '">' +
          '<img src="' + esc(urlImagem(c.imagem)) + '" alt="" />' +
          "<div><h4>" + esc(c.nome) + "</h4><small>" + brl(c.preco) + " cada</small></div>" +
          '<div class="direita"><div class="stepper"><button type="button" data-acao="menos" aria-label="Diminuir">−</button><span>' + i.quantidade +
          '</span><button type="button" data-acao="mais" aria-label="Aumentar">+</button></div>' +
          '<button type="button" class="remover" data-acao="remover">remover</button></div></div>';
      }).join("") +
      '<div class="resumo"><div><span>Subtotal</span><span>' + brl(subtotal) + "</span></div>" +
      "<div><span>Entrega</span><span>" + brl(TAXA_ENTREGA) + "</span></div>" +
      '<div class="total"><span>Total</span><span>' + brl(subtotal + TAXA_ENTREGA) + "</span></div></div>" +
      '<form id="form-pedido" class="formulario" novalidate>' +
      '<label for="endereco-entrega">Endereço de entrega</label>' +
      '<input id="endereco-entrega" maxlength="200" placeholder="Rua, número, bairro" value="' + esc(endereco) + '" />' +
      '<p class="erro-form" role="alert" hidden></p>' +
      '<button type="submit" class="botao largo">' + (estado.token ? "Finalizar pedido" : "Entrar para finalizar") + "</button></form>";
  }

  $("#carrinho-conteudo").addEventListener("click", function (e) {
    var botao = e.target.closest("[data-acao]");
    if (!botao) return;
    var id = parseInt(botao.closest(".item-carrinho").getAttribute("data-id"), 10);
    var item = estado.carrinho.filter(function (i) { return i.id === id; })[0];
    var acao = botao.getAttribute("data-acao");
    if (acao === "mais") {
      if (item.quantidade >= 50) toast("O limite é de 50 unidades por cupcake.");
      item.quantidade = Math.min(50, item.quantidade + 1);
    }
    if (acao === "menos") item.quantidade -= 1;
    if (acao === "remover" || item.quantidade < 1) {
      estado.carrinho = estado.carrinho.filter(function (i) { return i.id !== id; });
    }
    salvarCarrinho();
    desenharCarrinho();
  });

  $("#carrinho-conteudo").addEventListener("submit", function (e) {
    e.preventDefault();
    var form = e.target;
    if (!estado.token) {
      estado.aposLogin = function () { abrir("#dlg-carrinho"); };
      $("#dlg-carrinho").close();
      abrirConta("entrar");
      return;
    }
    var endereco = form.querySelector("#endereco-entrega").value.trim();
    if (!endereco) { mostrarErro(form, "Informe o endereço de entrega."); return; }

    var botao = form.querySelector("button[type=submit]");
    botao.disabled = true;
    botao.textContent = "Enviando...";
    mostrarErro(form, null);

    api("/api/pedidos", {
      metodo: "POST",
      corpo: {
        itens: estado.carrinho.map(function (i) { return { cupcakeId: i.id, quantidade: i.quantidade }; }),
        enderecoEntrega: endereco
      }
    }).then(function (pedido) {
      estado.carrinho = [];
      salvarCarrinho();
      $("#carrinho-conteudo").innerHTML =
        '<div class="sucesso-pedido"><div class="grande">🎉</div><h3>Pedido #' + pedido.id + " recebido!</h3>" +
        "<p>Total de <b>" + brl(pedido.total) + "</b>. Entrega em " + esc(pedido.enderecoEntrega) + ".</p>" +
        '<button type="button" class="botao largo" id="ver-pedidos">Acompanhar pedido</button></div>';
      $("#ver-pedidos").addEventListener("click", function () {
        $("#dlg-carrinho").close();
        abrirPerfil();
      });
    }).catch(function (erro) {
      mostrarErro(form, erro.message);
      botao.disabled = false;
      botao.textContent = "Finalizar pedido";
    });
  });

  // ---------- Conta ----------
  function atualizarTopo() {
    $("#rotulo-conta").textContent = estado.usuario ? estado.usuario.nome.split(" ")[0] : "Entrar";
  }

  function entrar(email, senha) {
    return api("/api/login", { metodo: "POST", corpo: { email: email, senha: senha } }).then(function (resp) {
      estado.token = resp.token;
      estado.usuario = resp.usuario;
      gravar(CHAVE_TOKEN, resp.token);
      gravar(CHAVE_USUARIO, resp.usuario);
      atualizarTopo();
      $("#dlg-conta").close();
      toast("Bem-vindo(a), " + resp.usuario.nome.split(" ")[0] + "!");
      if (estado.aposLogin) { var f = estado.aposLogin; estado.aposLogin = null; f(); }
    });
  }

  function sair(expirou) {
    estado.token = null;
    estado.usuario = null;
    gravar(CHAVE_TOKEN, null);
    gravar(CHAVE_USUARIO, null);
    atualizarTopo();
    if (!expirou) toast("Você saiu da conta.");
  }

  function trocarAba(dialogo, aba) {
    dialogo.querySelectorAll(".aba").forEach(function (b) {
      var ativa = b.getAttribute("data-aba") === aba;
      b.classList.toggle("ativa", ativa);
      b.setAttribute("aria-selected", ativa ? "true" : "false");
    });
  }

  function abrirConta(aba) {
    trocarAba($("#dlg-conta"), aba);
    $("#form-entrar").hidden = aba !== "entrar";
    $("#form-cadastrar").hidden = aba !== "cadastrar";
    abrir("#dlg-conta");
  }

  $("#dlg-conta").addEventListener("click", function (e) {
    var aba = e.target.closest(".aba");
    if (aba) abrirConta(aba.getAttribute("data-aba"));
  });

  $("#form-entrar").addEventListener("submit", function (e) {
    e.preventDefault();
    var form = e.target;
    var email = form.email.value.trim();
    var senha = form.senha.value;
    if (!email || !senha) { mostrarErro(form, "Preencha o e-mail e a senha."); return; }
    mostrarErro(form, null);
    entrar(email, senha).then(function () { form.reset(); }).catch(function (erro) { mostrarErro(form, erro.message); });
  });

  $("#form-cadastrar").addEventListener("submit", function (e) {
    e.preventDefault();
    var form = e.target;
    var dados = {
      nome: form.nome.value.trim(),
      email: form.email.value.trim(),
      telefone: form.telefone.value.replace(/\D/g, ""),
      endereco: form.endereco.value.trim(),
      senha: form.senha.value
    };
    if (!dados.nome || !dados.email || !dados.telefone || !dados.senha) return mostrarErro(form, "Preencha todos os campos obrigatórios.");
    if (dados.telefone.length < 10 || dados.telefone.length > 11) return mostrarErro(form, "Telefone deve ter DDD + número (10 ou 11 dígitos).");
    if (dados.senha.length < 8) return mostrarErro(form, "A senha deve ter no mínimo 8 caracteres.");
    if (dados.senha !== form.confirmar.value) return mostrarErro(form, "As senhas não são iguais.");
    mostrarErro(form, null);

    api("/api/usuarios", { metodo: "POST", corpo: dados })
      .then(function () { return entrar(dados.email, dados.senha); })
      .then(function () { form.reset(); })
      .catch(function (erro) { mostrarErro(form, erro.message); });
  });

  // ---------- Área do cliente ----------
  function abrirPerfil(aba) {
    aba = aba || "pedidos";
    var dlg = $("#dlg-perfil");
    $("#titulo-perfil").textContent = "Olá, " + estado.usuario.nome.split(" ")[0] + "!";
    trocarAba(dlg, aba);
    $("#lista-pedidos").hidden = aba !== "pedidos";
    $("#form-dados").hidden = aba !== "dados";
    if (aba === "pedidos") carregarPedidos();
    if (aba === "dados") preencherDados();
    abrir("#dlg-perfil");
  }

  $("#dlg-perfil").addEventListener("click", function (e) {
    var aba = e.target.closest(".aba");
    if (aba) abrirPerfil(aba.getAttribute("data-aba"));
    var cancelar = e.target.closest("[data-cancelar]");
    if (cancelar && window.confirm("Cancelar este pedido?")) {
      api("/api/pedidos/" + cancelar.getAttribute("data-cancelar") + "/cancelar", { metodo: "POST" })
        .then(function () { toast("Pedido cancelado."); carregarPedidos(); })
        .catch(function (erro) { toast(erro.message); });
    }
  });

  function carregarPedidos() {
    var alvo = $("#lista-pedidos");
    alvo.innerHTML = '<p class="carregando">Carregando pedidos...</p>';
    api("/api/pedidos/meus").then(function (pedidos) {
      if (!pedidos || pedidos.length === 0) {
        alvo.innerHTML = '<p class="vazio">Você ainda não fez pedidos.</p>';
        return;
      }
      alvo.innerHTML = pedidos.map(function (p) {
        var etapa = ETAPAS.indexOf(p.status);
        var linha = p.status === "Cancelado" ? "" :
          '<div class="linha-tempo" aria-hidden="true">' + ETAPAS.map(function (_, i) {
            return '<span class="' + (i <= etapa ? "feito" : "") + '"></span>';
          }).join("") + "</div>";
        return '<article class="pedido"><div class="pedido-topo"><b>Pedido #' + p.id + "</b>" +
          '<span class="selo selo-' + esc(p.status) + '">' + esc(p.statusDescricao) + "</span></div>" +
          "<small>" + dataBr(p.criadoEm) + "</small>" + linha +
          "<ul>" + p.itens.map(function (i) { return "<li>" + i.quantidade + "× " + esc(i.nome) + "</li>"; }).join("") + "</ul>" +
          '<div class="pedido-rodape"><b>' + brl(p.total) + "</b>" +
          (p.status === "Recebido" ? '<button type="button" class="botao perigo" data-cancelar="' + p.id + '">Cancelar</button>' : "") +
          "</div></article>";
      }).join("");
    }).catch(function (erro) {
      alvo.innerHTML = '<p class="vazio">' + esc(erro.message) + "</p>";
    });
  }

  function preencherDados() {
    var form = $("#form-dados");
    mostrarErro(form, null);
    api("/api/usuarios/me").then(function (u) {
      form.nome.value = u.nome;
      form.email.value = u.email;
      form.telefone.value = u.telefone;
      form.endereco.value = u.endereco || "";
    }).catch(function (erro) { mostrarErro(form, erro.message); });
  }

  $("#form-dados").addEventListener("submit", function (e) {
    e.preventDefault();
    var form = e.target;
    api("/api/usuarios/me", {
      metodo: "PUT",
      corpo: {
        nome: form.nome.value.trim(),
        email: form.email.value.trim(),
        telefone: form.telefone.value.replace(/\D/g, ""),
        endereco: form.endereco.value.trim()
      }
    }).then(function (u) {
      estado.usuario = u;
      gravar(CHAVE_USUARIO, u);
      atualizarTopo();
      mostrarErro(form, null);
      toast("Dados atualizados ✔");
    }).catch(function (erro) { mostrarErro(form, erro.message); });
  });

  $("#btn-sair").addEventListener("click", function () {
    $("#dlg-perfil").close();
    sair(false);
  });

  // ---------- Diálogos ----------
  function abrir(seletor) {
    var dlg = $(seletor);
    if (seletor === "#dlg-carrinho") desenharCarrinho();
    if (!dlg.open) dlg.showModal();
  }

  document.addEventListener("click", function (e) {
    if (e.target.closest("[data-fechar]")) {
      var dlg = e.target.closest("dialog");
      if (dlg) dlg.close();
    }
    // Clique fora da caixa (no fundo escurecido) fecha o diálogo.
    if (e.target.tagName === "DIALOG") {
      var r = e.target.getBoundingClientRect();
      var fora = e.clientX < r.left || e.clientX > r.right || e.clientY < r.top || e.clientY > r.bottom;
      if (fora) e.target.close();
    }
  });

  $("#btn-carrinho").addEventListener("click", function () { abrir("#dlg-carrinho"); });
  $("#btn-conta").addEventListener("click", function () {
    if (estado.usuario) abrirPerfil();
    else abrirConta("entrar");
  });

  // ---------- Início ----------
  atualizarTopo();
  salvarCarrinho();
  carregarCardapio();
})();
