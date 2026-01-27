using System.ComponentModel.DataAnnotations;

namespace SFManagement.Application.Common.Extensions;

public static class EnumHelper
{
    public static int ToOrder<T>(this Enum e) where T : struct, Enum
    {
        var field = typeof(T).GetField(e.ToString());
        if (field == null) return 0;
        
        var attributes = field.GetCustomAttributes(typeof(DisplayAttribute), false) as DisplayAttribute[];
        return attributes?.FirstOrDefault()?.Order ?? 0;
    }

    public static string? ToDescription<T>(this Enum e) where T : struct, Enum
    {
        var field = typeof(T).GetField(e.ToString());
        if (field == null) return null;
        
        var attributes = field.GetCustomAttributes(typeof(DisplayAttribute), false) as DisplayAttribute[];
        return attributes?.FirstOrDefault()?.Description;
    }

    public static string? ToShortName<T>(this Enum e) where T : struct, Enum
    {
        var field = typeof(T).GetField(e.ToString());
        if (field == null) return null;
        
        var attributes = field.GetCustomAttributes(typeof(DisplayAttribute), false) as DisplayAttribute[];
        return attributes?.FirstOrDefault()?.ShortName;
    }

    public static string? ToGroup<T>(this Enum e) where T : struct, Enum
    {
        var field = typeof(T).GetField(e.ToString());
        if (field == null) return null;
        
        var attributes = field.GetCustomAttributes(typeof(DisplayAttribute), false) as DisplayAttribute[];
        return attributes?.FirstOrDefault()?.GroupName;
    }

    public static string ToName<T>(this Enum e) where T : struct, Enum
    {
        var field = typeof(T).GetField(e.ToString());
        if (field == null) return e.ToString();

        var attributes = field.GetCustomAttributes(typeof(DisplayAttribute), false) as DisplayAttribute[];
        return attributes?.FirstOrDefault()?.Name ?? e.ToString();
    }
}
