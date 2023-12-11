using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
using KursAM2.ViewModel.Management;

namespace KursAM2.View.Management.Controls
{
    /// <summary>
    /// Interaction logic for ProfitAndLossesMainUI.xaml
    /// </summary>
    public partial class ProfitAndLossesMainUI 
    {
        public ProfitAndLossesMainUI()
        {
            InitializeComponent(); 
        }
        private void LayoutTabs_OnSelectedTabChildChanged(object sender, ValueChangedEventArgs<FrameworkElement> e)
        {
            if (!(DataContext is ProfitAndLossesWindowViewModel ctx)) return;
            if (e.NewValue.Name == "LayoutGroupBalans")
            {
                ctx.UpdateExtend();
                //dateStartBorder.BorderThickness = new Thickness(1, 1, 1, 1);
                //dateEndBorder.BorderThickness = new Thickness(1, 1, 1, 1);
                //dateStartBorder.BorderBrush = Brushes.Black;
                //dateEndBorder.BorderBrush = Brushes.Black;
            }
            if (e.NewValue.Name == "LayoutGroupFact")
            {
                ctx.UpdateExtend2();
                //dateStartBorder.BorderThickness = new Thickness(2, 2, 2, 2);
                //dateEndBorder.BorderThickness = new Thickness(2, 2, 2, 2);
                //dateStartBorder.BorderBrush = Brushes.Red;
                //dateEndBorder.BorderBrush = Brushes.Red;
            }
        }

        private void GridControlMain_OnSelectedItemChanged(object sender, SelectedItemChangedEventArgs e)
        {
            if (!(DataContext is ProfitAndLossesWindowViewModel ctx)) return;
            if (e.NewItem is ProfitAndLossesMainRowViewModel v) ctx.UpdateExtend(v.Id);
            //if (!(e.NewItem is ProfitAndLossesMainRowViewModel item))
            //{
            //    //col124.Visible = false;
            //    return;
            //}
            //col124.Visible = item.Name == "Финансовые операции";

        }

        private void GridControlMain_OnSelectedItemChanged2(object sender, SelectedItemChangedEventArgs e)
        {
            if (!(DataContext is ProfitAndLossesWindowViewModel ctx)) return;
            if (e.NewItem is ProfitAndLossesMainRowViewModel v) ctx.UpdateExtend2(v.Id);
            //if (!(e.NewItem is ProfitAndLossesMainRowViewModel item))
            //{
            //    //col124.Visible = false;
            //    return;
            //}
            //col124.Visible = item.Name == "Финансовые операции";
        }
    }
}
