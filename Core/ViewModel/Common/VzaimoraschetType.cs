using Core.EntityViewModel;
using Data;

namespace Core.ViewModel.Common
{
    public class VzaimoraschetType : SD_77ViewModel
    {
        public VzaimoraschetType()
        {
        }

        public VzaimoraschetType(SD_77 entity)
            : base(entity)
        {
        }

        public override decimal DocCode
        {
            get => Entity.DOC_CODE;
            set
            {
                if (Entity.DOC_CODE == value) return;
                Entity.DOC_CODE = value;
                RaisePropertyChanged();
            }
        }

        public override string Name
        {
            get => Entity.TV_NAME;
            set
            {
                if (Entity.TV_NAME == value) return;
                Entity.TV_NAME = value;
                RaisePropertyChanged();
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}