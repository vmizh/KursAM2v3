﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using Core.ViewModel.Base;
using Core.WindowsManager;
using KursAM2.Managers;
using KursDomain;
using KursDomain.ICommon;
using KursDomain.References;
using KursDomain.References.RedisCache;

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
            //LoadKontragentFromReference();
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

        private void LoadKontragentFromReference()
        {
            while (((RedisCacheReferences)GlobalOptions.ReferencesCache).isNomenklCacheLoad)
                Thread.Sleep(new TimeSpan(0, 0, 5));
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
                {
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
