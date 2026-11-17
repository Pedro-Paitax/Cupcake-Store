using CupcakeStore.Api.Models;
using CupcakeStore.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CupcakeStore.Api.Controllers;

[Authorize(Policy = "Admin")]
public class PedidosController(PedidoService pedidos) : Controller
{
    public async Task<IActionResult> Index(StatusPedido? status = null) =>
        View(new PedidosListaViewModel { Filtro = status, Pedidos = await pedidos.ListarAsync(status) });

    public async Task<IActionResult> Detalhes(int id)
    {
        var pedido = await pedidos.ObterAsync(id);
        return pedido is null ? NotFound() : View(pedido);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AtualizarStatus(int id, StatusPedido status)
    {
        // Um valor fora do enum (ex.: 99) não é lido como "Recebido": é recusado.
        if (!ModelState.IsValid || !Enum.IsDefined(status))
        {
            TempData["Erro"] = "Status inválido.";
            return RedirectToAction(nameof(Detalhes), new { id });
        }

        var resultado = await pedidos.AtualizarStatusAsync(id, status);
        if (resultado.Sucesso)
            TempData["Sucesso"] = $"Pedido #{id} agora está \"{status.Rotulo()}\".";
        else
            TempData["Erro"] = resultado.Mensagem;
        return RedirectToAction(nameof(Detalhes), new { id });
    }
}
