using HrMangmentSystem_Application.Common.Responses;

namespace HrMangmentSystem_Application.Common.PagedRequest
{
    public class PagedRequest
    {
        
        private const int MaxPageSize = 100;

        public int PageNumber { get; set; } = 1; 
        
        private int _pageSize { get; set; } = 10;

        //if the value < 0 set it to 10
        // if the value > 100 set it to 100 
        // Otherwise set the value
        public int PageSize  // Property
        {
            get => _pageSize; 
            set => _pageSize = value <= 0 ? 10 : value > MaxPageSize ? MaxPageSize : value; 
        }

        public string? SortBy { get; set; }

        public bool Desc { get; set; }

        public string? Term { get; set; }

    }
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();

        public int TotalCount { get; set; }
        public int Page {get; set; }
        public int PageSize { get; set; }

      
    }
  
}
