using System.Collections.Generic;

namespace CommonUtil.Models
{
    public interface IDataTableAjaxPostResponse<T> where T : class
    {
        int Draw { get; set; }
        int RecordsTotal { get; set; }
        int RecordsFiltered { get; set; }
        IEnumerable<T> Data { get; set; }
    }
}
