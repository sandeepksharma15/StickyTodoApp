using System;
using System.ComponentModel;

namespace StickyTodoApp.Models;

public enum Priority
{
    Normal,
    Urgent
}

public class TodoItem : INotifyPropertyChanged
{
    private string? _title;
    private Priority _priority;
    private DateTime? _dueDate;
    private bool _isDone;

    public string? Title
    {
        get => _title;
        set { _title = value; OnPropertyChanged(nameof(Title)); }
    }

    public Priority Priority
    {
        get => _priority;
        set { _priority = value; OnPropertyChanged(nameof(Priority)); }
    }

    public DateTime? DueDate
    {
        get => _dueDate;
        set { _dueDate = value; OnPropertyChanged(nameof(DueDate)); }
    }

    public bool IsDone
    {
        get => _isDone;
        set { _isDone = value; OnPropertyChanged(nameof(IsDone)); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
