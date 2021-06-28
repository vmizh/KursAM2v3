using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using Core;
using Core.Menu;
using Core.ViewModel.Base;

namespace KursAM2.ViewModel.Shop
{
    public class ShopParserExtFilesWindowViewModel : RSWindowViewModelBase, IDataErrorInfo
    {
        public ShopParserExtFilesWindowViewModel()
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.ExitOnlyRightBar(this);
        }

        #region Properties

        public ObservableCollection<ShopExtFileOrderItem> GeneratedItems { set; get; }
        public ObservableCollection<NomenklInfo> NomenklMainList { set; get; } = new ObservableCollection<NomenklInfo>();

       
        private string myTextForParse;
        public string TextForParse
        {
            get => myTextForParse;
            set
            {
                if (myTextForParse == value) return;
                myTextForParse = value;
                RaisePropertyChanged();
            }
        } 
        
        #endregion

        #region Command

        public ICommand GenerateNomenklCommand
        {
            get { return new Command(GenerateNomenkl, _ => !string.IsNullOrWhiteSpace(TextForParse)); }
        }

        private void GenerateNomenkl(object obj)
        {
            GeneratedItems = new ObservableCollection<ShopExtFileOrderItem>(ShopXMLParsingOrder(TextForParse));
            foreach (var item in GeneratedItems)
            {
                NomenklMainList.Add(new NomenklInfo
                {
                    Id = Guid.NewGuid(),
                    FullName = item.Name,
                    Name = item.Name.Substring(0,500),
                    NomenklNumber = item.OfferId,
                    Unit = MainReferences.Units[1750000001],
                    
                });
            }
        }

        public ICommand SaveGenerateNomenklCommand
        {
            get { return new Command(SaveGenerateNomenkl, _ => GeneratedItems?.Count > 0); }
        }

        private void SaveGenerateNomenkl(object obj)
        {
            
        }

        #endregion

        #region methods

        public static List<ShopExtFileOrderItem> ShopXMLParsingOrder(string text)
        {
            //string[] separatingStrings = {"<item", "/>"};
            string[] separatingStrings = {"\r\n"};
            string[] separatingStrings2 = {"\""};
            var OrderItems = new List<ShopExtFileOrderItem>();

            var str = text.Split(separatingStrings, StringSplitOptions.RemoveEmptyEntries);
            var str2 = new List<string>();
            for (var i = 3; i < str.Length - 3; i++) str2.Add(str[i]);
            foreach (var s in str2)
            {
                var sp = s.Split(separatingStrings2, StringSplitOptions.None);
                var newItem = new ShopExtFileOrderItem
                {
                    OfferId = sp[1],
                    Price = Convert.ToDecimal(sp[3],new NumberFormatInfo()
                    {
                        CurrencyDecimalSeparator = ".,"
                    }),
                    Count = Convert.ToDecimal(sp[5],new NumberFormatInfo()
                    {
                        CurrencyDecimalSeparator = ".,"
                    })
                };
                var ss = "";
                for (var i = 7; i < sp.Length; i++) ss = ss + sp[i];
                newItem.Name = ss;
                OrderItems.Add(newItem);
            }

            return OrderItems;
        }

        #endregion

        public string this[string columnName] => null;

        public string Error { get; } = null;
    }
}