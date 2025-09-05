using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using StickyTodoApp.Models;

namespace StickyTodoApp;

public static class StorageHelper
{
    public static string GetDataFilePath()
    {
        string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StickyTodoApp");

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        return Path.Combine(folder, "todos.json");
    }
}

public static class ArchiveService
{
    public static string GetArchiveFilePath()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "StickyTodoApp");
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
        return Path.Combine(folder, "archive.json");
    }

    public static void AppendToArchive(ObservableCollection<TodoItem> itemsToArchive)
    {
        try
        {
            var path = GetArchiveFilePath();
            ObservableCollection<TodoItem> archiveItems = new();

            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var loaded = JsonSerializer.Deserialize<ObservableCollection<TodoItem>>(json);
                if (loaded != null)
                    archiveItems = loaded;
            }

            foreach (var item in itemsToArchive)
                archiveItems.Add(item);

            var newJson = JsonSerializer.Serialize(archiveItems, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, newJson);
        }
        catch { /* log/ignore */ }
    }

    public static void ClearArchive()
    {
        try
        {
            var path = GetArchiveFilePath();
            if (File.Exists(path))
                File.Delete(path); // simplest: remove file entirely
        }
        catch { /* log/ignore */ }
    }
}