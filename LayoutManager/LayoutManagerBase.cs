using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using DevExpress.Data;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Core.Serialization;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.LayoutControl;
using DevExpress.Xpf.WindowsUI;
using DevExpress.XtraGrid;
using Helper;
using ColumnFilterMode = DevExpress.Xpf.Grid.ColumnFilterMode;

namespace LayoutManager
{
    public abstract class LayoutManagerBase
    {
        protected string AppDataPath
        {
            get
            {
                var spath = (string) Application.Current.Properties["DataPath"];
                if (!Directory.Exists(spath))
                    Directory.CreateDirectory(spath);
                return spath;
            }
        }

        [DataMember(IsRequired = false)] public XDocument OptionsData { set; get; }

        [DataMember] public WindowsScreenState WinState { set; get; }

        [DataMember] public MemoryStream LayoutBase { set; get; }

        [DataMember] public string FileName { set; get; }

        [DataMember] public Window Win { set; get; }

        /// <summary>
        ///     DevExpress conrol/ Conrol верхнего уровня для сериализации DXSerialize
        /// </summary>
        [DataMember]
        public DependencyObject LayoutControl { set; get; }



        public virtual void Save()
        {
            try
            {
                var saveLayout = WindowSave();
                var ms = new MemoryStream();
                if (LayoutControl != null && !(LayoutControl is DataLayoutControl))
                {
                    DXSerializer.Serialize(LayoutControl, ms, "Kurs", null);
                    saveLayout.Layout = ms.ToArray();
                }

                var writer = new FileStream($"{AppDataPath}\\{FileName}.xml", FileMode.Create);
                var ser =
                    new DataContractSerializer(typeof(WindowsScreenState));
                ser.WriteObject(writer, saveLayout);
                writer.Close();
            }
#pragma warning disable 168
            catch (Exception ex)
#pragma warning restore 168
            {
                /*
                 Закрыто для старого метода сохранения layout
                 */
                //WinUIMessageBox.Show(Application.Current.MainWindow,ex.Message,"Ошибка",MessageBoxButton.OK,MessageBoxImage.Error);
            }
        }

        public WindowsScreenState WindowSave()
        {
            var saveLayout = new WindowsScreenState {IsWindow = false};
            if (Win != null)
                saveLayout = new WindowsScreenState
                {
                    IsWindow = true,
                    FormHeight = Win.Height,
                    FormWidth = Win.Width,
                    FormLeft = Win.WindowState == WindowState.Maximized ? 0 : Win.Left,
                    FormTop = Win.WindowState == WindowState.Maximized ? 0 : Win.Top,
                    FormStartLocation = Win.WindowStartupLocation,
                    FormState = Win.WindowState
                };
            return saveLayout;
        }

        public virtual void ResetLayout(bool controlOnly = true)
        {
            try
            {
                if (WinState == null) return;
                if (!controlOnly)
                    if (Win != null)
                    {
                        Win.Height = WinState.FormHeight;
                        Win.Width = WinState.FormWidth;
                        Win.Left = WinState.FormLeft;
                        Win.Top = WinState.FormTop;
                        Win.WindowStartupLocation = WinState.FormStartLocation;
                        Win.WindowState = WinState.FormState;
                    }

                if (WinState.Layout == null) return;
                if (LayoutControl == null) return;
                var ms = new MemoryStream(WinState.Layout);
                DXSerializer.Deserialize(LayoutControl, ms, "Kurs", null);
                ms.Close();
            }
            catch (Exception ex)
            {
                var errText = new StringBuilder(ex.Message);
                if (ex.InnerException != null)
                {
                    var ex1 = ex.InnerException;
                    errText.Append(ex1.Message + "\n");
                    if (ex1.InnerException != null)
                        errText.Append(ex1.InnerException.Message);
                }

                WinUIMessageBox.Show(Application.Current.Windows.Cast<Window>().SingleOrDefault(x => x.IsActive),
                    errText.ToString(),
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.None, MessageBoxOptions.None,
                    FloatingMode.Adorner);
            }
        }

        public bool IsLayoutExists()
        {
            return File.Exists($"{AppDataPath}\\{FileName}.xml");
        }

