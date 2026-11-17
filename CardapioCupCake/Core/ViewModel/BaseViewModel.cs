using CommunityToolkit.Mvvm.ComponentModel;

namespace CardapioCupCake
{
    /// <summary>
    /// Base das ViewModels. Usa direto o ObservableObject do CommunityToolkit.
    /// Correção do PI I: a versão anterior redeclarava o evento PropertyChanged, e por isso
    /// as propriedades [ObservableProperty] (ex.: IsBusy, TotalValue) não atualizavam a tela.
    /// </summary>
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool isBusy;
    }
}
