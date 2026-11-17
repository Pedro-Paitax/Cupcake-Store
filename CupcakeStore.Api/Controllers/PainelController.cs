using CupcakeStore.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CupcakeStore.Api.Controllers;

[Authorize(Policy = "Admin")]
public class PainelController(PedidoService pedidos) : Controller
{
    public async Task<IActionResult> Index() => View(await pedidos.MontarDashboardAsync());
}
