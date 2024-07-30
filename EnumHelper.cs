using System.ComponentModel.DataAnnotations;

namespace SFManagement
{
    public static class EnumHelper
    {
        public static int ToOrder<T>(this Enum e)
        {
            var field = typeof(T).GetField(e.ToString());
            var attributes = field.GetCustomAttributes(typeof(DisplayAttribute), false) as DisplayAttribute[];
            return attributes.First().Order;
        }

        public static string ToDescription<T>(this Enum e)
        {
            var field = typeof(T).GetField(e.ToString());
            var attributes = field.GetCustomAttributes(typeof(DisplayAttribute), false) as DisplayAttribute[];
            return attributes.First().Description;
        }

        public static string ToShortName<T>(this Enum e)
        {
            var field = typeof(T).GetField(e.ToString());
            var attributes = field.GetCustomAttributes(typeof(DisplayAttribute), false) as DisplayAttribute[];
            return attributes.First().ShortName;
        }

        public static string ToGroup<T>(this Enum e)
        {
            var field = typeof(T).GetField(e.ToString());
            var attributes = field.GetCustomAttributes(typeof(DisplayAttribute), false) as DisplayAttribute[];
            return attributes.First().GroupName;
        }

        public static string ToName<T>(this Enum e)
        {
            var field = typeof(T).GetField(e.ToString());

            if (field == null)
            {
                return string.Empty;
            }

            var attributes = field.GetCustomAttributes(typeof(DisplayAttribute), false) as DisplayAttribute[];
            return attributes.Any() ? attributes.First().Name : e.ToString();
        }
    }
}
