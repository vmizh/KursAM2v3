﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
using Helper;
using KursAM2.ViewModel.Management.ManagementBalans;
using LayoutManager;
using WindowsScreenState = LayoutManager.WindowsScreenState;

namespace KursAM2.View.Management
{
    /// <summary>
    ///     Interaction logic for ManagementBalansView.xaml
    /// </summary>
    public partial class ManagementBalansView 
    {
        public ManagementBalansView()
        {
            InitializeComponent(); 
            if (DataContext is ManagementBalansWindowViewModel ctx)
                ctx.CurrentDate = DateTime.Today;
            DataContextChanged += ManagementBalansView_DataContextChanged;
        }

        static ManagementBalansView()
        {
            GridControlLocalizer.Active = new CustomDXGridLocalizer();
        }

        //public LayoutManager.LayoutManager LayoutManager { get; set; }
        //public string LayoutManagerName { get; set; }
        //public void SaveLayout()
        //{
        //    LayoutManager.Save();
        //}

        private void ManagementBalansView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is ManagementBalansWindowViewModel ctx)
                // ReSharper disable once PossibleNullReferenceException
                Dispatcher.BeginInvoke((Action) (() => rateCrsRecalc.EditValue =
                    ctx.CurrenciesForRecalc.First(_ => _.Name == "RUR" || _.Name == "RUB")));
        }

        private void GridRate_OnAutoGeneratedColumns(object sender, RoutedEventArgs e)
        {
            var delcol = new List<GridColumn>();
            foreach (var col in gridRate.Columns)
                if (col.FieldName == "Name" || col.FieldName == "Note")
                    delcol.Add(col);
            foreach (var col in delcol)
                gridRate.Columns.Remove(col);
        }

        #region Nested type: FormInfo

        [DataContract]
        internal class FormInfo : WindowsScreenState
        {
            [DataMember] public DateTime Current { set; get; }
            [DataMember] public DateTime Comhare { set; get; }
        }

        #endregion
    }

    public class ColumnRowIndexesCellValueConverter : MarkupExtension, IMultiValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == null) return Brushes.Transparent;
            var columnIndex = ((TreeListColumn) values[0]).VisibleIndex;
            var rowIndex = (int) values[1];
            if (columnIndex == 1 && rowIndex == 1)
                return Brushes.Blue;
            return Brushes.Transparent;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
