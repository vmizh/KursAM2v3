using System.ComponentModel.DataAnnotations;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.Helper;
using Core.ViewModel.Common;
using DevExpress.Mvvm.DataAnnotations;
using static System.Math;

namespace KursAM2.ViewModel.Management.Projects
{
    [MetadataType(typeof(DataAnnotationsProjectResultInfoViewModel))]
    public class ProjectResultInfo : Project, IMultyWithDilerCurrency
    {
        private decimal myDilerCHF;
        private decimal myDilerEUR;
        private decimal myDilerGBP;
        private decimal myDilerRUB;
        private decimal myDilerSEK;
        private decimal myDilerUSD;
        private decimal myLossCHF;
        private decimal myLossEUR;
        private decimal myLossGBP;
        private decimal myLossRUB;
        private decimal myLossSEK;
        private decimal myLossUSD;
        private decimal myProfitCHF;
        private decimal myProfitEUR;
        private decimal myProfitGBP;
        private decimal myProfitRUB;
        private decimal myProfitSEK;
        private decimal myProfitUSD;

        public ProjectResultInfo()
        {
        }

        public ProjectResultInfo(Project p)
        {
            updateProject(p);
        }

        public decimal DilerSEK
        {
            get => myDilerSEK;
            set
            {
                if (myDilerSEK == value) return;
                myDilerSEK = Abs(value);
                RaisePropertyChanged();
                RaisePropertiesChanged(nameof(ResultSEK));
            }
        }

        public decimal DilerUSD
        {
            get => myDilerUSD;
            set
            {
                if (myDilerUSD == value) return;
                myDilerUSD = Abs(value);
                RaisePropertyChanged();
                RaisePropertiesChanged(nameof(ResultUSD));
            }
        }

        public decimal DilerRUB
        {
            get => myDilerRUB;
            set
            {
                if (myDilerRUB == value) return;
                myDilerRUB = Abs(value);
                RaisePropertyChanged();
            }
        }

        public decimal DilerEUR
        {
            get => myDilerEUR;
            set
            {
                if (myDilerEUR == value) return;
                myDilerEUR = Abs(value);
                RaisePropertyChanged();
                RaisePropertiesChanged(nameof(ResultEUR));
            }
        }

        public decimal DilerGBP
        {
            get => myDilerGBP;
            set
            {
                if (myDilerGBP == value) return;
                myDilerGBP = Abs(value);
                RaisePropertyChanged();
            }
        }

        public decimal DilerCHF
        {
            get => myDilerCHF;
            set
            {
                if (myDilerCHF == value) return;
                myDilerCHF = Abs(value);
                RaisePropertyChanged();
                RaisePropertiesChanged(nameof(ResultCHF));
            }
        }

        /// <summary>
        ///     Шведская Крона
        /// </summary>
        public decimal LossSEK
        {
            get => myLossSEK;
            set
            {
                if (myLossSEK == value) return;
                myLossSEK = Abs(value);
                RaisePropertyChanged();
                RaisePropertiesChanged(nameof(ResultSEK));
            }
        }

        /// <summary>
        ///     Шведская Крона
        /// </summary>
        public decimal ProfitSEK
        {
            get => myProfitSEK;
            set
            {
                if (myProfitSEK == value) return;
                myProfitSEK = Abs(value);
                RaisePropertyChanged();
                RaisePropertiesChanged(nameof(ResultSEK));
            }
        }

        /// <summary>
        ///     Швейцарский франк
        /// </summary>
        public decimal LossCHF
        {
            get => myLossCHF;
            set
            {
                if (myLossCHF == value) return;
                myLossCHF = Abs(value);
                RaisePropertyChanged();
                RaisePropertiesChanged(nameof(ResultCHF));
            }
        }

        /// <summary>
        ///     Швейцарский франк
        /// </summary>
        public decimal ProfitCHF
        {
            get => myProfitCHF;
            set
            {
                if (myProfitCHF == value) return;
                myProfitCHF = Abs(value);
                RaisePropertyChanged();
                RaisePropertiesChanged(nameof(ResultCHF));
            }
        }

