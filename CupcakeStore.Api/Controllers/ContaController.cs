using System.Security.Claims;
using CupcakeStore.Api.Models;
using CupcakeStore.Api.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace CupcakeStore.Api.Controllers;

public class ContaController(UsuarioService usuarios) : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl = null) =>
        View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var resultado = await usuarios.AutenticarAsync(model.Email, model.Senha);
        if (!resultado.Sucesso)
        {
            ModelState.AddModelError(string.Empty, resultado.Mensagem);
            return View(model);
        }

        var usuario = resultado.Valor!;
        if (usuario.Perfil != Perfis.Admin)
        {
            ModelState.AddModelError(string.Empty, "Este acesso é exclusivo da administração. Clientes fazem pedidos pela loja.");
            return View(model);
        }

        var identidade = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Perfil)
        }, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identidade));

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return LocalRedirect(model.ReturnUrl);
        return RedirectToAction("Index", "Painel");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sair()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }
}
