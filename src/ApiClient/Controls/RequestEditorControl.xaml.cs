using ApiClient.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ApiClient.Controls;

public sealed partial class RequestEditorControl : UserControl
{
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(nameof(ViewModel), typeof(RequestTabViewModel), typeof(RequestEditorControl),
        new PropertyMetadata(null, OnViewModelChanged));

    public RequestTabViewModel? ViewModel
    {
        get => (RequestTabViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public RequestEditorControl()
    {
        InitializeComponent();
    }

    private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RequestEditorControl control)
        {
            control.DataContext = e.NewValue;
        }
    }
}