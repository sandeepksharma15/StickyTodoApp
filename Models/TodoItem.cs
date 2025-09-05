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
    private DateTime? _completedOn;

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
        set
        {
            if (_isDone != value)
            {
                _isDone = value;
                if (_isDone)
                    CompletedOn = DateTime.Now;
                else
                    CompletedOn = null;
                OnPropertyChanged(nameof(IsDone));
            }
        }
    }

    public DateTime? CompletedOn
    {
        get => _completedOn;
        set { _completedOn = value; OnPropertyChanged(nameof(CompletedOn)); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
