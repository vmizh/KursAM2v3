using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Input;
using Core.ViewModel.Base.Column;
using DevExpress.Xpf.Grid;
using JetBrains.Annotations;

namespace Core.ViewModel.Base
{
    [Obsolete("Используйте класс KursBaseViewModel")]
    public abstract class KursViewModelBase
    {
        private bool myDeleted;
        private Guid myId;

        // ReSharper disable once InconsistentNaming
        protected string myName;

        // ReSharper disable once InconsistentNaming
        protected string myNote;
        private bool mySaveEnable;
        [Display(AutoGenerateField = false)]
        public string LayoutName { set; get; }
        /// <summary>
        ///     задает и получает статус документа или строки документа
        /// </summary>
        [Display(AutoGenerateField = false)]
        public RowStatus State { set; get; }
        [Display(AutoGenerateField = false)]
        public Guid Id
        {
            get => myId;
            set
            {
                if (value == myId) return;
                myId = value;
                OnPropertyChanged(nameof(Id));
            }
        }
        [Display(AutoGenerateField = false)]
        public bool Deleted
        {
            get => myDeleted;
            set
            {
                if (value == myDeleted) return;
                myDeleted = value;
                OnPropertyChanged(nameof(Deleted));
            }
        }
        public virtual string Name
        {
            get => myName;
            set
            {
                if (value == myName) return;
                myName = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        public virtual string Note
        {
            get => myNote;
            set
            {
                if (value == myNote) return;
                myNote = value;
                OnPropertyChanged(nameof(Note));
            }
        }
        [Display(AutoGenerateField = false)]
        public ICommand CloseCommand
        {
            get { return new Command(CloseWindow, param => true); }
        }
        [Display(AutoGenerateField = false)]
        public ICommand SaveDataCommand
        {
            get { return new Command(param => SaveData(), param => IsSaveEnable); }
        }
        [Display(AutoGenerateField = false)]
        public bool IsSaveEnable
        {
            get => mySaveEnable;
            set
            {
                if (value.Equals(mySaveEnable)) return;
                mySaveEnable = value;
                OnPropertyChanged(nameof(IsSaveEnable));
                OnPropertyChanged(nameof(SaveDataCommand));
            }
        }
        [Display(AutoGenerateField = false)]
        public ICommand RefreshDataCommand
        {
            get { return new Command(param => RefreshData(), param => true); }
        }
        [Display(AutoGenerateField = false)]
        public virtual DateTime PeriodDate { set; get; }
        public GridTableViewInfo TableViewInfo { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public GridControl Grid { get; set; }

        public virtual void Initialize()
        {
        }

        /// <summary>
        ///     Событие изменения значения свойства представления
        /// </summary>
        public virtual event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Метод обработки события изменения значения свойства представления
        /// </summary>
        /// <param name="propertyName">Идентификатор свойства</param>
        [NotifyPropertyChangedInvocator("propertyName")]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler == null) return;
            handler(this, new PropertyChangedEventArgs(propertyName));
            if (State == RowStatus.NotEdited)
                State = RowStatus.Edited;
        }

        protected virtual void CloseWindow(object form)
        {
            var frm = form as Window;
            frm?.Close();
        }

        public virtual void RefreshData()
        {
        }

        protected virtual void SaveData()
        {
        }

        public override string ToString()
        {
            return Name;
        }

        public virtual void ColumnSetiings(ColumnBase obj)
        {
        }

        protected void SetNonModified(List<RSViewModelBase> docs)
        {
            foreach (var d in docs)
                d.State = RowStatus.NotEdited;
        }

        public void ResetSummary()
        {
            if (Grid == null || TableViewInfo == null) return;
            Grid.TotalSummary.Clear();
            foreach (var colSum in TableViewInfo.TotalSummary)
                Grid.TotalSummary.Add(new GridSummaryItem
                {
                    FieldName = colSum.FieldName,
                    DisplayFormat = colSum.DisplayFormat,
                    ShowInColumn = colSum.FieldName,
                    SummaryType = colSum.Type
                });
        }
    }
}