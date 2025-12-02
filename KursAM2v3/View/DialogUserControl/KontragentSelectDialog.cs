using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using Core.ViewModel.Base;
using KursDomain.WindowsManager.WindowsManager;
using KursAM2.Managers;
using KursDomain;
using KursDomain.ICommon;
using KursDomain.References;
using KursDomain.References.RedisCache;

namespace KursAM2.View.DialogUserControl
{
    public class KontragentSelectDialogOptions
    {
        private bool myIsClientOnly;
        private bool myIsProviderOnly;

        public KontragentSelectDialogOptions()
        {
            myIsClientOnly = false;
            myIsProviderOnly = false;
            Currency = null;
            IsDateCheck = false;
        }

        public bool IsProviderOnly
        {
            set
            {
                myIsProviderOnly = value;
                if (myIsProviderOnly)
                    myIsClientOnly = false;
            }
            get => myIsProviderOnly;
        }

        public bool IsClientOnly
        {
            set
            {
                myIsClientOnly = value;
                if (myIsClientOnly)
                    myIsProviderOnly = false;
            }
            get => myIsClientOnly;
        }

        public Currency Currency { set; get; }
        public bool IsDateCheck { set; get; }
        public bool IsBalans { set; get; }
        public decimal KontragentDC { set; get; }
    }

    public class KontragentSelectDialog : RSWindowViewModelBase, IDataUserControl
    {
        private readonly bool IsBalans;


        // ReSharper disable once NotAccessedField.Local
        private readonly bool isDateCheck;

        // ReSharper disable once NotAccessedField.Local
        private readonly decimal kontrDC;
        private Currency myCurrency;
        private Kontragent myCurrentKontragent;
        private KontragentSelectDialogUC myDataUserControl;

        private readonly bool myIsClientOnly;
        private readonly bool myIsProviderOnly;


        public KontragentSelectDialog(KontragentSelectDialogOptions options = null)
        {
            if (options is null)
                options = new KontragentSelectDialogOptions { IsBalans = true };
            isDateCheck = options.IsDateCheck;
            kontrDC = options.KontragentDC;
            IsBalans = options.IsBalans;
            Currency = options.Currency;
            myIsClientOnly = options.IsClientOnly;
            myIsProviderOnly = options.IsProviderOnly;
            LayoutControl =
                myDataUserControl = new KontragentSelectDialogUC();
            // ReSharper disable once VirtualMemberCallInConstructor
            WindowName = "Выбор контрагента";
            //LoadKontragentFromReference();
        }

        public override string LayoutName => "KontragentSelectDialog";

        public Currency Currency
        {
            get => myCurrency;
            set
            {
                if (Equals(myCurrency, value)) return;
                myCurrency = value;
                RaisePropertyChanged();
            }
        }

