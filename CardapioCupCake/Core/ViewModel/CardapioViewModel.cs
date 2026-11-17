using CardapioCupCake.Core.Models;
using CardapioCupCake.Core.Service;
using CardapioCupCake.Core.View;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace CardapioCupCake.Core.ViewModel
{
    public partial class CardapioViewModel : BaseViewModel
    {
        private readonly HubService _hubService = new();

        public ObservableCollection<CupcakeModel> Cupcakes { get; } = new();

        [ObservableProperty]
        private CupcakeModel? selectedItem;

        [ObservableProperty]
        private bool isRefreshing;

        [ObservableProperty]
        private string mensagemErro = string.Empty;

        public string NomeUsuario => Sessao.Usuario?.Nome?.Split(' ').FirstOrDefault() ?? "visitante";

        public int CartCount => CarrinhoService.QuantidadeTotal;

        public CardapioViewModel()
        {
            CarrinhoService.Alterado += (_, _) => OnPropertyChanged(nameof(CartCount));
            _ = CarregarAsync();
        }

        [RelayCommand]
        private async Task CarregarAsync()
        {
            IsBusy = true;
            MensagemErro = string.Empty;
            try
            {
                var (lista, erro) = await _hubService.GetCupcakesAsync();
                if (erro != null)
                {
                    MensagemErro = erro + " Puxe a tela para baixo para tentar de novo.";
                    return;
                }

                Cupcakes.Clear();
                foreach (var c in lista) Cupcakes.Add(c);
                if (Cupcakes.Count == 0) MensagemErro = "O cardápio está vazio no momento.";
            }
            finally
            {
                IsBusy = false;
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        public async Task EditCupCake(CupcakeModel? cupcake)
        {
            if (cupcake == null) return;
            await Shell.Current.CurrentPage.Navigation.PushAsync(new CupCakeDetalhesPage(new CupcakeDetailViewModel(cupcake)), false);
            SelectedItem = null; // permite tocar de novo no mesmo item
        }

        [RelayCommand]
        private async Task OpenCart()
        {
            await Shell.Current.CurrentPage.Navigation.PushAsync(new CarrinhoPage(new CarrinhoViewModel()));
        }
    }
}
