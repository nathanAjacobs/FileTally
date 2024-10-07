using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using FileTally.ViewModels;

namespace FileTally.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }

    public async void OnSelectFolderClicked(object sender, RoutedEventArgs e)
    {
        var viewModel = (DataContext as MainWindowViewModel)!;

        var folder = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select a Folder",
            AllowMultiple = false
        });

        if (folder.Count <= 0)
        {
            return;
        }

        await viewModel!.OnSelectFolderProvided(folder[0].Path.LocalPath);
    }
}