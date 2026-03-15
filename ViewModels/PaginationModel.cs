namespace QuanLyChamCong.ViewModels
{

    public class PaginationModel
    {
        public int    CurrentPage { get; set; }
        public int    TotalPages  { get; set; }
        public int    Total       { get; set; }
        public string BaseUrl     { get; set; } = "";
        public string? Search     { get; set; }
        public string? SortBy     { get; set; }
        public string? SortDir    { get; set; }

        public string? Extra1Key  { get; set; }
        public string? Extra1Val  { get; set; }
        public string? Extra2Key  { get; set; }
        public string? Extra2Val  { get; set; }
        public string? Extra3Key  { get; set; }
        public string? Extra3Val  { get; set; }
    }
}
