// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
using ApiClient.ViewModels;
using Microsoft.UI.Xaml.Controls;
namespace ApiClient.Views;
public sealed partial class ImportFromUrlDialog : ContentDialog
{
    public MainViewModel ViewModel
    {
        get;
    }
    public ImportFromUrlDialog(MainViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = ViewModel;
    }
}