        /// <summary>
        ///     Швейцарский франк
        /// </summary>
        public decimal LossGBP
        {
            get => myLossGBP;
            set
            {
                if (myLossGBP == value) return;
                myLossGBP = Abs(value);
                RaisePropertyChanged();
                RaisePropertiesChanged(nameof(ResultGBP));
            }
        }

        public decimal ProfitGBP
        {
            get => myProfitGBP;
            set
            {
                if (myProfitGBP == value) return;
                myProfitGBP = Abs(value);
                RaisePropertyChanged();
                RaisePropertiesChanged(nameof(ResultGBP));
            }
        }

        public decimal ProfitRUB
        {
            set
            {
                if (Equals(value, myProfitRUB)) return;
                myProfitRUB = Abs(value);
                RaisePropertyChanged();
                RaisePropertiesChanged(nameof(ResultRUB));
            }
            get => myProfitRUB;
        }

        public decimal LossRUB
        {
            set
            {
                if (Equals(value, myLossRUB)) return;
                myLossRUB = Abs(value);
                RaisePropertyChanged();
                RaisePropertiesChanged(nameof(ResultRUB));
            }
            get => myLossRUB;
        }

        public decimal ProfitUSD
        {
            set
            {
                if (Equals(value, myProfitUSD)) return;
                myProfitUSD = Abs(value);
                RaisePropertyChanged();
                RaisePropertiesChanged(nameof(ResultUSD));
            }
            get => myProfitUSD;
        }

        public decimal LossUSD
        {
            set
            {
                if (Equals(value, myLossUSD)) return;
                myLossUSD = Abs(value);
                RaisePropertyChanged();
                RaisePropertiesChanged(nameof(ResultUSD));
            }
            get => myLossUSD;
        }

        public decimal ProfitEUR
        {
            set
            {
                if (Equals(value, myProfitEUR)) return;
                myProfitEUR = Abs(value);
                RaisePropertyChanged();
                RaisePropertiesChanged(nameof(ResultEUR));
            }
            get => myProfitEUR;
        }

        public decimal LossEUR
        {
            set
            {
                if (Equals(value, myLossEUR)) return;
                myLossEUR = Abs(value);
                RaisePropertyChanged();
                RaisePropertiesChanged(nameof(ResultEUR));
            }
            get => myLossEUR;
        }

        public decimal ResultCHF => ProfitCHF - LossCHF - DilerCHF;
        public decimal ResultEUR => ProfitEUR - LossEUR - DilerEUR;
        public decimal ResultGBP => ProfitGBP - LossGBP - DilerGBP;
        public decimal ResultRUB => ProfitRUB - LossRUB - DilerRUB;
        public decimal ResultSEK => ProfitSEK - LossSEK - DilerSEK;
        public decimal ResultUSD => ProfitUSD - LossUSD - DilerUSD;

        private void updateProject(Project p)
        {
            Id = p.Id;
            ParentId = p.ParentId;
            Name = p.Name;
            DateStart = p.DateStart;
            DateEnd = p.DateEnd;
            IsClosed = p.IsClosed;
            IsDeleted = p.IsDeleted;
            Responsible = p.Responsible;
        }
    }

