﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using Core.EntityViewModel.CommonReferences;
using KursDomain.WindowsManager.WindowsManager;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.LayoutControl;
using Helper;
using KursAM2.ViewModel.Finance;
using KursDomain;
using KursDomain.ICommon;
using KursDomain.References;
using LayoutManager;

namespace KursAM2.View.Finance
{
    /// <summary>
    ///     Interaction logic for BankCurrencyChangeView.xaml
    /// </summary>
    public partial class BankCurrencyChangeView : ILayout
    {
        public PopupCalcEdit Sumordcont = new PopupCalcEdit();

        public BankCurrencyChangeView()
        {
            InitializeComponent();
            
            LayoutManager = new LayoutManager.LayoutManager(GlobalOptions.KursSystem(), GetType().Name, this, layoutItems);
            Loaded += BankCurrencyChangeView_Loaded;
            Unloaded += BankCurrencyChangeView_Unloaded;
            MinWidth = 1000;
            if (DataContext is BankCurrencyChangeWindowViewModel ctx)
                ctx.WindowName = "Обмен валюты";
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }

        public void SaveLayout()
        {
            LayoutManager.Save();
        }

        private void BankCurrencyChangeView_Unloaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Save();
        }

        private void BankCurrencyChangeView_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
        }

        private void LayoutItems_OnAutoGeneratedUI(object sender, EventArgs e)
        {
            foreach (var item in WindowHelper.GetLogicalChildCollection<MemoEdit>(layoutItems))
                if (item.EditValue != null && !string.IsNullOrWhiteSpace((string)item.EditValue))
                    item.Height = 80;
        }

        private void LayoutItems_OnAutoGeneratingItem(object sender, DataLayoutControlAutoGeneratingItemEventArgs e)
        {
            var ctx = DataContext as BankCurrencyChangeWindowViewModel;
            var doc = ctx?.Document;
            if (doc == null) return;
            ctx.WindowName =
                $"Банковский обмен валюты {doc.DocDate.ToShortDateString()} из {doc.BankFrom} в {doc.BankTo} ";
            var oldContent = e.Item.Content as BaseEdit;
            if (e.PropertyType == typeof(decimal) || e.PropertyType == typeof(decimal?))
            {
                var newContent = new PopupCalcEdit
                {
                    DisplayFormatString = "n2",
                    MaskUseAsDisplayFormat = true
                };
                BindingHelper.CopyBinding(oldContent, newContent, BaseEdit.EditValueProperty);
                e.Item.Content = newContent;
            }

            switch (e.PropertyName)
            {
                case nameof(doc.SummaFrom):
                    e.Item.Width = 300;
                    e.Item.HorizontalAlignment = HorizontalAlignment.Left;
                    break;
                case nameof(doc.CurrencyFrom):
                    e.Item.Width = 50;
                    e.Item.HorizontalAlignment = HorizontalAlignment.Left;
                    break;
                case nameof(doc.SummaTo):
                    e.Item.Width = 300;
                    e.Item.HorizontalAlignment = HorizontalAlignment.Left;
                    break;
                case nameof(doc.CurrencyTo):
                    e.Item.Width = 50;
                    e.Item.HorizontalAlignment = HorizontalAlignment.Left;
                    break;
                case nameof(doc.BankFrom):
                    e.Item.HorizontalAlignment = HorizontalAlignment.Left;
                    e.Item.HorizontalContentAlignment = HorizontalAlignment.Left;
                    e.Item.Width = 800;
                    break;
                case nameof(doc.Rate):
                    e.Item.HorizontalAlignment = HorizontalAlignment.Left;
                    e.Item.Width = 300;
                    break;
                case nameof(doc.BankTo):
                    if (doc.State == RowStatus.NewRow)
                    {
                        var cb1 = ViewFluentHelper.SetComboBoxEdit(e.Item, doc.BankTo, "BankTo",
                            GlobalOptions.ReferencesCache.GetBankAccountAll().Where(_ =>
                                ((IDocCode)_.Bank).DocCode == ((IDocCode)doc.BankFrom.Bank).DocCode
                                && ((IDocCode)_.Currency).DocCode != doc.CurrencyFrom?.DocCode).ToList());
                        cb1.EditValueChanged += Cb_BankToValueChanged;
                    }
                    else
                    {
                        var cb1 = ViewFluentHelper.SetComboBoxEdit(e.Item, doc.BankTo, "BankTo",
                            new List<BankAccount>
                            {
                                GlobalOptions.ReferencesCache.GetBankAccount(doc.BankToDC) as BankAccount
                            });
                        cb1.EditValueChanged += Cb_BankToValueChanged;
                    }

                    e.Item.HorizontalAlignment = HorizontalAlignment.Left;
                    e.Item.HorizontalContentAlignment = HorizontalAlignment.Left;
                    e.Item.Width = 800;
                    break;
                case nameof(doc.CREATOR):
                    e.Item.IsReadOnly = true;
                    e.Item.HorizontalAlignment = HorizontalAlignment.Right;
                    e.Item.Width = 200;
                    break;
                case nameof(doc.State):
                    e.Item.IsReadOnly = true;
                    e.Item.IsEnabled = false;
                    e.Item.HorizontalAlignment = HorizontalAlignment.Right;
                    e.Item.Width = 200;
                    break;
                case nameof(doc.DocDate):
                    e.Item.HorizontalAlignment = HorizontalAlignment.Left;
                    break;
                case nameof(doc.Note):
                    ViewFluentHelper.SetDefaultMemoEdit(e.Item);
                    break;
            }

            ViewFluentHelper.SetModeUpdateProperties(doc, e.Item, e.PropertyName);
        }

        private void Cb_BankToValueChanged(object sender, EditValueChangedEventArgs e)
        {
            if (!(DataContext is BankCurrencyChangeWindowViewModel ctx)) return;
            if (ctx.Document?.State == RowStatus.NotEdited) return;
            // ReSharper disable once UnusedVariable
            var doc = ctx.Document;
            if (ctx.Document != null && ctx.Document.State != RowStatus.NewRow)
            {
                WindowManager.ShowMessage("Документ уже проведен. Изменить банк получатель нельзя",
                    "Предупреждение", MessageBoxImage.Information);
                return;
            }

            using (var dbctx = GlobalOptions.GetEntities())
            {
                try
                {
                    var data = dbctx.TD_101
                        .Include(_ => _.SD_101)
                        .Where(_ => _.SD_101.VV_ACC_DC == (decimal)e.NewValue)
                        .Select(_ => _.VVT_CRS_DC).Distinct().ToList();
                    if (data.Count > 0)
                    {
                        if (ctx.Document != null)
                        {
                            var bankTo = GlobalOptions.ReferencesCache.GetBankAccount(ctx.Document.BankToDC);
                            ctx.Document.CurrencyTo = bankTo.Currency as Currency;
                            var rates = CurrencyRate.GetRate(ctx.Document.DocDate);
                            if (ctx.Document.CurrencyFrom != null && ctx.Document.CurrencyTo != null)
                                ctx.Document.Rate = CurrencyRate.GetCBSummaRate(ctx.Document.CurrencyFrom,
                                    ctx.Document.CurrencyTo, rates);
                            else
                                ctx.Document.Rate = 1;

                            if (ctx.Document.CrsToDC == GlobalOptions.SystemProfile.NationalCurrency.DocCode)
                                ctx.Document.SummaTo =
                                    ctx.Document.Rate == 0
                                        ? 0
                                        : decimal.Round((decimal)(ctx.Document.SummaFrom * ctx.Document.Rate), 2);
                            else
                                ctx.Document.SummaTo =
                                    ctx.Document.Rate == 0
                                        ? 0
                                        : decimal.Round((decimal)(ctx.Document.SummaFrom / ctx.Document.Rate), 2);
                        }
                    }
                    else
                    {
                        if (ctx.Document.CurrencyTo == null)
                            ctx.Document.CurrencyTo = ctx.Document.BankTo.Currency as Currency;
                        var rates = CurrencyRate.GetRate(ctx.Document.DocDate);
                        if (ctx.Document.CurrencyFrom != null && ctx.Document.CurrencyTo != null)
                            ctx.Document.Rate = CurrencyRate.GetCBSummaRate(ctx.Document.CurrencyFrom,
                                ctx.Document.CurrencyTo, rates);
                        else
                            ctx.Document.Rate = 1;

                        if (ctx.Document.CrsToDC == GlobalOptions.SystemProfile.NationalCurrency.DocCode)
                            ctx.Document.SummaTo =
                                ctx.Document.Rate == 0
                                    ? 0
                                    : decimal.Round((decimal)(ctx.Document.SummaFrom * ctx.Document.Rate), 2);
                        else
                            ctx.Document.SummaTo =
                                ctx.Document.Rate == 0
                                    ? 0
                                    : decimal.Round((decimal)(ctx.Document.SummaFrom / ctx.Document.Rate), 2);
                    }
                }
                catch (Exception ex)
                {
                    WindowManager.ShowError(ex);
                }
            }
        }
    }
}
