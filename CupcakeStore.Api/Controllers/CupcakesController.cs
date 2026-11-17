using CupcakeStore.Api.Models;
using CupcakeStore.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CupcakeStore.Api.Controllers;

[Authorize(Policy = "Admin")]
public class CupcakesController(CupcakeService cupcakes) : Controller
{
    public async Task<IActionResult> Index() => View(await cupcakes.ListarAsync(somenteAtivos: false));

    [HttpGet]
    public IActionResult Novo() => View("Form", new CupcakeFormViewModel());

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var c = await cupcakes.ObterAsync(id);
        if (c is null) return NotFound();
        return View("Form", new CupcakeFormViewModel
        {
            Id = c.Id,
            Nome = c.Nome,
            Descricao = c.Descricao,
            Preco = c.Preco,
            Imagem = c.Imagem,
            Ativo = c.Ativo
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Salvar(CupcakeFormViewModel form)
    {
        if (!ModelState.IsValid) return View("Form", form);

        var resultado = await cupcakes.SalvarAsync(form);
        if (!resultado.Sucesso)
        {
            ModelState.AddModelError(string.Empty, resultado.Mensagem);
            return View("Form", form);
        }

        TempData["Sucesso"] = form.Edicao
            ? $"\"{resultado.Valor!.Nome}\" atualizado."
            : $"\"{resultado.Valor!.Nome}\" adicionado ao cardápio.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlternarAtivo(int id)
    {
        var resultado = await cupcakes.AlternarAtivoAsync(id);
        if (!resultado.Sucesso)
            TempData["Erro"] = resultado.Mensagem;
        else
            TempData["Sucesso"] = resultado.Valor!.Ativo
                ? $"\"{resultado.Valor.Nome}\" voltou ao cardápio."
                : $"\"{resultado.Valor.Nome}\" saiu do cardápio.";
        return RedirectToAction(nameof(Index));
    }
}
