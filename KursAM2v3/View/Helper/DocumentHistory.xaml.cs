using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.ViewModel.Common;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.LayoutControl;
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
        }

        private void GridControl_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.ReadOnly = true;
        }
    }

    public static class DocumentHistoryManager
    {
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
                            .OrderBy(_ => _.Date)
                            .ToList()
                            .Select(d => new DocHistoryViewModel(d)));
                        break;
                    case DocumentType.CashIn:
                        if (dc == null) return;
                        hdoclist.AddRange(ctx.DocHistory.Where(_ => _.DocDC == dc)
                            .OrderBy(_ => _.Date)
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
            var data = deserializeToDictionary(json);
            var arr = (JArray) data["Document"];
            var doc = arr[0];
            var docdata = deserializeToDictionary(doc.ToString());
            foreach (var key in docdata.Keys.Where(_ => _ != "Rows"))
            {
                var newItem = new DataLayoutItem
                {
                    Label = key, Content = new TextEdit
                    {
                        Text = Convert.ToString(docdata[key]),
                        IsReadOnly = true
                    }
                };
                form.MainLayoutControl.Children.Add(newItem);
            }

            if (docdata.ContainsKey("Rows"))
            {
                var rows = (JArray) docdata["Rows"];
                form.gridControl.ItemsSource = JsonConvert.DeserializeObject(rows.ToString());
            }
            form.Show();
        }

        private static Dictionary<string, object> deserializeToDictionary(string jo)
        {
            var values = JsonConvert.DeserializeObject<Dictionary<string, object>>(jo);
            var values2 = new Dictionary<string, object>();
            if (values != null)
                foreach (var d in values)
                {
                    var fullName = d.Value.GetType().FullName;
                    if (fullName != null && d.Value != null && fullName.Contains("Newtonsoft.Json.Linq.JObject"))
                        values2.Add(d.Key, deserializeToDictionary(d.Value.ToString()));
                    else
                        values2.Add(d.Key, d.Value);
                }

            return values2;
        }
    }
}