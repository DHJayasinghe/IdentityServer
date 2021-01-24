using System.Collections.Generic;

namespace CommonUtil.Models
{
    public sealed class DataTableAjaxPostResponse<T> : IDataTableAjaxPostResponse<T> where T : class
    {
        public int Draw { get; set; }
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
        public IEnumerable<T> Data { get; set; }

        public DataTableAjaxPostResponse() { }

        public DataTableAjaxPostResponse(int draw, int recordsTotal, int recordsFiltered, IEnumerable<T> data)
        {
            Draw = draw;
            RecordsTotal = recordsTotal;
            RecordsFiltered = recordsFiltered;
            Data = data;
        }
    }
}
