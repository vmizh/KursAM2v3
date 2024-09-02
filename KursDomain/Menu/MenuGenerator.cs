using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using KursDomain.ViewModel.Base2;

namespace KursDomain.Menu;

public static class MenuGenerator
{
    public static ObservableCollection<MenuButtonInfo> StandartDialogRightBar(IDialogOperation vm)
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

    public static ObservableCollection<MenuButtonInfo> StandartDocRightBar(IFormCommands vm)
    {
        var prn = new MenuButtonInfo
        {
            Name = "Print",
            Alignment = Dock.Right,
            HAlignment = HorizontalAlignment.Right,
            Content = Application.Current.Resources["menuPrinter"] as ControlTemplate,
            ToolTip = "Печать",
            Command = vm.PrintCommand
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

    public static ObservableCollection<MenuButtonInfo> StandartDocWithDeleteRightBar(
        IFormCommands vm)
    {
        var prn = new MenuButtonInfo
        {
            Name = "Print",
            Alignment = Dock.Right,
            HAlignment = HorizontalAlignment.Right,
            Content = Application.Current.Resources["menuPrinter"] as ControlTemplate,
            ToolTip = "Печать",
            Command = vm.PrintCommand
        };
        var docNew = new MenuButtonInfo
        {
            Name = "New",
            Alignment = Dock.Right,
            HAlignment = HorizontalAlignment.Right,
            Content = Application.Current.Resources["menuDocumentAdd"] as ControlTemplate,
            ToolTip = "Новый документ",
            Command = vm.DocNewCommand
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

    public static ObservableCollection<MenuButtonInfo> StandartReestrRightBar(IFormCommands vm)
    {
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

    public static ObservableCollection<MenuButtonInfo> BankOpertionsRightBar(IFormCommands vm)
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

    public static ObservableCollection<MenuButtonInfo> KontragentCardRightBar(IFormCommands vm)
    {
        var prn = new MenuButtonInfo
        {
            Name = "Print",
            Alignment = Dock.Right,
            HAlignment = HorizontalAlignment.Right,
            Content = Application.Current.Resources["menuPrinter"] as ControlTemplate,
            ToolTip = "Печать",
            Command = vm.PrintCommand
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

    public static ObservableCollection<MenuButtonInfo> KontragentCardAddFormRightMenu(
        IFormCommands vm)
    {
        var prn = new MenuButtonInfo
        {
            Name = "Print",
            Alignment = Dock.Right,
            HAlignment = HorizontalAlignment.Right,
            Content = Application.Current.Resources["menuPrinter"] as ControlTemplate,
            ToolTip = "Печать",
            Command = vm.PrintCommand
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

    public static ObservableCollection<MenuButtonInfo> DocWithRowsLeftBar(IFormCommands vm)
    {
        var ret = BaseLeftBar(vm);
        //ret.Add(
        //    new MenuButtonInfo
        //    {
        //        Alignment = Dock.Right,
        //        HAlignment = HorizontalAlignment.Right,
        //        Content = Application.Current.Resources["menuRedo"] as ControlTemplate,
        //        ToolTip = "Восстановить удаленные строки",
        //        Command = vm.RedoCommand
        //    });
        return ret;
    }

    public static ObservableCollection<MenuButtonInfo> DocWithCreateLinkDocumentLeftBar(IFormCommands vm)
    {
        var ret = BaseLeftBar(vm);
        ret[0].SubMenu.Add(
            new MenuButtonInfo
            {
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Caption = "Создать связанный приходно/расходный документ",
                Image = Application.Current.Resources["ExportDrawingImage"] as DrawingImage,
                ToolTip = "Создать связанный приходно/расходный документ",
                Command = vm.CreateLinkDocumentCommand
            });
        return ret;
    }
    //ExportDrawingImage

    public static ObservableCollection<MenuButtonInfo> BaseLeftBar(IFormCommands vm)
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
                    },
                    new MenuButtonInfo
                    {
                        Image = Application.Current.Resources["imageDone"] as DrawingImage,
                        Caption = "Показать историю изменений",
                        Command = vm.ShowHistoryCommand
                    }
                }
            }
        };
        return ret;
    }

    public static ObservableCollection<MenuButtonInfo> BaseLeftBar(IFormSearchCommands vm)
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
                    },
                }
            }
        };
        return ret;
    }

    public static ObservableCollection<MenuButtonInfo> RightBarOfKontragentReferens(
        IFormCommands vm)
    {
        var prn = new MenuButtonInfo
        {
            Name = "Print",
            Alignment = Dock.Right,
            HAlignment = HorizontalAlignment.Right,
            Content = Application.Current.Resources["menuPrinter"] as ControlTemplate,
            ToolTip = "Печать",
            Command = vm.PrintCommand
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

    public static ObservableCollection<MenuButtonInfo> StandartSearchRightBar(IFormCommands vm)
    {
        var prn = new MenuButtonInfo
        {
            Name = "Print",
            Alignment = Dock.Right,
            HAlignment = HorizontalAlignment.Right,
            Content = Application.Current.Resources["menuPrinter"] as ControlTemplate,
            ToolTip = "Печать",
            Command = vm.PrintCommand
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

    public static ObservableCollection<MenuButtonInfo> StandartSearchRightBar(IFormSearchCommands vm)
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

    public static ObservableCollection<MenuButtonInfo> StandartAsyncSearchRightBar(
        IFormCommands vm)
    {
        var prn = new MenuButtonInfo
        {
            Name = "Print",
            Alignment = Dock.Right,
            HAlignment = HorizontalAlignment.Right,
            Content = Application.Current.Resources["menuPrinter"] as ControlTemplate,
            ToolTip = "Печать",
            Command = vm.PrintCommand
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

    /// <summary>
    ///     Кнопки: Обновить / Закрыть
    /// </summary>
    /// <param name="vm"></param>
    /// <returns></returns>
    public static ObservableCollection<MenuButtonInfo> StandartInfoRightBar(IFormCommands vm)
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

    public static ObservableCollection<MenuButtonInfo> StandartInfoRightBar(IFormSearchCommands vm)
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

    public static ObservableCollection<MenuButtonInfo> TableEditRightBar(IFormCommands vm)
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

    public static ObservableCollection<MenuButtonInfo> ExitOnlyRightBar(IFormCommands vm)
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

    public static ObservableCollection<MenuButtonInfo> RefreshOnlyRightBar(IFormCommands vm)
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

    public static ObservableCollection<MenuButtonInfo> CardRightBar(IFormCommands vm)
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

    public static ObservableCollection<MenuButtonInfo> ReferenceRightBar(IFormCommands vm)
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

    public static ObservableCollection<MenuButtonInfo> DialogRightBar(IFormCommands vm)
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
                Content = Application.Current.Resources["menuDocumentOpen"] as ControlTemplate,
                ToolTip = "Открыть выбранный документ",
                Command = vm.DocumentOpenCommand
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

    public static ObservableCollection<MenuButtonInfo> DialogStandartBar(IDialogOperation vm)
    {
        return new ObservableCollection<MenuButtonInfo>
        {
            new MenuButtonInfo
            {
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Content = Application.Current.Resources["menuDone"] as ControlTemplate,
                ToolTip = "Выбрать текущую позицию",
                Command = vm.OkCommand
            },
            new MenuButtonInfo
            {
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Content = Application.Current.Resources["menuCancel"] as ControlTemplate,
                ToolTip = "Отменить выбор",
                Command = vm.CancelCommand
            }
        };
    }


    public static ObservableCollection<MenuButtonInfo> NomenklCardRightBar(IFormCommands vm)
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
                ToolTip = "Закрыть документ",
                Command = vm.CloseWindowCommand
            }
        };
        return ret;
    }
}
