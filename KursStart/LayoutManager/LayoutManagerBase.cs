using System;
using System.IO;
using System.Runtime.Serialization;
using System.Windows;
using System.Xml;
using DevExpress.Data;
using DevExpress.Xpf.Core.Serialization;
using DevExpress.Xpf.Grid;
using System.Xml.Linq;

namespace LayoutManager
{
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

        [DataMember( IsRequired = false)]
        public XDocument OptionsData { set; get; }

        [DataMember]
        public WindowsScreenState WinState { set; get; }

        [DataMember]
        public MemoryStream LayoutBase { set; get; }

        [DataMember]
        public string FileName { set; get; }

        [DataMember]
        public Window Win { set; get; }

        /// <summary>
        ///     DevExpress conrol/ Conrol верхнего уровня для сериализации DXSerialize
        /// </summary>
        [DataMember]
        public DependencyObject LayoutControl { set; get; }

        public virtual void Save()
        {
            try
            {
                WindowsScreenState saveLayout = new WindowsScreenState {IsWindow = false};
                if (Win != null)
                {
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
                }
                var ms = new MemoryStream();
                if (LayoutControl != null)
                {
                    DXSerializer.Serialize(LayoutControl, ms, "Kurs", null);
                    saveLayout.Layout = ms.ToArray();
                }
                var writer = new FileStream($"{AppDataPath}\\{FileName}.xml", FileMode.Create);
                var ser =
                    new DataContractSerializer(typeof (WindowsScreenState));
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

        public virtual void ResetLayout(bool controlOnly = true)
        {
            if (WinState == null) return;
            if (!controlOnly)
            {
                if (Win != null)
                {
                    Win.Height = WinState.FormHeight;
                    Win.Width = WinState.FormWidth;
                    Win.Left = WinState.FormLeft;
                    Win.Top = WinState.FormTop;
                    Win.WindowStartupLocation = WinState.FormStartLocation;
                    Win.WindowState = WinState.FormState;
                }
            }
            if (WinState.Layout == null) return;
            if (LayoutControl == null) return;
            var ms = new MemoryStream(WinState.Layout);
            DXSerializer.Deserialize(LayoutControl, ms, "Kurs", null);
            ms.Close();
        }

        public virtual void Load(bool autoSummary = false)
        {
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

                if (LayoutControl != null)
                {
                    var ms1 = new MemoryStream();
                    DXSerializer.Serialize(LayoutControl, ms1, "Kurs", null);
                    WinState.Layout = ms1.ToArray();
                }

                if (!File.Exists($"{AppDataPath}\\{FileName}.xml")) return;
                using (var fs = File.OpenRead($"{AppDataPath}\\{FileName}.xml"))
                {
                    var r = XmlReader.Create(fs);
                    var p =
                        new DataContractSerializer(typeof (WindowsScreenState)).ReadObject(r) as
                            WindowsScreenState;
                    if (p == null) return;
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
                    if (autoSummary)
                    {
                        if (LayoutControl is GridControl)
                        {
                            var ctrl = LayoutControl as GridControl;
                            ctrl.TotalSummary.Clear();
                            foreach (var column in ctrl.Columns)
                            {
                                if (column.FieldType == typeof(decimal) || column.FieldType == typeof(decimal?))
                                {
                                    var summary = new GridSummaryItem
                                    {
                                        SummaryType = SummaryItemType.Sum,
                                        ShowInColumn = column.FieldName,
                                        DisplayFormat = "{0:n2}",
                                        FieldName = column.FieldName
                                    };
                                    ctrl.TotalSummary.Add(summary);
                                }
                            }
                        }
                        if (LayoutControl is TreeListControl)
                        {
                            var ctrl = LayoutControl as TreeListControl;
                            ctrl.TotalSummary.Clear();
                            foreach (var column in ctrl.Columns)
                            {
                                if (column.FieldType == typeof(decimal) || column.FieldType == typeof(decimal?))
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

    }

    /// <summary>
    /// Сохранение и восстановление разметки по каждому объекту отдельно
    /// </summary>
    public class LayoutManagerSeparated : LayoutManagerBase
    {
        
    }
}