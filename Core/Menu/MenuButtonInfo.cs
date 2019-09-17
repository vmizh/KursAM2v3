using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Helper;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
namespace Core.Menu
{
    public class MenuButtonInfo
    {
        public MenuButtonInfo()
        {
            SubMenu = new ObservableCollection<MenuButtonInfo>();
        }

        public string Name { set; get; }
        public Dock Alignment { set; get; }
        public HorizontalAlignment HAlignment { set; get; }
        public ControlTemplate Content { set; get; }
        public ICommand Command { set; get; }
        public object CommandParameter { set; get; }
        public string Caption { set; get; }
        public DrawingImage Image { set; get; }
        public bool IsSeparator { set; get; }
        public ObservableCollection<MenuButtonInfo> SubMenu { set; get; }
        public string ToolTip { set; get; }

        public void MenuOpen(DependencyObject dp)
        {
            var ctxMenu = new ContextMenu();
            foreach (var item in SubMenu)
            {
                if (item.IsSeparator)
                {
                    ctxMenu.Items.Add(new Separator());
                    continue;
                }
                var menuItem = new MenuItem
                {
                    Header = item.Caption,
                    Command = item.Command,
                    CommandParameter = WindowHelper.FindParentWindow(dp),
                    Icon = item.Image != null
                        ? new Image
                        {
                            Source = item.Image
                        }
                        : null
                };
                if (item.SubMenu != null && item.SubMenu.Count > 0)
                {
                    if (item.IsSeparator)
                    {
                        menuItem.Items.Add(new Separator());
                        continue;
                    }
                    foreach (var subitem in item.SubMenu)
                        menuItem.Items.Add(new MenuItem
                        {
                            Header = subitem.Caption,
                            Command = subitem.Command,
                            CommandParameter = WindowHelper.FindParentWindow(dp),
                            Icon = subitem.Image != null
                                ? new Image
                                {
                                    Source = subitem.Image
                                }
                                : null
                        });
                }
                ctxMenu.Items.Add(menuItem);
            }
            ctxMenu.IsOpen = true;
        }
    }
}