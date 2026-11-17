using CardapioCupCake.Core.ViewModel;

namespace CardapioCupCake.Core.View;

public partial class CarrinhoPage : ContentPage
{
    CarrinhoViewModel ViewModel;

    public CarrinhoPage(CarrinhoViewModel vm)
    {
        InitializeComponent();
        BindingContext = ViewModel = vm;
    }
}
