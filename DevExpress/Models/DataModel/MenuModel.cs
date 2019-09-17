namespace DevExpress.Models.DataModel
{
    public class MenuModel
    {
        public int Id { set; get; }
        public int? ParentId { set; get; }
        public string Name { set; get; }
        public int? OrderBy { set; get; }
        public string StrLink { set; get; }
    }
}