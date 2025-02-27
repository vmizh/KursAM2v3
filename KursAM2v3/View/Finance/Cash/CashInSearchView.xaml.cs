﻿using System.ComponentModel;
using System.Windows;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using KursDomain;
using LayoutManager;

namespace KursAM2.View.Finance.Cash
{
    /// <summary>
    ///     Interaction logic for CashInSearchView.xaml
    /// </summary>
    public partial class CashInSearchView //: ILayout
    {
        public CashInSearchView()
        {
            InitializeComponent(); 
            
            // LayoutManager = new LayoutManager.LayoutManager(GlobalOptions.KursSystem(),GetType().Name, this, mainLayoutControl);
            // Loaded += CashInSearchView_Loaded;
            // Closing += CashInSearchView_Closing;
        }

        private void CashInSearchView_Closing(object sender, CancelEventArgs e)
        {
            LayoutManager.Save();
        }

        private void CashInSearchView_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
        }

        private void GridControlSearch_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
            e.Column.ReadOnly = true;
        }

        private void GridControlSearch_OnAutoGeneratedColumns(object sender, RoutedEventArgs e)
        {
            foreach (var c in gridControlSearch.Columns)
            {
                if(c.EditSettings == null) c.EditSettings = new TextEditSettings
                {
                    SelectAllOnMouseUp = true
                };
            }
            foreach (var col in gridControlSearch.Columns)
            {
                if (col.FieldType == typeof(decimal) || col.FieldType == typeof(decimal?))
                    col.EditSettings = new CalcEditSettings
                    {
                        AllowDefaultButton = false,
                        DisplayFormat = "n2"
                    };
                switch (col.FieldName)
                {
                    case "NOTES_ORD":
                    case "OSN_ORD":
                    case "NAME_ORD":
                    case "SFactName":
                        col.EditSettings = new MemoEditSettings
                        {
                            ShowIcon = false
                        };
                        break;
                }
            }
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }
        public void SaveLayout()
        {
            LayoutManager.Save();
        }

        public void ResetLayot()
        {
            throw new System.NotImplementedException();
        }
    }
}
