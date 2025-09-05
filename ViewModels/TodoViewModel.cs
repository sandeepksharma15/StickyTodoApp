using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Input;
using StickyTodoApp.Commands;
using StickyTodoApp.Models;

namespace StickyTodoApp.ViewModels;

public class TodoViewModel : INotifyPropertyChanged
{
    public ObservableCollection<TodoItem> Items { get; set; } = [];
    public IEnumerable<Priority> PriorityValues { get; } = Enum.GetValues<Priority>();

    private string? _newTitle;
    private Priority _newPriority = Priority.Normal;
    private DateTime? _newDueDate;
    private bool _showArchive;

    public string? NewTitle
    {
        get => _newTitle;
        set
        {
            if (_newTitle == value) return;
            _newTitle = value;
            OnPropertyChanged(nameof(NewTitle));
            // Update Add button enabled state
            (AddCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }

    public Priority NewPriority
    {
        get => _newPriority;
        set { _newPriority = value; OnPropertyChanged(nameof(NewPriority)); }
    }

    public DateTime? NewDueDate
    {
        get => _newDueDate;
        set { _newDueDate = value; OnPropertyChanged(nameof(NewDueDate)); }
    }

    public bool ShowArchive
    {
        get => _showArchive;
        set { _showArchive = value; OnPropertyChanged(nameof(ShowArchive)); }
    }

    public ICommand AddCommand { get; }

    public TodoViewModel()
    {
        AddCommand = new RelayCommand(_ => AddItem(), _ => !string.IsNullOrWhiteSpace(NewTitle));

        LoadItems();
        LoadArchive();

        Items.CollectionChanged += (_, __) =>
        {
            SaveItems();
            OnPropertyChanged(nameof(OpenItems));
            OnPropertyChanged(nameof(DoneItems));
        };

        // Run archive check at startup
        ArchiveOldDoneItems();

        // Optional: also run periodically (e.g., every 6 hours)
        var timer = new System.Timers.Timer(TimeSpan.FromHours(6).TotalMilliseconds);
        timer.Elapsed += (_, __) => ArchiveOldDoneItems();
        timer.AutoReset = true;
        timer.Start();
    }

    private void LoadItems()
    {
        try
        {
            var path = StorageHelper.GetDataFilePath();
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var items = JsonSerializer.Deserialize<ObservableCollection<TodoItem>>(json);
                if (items != null)
                {
                    Items = items;
                    OnPropertyChanged(nameof(Items));
                    OnPropertyChanged(nameof(OpenItems));
                    OnPropertyChanged(nameof(DoneItems));
                }

                foreach (var item in Items)
                {
                    item.PropertyChanged += (_, e) =>
                    {
                        SaveItems();

                        if (e.PropertyName == nameof(TodoItem.IsDone) || e.PropertyName == nameof(TodoItem.Priority) || e.PropertyName == nameof(TodoItem.DueDate))
                        {
                            OnPropertyChanged(nameof(OpenItems));
                            OnPropertyChanged(nameof(DoneItems));
                        }
                    };
                }
            }
        }
        catch { /* log or ignore */ }
    }

    private void SaveItems()
    {
        try
        {
            var path = StorageHelper.GetDataFilePath();
            var json = JsonSerializer.Serialize(Items, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }
        catch { /* log or ignore */ }
    }

    public IEnumerable<TodoItem> OpenItems =>
        Items.Where(i => !i.IsDone)
             .OrderByDescending(i => i.Priority)
             .ThenBy(i => i.DueDate ?? DateTime.MaxValue);

    public IEnumerable<TodoItem> DoneItems =>
        Items.Where(i => i.IsDone);

    private ObservableCollection<TodoItem> _archivedItems = new();
    public ObservableCollection<TodoItem> ArchivedItems
    {
        get => _archivedItems;
        set { _archivedItems = value; OnPropertyChanged(nameof(ArchivedItems)); }
    }

    public void LoadArchive()
    {
        try
        {
            var path = ArchiveService.GetArchiveFilePath();
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var loaded = JsonSerializer.Deserialize<ObservableCollection<TodoItem>>(json);
                if (loaded != null)
                    ArchivedItems = loaded;
            }
        }
        catch { }
    }

    // Adds item from input fields
    private void AddItem()
    {
        var item = new TodoItem
        {
            Title = NewTitle?.Trim(),
            Priority = NewPriority,
            DueDate = NewDueDate
        };

        item.PropertyChanged += (_, e) =>
        {
            SaveItems();

            if (e.PropertyName == nameof(TodoItem.IsDone) || e.PropertyName == nameof(TodoItem.Priority) || e.PropertyName == nameof(TodoItem.DueDate))
            {
                OnPropertyChanged(nameof(OpenItems));
                OnPropertyChanged(nameof(DoneItems));
            }
        };

        Items.Add(item);

        NewTitle = string.Empty;
        NewPriority = Priority.Normal;
        NewDueDate = null;

        OnPropertyChanged(nameof(OpenItems));
        OnPropertyChanged(nameof(DoneItems));
    }

    // Public overload to allow seeding items programmatically
    public void AddItem(TodoItem item)
    {
        if (item is null) return;
        Items.Add(item);
        OnPropertyChanged(nameof(OpenItems));
        OnPropertyChanged(nameof(DoneItems));
    }

    private void ArchiveOldDoneItems()
    {
        var cutoff = DateTime.Now.AddDays(-14);

        var toArchive = Items
            .Where(i => i.IsDone && i.CompletedOn.HasValue && i.CompletedOn.Value < cutoff)
            .ToList();

        if (toArchive.Any())
        {
            ArchiveService.AppendToArchive(new ObservableCollection<TodoItem>(toArchive));

            foreach (var item in toArchive)
                Items.Remove(item);

            SaveItems();
            OnPropertyChanged(nameof(OpenItems));
            OnPropertyChanged(nameof(DoneItems));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
