using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Core.ViewModel.Base;

namespace Core.Menu
{
    public static class MenuGenerator
    {
        public static ObservableCollection<MenuButtonInfo> StandartDialogRightBar(RSWindowViewModelBase vm)
        {
            var ret = new ObservableCollection<MenuButtonInfo>
            {
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuDone"] as ControlTemplate,
                    ToolTip = "Выйти из диалога с подтверждением выбора",
                    Command = vm.DoneCommand
                },
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuExit"] as ControlTemplate,
                    ToolTip = "Закрыть диалог без выбора",
                    Command = vm.CloseWindowCommand
                }
            };
            return ret;
        }

        public static ObservableCollection<MenuButtonInfo> StandartDialogRightBar(KursBaseControlViewModel vm)
        {
            var ret = new ObservableCollection<MenuButtonInfo>
            {
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuDone"] as ControlTemplate,
                    ToolTip = "Выйти из диалога с подтверждением выбора",
                    Command = vm.DoneCommand
                },
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuExit"] as ControlTemplate,
                    ToolTip = "Закрыть диалог без выбора",
                    Command = vm.CloseWindowCommand
                }
            };
            return ret;
        }

        public static ObservableCollection<MenuButtonInfo> StandartDocRightBar(RSWindowViewModelBase vm)
        {
            var prn = new MenuButtonInfo
            {
                Name = "Print",
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Content = Application.Current.Resources["menuPrinter"] as ControlTemplate,
                ToolTip = "Печать"
            };
            var docNew = new MenuButtonInfo
            {
                Name = "New",
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Content = Application.Current.Resources["menuDocumentAdd"] as ControlTemplate,
                ToolTip = "Новый документ"
            };
            docNew.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "Пустой документ",
                Image = Application.Current.Resources["imageDocumentNewEmpty"] as DrawingImage,
                Command = vm.DocNewEmptyCommand
            });
            docNew.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "С текущими реквизитами",
                Image = Application.Current.Resources["imageDocumentNewRequisite"] as DrawingImage,
                Command = vm.DocNewCopyRequisiteCommand
            });
            docNew.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "Копия текущего",
                Image = Application.Current.Resources["imageDocumentNewCopy"] as DrawingImage,
                Command = vm.DocNewCopyCommand
            });
            var ret = new ObservableCollection<MenuButtonInfo>
            {
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuRefresh"] as ControlTemplate,
                    ToolTip = "Обновить",
                    Command = vm.RefreshDataCommand
                },
                docNew,
                prn,
                new MenuButtonInfo
                {
                    Name = "saveButton",
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuSave"] as ControlTemplate,
                    ToolTip = "Сохранить изменения",
                    Command = vm.SaveDataCommand
                },
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuExit"] as ControlTemplate,
                    ToolTip = "Закрыть документ",
                    Command = vm.CloseWindowCommand
                }
            };
            return ret;
        }

        public static ObservableCollection<MenuButtonInfo> StandartDocRightBar(KursBaseControlViewModel vm)
        {
            var prn = new MenuButtonInfo
            {
                Name = "Print",
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Content = Application.Current.Resources["menuPrinter"] as ControlTemplate,
                ToolTip = "Печать"
            };
            var docNew = new MenuButtonInfo
            {
                Name = "New",
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Content = Application.Current.Resources["menuDocumentAdd"] as ControlTemplate,
                ToolTip = "Новый документ"
            };
            docNew.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "Пустой документ",
                Image = Application.Current.Resources["imageDocumentNewEmpty"] as DrawingImage,
                Command = vm.DocNewEmptyCommand
            });
            docNew.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "С текущими реквизитами",
                Image = Application.Current.Resources["imageDocumentNewRequisite"] as DrawingImage,
                Command = vm.DocNewCopyRequisiteCommand
            });
            docNew.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "Копия текущего",
                Image = Application.Current.Resources["imageDocumentNewCopy"] as DrawingImage,
                Command = vm.DocNewCopyCommand
            });
            var ret = new ObservableCollection<MenuButtonInfo>
            {
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuRefresh"] as ControlTemplate,
                    ToolTip = "Обновить",
                    Command = vm.RefreshDataCommand
                },
                docNew,
                prn,
                new MenuButtonInfo
                {
                    Name = "saveButton",
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuSave"] as ControlTemplate,
                    ToolTip = "Сохранить изменения",
                    Command = vm.SaveDataCommand
                },
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuExit"] as ControlTemplate,
                    ToolTip = "Закрыть документ",
                    Command = vm.CloseWindowCommand
                }
            };
            return ret;
        }

        public static ObservableCollection<MenuButtonInfo> StandartDocWithDeleteRightBar(RSWindowViewModelBase vm)
        {
            var prn = new MenuButtonInfo
            {
                Name = "Print",
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Content = Application.Current.Resources["menuPrinter"] as ControlTemplate,
                ToolTip = "Печать"
            };
            var docNew = new MenuButtonInfo
            {
                Name = "New",
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Content = Application.Current.Resources["menuDocumentAdd"] as ControlTemplate,
                ToolTip = "Новый документ"
            };
            docNew.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "Пустой документ",
                Image = Application.Current.Resources["imageDocumentNewEmpty"] as DrawingImage,
                Command = vm.DocNewEmptyCommand
            });
            docNew.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "С текущими реквизитами",
                Image = Application.Current.Resources["imageDocumentNewRequisite"] as DrawingImage,
                Command = vm.DocNewCopyRequisiteCommand
            });
            docNew.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "Копия текущего",
                Image = Application.Current.Resources["imageDocumentNewCopy"] as DrawingImage,
                Command = vm.DocNewCopyCommand
            });
            var docDelete = new MenuButtonInfo
            {
                Name = "Delete",
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Content = Application.Current.Resources["menuDocDelete"] as ControlTemplate,
                ToolTip = "Удалить документ",
                Command = vm.DoсDeleteCommand
            };
            var ret = new ObservableCollection<MenuButtonInfo>
            {
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuRefresh"] as ControlTemplate,
                    ToolTip = "Обновить",
                    Command = vm.RefreshDataCommand
                },
                docNew,
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuSave"] as ControlTemplate,
                    ToolTip = "Сохранить изменения",
                    Command = vm.SaveDataCommand
                },
                docDelete,
                prn,
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuExit"] as ControlTemplate,
                    ToolTip = "Закрыть документ",
                    Command = vm.CloseWindowCommand
                }
            };
            return ret;
        }

        public static ObservableCollection<MenuButtonInfo> BankOpertionsRightBar(RSWindowViewModelBase vm)
        {
            var docNew = new MenuButtonInfo
            {
                Name = "New",
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Content = Application.Current.Resources["menuDocumentAdd"] as ControlTemplate,
                ToolTip = "Новый документ"
            };
            docNew.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "Пустой документ",
                Image = Application.Current.Resources["imageDocumentNewEmpty"] as DrawingImage,
                Command = vm.DocNewEmptyCommand
            });
            docNew.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "Копия текущего",
                Image = Application.Current.Resources["imageDocumentNewCopy"] as DrawingImage,
                Command = vm.DocNewCopyCommand
            });
            //var docDelete = new MenuButtonInfo
            //{
            //    Name = "Delete",
            //    Alignment = Dock.Right,
            //    HAlignment = HorizontalAlignment.Right,
            //    Content = Application.Current.Resources["menuDocDelete"] as ControlTemplate,
            //    ToolTip = "Удалить документ",
            //    Command = vm.DoсDeleteCommand
            //};
            var ret = new ObservableCollection<MenuButtonInfo>
            {
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuRefresh"] as ControlTemplate,
                    ToolTip = "Обновить",
                    Command = vm.RefreshDataCommand
                },
                docNew,
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuExit"] as ControlTemplate,
                    ToolTip = "Закрыть документ",
                    Command = vm.CloseWindowCommand
                }
            };
            return ret;
        }

        public static ObservableCollection<MenuButtonInfo> KontragentCardRightBar(RSWindowViewModelBase vm)
        {
            var prn = new MenuButtonInfo
            {
                Name = "Print",
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Content = Application.Current.Resources["menuPrinter"] as ControlTemplate,
                ToolTip = "Печать"
            };
            var ret = new ObservableCollection<MenuButtonInfo>
            {
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuRefresh"] as ControlTemplate,
                    ToolTip = "Обновить",
                    Command = vm.RefreshDataCommand
                },
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuSave"] as ControlTemplate,
                    ToolTip = "Сохранить изменения",
                    Command = vm.SaveDataCommand
                },
                prn,
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuExit"] as ControlTemplate,
                    ToolTip = "Закрыть документ",
                    Command = vm.CloseWindowCommand
                }
            };
            return ret;
        }

        public static ObservableCollection<MenuButtonInfo> KontragentCardAddFormRightMenu(RSWindowViewModelBase vm)
        {
            var prn = new MenuButtonInfo
            {
                Name = "Print",
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Content = Application.Current.Resources["menuPrinter"] as ControlTemplate,
                ToolTip = "Печать"
            };
            var docDelete = new MenuButtonInfo
            {
                Name = "Delete",
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Content = Application.Current.Resources["menuDocDelete"] as ControlTemplate,
                ToolTip = "Удалить документ",
                Command = vm.DoсDeleteCommand
            };
            var ret = new ObservableCollection<MenuButtonInfo>
            {
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuRefresh"] as ControlTemplate,
                    ToolTip = "Обновить",
                    Command = vm.RefreshDataCommand
                },
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuSave"] as ControlTemplate,
                    ToolTip = "Сохранить изменения",
                    Command = vm.SaveDataCommand
                },
                docDelete,
                prn,
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuExit"] as ControlTemplate,
                    ToolTip = "Закрыть документ",
                    Command = vm.CloseWindowCommand
                }
            };
            return ret;
        }

        public static ObservableCollection<MenuButtonInfo> DocWithRowsLeftBar(RSWindowViewModelBase vm)
        {
            var ret = BaseLeftBar(vm);
            ret.Add(
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuRedo"] as ControlTemplate,
                    ToolTip = "Восстановить удаленные строки",
                    Command = vm.RedoCommand
                });
            return ret;
        }

        public static ObservableCollection<MenuButtonInfo> BaseLeftBar(RSWindowViewModelBase vm)
        {
            var ret = new ObservableCollection<MenuButtonInfo>
            {
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuOptions"] as ControlTemplate,
                    ToolTip = "Настройки",
                    SubMenu = new ObservableCollection<MenuButtonInfo>
                    {
                        new MenuButtonInfo
                        {
                            Image = Application.Current.Resources["imageResetLayout"] as DrawingImage,
                            Caption = "Переустановить разметку",
                            Command = vm.ResetLayoutCommand
                        }
                    }
                }
            };
            return ret;
        }

        public static ObservableCollection<MenuButtonInfo> RightBarOfKontragentReferens(RSWindowViewModelBase vm)
        {
            var prn = new MenuButtonInfo
            {
                Name = "Print",
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Content = Application.Current.Resources["menuPrinter"] as ControlTemplate,
                ToolTip = "Печать"
            };
            var docNew = new MenuButtonInfo
            {
                Name = "New",
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Content = Application.Current.Resources["menuDocumentAdd"] as ControlTemplate,
                ToolTip = "Новый документ"
            };
            docNew.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "Пустой документ",
                Image = Application.Current.Resources["imageDocumentNewEmpty"] as DrawingImage,
                Command = vm.DocNewEmptyCommand
            });
            docNew.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "Копия текущего",
                Image = Application.Current.Resources["imageDocumentNewCopy"] as DrawingImage,
                Command = vm.DocNewCopyCommand
            });
            var docDelete = new MenuButtonInfo
            {
                Name = "Delete",
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Content = Application.Current.Resources["menuDocDelete"] as ControlTemplate,
                ToolTip = "Удалить документ",
                Command = vm.DoсDeleteCommand
            };
            var ret = new ObservableCollection<MenuButtonInfo>
            {
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuRefresh"] as ControlTemplate,
                    ToolTip = "Обновить список документов",
                    Command = vm.RefreshDataCommand
                },
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuDocumentOpen"] as ControlTemplate,
                    ToolTip = "Открыть выбранный документ",
                    Command = vm.DocumentOpenCommand
                },
                docNew,
                docDelete,
                prn,
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuExit"] as ControlTemplate,
                    ToolTip = "Закрыть поиск",
                    Command = vm.CloseWindowCommand
                }
            };
            return ret;
        }

        public static ObservableCollection<MenuButtonInfo> StandartSearchRightBar(RSWindowViewModelBase vm)
        {
            var prn = new MenuButtonInfo
            {
                Name = "Print",
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Content = Application.Current.Resources["menuPrinter"] as ControlTemplate,
                ToolTip = "Печать"
            };
            var docNew = new MenuButtonInfo
            {
                Name = "New",
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Content = Application.Current.Resources["menuDocumentAdd"] as ControlTemplate,
                ToolTip = "Новый документ"
            };
            docNew.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "Пустой документ",
                Image = Application.Current.Resources["imageDocumentNewEmpty"] as DrawingImage,
                Command = vm.DocNewEmptyCommand
            });
            docNew.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "С текущими реквизитами",
                Image = Application.Current.Resources["imageDocumentNewRequisite"] as DrawingImage,
                Command = vm.DocNewCopyRequisiteCommand
            });
            docNew.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "Копия текущего",
                Image = Application.Current.Resources["imageDocumentNewCopy"] as DrawingImage,
                Command = vm.DocNewCopyCommand
            });
            var ret = new ObservableCollection<MenuButtonInfo>
            {
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuRefresh"] as ControlTemplate,
                    ToolTip = "Обновить список документов",
                    Command = vm.RefreshDataCommand
                },
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuDocumentOpen"] as ControlTemplate,
                    ToolTip = "Открыть выбранный документ",
                    Command = vm.DocumentOpenCommand
                },
                docNew,
                prn,
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuExit"] as ControlTemplate,
                    ToolTip = "Закрыть поиск",
                    Command = vm.CloseWindowCommand
                }
            };
            return ret;
        }

        public static ObservableCollection<MenuButtonInfo> StandartAsyncSearchRightBar(RSWindowViewModelBase vm)
        {
            var prn = new MenuButtonInfo
            {
                Name = "Print",
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Content = Application.Current.Resources["menuPrinter"] as ControlTemplate,
                ToolTip = "Печать"
            };
            var docNew = new MenuButtonInfo
            {
                Name = "New",
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Content = Application.Current.Resources["menuDocumentAdd"] as ControlTemplate,
                ToolTip = "Новый документ"
            };
            docNew.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "Пустой документ",
                Image = Application.Current.Resources["imageDocumentNewEmpty"] as DrawingImage,
                Command = vm.DocNewEmptyCommand
            });
            docNew.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "С текущими реквизитами",
                Image = Application.Current.Resources["imageDocumentNewRequisite"] as DrawingImage,
                Command = vm.DocNewCopyRequisiteCommand
            });
            docNew.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "Копия текущего",
                Image = Application.Current.Resources["imageDocumentNewCopy"] as DrawingImage,
                Command = vm.DocNewCopyCommand
            });
            var ret = new ObservableCollection<MenuButtonInfo>
            {
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuRefresh"] as ControlTemplate,
                    ToolTip = "Обновить список документов",
                    Command = vm.AsyncRefreshDataCommand
                },
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuDocumentOpen"] as ControlTemplate,
                    ToolTip = "Открыть выбранный документ",
                    Command = vm.DocumentOpenCommand
                },
                docNew,
                prn,
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuExit"] as ControlTemplate,
                    ToolTip = "Закрыть поиск",
                    Command = vm.CloseWindowCommand
                }
            };
            return ret;
        }

        /// <summary>
        ///     Кнопки: Обновить / Закрыть
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        public static ObservableCollection<MenuButtonInfo> StandartInfoRightBar(RSWindowViewModelBase vm)
        {
            return new ObservableCollection<MenuButtonInfo>
            {
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuRefresh"] as ControlTemplate,
                    ToolTip = "Обновить",
                    Command = vm.RefreshDataCommand
                },
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuExit"] as ControlTemplate,
                    ToolTip = "Закрыть форму",
                    Command = vm.CloseWindowCommand
                }
            };
        }

        public static ObservableCollection<MenuButtonInfo> TableEditRightBar(RSWindowViewModelBase vm)
        {
            return new ObservableCollection<MenuButtonInfo>
            {
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuRefresh"] as ControlTemplate,
                    ToolTip = "Обновить",
                    Command = vm.RefreshDataCommand
                },
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuSave"] as ControlTemplate,
                    ToolTip = "Сохранить изменения",
                    Command = vm.SaveDataCommand
                },
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuExit"] as ControlTemplate,
                    ToolTip = "Закрыть форму",
                    Command = vm.CloseWindowCommand
                }
            };
        }

        public static ObservableCollection<MenuButtonInfo> ExitOnlyRightBar(RSWindowViewModelBase vm)
        {
            return new ObservableCollection<MenuButtonInfo>
            {
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuExit"] as ControlTemplate,
                    ToolTip = "Закрыть форму",
                    Command = vm.CloseWindowCommand
                }
            };
        }

        public static ObservableCollection<MenuButtonInfo> RefreshOnlyRightBar(RSWindowViewModelBase vm)
        {
            return new ObservableCollection<MenuButtonInfo>
            {
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuRefresh"] as ControlTemplate,
                    ToolTip = "Обновить данные",
                    Command = vm.RefreshDataCommand
                }
            };
        }

        public static ObservableCollection<MenuButtonInfo> CardRightBar(RSWindowViewModelBase vm)
        {
            return new ObservableCollection<MenuButtonInfo>
            {
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuRefresh"] as ControlTemplate,
                    ToolTip = "Обновить",
                    Command = vm.RefreshDataCommand
                },
                new MenuButtonInfo
                {
                    Name = "Delete",
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuDocDelete"] as ControlTemplate,
                    ToolTip = "Удалить документ"
                },
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuSave"] as ControlTemplate,
                    ToolTip = "Сохранить изменения",
                    Command = vm.SaveDataCommand
                },
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuExit"] as ControlTemplate,
                    ToolTip = "Закрыть форму",
                    Command = vm.CloseWindowCommand
                }
            };
        }

        public static ObservableCollection<MenuButtonInfo> ReferenceRightBar(RSWindowViewModelBase vm)
        {
            return new ObservableCollection<MenuButtonInfo>
            {
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuRefresh"] as ControlTemplate,
                    ToolTip = "Обновить",
                    Command = vm.RefreshDataCommand
                },
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuSave"] as ControlTemplate,
                    ToolTip = "Сохранить изменения",
                    Command = vm.SaveDataCommand
                },
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuExit"] as ControlTemplate,
                    ToolTip = "Закрыть форму",
                    Command = vm.CloseWindowCommand
                }
            };
        }

        public static ObservableCollection<MenuButtonInfo> StandartDocWithDeleteRightBar(KursBaseControlViewModel vm)
        {
            var prn = new MenuButtonInfo
            {
                Name = "Print",
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Content = Application.Current.Resources["menuPrinter"] as ControlTemplate,
                ToolTip = "Печать"
            };
            var docNew = new MenuButtonInfo
            {
                Name = "New",
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Content = Application.Current.Resources["menuDocumentAdd"] as ControlTemplate,
                ToolTip = "Новый документ"
            };
            docNew.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "Пустой документ",
                Image = Application.Current.Resources["imageDocumentNewEmpty"] as DrawingImage,
                Command = vm.DocNewEmptyCommand
            });
            docNew.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "С текущими реквизитами",
                Image = Application.Current.Resources["imageDocumentNewRequisite"] as DrawingImage,
                Command = vm.DocNewCopyRequisiteCommand
            });
            docNew.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "Копия текущего",
                Image = Application.Current.Resources["imageDocumentNewCopy"] as DrawingImage,
                Command = vm.DocNewCopyCommand
            });
            var docDelete = new MenuButtonInfo
            {
                Name = "Delete",
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Content = Application.Current.Resources["menuDocDelete"] as ControlTemplate,
                ToolTip = "Удалить документ",
                Command = vm.DoсDeleteCommand
            };
            var ret = new ObservableCollection<MenuButtonInfo>
            {
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuRefresh"] as ControlTemplate,
                    ToolTip = "Обновить",
                    Command = vm.RefreshDataCommand
                },
                docNew,
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuSave"] as ControlTemplate,
                    ToolTip = "Сохранить изменения",
                    Command = vm.SaveDataCommand
                },
                docDelete,
                prn,
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuExit"] as ControlTemplate,
                    ToolTip = "Закрыть документ",
                    Command = vm.CloseWindowCommand
                }
            };
            return ret;
        }

        public static ObservableCollection<MenuButtonInfo> BankOpertionsRightBar(KursBaseControlViewModel vm)
        {
            var docNew = new MenuButtonInfo
            {
                Name = "New",
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Content = Application.Current.Resources["menuDocumentAdd"] as ControlTemplate,
                ToolTip = "Новый документ"
            };
            docNew.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "Пустой документ",
                Image = Application.Current.Resources["imageDocumentNewEmpty"] as DrawingImage,
                Command = vm.DocNewEmptyCommand
            });
            docNew.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "Копия текущего",
                Image = Application.Current.Resources["imageDocumentNewCopy"] as DrawingImage,
                Command = vm.DocNewCopyCommand
            });
            //var docDelete = new MenuButtonInfo
            //{
            //    Name = "Delete",
            //    Alignment = Dock.Right,
            //    HAlignment = HorizontalAlignment.Right,
            //    Content = Application.Current.Resources["menuDocDelete"] as ControlTemplate,
            //    ToolTip = "Удалить документ",
            //    Command = vm.DoсDeleteCommand
            //};
            var ret = new ObservableCollection<MenuButtonInfo>
            {
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuRefresh"] as ControlTemplate,
                    ToolTip = "Обновить",
                    Command = vm.RefreshDataCommand
                },
                docNew,
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuExit"] as ControlTemplate,
                    ToolTip = "Закрыть документ",
                    Command = vm.CloseWindowCommand
                }
            };
            return ret;
        }

        public static ObservableCollection<MenuButtonInfo> KontragentCardRightBar(KursBaseControlViewModel vm)
        {
            var prn = new MenuButtonInfo
            {
                Name = "Print",
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Content = Application.Current.Resources["menuPrinter"] as ControlTemplate,
                ToolTip = "Печать"
            };
            var ret = new ObservableCollection<MenuButtonInfo>
            {
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuRefresh"] as ControlTemplate,
                    ToolTip = "Обновить",
                    Command = vm.RefreshDataCommand
                },
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuSave"] as ControlTemplate,
                    ToolTip = "Сохранить изменения",
                    Command = vm.SaveDataCommand
                },
                prn,
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuExit"] as ControlTemplate,
                    ToolTip = "Закрыть документ",
                    Command = vm.CloseWindowCommand
                }
            };
            return ret;
        }

        public static ObservableCollection<MenuButtonInfo> KontragentCardAddFormRightMenu(KursBaseControlViewModel vm)
        {
            var prn = new MenuButtonInfo
            {
                Name = "Print",
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Content = Application.Current.Resources["menuPrinter"] as ControlTemplate,
                ToolTip = "Печать"
            };
            var docDelete = new MenuButtonInfo
            {
                Name = "Delete",
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Content = Application.Current.Resources["menuDocDelete"] as ControlTemplate,
                ToolTip = "Удалить документ",
                Command = vm.DoсDeleteCommand
            };
            var ret = new ObservableCollection<MenuButtonInfo>
            {
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuRefresh"] as ControlTemplate,
                    ToolTip = "Обновить",
                    Command = vm.RefreshDataCommand
                },
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuSave"] as ControlTemplate,
                    ToolTip = "Сохранить изменения",
                    Command = vm.SaveDataCommand
                },
                docDelete,
                prn,
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuExit"] as ControlTemplate,
                    ToolTip = "Закрыть документ",
                    Command = vm.CloseWindowCommand
                }
            };
            return ret;
        }

        public static ObservableCollection<MenuButtonInfo> DocWithRowsLeftBar(KursBaseControlViewModel vm)
        {
            var ret = BaseLeftBar(vm);
            ret.Add(
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuRedo"] as ControlTemplate,
                    ToolTip = "Восстановить удаленные строки",
                    Command = vm.RedoCommand
                });
            return ret;
        }

        public static ObservableCollection<MenuButtonInfo> BaseLeftBar(KursBaseControlViewModel vm)
        {
            var ret = new ObservableCollection<MenuButtonInfo>
            {
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuOptions"] as ControlTemplate,
                    ToolTip = "Настройки",
                    SubMenu = new ObservableCollection<MenuButtonInfo>
                    {
                        new MenuButtonInfo
                        {
                            Image = Application.Current.Resources["imageResetLayout"] as DrawingImage,
                            Caption = "Переустановить разметку",
                            Command = vm.ResetLayoutCommand
                        }
                    }
                }
            };
            return ret;
        }

        public static ObservableCollection<MenuButtonInfo> RightBarOfKontragentReferens(KursBaseControlViewModel vm)
        {
            var prn = new MenuButtonInfo
            {
                Name = "Print",
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Content = Application.Current.Resources["menuPrinter"] as ControlTemplate,
                ToolTip = "Печать"
            };
            var docNew = new MenuButtonInfo
            {
                Name = "New",
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Content = Application.Current.Resources["menuDocumentAdd"] as ControlTemplate,
                ToolTip = "Новый документ"
            };
            docNew.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "Пустой документ",
                Image = Application.Current.Resources["imageDocumentNewEmpty"] as DrawingImage,
                Command = vm.DocNewEmptyCommand
            });
            docNew.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "Копия текущего",
                Image = Application.Current.Resources["imageDocumentNewCopy"] as DrawingImage,
                Command = vm.DocNewCopyCommand
            });
            var docDelete = new MenuButtonInfo
            {
                Name = "Delete",
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Content = Application.Current.Resources["menuDocDelete"] as ControlTemplate,
                ToolTip = "Удалить документ",
                Command = vm.DoсDeleteCommand
            };
            var ret = new ObservableCollection<MenuButtonInfo>
            {
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuRefresh"] as ControlTemplate,
                    ToolTip = "Обновить список документов",
                    Command = vm.RefreshDataCommand
                },
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuDocumentOpen"] as ControlTemplate,
                    ToolTip = "Открыть выбранный документ",
                    Command = vm.DocumentOpenCommand
                },
                docNew,
                docDelete,
                prn,
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuExit"] as ControlTemplate,
                    ToolTip = "Закрыть поиск",
                    Command = vm.CloseWindowCommand
                }
            };
            return ret;
        }

        public static ObservableCollection<MenuButtonInfo> StandartSearchRightBar(KursBaseControlViewModel vm)
        {
            var docNew = new MenuButtonInfo
            {
                Name = "New",
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Content = Application.Current.Resources["menuDocumentAdd"] as ControlTemplate,
                ToolTip = "Новый документ"
            };
            docNew.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "Пустой документ",
                Image = Application.Current.Resources["imageDocumentNewEmpty"] as DrawingImage,
                Command = vm.DocNewEmptyCommand
            });
            docNew.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "С текущими реквизитами",
                Image = Application.Current.Resources["imageDocumentNewRequisite"] as DrawingImage,
                Command = vm.DocNewCopyRequisiteCommand
            });
            docNew.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "Копия текущего",
                Image = Application.Current.Resources["imageDocumentNewCopy"] as DrawingImage,
                Command = vm.DocNewCopyCommand
            });
            var ret = new ObservableCollection<MenuButtonInfo>
            {
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuRefresh"] as ControlTemplate,
                    ToolTip = "Обновить список документов",
                    Command = vm.RefreshDataCommand
                },
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuDocumentOpen"] as ControlTemplate,
                    ToolTip = "Открыть выбранный документ",
                    Command = vm.DocumentOpenCommand
                },
                docNew,
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuExit"] as ControlTemplate,
                    ToolTip = "Закрыть поиск",
                    Command = vm.CloseWindowCommand
                }
            };
            return ret;
        }

        /// <summary>
        ///     Кнопки: Обновить / Закрыть
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        public static ObservableCollection<MenuButtonInfo> StandartInfoRightBar(KursBaseControlViewModel vm)
        {
            return new ObservableCollection<MenuButtonInfo>
            {
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuRefresh"] as ControlTemplate,
                    ToolTip = "Обновить",
                    Command = vm.RefreshDataCommand
                },
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuExit"] as ControlTemplate,
                    ToolTip = "Закрыть форму",
                    Command = vm.CloseWindowCommand
                }
            };
        }

        public static ObservableCollection<MenuButtonInfo> TableEditRightBar(KursBaseControlViewModel vm)
        {
            return new ObservableCollection<MenuButtonInfo>
            {
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuRefresh"] as ControlTemplate,
                    ToolTip = "Обновить",
                    Command = vm.RefreshDataCommand
                },
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuSave"] as ControlTemplate,
                    ToolTip = "Сохранить изменения",
                    Command = vm.SaveDataCommand
                },
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuExit"] as ControlTemplate,
                    ToolTip = "Закрыть форму",
                    Command = vm.CloseWindowCommand
                }
            };
        }

        public static ObservableCollection<MenuButtonInfo> ExitOnlyRightBar(KursBaseControlViewModel vm)
        {
            return new ObservableCollection<MenuButtonInfo>
            {
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuExit"] as ControlTemplate,
                    ToolTip = "Закрыть форму",
                    Command = vm.CloseWindowCommand
                }
            };
        }

        public static ObservableCollection<MenuButtonInfo> RefreshOnlyRightBar(KursBaseControlViewModel vm)
        {
            return new ObservableCollection<MenuButtonInfo>
            {
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuRefresh"] as ControlTemplate,
                    ToolTip = "Обновить данные",
                    Command = vm.RefreshDataCommand
                }
            };
        }

        public static ObservableCollection<MenuButtonInfo> CardRightBar(KursBaseControlViewModel vm)
        {
            return new ObservableCollection<MenuButtonInfo>
            {
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuRefresh"] as ControlTemplate,
                    ToolTip = "Обновить",
                    Command = vm.RefreshDataCommand
                },
                new MenuButtonInfo
                {
                    Name = "Delete",
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuDocDelete"] as ControlTemplate,
                    ToolTip = "Удалить документ"
                },
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuSave"] as ControlTemplate,
                    ToolTip = "Сохранить изменения",
                    Command = vm.SaveDataCommand
                },
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuExit"] as ControlTemplate,
                    ToolTip = "Закрыть форму",
                    Command = vm.CloseWindowCommand
                }
            };
        }

        public static ObservableCollection<MenuButtonInfo> ReferenceRightBar(KursBaseControlViewModel vm)
        {
            return new ObservableCollection<MenuButtonInfo>
            {
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuRefresh"] as ControlTemplate,
                    ToolTip = "Обновить",
                    Command = vm.RefreshDataCommand
                },
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuSave"] as ControlTemplate,
                    ToolTip = "Сохранить изменения",
                    Command = vm.SaveDataCommand
                },
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuExit"] as ControlTemplate,
                    ToolTip = "Закрыть форму",
                    Command = vm.CloseWindowCommand
                }
            };
        }
    }
}