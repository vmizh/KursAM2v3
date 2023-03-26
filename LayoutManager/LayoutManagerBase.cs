using System;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using Data;
using DevExpress.Data;
using DevExpress.Xpf.Core.Serialization;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.LayoutControl;
using DevExpress.XtraGrid;
using Helper;
using ColumnFilterMode = DevExpress.Xpf.Grid.ColumnFilterMode;

namespace LayoutManager
{
    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    public abstract class LayoutManagerBase
    {
        protected string AppDataPath
        {
            get
            {
                var spath = (string)Application.Current.Properties["DataPath"];
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

        [DataMember] public bool IsWindowOnly { set; get; }

        public virtual void Save()
        {
            try
            {
                var saveLayout = WindowSave();
                var sb = new StringBuilder();
                if (!IsWindowOnly)
                {
                    var ms = new MemoryStream();
                    if (LayoutControl != null && !(LayoutControl is DataLayoutControl))
                    {
                        DXSerializer.Serialize(LayoutControl, ms, "Kurs");
                        saveLayout.Layout = ms.ToArray();
                    }

                    var ser1 =
                        new DataContractSerializer(typeof(WindowsScreenState));
                    using (var writer = XmlWriter.Create(sb))
                    {
                        ser1.WriteObject(writer, saveLayout);
                        writer.Flush();
                    }
                }

                var connString = new SqlConnectionStringBuilder
                {
                    DataSource = "172.16.1.1",
                    InitialCatalog = "KursSystem",
                    UserID = "sa",
                    Password = "CbvrfFhntvrf65"
                }.ToString();
                using (var ctx = new KursSystemEntities(connString))
                {
                    if (CurrentUser.UserInfo == null) return;
                    var w = Win != null ? Win.GetType().Name : "Control";
                    var l = ctx.FormLayout.FirstOrDefault(_ => _.UserId == CurrentUser.UserInfo.KursId
                                                               && _.FormName == w && _.ControlName == FileName);
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
                                ControlName = FileName,
                                Layout = sb.ToString()
                            });
                        }
                        else
                        {
                            l.UpdateDate = DateTime.Now;
                            l.UserId = CurrentUser.UserInfo.KursId;
                            l.FormName = w;
                            l.ControlName = FileName;
                            l.Layout = sb.ToString();
                        }

                        ctx.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка сохранения разметки {w} / {FileName}" + $"{ex}");
                    }
                }
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
            var saveLayout = new WindowsScreenState { IsWindow = false };
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

        public virtual void ResetLayout()
        {
        }

        public bool IsLayoutExists()
        {
            return File.Exists($"{AppDataPath}\\{FileName}.xml");
        }

        public virtual void Load(bool autoSummary = false)
        {
            if (CurrentUser.UserInfo == null) return;
            var connString = new SqlConnectionStringBuilder
            {
                DataSource = "172.16.1.1",
                InitialCatalog = "KursSystem",
                UserID = "sa",
                Password = "CbvrfFhntvrf65"
            }.ToString();
            string layoutData = null;
            using (var ctx = new KursSystemEntities(connString))
            {
                var w = Win != null ? Win.GetType().Name : "Control";
                try
                {
                    var l = ctx.FormLayout.FirstOrDefault(_ => _.UserId == CurrentUser.UserInfo.KursId
                                                               && _.FormName == w && _.ControlName == FileName);
                    if (l != null) layoutData = l.Layout;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки разметки {w} " + ex.Message);
                }
            }

            if (!IsLayoutExists() && layoutData == null) return;
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
                    DXSerializer.Serialize(LayoutControl, ms1, "Kurs");
                    WinState.Layout = ms1.ToArray();
                }

                if (!File.Exists($"{AppDataPath}\\{FileName}.xml") && layoutData == null) return;
                XmlReader r;
                FileStream fs;
                if (layoutData != null)
                {
                    var myEncoder = new UnicodeEncoding();
                    var bytes = myEncoder.GetBytes(layoutData);
                    var mss = new MemoryStream(bytes);
                    r = XmlReader.Create(mss);
                }
                else
                {
                    using (fs = File.OpenRead($"{AppDataPath}\\{FileName}.xml"))
                    {
                        r = XmlReader.Create(fs);
                    }
                }

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

                if (p.Layout == null || IsWindowOnly) return;
                var ms = new MemoryStream(p.Layout);
                //var doc = XDocument.Load(ms);
                if (LayoutControl != null)
                    DXSerializer.Deserialize(LayoutControl, ms, "Kurs");
                var grids = WindowHelper.GetLogicalChildCollection<GridControl>(LayoutControl);
                var trees = WindowHelper.GetLogicalChildCollection<TreeListControl>(LayoutControl);
                if (grids != null && grids.Count > 0)
                    foreach (var ctrl in grids)
                    foreach (var column in ctrl.Columns)
                    {
                        column.AutoFilterCondition = AutoFilterCondition.Contains;
                        if (column.FieldType != typeof(decimal) && column.FieldType != typeof(decimal?)
                                                                && column.FieldType != typeof(double) &&
                                                                column.FieldType != typeof(double?)
                                                                && column.FieldType != typeof(float) &&
                                                                column.FieldType != typeof(float?)
                                                                && column.FieldType != typeof(DateTime) &&
                                                                column.FieldType != typeof(DateTime?))
                        {
                            column.ColumnFilterMode = ColumnFilterMode.DisplayText;
                            column.SortMode = ColumnSortMode.DisplayText;
                            if (column.FieldType == typeof(string))
                                column.EditSettings = new TextEditSettings
                                {
                                    SelectAllOnMouseUp = true
                                };
                        }
                    }

                if (trees != null && trees.Count > 0)
                    foreach (var ctrl in trees)
                    foreach (var column in ctrl.Columns)
                    {
                        column.AutoFilterCondition = AutoFilterCondition.Contains;
                        if (column.FieldType == typeof(decimal) || column.FieldType == typeof(decimal?) ||
                            column.FieldType == typeof(double) || column.FieldType == typeof(double?) ||
                            column.FieldType != typeof(float) || column.FieldType != typeof(float?) ||
                            column.FieldType == typeof(DateTime) || column.FieldType == typeof(DateTime?)) continue;
                        column.ColumnFilterMode = ColumnFilterMode.DisplayText;
                        column.SortMode = ColumnSortMode.DisplayText;
                        if (column.FieldType == typeof(string))
                            column.EditSettings = new TextEditSettings
                            {
                                SelectAllOnMouseUp = true
                            };
                    }

                if (autoSummary)
                {
                    if (LayoutControl is GridControl ctrl1)
                    {
                        ctrl1.TotalSummary.Clear();
                        foreach (var column in ctrl1.Columns)
                        {
                            column.AutoFilterCondition = AutoFilterCondition.Contains;
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

                            if (column.FieldType == typeof(decimal) || column.FieldType == typeof(decimal?) ||
                                column.FieldType == typeof(double) || column.FieldType == typeof(double?) ||
                                column.FieldType == typeof(float) || column.FieldType == typeof(float?) ||
                                column.FieldType == typeof(DateTime) || column.FieldType == typeof(DateTime?)) continue;
                            column.ColumnFilterMode = ColumnFilterMode.DisplayText;
                            column.SortMode = ColumnSortMode.DisplayText;
                            if (column.FieldType == typeof(string))
                                column.EditSettings = new TextEditSettings
                                {
                                    SelectAllOnMouseUp = true
                                };
                        }
                    }


                    if (LayoutControl is TreeListControl ctrl)
                    {
                        ctrl.TotalSummary.Clear();
                        foreach (var column in ctrl.Columns)
                        {
                            column.AutoFilterCondition = AutoFilterCondition.Contains;
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

                            if (column.FieldType == typeof(decimal) || column.FieldType == typeof(decimal?) ||
                                column.FieldType == typeof(double) || column.FieldType == typeof(double?) ||
                                column.FieldType == typeof(float) || column.FieldType == typeof(float?) ||
                                column.FieldType == typeof(DateTime) || column.FieldType == typeof(DateTime?)) continue;
                            column.ColumnFilterMode = ColumnFilterMode.DisplayText;
                            column.SortMode = ColumnSortMode.DisplayText;
                            if (column.FieldType == typeof(string))
                                column.EditSettings = new TextEditSettings
                                {
                                    SelectAllOnMouseUp = true
                                };
                        }
                    }
                }

                ms.Close();
                if (Win?.ActualHeight <= 50 || Win?.ActualWidth <= 160)
                {
                    Win.Height = 400;
                    Win.Width = 800;
                }
            }
            catch
            {
                if (!File.Exists($"{AppDataPath}\\{FileName}.xml"))
                    File.Delete($"{AppDataPath}\\{FileName}.xml");
                ResetLayout();
            }
        }
    }

    /// <summary>
    ///     Сохранение и восстановление разметки по каждому объекту отдельно
    /// </summary>
    public class LayoutManagerSeparated : LayoutManagerBase
    {
    }
}
