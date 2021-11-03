using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DevExpress.Mvvm;
using Newtonsoft.Json;

namespace KursAM2.ViewModel.Signatures
{
    public class UserSignViewModel : ViewModelBase
    {
        [Display(AutoGenerateField = false, Description = "Id подписи")]
        [Editable(false)]
        public Guid Id
        {
            get => GetValue<Guid>();
            set => SetValue(value);
        }
        [Display(AutoGenerateField = true, Name = "Подпись")]
        [Editable(false)]
        public string SignUserName
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
    }

    public class SignatureViewModel : ViewModelBase
    {
        [Display(AutoGenerateField = false, Description = "Тип документа, к которому привязана подпись")]
        public int DocumentType
        {
            get => GetValue<int>();
            set => SetValue(value);
        }

        [Display(AutoGenerateField = false, Description = "Id подписи")]
        [Editable(false)]
        public Guid Id
        {
            get => GetValue<Guid>();
            set => SetValue(value);
        }
        private Guid? myParentId;
        [Display(AutoGenerateField = false, Description = "Родительский Ид подписи")]
        [Editable(false)]
        public Guid? ParentId
        {
            get => myParentId;
            set
            {
                if (myParentId == value) return;
                myParentId = value;
                RaisePropertyChanged();
            }
        }

        [Display(AutoGenerateField = false, Description = "Ид типа подписи")]
        [Editable(false)]
        public Guid SignTypeId
        {
            get => GetValue<Guid>();
            set => SetValue(value);
        }

        [Display(AutoGenerateField = true, Name = "Тип подписи")]
        [Editable(false)]
        public string SignTypeName
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        [Display(AutoGenerateField = false, Description = "Ид подписавшего")]
        public Guid? UserId
        {
            get => GetValue<Guid?>();
            set => SetValue(value);
        }

        [Display(AutoGenerateField = true, Name = "Подпись")]
        [Editable(false)]
        public string SignUserName
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        [Display(AutoGenerateField = true, Name = "Полное имя")]
        [Editable(false)]
        public string SignUserFullName
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        [Display(AutoGenerateField = true, Name = "Примечания")]
        public string Note
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        [Display(AutoGenerateField = true, Name = "Обязательно")]
        [Editable(false)]
        public bool IsRequired
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        public List<UserSignViewModel> UsersCanSigned { set; get; } = new List<UserSignViewModel>();

        public object ToJson()
        {
            return new
            {
                Id = Id,
                ParentId = ParentId,
                DocumentType = DocumentType,
                SignTypeId = SignTypeId,
                SignTypeName = SignTypeName,
                UserId = UserId,
                SignUserName = SignUserName,
                SignUserFullName = SignUserFullName,
                Note = Note,
                IsRequired = IsRequired
            };
        }

        public override string ToString()
        {
            return $"Подпись -> {SignTypeName} : {SignUserName}";
        }
    }
}