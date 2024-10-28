using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Core.ViewModel.Base;
using Core.WindowsManager;
using KursAM2.Managers;
using KursDomain;
using KursDomain.ICommon;
using KursDomain.References;
using KursDomain.References.RedisCache;
using ServiceStack.Redis;

namespace KursAM2.View.DialogUserControl
{
    public class StockHolderSelectDialog : RSWindowViewModelBase, IDataUserControl
    {
        public DependencyObject LayoutControl { get; }
    }

    public class KontragentSelectDialog : RSWindowViewModelBase, IDataUserControl
    {
        private readonly bool? IsBalans;

        // ReSharper disable once NotAccessedField.Local
        private readonly bool isDateCheck;

        // ReSharper disable once NotAccessedField.Local
        private readonly decimal kontrDC;
        private Currency myCurrency;
        private Kontragent myCurrentKontragent;
        private KontragentSelectDialogUC myDataUserControl;

        public KontragentSelectDialog(Currency crs, bool isDateCheck = false)
        {
            this.isDateCheck = isDateCheck;
            Currency = crs;
            LayoutControl =
                myDataUserControl = new KontragentSelectDialogUC();
            // ReSharper disable once VirtualMemberCallInConstructor
            WindowName = "Выбор контрагента";
            LoadKontragentFromReference();
        }

        public KontragentSelectDialog(Currency crs, bool isDateCheck = false, bool? isBalans = true)
        {
            this.isDateCheck = isDateCheck;
            IsBalans = isBalans;
            Currency = crs;
            LayoutControl =
                myDataUserControl = new KontragentSelectDialogUC();
            // ReSharper disable once VirtualMemberCallInConstructor
            WindowName = "Выбор контрагента";
            LoadKontragentFromReference();
        }

        public KontragentSelectDialog(decimal kontrDC)
        {
            this.kontrDC = kontrDC;
            LayoutControl =
                myDataUserControl = new KontragentSelectDialogUC();
            // ReSharper disable once VirtualMemberCallInConstructor
            WindowName = "Выбор контрагента";
            LoadKontragentFromReference();
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

        private void loadAllKontragent()
        {
            if (GlobalOptions.ReferencesCache is not RedisCacheReferences) return;
            while (((RedisCacheReferences)GlobalOptions.ReferencesCache).isStartLoad)
            {
                Thread.Sleep(3000);
            }
            RedisManagerPool redisManager =
                new RedisManagerPool(ConfigurationManager.AppSettings["redis.connection"]);
            using (var redisClient = redisManager.GetClient())
            {
                redisClient.Db = GlobalOptions.RedisDBId ?? 0;
                var redis = redisClient.As<Kontragent>();
                var allKeys = redisClient.GetKeysByPattern("Cache:Kontragent:*").ToList();
                var kontrs = ((RedisCacheReferences)GlobalOptions.ReferencesCache).Kontragents;
                var keys = (from k in allKeys
                    let dc = Convert.ToDecimal(k.Split('@')[0].Split(':')[2])
                    where !kontrs.ContainsKey(dc) &&
                          ((ICache)kontrs[dc]).UpdateDate != Convert.ToDateTime(k.Split('@')[1])
                    select k).ToList();
                if (!keys.Any()) return;
                using (var pipe = redis.CreatePipeline())
                {
                    
                    foreach (var key in keys)
                        pipe.QueueCommand(r => r.GetValue(key),
                            x =>
                            {
                                x.LoadFromCache();
                                if (kontrs.ContainsKey(x.DocCode))
                                    kontrs[x.DocCode] = x;
                                else kontrs.Add(x.DocCode, x);
                            });

                    pipe.Flush();
                }
            }
        }

        private void LoadKontragentFromReference()
        {
            //loadAllKontragent();
            var KontrList = new List<Kontragent>();
            try
            {
                KontrList.Clear();
                if (Currency == null)
                    foreach (var k in KontragentManager.GetAllKontragentSortedByUsed())
                    {
                        if (k.IsDeleted) continue;
                        KontrList.Add(new Kontragent
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
                            Notes = k.Notes,
                            StartBalans = k.StartBalans
                        };
                        KontrList.Add(kontr);
                    }

                KontragentCollection = IsBalans != null
                    ? IsBalans.Value
                        ? new ObservableCollection<Kontragent>(KontrList.Where(_ => _.IsBalans))
                        : new ObservableCollection<Kontragent>(KontrList.Where(_ => _.IsBalans == false))
                    : new ObservableCollection<Kontragent>(KontrList);
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
                foreach (var k in GlobalOptions.ReferencesCache.GetKontragentsAll().Cast<Kontragent>())
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
                        Notes = k.Notes,
                        StartBalans = k.StartBalans
                    });
                }

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

        protected override void OnWindowLoaded(object obj)
        {
            base.OnWindowLoaded(obj);

        }

        //public override void ResetLayout(object form)
        //{
        //    myDataUserControl?.LayoutManager.ResetLayout();
        //}
    }
}
