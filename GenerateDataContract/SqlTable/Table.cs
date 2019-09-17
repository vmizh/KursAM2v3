using System.Collections.Generic;

namespace GenerateDataContract.SqlTable
{
    public class Table
    {
        public string Name { set; get; }
        public List<Column> Columns { get; } = new List<Column>();
        public List<ForeignKey> Keys { get; } = new List<ForeignKey>();

        public List<ForeignKey> ChildKeys { get; } = new List<ForeignKey>();
        /// <summary>
        /// Текст кода класса
        /// </summary>
        public string TextClassDto { set; get; }

    }
}