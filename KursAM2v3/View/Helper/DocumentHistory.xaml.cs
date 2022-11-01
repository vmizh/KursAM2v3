using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.WindowsManager;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.LayoutControl;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.Systems;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KursAM2.View.Helper
{
    /// <summary>
    ///     Interaction logic for DocumentHistory.xaml
    /// </summary>
    public partial class DocumentHistory
    {
        public DocumentHistory()
        {
            InitializeComponent(); 
            ApplicationThemeHelper.ApplicationThemeName = Theme.MetropolisLightName;
        }

        private void GridControl_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
            e.Column.ReadOnly = true;
            switch (e.Column.Name)
            {
                case "Name":
                case "Note":
                    e.Column.Visible = false;
                    break;
            }
        }
    }

    public static class DocumentHistoryManager
    {
        public static void LoadHistory(decimal dc)
        {
            var hdoclist = new List<DocHistoryViewModel>();
            using (var ctx = GlobalOptions.GetEntities())
            {
                hdoclist.AddRange(ctx.DocHistory.Where(_ => _.DocDC == dc)
                    .OrderByDescending(_ => _.Date)
                    .ToList()
                    .Select(d => new DocHistoryViewModel(d)));
            }
            var form = new DocHistoryList
            {
                Owner = Application.Current.MainWindow,
                DataContext = new DocHistoryWindowViewModel(hdoclist)
            };
            form.Show();
        }

        public static void LoadHistory(Guid id)
        {
            var hdoclist = new List<DocHistoryViewModel>();
            using (var ctx = GlobalOptions.GetEntities())
            {
                hdoclist.AddRange(ctx.DocHistory.Where(_ => _.DocId == id)
                    .OrderByDescending(_ => _.Date)
                    .ToList()
                    .Select(d => new DocHistoryViewModel(d)));
            }
            var form = new DocHistoryList
            {
                Owner = Application.Current.MainWindow,
                DataContext = new DocHistoryWindowViewModel(hdoclist)
            };
            form.Show();
        }

        public static void LoadHistory(decimal dc, int code)
        {
            var hdoclist = new List<DocHistoryViewModel>();
            using (var ctx = GlobalOptions.GetEntities())
            {
                hdoclist.AddRange(ctx.DocHistory.Where(_ => _.DocDC == dc && _.Code == code)
                    .OrderByDescending(_ => _.Date)
                    .ToList()
                    .Select(d => new DocHistoryViewModel(d)));
            }
            var form = new DocHistoryList
            {
                Owner = Application.Current.MainWindow,
                DataContext = new DocHistoryWindowViewModel(hdoclist)
            };
            form.Show();
        }

        public static void LoadHistory(DocumentType type, Guid? id = null, decimal? dc = null, int? code = null)
        {
            if (id == null && dc == null && code == null) return;
            var hdoclist = new List<DocHistoryViewModel>();
            using (var ctx = GlobalOptions.GetEntities())
            {
                switch (type)
                {
                    case DocumentType.Bank:
                        if (dc == null || code == null) return;
                        hdoclist.AddRange(ctx.DocHistory.Where(_ => _.DocDC == dc && _.Code == code)
                            .OrderByDescending(_ => _.Date)
                            .ToList()
                            .Select(d => new DocHistoryViewModel(d)));
                        break;
                    case DocumentType.DogovorOfSupplier:
                    case DocumentType.AktSpisaniya:
                    case DocumentType.AccruedAmountOfSupplier:
                        if (id == null) return;
                        hdoclist.AddRange(ctx.DocHistory.Where(_ => _.DocId == id)
                            .OrderByDescending(_ => _.Date)
                            .ToList()
                            .Select(d => new DocHistoryViewModel(d)));
                        break;
                    default:
                        if (dc == null) return;
                        hdoclist.AddRange(ctx.DocHistory.Where(_ => _.DocDC == dc)
                            .OrderByDescending(_ => _.Date)
                            .ToList()
                            .Select(d => new DocHistoryViewModel(d)));
                        break;
                }
            }

            var form = new DocHistoryList
            {
                Owner = Application.Current.MainWindow,
                DataContext = new DocHistoryWindowViewModel(hdoclist)
            };
            form.Show();
        }

        public static void ShowDocument(string json)
        {
            var form = new DocumentHistory();
            try
            {
                var jdata = (JObject)JsonConvert.DeserializeObject(json);
                if (jdata == null) return;
                foreach (var p in jdata.Properties())
                {
                    if (p.Name == "Позиции") continue;
                    var newItem = new DataLayoutItem
                    {
                        Label = p.Name.Replace("_"," "), Content = new TextEdit
                        {
                            Text = Convert.ToString(p.Value),
                            IsReadOnly = true
                        }
                    };
                    form.MainLayoutControl.Children.Add(newItem);
                }

                var rows = jdata.Property("Позиции");
                if (rows == null)
                {
                    form.gridControl.Visibility = Visibility.Hidden;
                    form.LayoutTable.Height = 20;
                }
                else
                    form.gridControl.ItemsSource = rows.Value;
                
                form.Show();
            }
            catch (Exception ex)
            {
                WindowManager.ShowDBError(ex);
            }
        }

        public static DataTable Tabulate(string json)
        {
            var jsonLinq = JObject.Parse(json);

            // Find the first array using Linq
            var srcArray = jsonLinq.Descendants().First(d => d is JArray);
            var trgArray = new JArray();
            foreach (var row in srcArray.Children<JObject>())
            {
                var cleanRow = new JObject();
                foreach (var column in row.Properties())
                    // Only include JValue types
                    if (column.Value is JValue)
                        cleanRow.Add(column.Name, column.Value);

                trgArray.Add(cleanRow);
            }

            return JsonConvert.DeserializeObject<DataTable>(trgArray.ToString());
        }
    }
}
