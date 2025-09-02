using StickyTodoApp.Commands;
using StickyTodoApp.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace StickyTodoApp.ViewModels;

public class TodoViewModel : INotifyPropertyChanged
{
    public ObservableCollection<TodoItem> Items { get; set; } = [];

    // Expose enum values for binding (Priority dropdown)
    public IEnumerable<Priority> PriorityValues { get; } = Enum.GetValues<Priority>();

    private string? _newTitle;
    private Priority _newPriority = Priority.Normal;
    private DateTime? _newDueDate;

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

    public ICommand AddCommand { get; }

    public TodoViewModel()
    {
        AddCommand = new RelayCommand(_ => AddItem(), _ => !string.IsNullOrWhiteSpace(NewTitle));
    }

    public IEnumerable<TodoItem> OpenItems =>
        Items.Where(i => !i.IsDone)
             .OrderByDescending(i => i.Priority)
             .ThenBy(i => i.DueDate ?? DateTime.MaxValue);

    public IEnumerable<TodoItem> DoneItems =>
        Items.Where(i => i.IsDone);

    // Adds item from input fields
    private void AddItem()
    {
        Items.Add(new TodoItem
        {
            Title = NewTitle?.Trim(),
            Priority = NewPriority,
            DueDate = NewDueDate
        });

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

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
