using CardapioCupCake.Core.ViewModel;

namespace CardapioCupCake.Core.View;

public partial class MeusPedidosPage : ContentPage
{
    MeusPedidosViewModel ViewModel;

    public MeusPedidosPage(MeusPedidosViewModel vm)
    {
        InitializeComponent();
        BindingContext = ViewModel = vm;
    }
}
