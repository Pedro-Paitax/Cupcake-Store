using CardapioCupCake.Core.Models;
using CardapioCupCake.Core.Service;
using CardapioCupCake.Core.View;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CardapioCupCake.Core.ViewModel
{
    public partial class CupcakeDetailViewModel : BaseViewModel
    {
        public CupcakeModel Cupcake { get; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TotalValue))]
        private int quantity = 1;

        public double TotalValue => Quantity * Cupcake.Price;

        public CupcakeDetailViewModel(CupcakeModel cupcake)
        {
            Cupcake = cupcake;
        }

        [RelayCommand]
        private void IncreaseQuantity()
        {
            if (Quantity < CarrinhoService.QuantidadeMaxima) Quantity++;
        }

        [RelayCommand]
        private void DecreaseQuantity()
        {
            if (Quantity > 1) Quantity--;
        }

        [RelayCommand]
        private async Task AddToCart()
        {
            CarrinhoService.Adicionar(Cupcake, Quantity);

            var irParaCarrinho = await Shell.Current.DisplayAlert("Adicionado! 🧁",
                $"{Quantity} x {Cupcake.Name} no carrinho.",
                "Ver carrinho", "Continuar comprando");

            var navegacao = Shell.Current.CurrentPage.Navigation;
            if (irParaCarrinho)
                await navegacao.PushAsync(new CarrinhoPage(new CarrinhoViewModel()));
            else
                await navegacao.PopAsync();
        }
    }
}
