using ApiClient.ViewModels;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ApiClient.Views;
public sealed partial class ImportFromJsonDialog : ContentDialog
{
    public MainViewModel ViewModel
    {
        get;
    }
    public ImportFromJsonDialog(MainViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = ViewModel;
    }
}
