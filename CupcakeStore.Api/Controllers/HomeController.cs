using Microsoft.AspNetCore.Mvc;

namespace CupcakeStore.Api.Controllers;

public class HomeController : Controller
{
    /// <summary>Página inicial: atalhos para a loja (clientes) e para o painel (administração).</summary>
    public IActionResult Index() => View();

    public IActionResult Erro(int? codigo = null)
    {
        ViewBag.Codigo = codigo;
        return View();
    }
}
