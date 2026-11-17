using CardapioCupCake.Core.Models;
using CardapioCupCake.Core.Service;
using CardapioCupCake.Core.View;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;

namespace CardapioCupCake.Core.ViewModel
{
    public partial class CarrinhoViewModel : BaseViewModel
    {
        private static readonly CultureInfo Br = new("pt-BR");
        private readonly HubService _hubService = new();

        // Mesmo valor configurado na API (Loja:TaxaEntrega). O total oficial vem da API.
        public const double TaxaEntrega = 5.00;

        public ObservableCollection<ItensModel> Items => CarrinhoService.Itens;

        [ObservableProperty]
        private CadastroModel? customerInfo;

        [ObservableProperty]
        private string enderecoEntrega = string.Empty;

        public bool CarrinhoVazio => Items.Count == 0;
        public bool TemItens => !CarrinhoVazio;

        public double Subtotal => CarrinhoService.Subtotal;
        public double TotalPrice => CarrinhoVazio ? 0 : Subtotal + TaxaEntrega;

        public string DisplaySubtotal => Subtotal.ToString("C2", Br);
        public string DisplayDeliveryFee => TaxaEntrega.ToString("C2", Br);
        public string DisplayTotalPrice => TotalPrice.ToString("C2", Br);

        public CarrinhoViewModel()
        {
            CustomerInfo = Sessao.Usuario;
            EnderecoEntrega = Sessao.Usuario?.Endereco ?? string.Empty;
            CarrinhoService.Alterado += (_, _) => UpdateTotals();
        }

        private void UpdateTotals()
        {
            OnPropertyChanged(nameof(Subtotal));
            OnPropertyChanged(nameof(TotalPrice));
            OnPropertyChanged(nameof(DisplaySubtotal));
            OnPropertyChanged(nameof(DisplayTotalPrice));
            OnPropertyChanged(nameof(CarrinhoVazio));
            OnPropertyChanged(nameof(TemItens));
        }

        [RelayCommand]
        private void IncreaseItem(ItensModel item) => CarrinhoService.AlterarQuantidade(item, +1);

        [RelayCommand]
        private void DecreaseItem(ItensModel item) => CarrinhoService.AlterarQuantidade(item, -1);

        [RelayCommand]
        private async Task AlterarEndereco()
        {
            var novo = await Shell.Current.DisplayPromptAsync("Endereço de entrega",
                "Para onde vamos levar os cupcakes?", "Salvar", "Cancelar",
                placeholder: "Rua, número, bairro", maxLength: 200, initialValue: EnderecoEntrega);
            if (!string.IsNullOrWhiteSpace(novo)) EnderecoEntrega = novo.Trim();
        }

        [RelayCommand]
        private async Task FinalizeOrder()
        {
            if (IsBusy) return;

            if (CarrinhoVazio)
            {
                await Shell.Current.DisplayAlert("Carrinho vazio", "Escolha pelo menos um cupcake no cardápio.", "OK");
                return;
            }
            if (string.IsNullOrWhiteSpace(EnderecoEntrega))
            {
                await AlterarEndereco();
                if (string.IsNullOrWhiteSpace(EnderecoEntrega)) return;
            }

            IsBusy = true;
            try
            {
                var (pedido, erro) = await _hubService.CriarPedidoAsync(Items, EnderecoEntrega);
                if (pedido == null)
                {
                    await Shell.Current.DisplayAlert("Não foi possível finalizar", erro ?? "Tente novamente.", "OK");
                    return;
                }

                CarrinhoService.Limpar();
                await Shell.Current.DisplayAlert("Pedido enviado! 🎉",
                    $"Pedido #{pedido.Id} recebido. Total de {pedido.DisplayTotal}.\nEntrega em: {pedido.EnderecoEntrega}", "Acompanhar");
                await Shell.Current.CurrentPage.Navigation.PushAsync(new MeusPedidosPage(new MeusPedidosViewModel()));
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
