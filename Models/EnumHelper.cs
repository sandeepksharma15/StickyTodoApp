namespace StickyTodoApp.Models;

public static class EnumHelper<T> where T : Enum
{
    public static IEnumerable<T> Values => (T[])Enum.GetValues(typeof(T));
}
