using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Core;
using Core.ViewModel.Base;
using KursDomain;
using KursDomain.Menu;

namespace KursAM2.ViewModel.Reconcilation
{
    public class AOFViewModel : RSWindowViewModelBase
    {
        private ResponsibleCorporate myCurrentCorporate;
        private DatePeriod myCurrentPeriod;
        private Responsible myResponsible;

        public AOFViewModel()
        {
            Period = new List<DatePeriod>();
            Responsibles = new ObservableCollection<Responsible>();
            ResponsibleCorporates = new ObservableCollection<ResponsibleCorporate>();
            Acts = new ObservableCollection<ActOfResponsibleShort>();
            ResponsibleSelectedCorporates = new ObservableCollection<ResponsibleCorporate>();
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
            var d = new List<DateTime>();
            var dd = DateTime.Today;
            while (dd >= new DateTime(2014, 11, 1))
            {
                d.Add(dd);
                dd = dd.AddDays(-1);
            }

            foreach (var mm in DatePeriod.GenerateIerarhy(d, PeriodIerarhy.YearMonth)
                .Where(_ => _.PeriodType == PeriodType.Month))
                Period.Add(new DatePeriod
                {
                    Id = mm.Id,
                    ParentId = mm.ParentId,
                    Name = mm.DateStart.Year + " " + mm.Name,
                    DateStart = mm.DateStart,
                    DateEnd = mm.DateEnd,
                    PeriodType = PeriodType.Month
                });
        }

        public List<DatePeriod> Period { set; get; }
        public ObservableCollection<Responsible> Responsibles { set; get; }
        public ObservableCollection<ResponsibleCorporate> ResponsibleCorporates { set; get; }
        public ObservableCollection<ActOfResponsibleShort> Acts { set; get; }
        public ObservableCollection<ResponsibleCorporate> ResponsibleSelectedCorporates { set; get; }

        public Responsible Responsible
        {
            get => myResponsible;
            set
            {
                if (Equals(value, myResponsible)) return;
                myResponsible = value;
                ResponsibleCorporates.Clear();
                if (Responsible != null)
                    foreach (var c in myResponsible.Corporates)
                        ResponsibleCorporates.Add(c);
                RaisePropertyChanged(nameof(Responsible));
                RaisePropertyChanged(nameof(ResponsibleCorporates));
            }
        }

        //CurrentPeriod
        public DatePeriod CurrentPeriod
        {
            get => myCurrentPeriod;
            set
            {
                if (Equals(value, myCurrentPeriod)) return;
                myCurrentPeriod = value;
                RaisePropertyChanged(nameof(CurrentPeriod));
            }
        }

        public ResponsibleCorporate CurrentCorporate
        {
            get => myCurrentCorporate;
            set
            {
                if (Equals(value, myCurrentCorporate)) return;
                myCurrentCorporate = value;
                Acts.Clear();
                if (myCurrentCorporate != null)
                    foreach (var c in myCurrentCorporate.Acts)
                        Acts.Add(c);
                RaisePropertyChanged(nameof(Acts));
            }
        }

        public bool IsRefreshEnable => CurrentPeriod != null;

        #region Command

        public ICommand PeriodChangedCommand
        {
            get { return new Command(PeriodChanged, param => true); }
        }

        public void PeriodChanged(object obj)
        {
            using (var ent = GlobalOptions.GetEntities())
            {
                ResponsibleCorporates.Clear();
                Acts.Clear();
                Responsibles.Clear();
                var resp = ent.SD_2.Select(r => new Responsible
                    {
                        Name = r.NAME,
                        TabelNumber = r.TABELNUMBER
                    })
                    .ToList();
                resp.Add(new Responsible
                {
                    Name = "Ответственный не указан",
                    TabelNumber = null
                });
                var kontrs = ent.SD_43.Where(_ => _.FLAG_BALANS == 1).ToList();
                var docs = ent.SD_430.Where(
                        _ =>
                            _.ASV_FIKS_DATE >= CurrentPeriod.DateStart &&
                            _.ASV_FIKS_DATE <= CurrentPeriod.DateEnd)
                    .ToList();
                foreach (var s in resp)
                {
                    var s1 = s;
                    foreach (
                        var cc in
                        kontrs.Where(_ => _.OTVETSTV_LICO == s1.TabelNumber)
                            .Select(c => new ResponsibleCorporate
                            {
                                DocCode = c.DOC_CODE,
                                Name = c.NAME
                            }))
                    {
                        s.AllCorporate++;
                        var cc1 = cc;
                        foreach (var aof in docs.Where(_ => _.ASV_KONTR_DC == cc1.DocCode))
                            cc.Acts.Add(new ActOfResponsibleShort
                            {
                                Num = aof.ASV_NUM.ToString(),
                                Notes = aof.ASV_NOTES,
                                Creator = aof.CREATOR,
                                Date = aof.ASV_DATE
                            });
                        if (cc.Acts.Count > 0)
                            s.QuantityAOF++;
                        else
                            s.QuatityNotAOF++;
                        s.Corporates.Add(cc);
                    }

                    if (s.AllCorporate > 0)
                        Responsibles.Add(s);
                }
            }

            RaisePropertyChanged(nameof(Responsibles));
            RaisePropertyChanged(nameof(ResponsibleCorporates));
            RaisePropertyChanged(nameof(IsRefreshEnable));
        }

        public override void RefreshData(object obj)
        {
            Responsibles.Clear();
            PeriodChanged(null);
        }

        #endregion
    }
}