        public virtual void Load(bool autoSummary = false)
        {
            if (!IsLayoutExists()) return;
            try
            {
                WinState = new WindowsScreenState();
                if (Win != null)
                    WinState = new WindowsScreenState
                    {
                        FormHeight = Win.Height,
                        FormWidth = Win.Width,
                        FormLeft = Win.Left,
                        FormTop = Win.Top,
                        FormStartLocation = Win.WindowStartupLocation,
                        FormState = Win.WindowState
                    };

                if (LayoutControl != null && !(LayoutControl is DataLayoutControl))
                {
                    var ms1 = new MemoryStream();
                    DXSerializer.Serialize(LayoutControl, ms1, "Kurs", null);
                    WinState.Layout = ms1.ToArray();
                }

                if (!File.Exists($"{AppDataPath}\\{FileName}.xml")) return;
                using (var fs = File.OpenRead($"{AppDataPath}\\{FileName}.xml"))
                {
                    var r = XmlReader.Create(fs);
                    if (!(new DataContractSerializer(typeof(WindowsScreenState)).ReadObject(r) is WindowsScreenState p)
                    ) return;
                    if (Win != null)
                    {
                        Win.WindowStartupLocation = p.FormStartLocation;
                        Win.WindowState = WindowState.Normal;
                        Win.Height = p.FormHeight;
                        Win.Width = p.FormWidth;
                        Win.Left = p.FormLeft < 0 ? 0 : p.FormLeft;
                        Win.Top = p.FormTop < 0 ? 0 : p.FormTop;
                    }

                    if (p.Layout == null) return;
                    var ms = new MemoryStream(p.Layout);
                    DXSerializer.Deserialize(LayoutControl, ms, "Kurs", null);
                    var grids = WindowHelper.GetLogicalChildCollection<GridControl>(LayoutControl);
                    var trees = WindowHelper.GetLogicalChildCollection<TreeListControl>(LayoutControl);
                    if(grids != null && grids.Count > 0)
                        foreach (var ctrl in grids)
                        {
                            foreach (var column in ctrl.Columns)
                            {
                                if (column.FieldType != typeof(decimal) && column.FieldType != typeof(decimal?)
                                                                        && column.FieldType != typeof(double) &&
                                                                        column.FieldType != typeof(double?)
                                                                        && column.FieldType == typeof(float) &&
                                                                        column.FieldType == typeof(float?)
                                                                        && column.FieldType != typeof(DateTime) &&
                                                                        column.FieldType != typeof(DateTime?))
                                {
                                    column.AutoFilterCondition = AutoFilterCondition.Contains;
                                    column.ColumnFilterMode = ColumnFilterMode.DisplayText;
                                    column.SortMode = ColumnSortMode.DisplayText;
                                }
                            }
                        }
                    if (trees != null && trees.Count > 0)
                        foreach (var ctrl in trees)
                        {
                            foreach (var column in ctrl.Columns)
                            {
                                if (column.FieldType != typeof(decimal) && column.FieldType != typeof(decimal?)
                                                                        && column.FieldType != typeof(double) &&
                                                                        column.FieldType != typeof(double?)
                                                                        && column.FieldType == typeof(float) &&
                                                                        column.FieldType == typeof(float?)
                                                                        && column.FieldType != typeof(DateTime) &&
                                                                        column.FieldType != typeof(DateTime?))
                                {
                                    column.AutoFilterCondition = AutoFilterCondition.Contains;
                                    column.ColumnFilterMode = ColumnFilterMode.DisplayText;
                                    column.SortMode = ColumnSortMode.DisplayText;
                                }
                            }
                        }

                    if (autoSummary)
                    {
                        if (LayoutControl is GridControl ctrl1)
                        {
                            ctrl1.TotalSummary.Clear();
                            foreach (var column in ctrl1.Columns)
                                if (column.FieldType == typeof(decimal) || column.FieldType == typeof(decimal?)
                                                                        || column.FieldType == typeof(double) ||
                                                                        column.FieldType == typeof(double?)
                                                                        || column.FieldType == typeof(float) ||
                                                                        column.FieldType == typeof(float?))
                                {
                                    var summary = new GridSummaryItem
                                    {
                                        SummaryType = SummaryItemType.Sum,
                                        ShowInColumn = column.FieldName,
                                        DisplayFormat = "{0:n2}",
                                        FieldName = column.FieldName
                                    };
                                    ctrl1.TotalSummary.Add(summary);
                                }
                                else
                                {
                                    if (column.FieldType != typeof(decimal) && column.FieldType != typeof(decimal?)
                                                                            && column.FieldType != typeof(double) &&
                                                                            column.FieldType != typeof(double?)
                                        || column.FieldType == typeof(float) ||
                                        column.FieldType == typeof(float?))
                                    {
                                        column.AutoFilterCondition = AutoFilterCondition.Contains;
                                        column.ColumnFilterMode = ColumnFilterMode.DisplayText;
                                        column.SortMode = ColumnSortMode.DisplayText;
                                    }
                                }
                        }

                        if (LayoutControl is TreeListControl ctrl)
                        {
                            ctrl.TotalSummary.Clear();
                            foreach (var column in ctrl.Columns)
                                if (column.FieldType == typeof(decimal) || column.FieldType == typeof(decimal?)
                                                                        || column.FieldType == typeof(double) ||
                                                                        column.FieldType == typeof(double?)
                                                                        || column.FieldType == typeof(float) ||
                                                                        column.FieldType == typeof(float?))
                                {
                                    var summary = new TreeListSummaryItem
                                    {
                                        SummaryType = SummaryItemType.Sum,
                                        ShowInColumn = column.FieldName,
                                        DisplayFormat = "{0:n2}",
                                        FieldName = column.FieldName
                                    };
                                    ctrl.TotalSummary.Add(summary);
                                }
                                else
                                {
                                    foreach (var col in ctrl.Columns)
                                        if (col.FieldType != typeof(decimal) && col.FieldType != typeof(decimal?)
                                                                             && col.FieldType != typeof(double) &&
                                                                             col.FieldType != typeof(double?))
                                            col.AutoFilterCondition = AutoFilterCondition.Contains;
                                }
                        }
                    }
                    ms.Close();
                }
            }
            catch (Exception)
            {
                if (!File.Exists($"{AppDataPath}\\{FileName}.xml"))
                    File.Delete($"{AppDataPath}\\{FileName}.xml");
                ResetLayout();
            }
        }

        public static bool ColumnFieldTypeCheckDecimal(Type type)
        {
            return type == typeof(decimal) || type == typeof(decimal?)
                                           || type == typeof(double) ||
                                           type == typeof(double?)
                                           || type == typeof(float) ||
                                           type == typeof(float?);
        }
    }

    /// <summary>
    ///     Сохранение и восстановление разметки по каждому объекту отдельно
    /// </summary>
    public class LayoutManagerSeparated : LayoutManagerBase
    {
    }
}