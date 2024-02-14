using System.Windows.Input;

namespace KursAM2.ViewModel
{
    public class KursTile
    {
        public string Header { get; set; }
        public ICommand NewDocumentCommand { get; }

        public bool IsNewButtonEnable { get; set; }

        public bool IsFavorite { set; get; }

        public int MenuId { get; set; }
    }
}
