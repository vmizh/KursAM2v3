using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Xml;
using Data;
using DevExpress.Data;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.LayoutControl;
using Helper;

namespace LayoutManager
{
    [DataContract(Name = "GridLayout")]
    public class ControlLayoutInfo
    {
        [DataMember]
        public double Width { set; get; } = double.NaN;
        [DataMember]
        public double Height { set; get; } = double.NaN;
        [DataMember]
        public LayoutGridColumnItems ColumnsInfo { set; get; } = new LayoutGridColumnItems();
    }

    public class LayoutManagerGridAutoGenerationColumns : LayoutManagerBase
    {
        public LayoutManagerGridAutoGenerationColumns(string fname, DataControlBase ctrl)
        {
            FileName = fname;
            Win = null;
            LayoutControl = ctrl;
        }

        public LayoutManagerGridAutoGenerationColumns(string fname, LayoutGroup ctrl)
        {
            FileName = fname;
            Win = null;
            LayoutControl = ctrl;
        }

        public override void Save()
        {
            string LayoutControlName = null;
            var ctrlLayout = new ControlLayoutInfo();
            switch (LayoutControl)
            {
                case GridControl ctrl1:
                    LayoutControlName = ctrl1.Name;
                    ctrlLayout.Height = ctrl1.ActualHeight;
                    ctrlLayout.Width = ctrl1.ActualWidth;
                    foreach (var col in ctrl1.Columns)
                        ctrlLayout.ColumnsInfo.Add(new LayoutGridColumnItem
                        {
                            Order = col.VisibleIndex,
                            Width = col.ActualWidth,
                            FieldName = col.FieldName,
                            IsReadOnly = col.ReadOnly,
                            IsVisible = col.Visible
                        });
                    break;
                case TreeListControl ctrl:
                    LayoutControlName = ctrl.Name;
                    ctrlLayout.Height = ctrl.ActualHeight;
                    ctrlLayout.Width = ctrl.ActualWidth;
                    foreach (var col in ctrl.Columns)
                        ctrlLayout.ColumnsInfo.Add(new LayoutGridColumnItem
                        {
                            Order = col.VisibleIndex,
                            Width = col.ActualWidth,
                            FieldName = col.FieldName,
                            IsReadOnly = col.ReadOnly,
                            IsVisible = col.Visible
                        });
                    break;
                case LayoutGroup ctrl:
                    LayoutControlName = ctrl.Name;
                    ctrlLayout.Height = ctrl.ActualHeight;
                    ctrlLayout.Width = ctrl.ActualWidth;
                    break;
            }
            var ser = new DataContractSerializer(typeof(ControlLayoutInfo));
            var sb = new StringBuilder();
            using (var writer = XmlWriter.Create(sb))
            {
                ser.WriteObject(writer, ctrlLayout);
                writer.Flush();
            }
            var connString = new SqlConnectionStringBuilder
            {
                DataSource = "172.16.1.1",
                InitialCatalog = "KursSystem",
                UserID = "KursUser",
                Password = "KursUser"
            }.ToString();
            using (var ctx = new KursSystemEntities(connString))
            {
                if (CurrentUser.UserInfo == null) return;
                var w = Win != null ? Win.GetType().Name : FileName;
                var l = ctx.FormLayout.FirstOrDefault(_ => _.UserId == CurrentUser.UserInfo.KursId
                                                           && _.FormName == w && _.ControlName == LayoutControlName);
                try
                {
                    if (l == null)
                    {
                        ctx.FormLayout.Add(new FormLayout
                        {
                            Id = Guid.NewGuid(),
                            UpdateDate = DateTime.Now,
                            UserId = CurrentUser.UserInfo.KursId,
                            FormName = w,
                            ControlName = LayoutControlName,
                            Layout = sb.ToString()
                        });
                    }
                    else
                    {
                        l.UpdateDate = DateTime.Now;
                        l.UserId = CurrentUser.UserInfo.KursId;
                        l.FormName = w;
                        l.ControlName = LayoutControlName;
                        l.Layout = sb.ToString();
                    }
                    ctx.SaveChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка сохранения разметки {w} / {FileName}" + $"{ex}");
                }
            }
//            try
//            {
//                var writer = new FileStream($"{AppDataPath}\\{FileName}.{LayoutControlName}.xml", FileMode.Create);
//                var ser =
//                    new DataContractSerializer(typeof(ControlLayoutInfo));
//                ser.WriteObject(writer, ctrlLayout);
//                writer.Close();
//            }
//#pragma warning disable 168
//            catch (Exception ex)
//#pragma warning restore 168
//            {
//                //WindowManager.ShowError(ex);
//            }
        }

