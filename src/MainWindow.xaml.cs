using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace DirSizeViewer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public ObservableCollection<DirectoryItem> DirectoryItems { get; set; }

    public MainWindow()
    {
        InitializeComponent();
        DirectoryItems = new ObservableCollection<DirectoryItem>();
        FilesDataGrid.ItemsSource = DirectoryItems;
    }

    private async void SelectFolderButton_Click(object sender, RoutedEventArgs e)
    {
        SimpleLogger.Logger.Log("ボタンが押されました");

        var dialog = new CommonOpenFileDialog
        {
            IsFolderPicker = true
        };

        if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
        {
            await LoadDirectoryAsync(dialog.FileName);
        }
    }

    private async Task LoadDirectoryAsync(string path)
    {
        DirectoryItems.Clear();
        SelectFolderButton.IsEnabled = false;
        Title = $"DirSizeViewer - {path} を計算中...";

        try
        {
            var items = await Task.Run(() =>
            {
                var directoryInfo = new DirectoryInfo(path);
                var tempItems = new List<DirectoryItem>();

                // Files
                foreach (var file in directoryInfo.EnumerateFiles())
                {
                    tempItems.Add(new DirectoryItem
                    {
                        Name = file.Name,
                        Size = file.Length,
                        SizeString = FormatSize(file.Length),
                        Type = "ファイル"
                    });
                }

                // Directories
                foreach (var dir in directoryInfo.EnumerateDirectories())
                {
                    long size = GetDirectorySize(dir.FullName);
                    tempItems.Add(new DirectoryItem
                    {
                        Name = dir.Name,
                        Size = size,
                        SizeString = FormatSize(size),
                        Type = "フォルダ"
                    });
                }
                // Default sort by size descending
                return tempItems.OrderByDescending(item => item.Size).ToList();
            });

            foreach (var item in items)
            {
                DirectoryItems.Add(item);
            }
        }
        catch (System.Exception ex)
        {
            MessageBox.Show($"エラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            SelectFolderButton.IsEnabled = true;
            Title = $"DirSizeViewer - {path}";
        }
    }

    private long GetDirectorySize(string path)
    {
        try
        {
            // Enumerate all files in all subdirectories.
            // This can be slow for large directories.
            return new DirectoryInfo(path).EnumerateFiles("*", SearchOption.AllDirectories).Sum(fi => fi.Length);
        }
        catch (System.Exception)
        {
            // Catch exceptions like UnauthorizedAccessException and return 0.
            return 0;
        }
    }

    private static string FormatSize(long bytes)
    {
        if (bytes == 0) return "0 B";
        const int scale = 1024;
        string[] suffixes = { "B", "KB", "MB", "GB", "TB", "PB" };
        int i = 0;
        double dBytes = bytes;
        while (dBytes >= scale && i < suffixes.Length - 1)
        {
            dBytes /= scale;
            i++;
        }
        return $"{dBytes:0.##} {suffixes[i]}";
    }
}