        public KontragentSelectDialogUC DataUserControl
        {
            get => myDataUserControl;
            set
            {
                if (Equals(myDataUserControl, value)) return;
                myDataUserControl = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<Kontragent> KontragentCollection { set; get; } =
            new ObservableCollection<Kontragent>();

        public Kontragent CurrentKontragent
        {
            get => myCurrentKontragent;
            set
            {
                if (Equals(myCurrentKontragent, value)) return;
                myCurrentKontragent = value;
                RaisePropertyChanged();
            }
        }

        public override bool IsCanSearch => !string.IsNullOrWhiteSpace(SearchText);

        public DependencyObject LayoutControl { get; }

        private void LoadKontragentFromReference()
        {
            var includeDCs = new List<decimal>();
            if (myIsClientOnly || myIsProviderOnly)
                using (var ctx = GlobalOptions.GetEntities())
                {
                    if (myIsClientOnly)
                        includeDCs.AddRange(ctx.SD_84.AsNoTracking().Select(_ => _.SF_CLIENT_DC.Value).Distinct().ToList());
                    if (myIsProviderOnly)
                        includeDCs.AddRange(ctx.SD_26.AsNoTracking().Select(_ => _.SF_POST_DC).Distinct().ToList());
                }

            //while (((RedisCacheReferences)GlobalOptions.ReferencesCache).isNomenklCacheLoad)
            //    Thread.Sleep(new TimeSpan(0, 0, 5));
            var KontrList = new List<Kontragent>();
            try
            {
                KontrList.Clear();
                if (Currency == null)
                    KontrList.AddRange(from k in KontragentManager.GetAllKontragentSortedByUsed()
                        where !k.IsDeleted
                        select new Kontragent
                        {
                            DocCode = k.DocCode,
                            Name = k.Name,
                            Currency = k.Currency,
                            ResponsibleEmployee = k.ResponsibleEmployee,
                            FullName = k.FullName,
                            IsBalans = k.IsBalans,
                            INN = k.INN,
                            IsDeleted = k.IsDeleted,
                            Notes = k.Notes,
                            StartBalans = k.StartBalans
                        });
                else
                    KontrList.AddRange(from k in KontragentManager.GetAllKontragentSortedByUsed()
                            .Where(_ => ((Currency)_.Currency).DocCode == Currency.DocCode)
                            .ToList()
                        where !k.IsDeleted
                        select new Kontragent
                        {
                            DocCode = k.DocCode,
                            Name = k.Name,
                            Currency = k.Currency,
                            ResponsibleEmployee = k.ResponsibleEmployee,
                            FullName = k.FullName,
                            IsBalans = k.IsBalans,
                            INN = k.INN,
                            IsDeleted = k.IsDeleted,
                            Notes = k.Notes,
                            StartBalans = k.StartBalans
                        });
                if (myIsClientOnly || myIsProviderOnly)
                {
                    foreach (var kontr in includeDCs.Select(dc => KontrList.FirstOrDefault(_ => _.DocCode == dc))
                                 .Where(kontr => kontr is not null))
                    {
                        if (IsBalans)
                        {
                            if (kontr.IsBalans)
                            {
                                KontragentCollection.Add(kontr);
                            }
                        }
                        else
                        {
                            KontragentCollection.Add(kontr);
                        }
                    }
                }
                else
                {
                    KontragentCollection = IsBalans
                        ? new ObservableCollection<Kontragent>(KontrList.Where(_ => _.IsBalans))
                        : new ObservableCollection<Kontragent>(KontrList);
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once UnusedParameter.Local
        private void LoadKontragentFromReference(decimal KontrDC)
        {
            try
            {
                KontragentCollection.Clear();
                if (Currency == null)
                    foreach (var k in KontragentManager.GetAllKontragentSortedByUsed())
                    {
                        if (k.IsDeleted) continue;
                        KontragentCollection.Add(new Kontragent
                        {
                            DocCode = k.DocCode,
                            Name = k.Name,
                            Currency = k.Currency,
                            ResponsibleEmployee = k.ResponsibleEmployee,
                            FullName = k.FullName,
                            IsBalans = k.IsBalans,
                            INN = k.INN,
                            IsDeleted = k.IsDeleted,
                            Notes = k.Notes
                        });
                    }
                else
                    foreach (var k in KontragentManager.GetAllKontragentSortedByUsed()
                                 .Where(_ => ((IName)_.Currency).Name == Currency.Name))
                    {
                        if (k.IsDeleted) continue;
                        var kontr = new Kontragent
                        {
                            DocCode = k.DocCode,
                            Name = k.Name,
                            Currency = k.Currency,
                            ResponsibleEmployee = k.ResponsibleEmployee,
                            FullName = k.FullName,
                            IsBalans = k.IsBalans,
                            INN = k.INN,
                            IsDeleted = k.IsDeleted,
                            Notes = k.Notes
                        };
                        KontragentCollection.Add(kontr);
                    }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        public override void SearchClear(object obj)
        {
            SearchText = null;
            Search(null);
        }

        public override void Search(object obj)
        {
            KontragentCollection.Clear();
            if (string.IsNullOrEmpty(SearchText))
            {
                LoadKontragentFromReference();
                return;
            }

            try
            {
                foreach (var k in GlobalOptions.ReferencesCache.GetKontragentsAll().Cast<Kontragent>())
                    if (k.Name.ToUpper().Contains(SearchText.ToUpper())
                        || ((IName)k.Currency).Name.ToUpper().Contains(SearchText.ToUpper())
                        || (k.ResponsibleEmployee != null && ((IName)k.ResponsibleEmployee).Name.ToUpper()
                            .Contains(SearchText.ToUpper()))
                        || (k.FullName != null && k.FullName.ToUpper().Contains(SearchText.ToUpper()))
                        || (k.INN != null && k.INN.ToUpper().Contains(SearchText.ToUpper())))
                        if (Currency == null)
                        {
                            KontragentCollection.Add(new Kontragent
                            {
                                DocCode = k.DocCode,
                                Name = k.Name,
                                Currency = k.Currency,
                                ResponsibleEmployee = k.ResponsibleEmployee,
                                FullName = k.FullName,
                                IsBalans = k.IsBalans,
                                INN = k.INN,
                                IsDeleted = k.IsDeleted,
                                Notes = k.Notes
                            });
                        }
                        else
                        {
                            if (((IName)k.Currency).Name == Currency?.Name)
                                KontragentCollection.Add(new Kontragent
                                {
                                    DocCode = k.DocCode,
                                    Name = k.Name,
                                    Currency = k.Currency,
                                    ResponsibleEmployee = k.ResponsibleEmployee,
                                    FullName = k.FullName,
                                    IsBalans = k.IsBalans,
                                    INN = k.INN,
                                    IsDeleted = k.IsDeleted,
                                    Notes = k.Notes
                                });
                        }

                RaisePropertyChanged(nameof(KontragentCollection));
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }
    }
}
