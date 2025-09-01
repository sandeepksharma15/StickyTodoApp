using StickyTodoApp.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace StickyTodoApp.ViewModels;

public class TodoViewModel : INotifyPropertyChanged
{
    public ObservableCollection<TodoItem> Items { get; set; } = [];

    // Sorted view: urgent first, then due date
    public IEnumerable<TodoItem> OpenItems =>
        Items.Where(i => !i.IsDone)
             .OrderByDescending(i => i.Priority) // Urgent first
             .ThenBy(i => i.DueDate ?? DateTime.MaxValue);

    public IEnumerable<TodoItem> DoneItems =>
        Items.Where(i => i.IsDone);

    public void AddItem(TodoItem item)
    {
        Items.Add(item);
        OnPropertyChanged(nameof(OpenItems));
        OnPropertyChanged(nameof(DoneItems));
    }

    public void UpdateItems()
    {
        OnPropertyChanged(nameof(OpenItems));
        OnPropertyChanged(nameof(DoneItems));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
