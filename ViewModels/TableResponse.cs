namespace SFManagement.ViewModels
{
    public class TableResponse<T>
    {
        public int Total { get; set; } = 0;

        public int Show { get; set; } = 0;

        public int Page { get; set; } = 0;

        public List<T> Data { get; set; } = new List<T>();
    }
}
