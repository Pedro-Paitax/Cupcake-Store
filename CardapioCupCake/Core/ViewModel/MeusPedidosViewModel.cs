using CardapioCupCake.Core.Models;
using CardapioCupCake.Core.Service;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace CardapioCupCake.Core.ViewModel
{
    public partial class MeusPedidosViewModel : BaseViewModel
    {
        private readonly HubService _hubService = new();

        public ObservableCollection<PedidoModel> Pedidos { get; } = new();

        [ObservableProperty]
        private bool isRefreshing;

        [ObservableProperty]
        private string mensagem = string.Empty;

        public MeusPedidosViewModel()
        {
            _ = CarregarAsync();
        }

        [RelayCommand]
        private async Task CarregarAsync()
        {
            IsBusy = true;
            try
            {
                var (lista, erro) = await _hubService.GetMeusPedidosAsync();
                Pedidos.Clear();
                foreach (var p in lista) Pedidos.Add(p);
                Mensagem = erro ?? (Pedidos.Count == 0 ? "Você ainda não fez nenhum pedido." : string.Empty);
            }
            finally
            {
                IsBusy = false;
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        private async Task Cancelar(PedidoModel pedido)
        {
            if (pedido == null || !pedido.PodeCancelar) return;
            var confirmar = await Shell.Current.DisplayAlert("Cancelar pedido",
                $"Deseja cancelar o {pedido.Titulo}?", "Sim, cancelar", "Não");
            if (!confirmar) return;

            var (ok, erro) = await _hubService.CancelarPedidoAsync(pedido.Id);
            if (!ok)
            {
                await Shell.Current.DisplayAlert("Aviso", erro ?? "Não foi possível cancelar.", "OK");
                return;
            }
            await CarregarAsync();
        }
    }
}
