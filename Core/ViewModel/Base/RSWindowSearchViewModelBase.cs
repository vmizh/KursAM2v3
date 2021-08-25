using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;

namespace Core.ViewModel.Base
{
    [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
    public abstract class RSWindowSearchViewModelBase : RSWindowViewModelBase
    {
        private DateTime myDateEnd;
        private DateTime myDateStart;
        private string myFirstSearchName;
        private string mySecondSearchName;

        public RSWindowSearchViewModelBase()
        {
            StartDate = DateTime.Today.AddDays(-14);
            EndDate = DateTime.Today;
        }

        public RSWindowSearchViewModelBase(Window form) : base(form)
        {
            StartDate = DateTime.Today.AddDays(-14);
            EndDate = DateTime.Today;
        }


        public string FirstSearchName
        {
            get => myFirstSearchName;
            set
            {
                if (myFirstSearchName == value) return;
                myFirstSearchName = value;
                RaisePropertyChanged();
            }
        }

        public string SecondSearchName
        {
            get => mySecondSearchName;
            set
            {
                if (mySecondSearchName == value) return;
                mySecondSearchName = value;
                RaisePropertyChanged();
            }
        }

        public virtual DateTime EndDate
        {
            get => myDateEnd;
            set
            {
                if (myDateEnd == value) return;
                myDateEnd = value;
                RaisePropertyChanged();
                if (myDateStart < myDateEnd) return;
                myDateStart = myDateEnd;
                RaisePropertyChanged(nameof(StartDate));
            }
        }

        public virtual DateTime StartDate
        {
            get => myDateStart;
            set
            {
                if (myDateStart == value) return;
                myDateStart = value;
                RaisePropertyChanged();
                if (myDateStart <= myDateEnd) return;
                myDateEnd = myDateStart;
                RaisePropertyChanged(nameof(EndDate));
            }
        }

        public virtual string SplashCaption { set; get; } = "Загрузка...";

        public Command AutoGeneratingColumnCommand
        {
            get { return new(AutogeneratingSearchGridColumns, _ => true); }
        }

        //AutoGeneratingColumnEventArgs
        public virtual void AutogeneratingSearchGridColumns(object args)
        {
            if (args is not AutoGeneratingColumnEventArgs e) return;
            e.Column.Name = e.Column.FieldName;
            e.Column.ReadOnly = true;
            switch (e.Column.FieldName)
            {
                case "State":
                    e.Column.Visible = false;
                    break;
                case "Note":
                    e.Column.EditSettings = new TextEditSettings
                    {
                        AcceptsReturn = true
                    };
                    break;
            }
        }
    }
}