using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.EntityViewModel.CommonReferences.Kontragent;
using Core.ViewModel.Base;
using Core.WindowsManager;
using KursAM2.Managers;
using KursDomain.Documents.CommonReferences.Kontragent;

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
            while (!MainReferences.IsReferenceLoadComplete)
            {
            }

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
            while (!MainReferences.IsReferenceLoadComplete)
            {
            }

            // ReSharper disable once VirtualMemberCallInConstructor
            WindowName = "Выбор контрагента";
            LoadKontragentFromReference();
        }

        public KontragentSelectDialog(decimal kontrDC)
        {
            this.kontrDC = kontrDC;
            LayoutControl =
                myDataUserControl = new KontragentSelectDialogUC();
            while (!MainReferences.IsReferenceLoadComplete)
            {
            }

            // ReSharper disable once VirtualMemberCallInConstructor
            WindowName = "Выбор контрагента";
            LoadKontragentFromReference();
        }

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
                if (myCurrentKontragent != null && myCurrentKontragent.Equals(value)) return;
                myCurrentKontragent = value;
                RaisePropertyChanged();
            }
        }

        public override bool IsCanSearch => !string.IsNullOrWhiteSpace(SearchText);

        public DependencyObject LayoutControl { get; }

        private void LoadKontragentFromReference()
        {
            List<Kontragent> KontrList = new List<Kontragent>();
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
                            BalansCurrency = k.BalansCurrency,
                            OtvetstLico = k.OtvetstLico,
                            FullName = k.FullName,
                            IsBalans = k.IsBalans,
                            INN = k.INN,
                            DELETED = k.DELETED ?? 0,
                            Note = k.Note
                        });
                    }
                else
                    foreach (var k in KontragentManager.GetAllKontragentSortedByUsed()
                        .Where(_ => _.BalansCurrency.Name == Currency.Name))
                    {
                        if (k.IsDeleted) continue;
                        var kontr = new Kontragent
                        {
                            DocCode = k.DocCode,
                            Name = k.Name,
                            BalansCurrency = k.BalansCurrency,
                            OtvetstLico = k.OtvetstLico,
                            FullName = k.FullName,
                            IsBalans = k.IsBalans,
                            INN = k.INN,
                            DELETED = k.DELETED ?? 0,
                            Note = k.Note
                        };
                        KontrList.Add(kontr);
                    }

                KontragentCollection = IsBalans != null
                    ? IsBalans.Value ? new ObservableCollection<Kontragent>(KontrList.Where(_ => _.IsBalans)) :
                    new ObservableCollection<Kontragent>(KontrList.Where(_ => _.IsBalans == false))
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
                            BalansCurrency = k.BalansCurrency,
                            OtvetstLico = k.OtvetstLico,
                            FullName = k.FullName,
                            IsBalans = k.IsBalans,
                            INN = k.INN,
                            DELETED = k.DELETED ?? 0
                        });
                    }
                else
                    foreach (var k in KontragentManager.GetAllKontragentSortedByUsed()
                        .Where(_ => _.BalansCurrency.Name == Currency.Name))
                    {
                        if (k.IsDeleted) continue;
                        var kontr = new Kontragent
                        {
                            DocCode = k.DocCode,
                            Name = k.Name,
                            BalansCurrency = k.BalansCurrency,
                            OtvetstLico = k.OtvetstLico,
                            FullName = k.FullName,
                            IsBalans = k.IsBalans,
                            INN = k.INN,
                            DELETED = k.DELETED ?? 0
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
            LoadKontragentFromReference();
        }

        public override void Search(object obj)
        {
            KontragentCollection.Clear();
            try
            {
                foreach (var k in MainReferences.GetAllKontragents().Values)
                    if (k.Name.ToUpper().Contains(SearchText.ToUpper())
                        || k.BalansCurrency.Name.ToUpper().Contains(SearchText.ToUpper())
                        || k.OtvetstLico != null && k.OtvetstLico.Name.ToUpper().Contains(SearchText.ToUpper())
                        || k.FullName != null && k.FullName.ToUpper().Contains(SearchText.ToUpper())
                        || k.INN != null && k.INN.ToUpper().Contains(SearchText.ToUpper()))
                        if (Currency == null)
                        {
                            KontragentCollection.Add(new Kontragent
                            {
                                Name = k.Name,
                                DocCode = k.DocCode,
                                BalansCurrency = k.BalansCurrency,
                                OtvetstLico = k.OtvetstLico,
                                FullName = k.FullName,
                                IsBalans = k.IsBalans,
                                INN = k.INN,
                                DELETED = k.DELETED ?? 0
                            });
                        }
                        else
                        {
                            if (k.BalansCurrency.Name == Currency?.Name)
                                KontragentCollection.Add(new Kontragent
                                {
                                    Name = k.Name,
                                    DocCode = k.DocCode,
                                    BalansCurrency = k.BalansCurrency,
                                    OtvetstLico = k.OtvetstLico,
                                    FullName = k.FullName,
                                    IsBalans = k.IsBalans,
                                    INN = k.INN,
                                    DELETED = k.DELETED ?? 0
                                });
                        }

                RaisePropertyChanged(nameof(KontragentCollection));
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        public override void ResetLayout(object form)
        {
            myDataUserControl?.LayoutManager.ResetLayout();
        }
    }
}
