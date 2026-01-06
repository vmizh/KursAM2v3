using System;
using System.ComponentModel.DataAnnotations;
using KursDomain.Documents.CommonReferences;
using KursDomain.References;

namespace KursRepositories.Repositories.Projects
{

    public class ProjectManagerDocumentInfo
    {
        [Display(AutoGenerateField = false)] public Guid Id { set; get; }
        [Display(AutoGenerateField = false)] public decimal DocCode { set; get; }

        [Display(AutoGenerateField = true, Name = "Тип документа")]
        public DocumentType DocType { set; get; }

        [Display(AutoGenerateField = true, Name = "№")]
        public int NumberInner { set; get; }

        [Display(AutoGenerateField = true, Name = "Внеш.№")]
        public string NumberOuter { set; get; }

        [Display(AutoGenerateField = true, Name = "Дата")]
        public DateTime DocDate { set; get; }

        [Display(AutoGenerateField = true, Name = "Контрагент")]
        public Kontragent Kontragent { set; get; }

        [Display(AutoGenerateField = true, Name = "Сумма")]
        [DisplayFormat(DataFormatString = "n2", ApplyFormatInEditMode = true)]
        public decimal Summa { set; get; }

        [Display(AutoGenerateField = true, Name = "Валюта")]
        public Currency Currency { set; get; }

        [Display(AutoGenerateField = true, Name = "Примечание")]
        public string Note { set; get; }

        [Display(AutoGenerateField = true, Name = "Создатель")]
        public string Creator { set; get; }

    }


    public class ProjectsForDocumentInfo
    {
        [Display(AutoGenerateField = false)]
        public Guid Id { set; get; }
        [Display(AutoGenerateField = true, Name = "Основной проект")]
        public string Name { set; get; }
        [Display(AutoGenerateField = true, Name = "Подпроект")]
        public string Name2 { set; get; }
        [Display(AutoGenerateField = true, Name = "Подпроект 2")]
        public string Name3 { set; get; }
    }

    public record ProjectManagerDocumentInfoRequest
    {
        public Guid Id { set; get; }
        public decimal DocCode { set; get; }

        public int DocType { set; get; }

        public int NumberInner { set; get; }

        public string NumberOuter { set; get; }

        public DateTime DocDate { set; get; }

        public decimal? KontrDC { set; get; }

        public decimal? Summa { set; get; }

        public decimal? CurrencyDC { set; get; }

        public string Note { set; get; }

        public string Creator { set; get; }

    }
}
