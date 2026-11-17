using CardapioCupCake.Core.View;
using CardapioCupCake.Core.ViewModel;

namespace CardapioCupCake.Core.UI.Componentes;

public partial class MenuComp : Grid
{
    public MenuComp()
    {
        InitializeComponent();
    }

    private static INavigation Navegacao => Shell.Current.CurrentPage.Navigation;

    private async void OnHomeTapped(object sender, TappedEventArgs e)
    {
        // Volta para o cardápio que já está na pilha em vez de empilhar outro igual.
        var cardapio = Navegacao.NavigationStack.LastOrDefault(p => p is CardapioPage);
        if (cardapio == null)
        {
            await Navegacao.PushAsync(new CardapioPage(new CardapioViewModel()));
            return;
        }
        while (Navegacao.NavigationStack.Count > 0 && Navegacao.NavigationStack[^1] != cardapio)
            await Navegacao.PopAsync(false);
    }

    private async void OnPedidosTapped(object sender, TappedEventArgs e)
    {
        if (Shell.Current.CurrentPage is MeusPedidosPage) return;
        await Navegacao.PushAsync(new MeusPedidosPage(new MeusPedidosViewModel()));
    }

    private async void OnshoppingTapped(object sender, TappedEventArgs e)
    {
        if (Shell.Current.CurrentPage is CarrinhoPage) return;
        await Navegacao.PushAsync(new CarrinhoPage(new CarrinhoViewModel()));
    }

    private async void OnpersonTapped(object sender, TappedEventArgs e)
    {
        if (Shell.Current.CurrentPage is ProfilePage) return;
        await Navegacao.PushAsync(new ProfilePage(new ProfileViewModel()));
    }
}
