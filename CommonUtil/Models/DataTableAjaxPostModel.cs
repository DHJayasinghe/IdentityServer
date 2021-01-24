using System.Collections.Generic;

namespace CommonUtil.Models
{
    public sealed class DataTableAjaxPostModel
    {
        public int Draw { get; set; }

        public int Start { get; set; }

        public int Length { get; set; }

        public List<ColumnModel> Columns { get; set; }

        public SearchModel Search { get; set; }

        public List<OrderModel> Order { get; set; }
    }

    public sealed class ColumnModel
    {
        public string Data { get; set; }

        public string Name { get; set; }

        public bool Searchable { get; set; }

        public bool Orderable { get; set; }

        public SearchModel Search { get; set; }
    }

    public sealed class SearchModel
    {
        public string Value { get; set; }

        //public string Regex { get; set; }
    }

    public sealed class OrderModel
    {
        public int Column { get; set; }

        public string Dir { get; set; }
    }
}
