namespace WebAPI.Helpers
{
    public class Pagination<T> where T : class
    {
        public Pagination(int pageSize, int pagenNumber, int pageCount, IReadOnlyList<T> data)
        {
            PageSize = pageSize;
            PageNumber = pagenNumber;
            PageCount = pageCount;
            Data = data;
        }

        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public int PageCount { get; set; }
        public IReadOnlyList<T> Data { get; set; }
    }
}
