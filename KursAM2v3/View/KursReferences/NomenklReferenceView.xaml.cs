﻿using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DevExpress.Utils;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
using KursAM2.ViewModel.Reference.Nomenkl;
using KursDomain;
using KursDomain.Documents.NomenklManagement;
using KursDomain.ICommon;
using KursDomain.References;
using LayoutManager;

namespace KursAM2.View.KursReferences
{
    /// <summary>
    ///     Interaction logic for NomenklReferenceView.xaml
    /// </summary>
    public partial class NomenklReferenceView : ILayout
    {
        public NomenklReferenceView()
        {
            InitializeComponent();
            
            LayoutManager = new LayoutManager.LayoutManager(GlobalOptions.KursSystem(),GetType().Name, this, mainLayoutControl);
            Loaded += NomenklReferenceView_Loaded;
            Closing += NomenklReferenceView_Closing;
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }

        public void SaveLayout()
        {
            LayoutManager.Save();
        }

        private void NomenklReferenceView_Closing(object sender, CancelEventArgs e)
        {
            LayoutManager.Save();
        }

        private void NomenklReferenceView_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
        }

        //private void MenuItemEdit_OnClick(object sender, RoutedEventArgs e)
        //{
        //    tcol_1.AllowEditing = DefaultBoolean.True;
        //    treeListViewCategory.ShowEditor(false);
        //}

        private void treeListViewCategory_FocusedRowChanged(object sender,
            CurrentItemChangedEventArgs currentItemChangedEventArgs)
        {
            tcol_1.AllowEditing = DefaultBoolean.False;
        }

        private void TreeListViewCategory_OnMouseDoubleClick(object sender,
            MouseButtonEventArgs mouseButtonEventArgs)
        {
            tcol_1.AllowEditing = DefaultBoolean.True;
            treeListViewCategory.ShowEditor(false);
        }

        private void treeListViewCategory_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!(Keyboard.FocusedElement is TextBox)) return;
            if (((e.Key == Key.Right &&
                  ((TextBox) Keyboard.FocusedElement).CaretIndex == ((TextBox) Keyboard.FocusedElement).Text.Length) ||
                 (e.Key == Key.Left &&
                  ((TextBox) Keyboard.FocusedElement).CaretIndex == 0)) &&
                Equals(treeListCategory.CurrentColumn, treeListCategory.Columns[treeListCategory.Columns.Count - 1]))
                e.Handled = true;
        }

        private void GridControlNomenklMain_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var ctx = DataContext as ReferenceWindowViewModel;
            ctx?.NomenklMainEdit(ctx.CurrentNomenklMain.Id);
        }

        private void tableViewNomenkl_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName != "Currency") return;
            if (!(e.Row is Nomenkl r)) return;
            if (r.NomenklNumber.Length <= 5 ||
                r.NomenklNumber.Substring(r.NomenklNumber.Length - 4, 3) != ((IName)r.Currency).Name)
                r.NomenklNumber += " " + ((IName)r.Currency).Name;
        }
    }
}
