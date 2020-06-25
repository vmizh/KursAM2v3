using System;
using System.Collections.ObjectModel;
using System.Linq;
using Core.Logger;
using Core.Repository.Base;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.Native;
using KursRepozit.Auxiliary;
using KursRepozit.Repositories;
using KursRepozit.Views;

namespace KursRepozit.ViewModels
{
    public class DataSourcesViewModel : KursBaseControlViewModel, IDocumentOperation
    {
        #region Constructors

        public DataSourcesViewModel()
        {
            kursSystemRepository = new GenericKursSystemRepository<DataSources>(unitOfWork);
            employeeRepository = new DataSourcesKursSystemRepository(unitOfWork);
            ModelView = new DataSourceView();
            WindowName = "Источники для системы курс";
            LeftMenuBar = WindowMenuGenerator.BaseLeftBar(this);
            RightMenuBar = WindowMenuGenerator.ReferenceRightBar(this);
        }

        #endregion

        #region Fields

        private readonly UnitOfWork<KursSystemEntities> unitOfWork = new UnitOfWork<KursSystemEntities>();
        private readonly GenericKursSystemRepository<DataSources> kursSystemRepository;

        // ReSharper disable once NotAccessedField.Local
        private IDataSourcesRepository employeeRepository;

        #endregion

        #region Properties

        public ObservableCollection<DataSourceViewModel> DataSources { set; get; } =
            new ObservableCollection<DataSourceViewModel>();

        public DataSourceViewModel CurrentDataSource
        {
            get => GetValue<DataSourceViewModel>();
            set => SetValue(value);
        }

        #endregion

        #region Methods

        #endregion

        #region Commands

        [Command]
        public override void Load()
        {
            DataSources.Clear();
            foreach (var ds in kursSystemRepository.GetAll())
            {
                var newItem = new DataSourceViewModel(ds);
                DataSources.Add(newItem);
            }
        }

        public override bool CanLoad()
        {
            return true;
        }

        [Command]
        public override void Save()
        {
            try
            {
                unitOfWork.CreateTransaction();
                foreach (var d in DataSources.Where(_ => _.State != RowStatus.NotEdited))
                    switch (d.State)
                    {
                        case RowStatus.NewRow:
                            kursSystemRepository.Insert(d.Entity);
                            break;
                        case RowStatus.Deleted:
                            kursSystemRepository.Delete(d.Entity);
                            break;
                        case RowStatus.Edited:
                            kursSystemRepository.Update(d.Entity);
                            break;
                    }

                unitOfWork.Save();
                unitOfWork.Commit();
                DataSources.ForEach(_ => _.State = RowStatus.NotEdited);
                Load();
            }

            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
                LoggerHelper.WriteError(ex);
                unitOfWork.Rollback();
            }
        }

        public override bool CanSave()
        {
            return DataSources.Any(_ => _.State != RowStatus.NotEdited);
        }

        [Command]
        public void AddSource()
        {
            DataSources.Add(new DataSourceViewModel(new DataSources(), RowStatus.NewRow));
        }

        [Command]
        public void DeleteSource()
        {
            CurrentDataSource.SetChangeStatus(RowStatus.Deleted);
        }

        #endregion
    }
}