using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Core;
using Core.ViewModel.Base;
using Core.ViewModel.Base.Column;
using DevExpress.Data;

namespace KursAM2.ViewModel.Personal
{
    public class PayRollVedomostSearch : RSViewModelData
    {
        private DateTime _Date;
        private decimal _EUR;
        private bool _Istemplate;
        private decimal _RUB;
        private decimal _USD;
        private string creator;
        private string myId;
        public List<KursMoney> NachCurrency = new List<KursMoney>();

        public new string Id
        {
            get => myId;
            set
            {
                if (myId == value) return;
                myId = value;
                RaisePropertyChanged();
            }
        }

        [Display(GroupName = "<Creator>", Name = "Создатель")]
        [DataMember]
        [GridColumnView("Создатель", SettingsType.Default)]
        public string Creator
        {
            get => creator;
            set
            {
                if (creator == value) return;
                creator = value;
                if (State == RowStatus.NotEdited)
                    State = RowStatus.Edited;
                RaisePropertyChanged(nameof(Creator));
            }
        }

        [DataMember]
        [GridColumnView("Дата", SettingsType.Default)]
        public DateTime Date
        {
            get => _Date;
            set
            {
                if (_Date == value) return;
                _Date = value;
                if (State == RowStatus.NotEdited)
                    State = RowStatus.Edited;
                RaisePropertyChanged();
            }
        }

        [GridColumnView("Шаблон", SettingsType.Default, ReadOnly = true)]
        public bool Istemplate
        {
            get => _Istemplate;
            set
            {
                if (_Istemplate == value) return;
                _Istemplate = value;
                if (State == RowStatus.NotEdited)
                    State = RowStatus.Edited;
                RaisePropertyChanged();
            }
        }

        [GridColumnSummary(SummaryItemType.Sum, "n2")]
        [GridColumnView("USD", SettingsType.Decimal, ReadOnly = true)]
        public decimal USD
        {
            get => _USD;
            set
            {
                if (_USD == value) return;
                _USD = value;
                if (State == RowStatus.NotEdited)
                    State = RowStatus.Edited;
                RaisePropertyChanged();
            }
        }

        [GridColumnSummary(SummaryItemType.Sum, "n2")]
        [GridColumnView("RUB", SettingsType.Decimal, ReadOnly = true)]
        public decimal RUB
        {
            get => _RUB;
            set
            {
                if (_RUB == value) return;
                _RUB = value;
                if (State == RowStatus.NotEdited)
                    State = RowStatus.Edited;
                RaisePropertyChanged();
            }
        }

        [GridColumnSummary(SummaryItemType.Sum, "n2")]
        [GridColumnView("EUR", SettingsType.Decimal, ReadOnly = true)]
        public decimal EUR
        {
            get => _EUR;
            set
            {
                if (_EUR == value) return;
                _EUR = value;
                if (State == RowStatus.NotEdited)
                    State = RowStatus.Edited;
                RaisePropertyChanged();
            }
        }
    }
}