    public class DataAnnotationsProjectResultInfoViewModel : DataAnnotationForFluentApiBase,
        IMetadataProvider<ProjectResultInfo>
    {
        void IMetadataProvider<ProjectResultInfo>.BuildMetadata(MetadataBuilder<ProjectResultInfo> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(_ => _.Name).AutoGenerated().DisplayName("Наименование");
            builder.Property(_ => _.DateEnd).AutoGenerated().DisplayName("Дата окончания");
            builder.Property(_ => _.DateStart).AutoGenerated().DisplayName("Дата начала");
            builder.Property(_ => _.IsClosed).AutoGenerated().DisplayName("Закрыт");
            builder.Property(_ => _.IsDeleted).AutoGenerated().DisplayName("Удален");
            builder.Property(_ => _.Responsible).AutoGenerated().DisplayName("Ответственный");
            builder.Property(_ => _.ProfitRUB).AutoGenerated().DisplayName("Доход");
            builder.Property(_ => _.LossRUB).AutoGenerated().DisplayName("Расход");
            builder.Property(_ => _.ResultRUB).AutoGenerated().DisplayName("Результат");
            builder.Property(_ => _.ProfitUSD).AutoGenerated().DisplayName("Доход");
            builder.Property(_ => _.LossUSD).AutoGenerated().DisplayName("Расход");
            builder.Property(_ => _.ResultUSD).AutoGenerated().DisplayName("Результат");
            builder.Property(_ => _.ProfitGBP).AutoGenerated().DisplayName("Доход");
            builder.Property(_ => _.LossGBP).AutoGenerated().DisplayName("Расход");
            builder.Property(_ => _.ResultGBP).AutoGenerated().DisplayName("Результат");
            builder.Property(_ => _.ProfitEUR).AutoGenerated().DisplayName("Доход");
            builder.Property(_ => _.LossEUR).AutoGenerated().DisplayName("Расход");
            builder.Property(_ => _.ResultEUR).AutoGenerated().DisplayName("Результат");
            builder.Property(_ => _.ProfitCHF).AutoGenerated().DisplayName("Доход");
            builder.Property(_ => _.LossCHF).AutoGenerated().DisplayName("Расход");
            builder.Property(_ => _.ResultCHF).AutoGenerated().DisplayName("Результат");
            builder.Property(_ => _.ProfitSEK).AutoGenerated().DisplayName("Доход");
            builder.Property(_ => _.LossSEK).AutoGenerated().DisplayName("Расход");
            builder.Property(_ => _.ResultSEK).AutoGenerated().DisplayName("Результат");
            builder.Property(_ => _.DilerRUB).AutoGenerated().DisplayName("Дилер");
            builder.Property(_ => _.DilerUSD).AutoGenerated().DisplayName("Дилер");
            builder.Property(_ => _.DilerEUR).AutoGenerated().DisplayName("Дилер");
            builder.Property(_ => _.DilerCHF).AutoGenerated().DisplayName("Дилер");
            builder.Property(_ => _.DilerSEK).AutoGenerated().DisplayName("Дилер");
            builder.Property(_ => _.DilerGBP).AutoGenerated().DisplayName("Дилер");
            builder.Group("Проект")
                .ContainsProperty(_ => _.Name)
                .ContainsProperty(_ => _.DateEnd)
                .ContainsProperty(_ => _.DateStart)
                .ContainsProperty(_ => _.IsClosed)
                .ContainsProperty(_ => _.IsDeleted)
                .ContainsProperty(_ => _.Responsible).EndGroup();
            builder.Group("RUB")
                .ContainsProperty(_ => _.ProfitRUB)
                .ContainsProperty(_ => _.LossRUB)
                .ContainsProperty(_ => _.DilerRUB)
                .ContainsProperty(_ => _.ResultRUB).EndGroup();
            builder.Group("USD")
                .ContainsProperty(_ => _.ProfitUSD)
                .ContainsProperty(_ => _.LossUSD)
                .ContainsProperty(_ => _.DilerUSD)
                .ContainsProperty(_ => _.ResultUSD).EndGroup();
            builder.Group("EUR")
                .ContainsProperty(_ => _.ProfitEUR)
                .ContainsProperty(_ => _.LossEUR)
                .ContainsProperty(_ => _.DilerEUR)
                .ContainsProperty(_ => _.ResultEUR).EndGroup();
            builder.Group("GBP")
                .ContainsProperty(_ => _.ProfitGBP)
                .ContainsProperty(_ => _.LossGBP)
                .ContainsProperty(_ => _.DilerGBP)
                .ContainsProperty(_ => _.ResultGBP).EndGroup();
            builder.Group("CHF")
                .ContainsProperty(_ => _.ProfitCHF)
                .ContainsProperty(_ => _.LossCHF)
                .ContainsProperty(_ => _.DilerCHF)
                .ContainsProperty(_ => _.ResultCHF).EndGroup();
            builder.Group("SEK")
                .ContainsProperty(_ => _.ProfitSEK)
                .ContainsProperty(_ => _.LossSEK)
                .ContainsProperty(_ => _.DilerSEK)
                .ContainsProperty(_ => _.ResultSEK).EndGroup();
        }
    }
}