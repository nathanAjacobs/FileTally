using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileTally.ViewModels;

public enum SearchMode
{
    Include,
    Ignore
}

internal partial class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<SearchMode> SearchModeValues { get; }

    [ObservableProperty]
    private SearchMode _selectedSearchMode;

    [ObservableProperty]
    private string fileCount = "";

    [ObservableProperty]
    private string fileTypes = "";

    [ObservableProperty]
    private string selectedFolder = "";

    [ObservableProperty]
    private bool includeSubFolders;

    public MainWindowViewModel()
    {
        SearchModeValues = new ObservableCollection<SearchMode>(Enum.GetValues<SearchMode>());
        SelectedSearchMode = SearchMode.Include;
        includeSubFolders = true;
    }

    private void ClearOutput()
    {
        SelectedFolder = "";
        FileCount = "";
    }

    public async Task OnSelectFolderProvided(string directoryPath)
    {
        ClearOutput();

        var fileTypes = GetFileTypes();

        SelectedFolder = directoryPath;

        FileCount = "Calculating...";

        long count = await Task.Run(() => GetTotalFileCount(directoryPath, fileTypes, SelectedSearchMode, IncludeSubFolders));

        FileCount = $"Total Files: {count}";
    }

    private List<string> GetFileTypes()
    {
        return GetFileTypeEnumerable(FileTypes).ToList();
    }

    private static IEnumerable<string> GetFileTypeEnumerable(string fileTypes)
    {
        return fileTypes.Split(',').Select(ft => ReplaceFirstOccurrence(ft, ".", "").Trim().ToLower().Insert(0, ".")).Distinct(StringComparer.OrdinalIgnoreCase);
    }

    private static string ReplaceFirstOccurrence(string source, string oldString, string newString)
    {
        int firstIndex = source.IndexOf(oldString);
        if (firstIndex == -1)
        {
            return source;  // Substring not found
        }

        return string.Concat(source.AsSpan(0, firstIndex), newString, source.AsSpan(firstIndex + oldString.Length));
    }

    private static long GetTotalFileCount(string directoryPath, IEnumerable<string> fileTypes, SearchMode searchMode, bool includeSubDirectories)
    {
        IEnumerable<string> enumerable;

        HashSet<string> extensionHashSet = new HashSet<string>(fileTypes, StringComparer.OrdinalIgnoreCase);

        if (extensionHashSet.Count == 0)
            return 0;

        SearchOption option = (includeSubDirectories) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        if (searchMode == SearchMode.Include)
        {
            enumerable = Directory.EnumerateFiles(directoryPath, "*", option)
                                  .Where(file => extensionHashSet.Contains(Path.GetExtension(file)));
        }
        else
        {
            // Ignore mode
            enumerable = Directory.EnumerateFiles(directoryPath, "*", option)
                                  .Where(file => !extensionHashSet.Contains(Path.GetExtension(file)));
        }

        return enumerable.LongCount();
    }

    //private static long GetFileCount(string directoryPath, string fileType)
    //{
    //    long count = 0;

    //    foreach (var file in Directory.EnumerateFiles(directoryPath, $"*.{fileType}", SearchOption.AllDirectories))
    //    {
    //        count++;
    //    }

    //    return count;
    //}
}