        public void AutoGeneratedColumnSetProperties(LayoutGridColumnItems columnsInfo, ColumnBase col,
            bool IsSummaryGenerate = true)
        {
            switch (LayoutControl)
            {
                case GridControl ctrl:
                    if (KursGridControlHelper.ColumnFieldTypeCheckDecimal(col.FieldType))
                    {
                        if (IsSummaryGenerate)
                        {
                            var summary = new GridSummaryItem
                            {
                                SummaryType = SummaryItemType.Sum,
                                ShowInColumn = col.FieldName,
                                DisplayFormat = "{0:n2}",
                                FieldName = col.FieldName
                            };
                            ctrl.TotalSummary.Add(summary);
                        }
                        col.EditSettings = new CalcEditSettings
                        {
                            DisplayFormat = "n2",
                            Name = col.FieldName + "Calc"
                        };
                    }
                    if (columnsInfo != null && columnsInfo.Count > 0)
                    {
                        var dcol = columnsInfo.FirstOrDefault(_ => _.FieldName == col.FieldName);
                        if (dcol != null)
                        {
                            col.VisibleIndex = dcol.Order;
                            col.Width = dcol.Width;
                            col.ReadOnly = dcol.IsReadOnly;
                            col.Visible = dcol.IsVisible;
                        }
                    }
                    break;
                case TreeListControl ctrl1:
                    if (KursGridControlHelper.ColumnFieldTypeCheckDecimal(col.FieldType))
                        if (IsSummaryGenerate)
                        {
                            var summary = new TreeListSummaryItem
                            {
                                SummaryType = SummaryItemType.Sum,
                                ShowInColumn = col.FieldName,
                                DisplayFormat = "{0:n2}",
                                FieldName = col.FieldName
                            };
                            ctrl1.TotalSummary.Add(summary);
                        }
                    if (columnsInfo != null && columnsInfo.Count > 0)
                    {
                        var dcol = columnsInfo.FirstOrDefault(_ => _.FieldName == col.FieldName);
                        if (dcol != null)
                        {
                            
                            col.VisibleIndex = dcol.Order;
                            col.Width = dcol.Width;
                            col.ReadOnly = dcol.IsReadOnly;
                            col.Visible = dcol.IsVisible;
                        }
                    }
                    break;
            }
        }

        public ControlLayoutInfo Load()
        {
            if (Helper.CurrentUser.UserInfo == null) return null;
            double height = double.NaN, width = double.NaN;
            string LayoutControlName = null;
            if (LayoutControl == null) return new ControlLayoutInfo();
            var ret = new ControlLayoutInfo();
            var isLayoutGroup = false;
            switch (LayoutControl)
            {
                case GridControl ctrl1:
                    LayoutControlName = ctrl1.Name;
                    //height = ctrl1.Height;
                    //width = ctrl1.Width;
                    break;
                case TreeListControl ctrl:
                    LayoutControlName = ctrl.Name;
                    //height = ctrl.Height;
                    //width = ctrl.Width;
                    break;
                case LayoutGroup ctrl2:
                    LayoutControlName = ctrl2.Name;
                    height = ctrl2.ActualHeight;
                    width = ctrl2.ActualWidth;
                    isLayoutGroup = true;
                    break;
            }
            if (File.Exists($"{AppDataPath}\\{FileName}.{LayoutControlName}.xml"))
            {
                using (var fs = File.OpenRead($"{AppDataPath}\\{FileName}.{LayoutControlName}.xml"))
                {
                    var r = XmlReader.Create(fs);
                    try
                    {
                        ret =
                            (ControlLayoutInfo) new DataContractSerializer(typeof(ControlLayoutInfo)).ReadObject(r);
                    }
#pragma warning disable 168
                    catch (Exception ex)
#pragma warning restore 168
                    {
                        //WindowManager.ShowError(ex, "Ошибка разметки.");
                    }
                } 
                File.Delete($"{AppDataPath}\\{FileName}.{LayoutControlName}.xml");
            }
            else
            {
                var connString = new SqlConnectionStringBuilder
                {
                    DataSource = "172.16.1.1",
                    InitialCatalog = "KursSystem",
                    UserID = "KursUser",
                    Password = "KursUser"
                }.ToString();
                string layoutData = null;
                using (var ctx = new KursSystemEntities(connString))
                {
                    var w = Win != null ? Win.GetType().Name : FileName;
                    try
                    {
                        var l = ctx.FormLayout.FirstOrDefault(_ => _.UserId == Helper.CurrentUser.UserInfo.KursId
                                                                            && _.FormName == w && _.ControlName == LayoutControlName);
                        if (l != null) layoutData = l.Layout;
                        if (layoutData != null)
                        {
                            DataContractSerializer s = new DataContractSerializer(typeof(ControlLayoutInfo)); 
                            XmlReader xmlReader = XmlReader.Create(new StringReader(layoutData));
                            ret = (ControlLayoutInfo) s.ReadObject(xmlReader);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка загрузки разметки {w} " + ex.Message);
                    }
                }
            }
            if (isLayoutGroup)
            {
                ret.Height = ret.Height == 0 ? height : ret.Height;
                ret.Width = ret.Width == 0 ? width : ret.Width;
            }
            return ret;
        }

        #region Fields

        #endregion

        #region Constructors

        #endregion

        #region Properties

        #endregion

        #region Commands

        #endregion
    }
}
