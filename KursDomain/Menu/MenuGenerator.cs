﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using KursDomain.ViewModel.Base2;

namespace KursDomain.Menu;

public static class MenuGenerator
{
    private static bool IsMenuItemVisible(MenuGeneratorItemVisibleEnum menuType,
        Dictionary<MenuGeneratorItemVisibleEnum, bool> menuVisibleSettings)
    {
        return menuVisibleSettings.ContainsKey(menuType) && menuVisibleSettings[menuType];
    }

    public static ObservableCollection<MenuButtonInfo> StandartDialogRightBar(IDialogOperation vm,
        Dictionary<MenuGeneratorItemVisibleEnum, bool> menuVisibleSettings = null)
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

    public static ObservableCollection<MenuButtonInfo> StandartDocRightBar(IFormCommands vm,
        Dictionary<MenuGeneratorItemVisibleEnum, bool> menuVisibleSettings = null)
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
        IFormCommands vm,
        Dictionary<MenuGeneratorItemVisibleEnum, bool> menuVisibleSettings = null)
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

    public static ObservableCollection<MenuButtonInfo> StandartReestrRightBar(IFormCommands vm,
        Dictionary<MenuGeneratorItemVisibleEnum, bool> menuVisibleSettings = null)
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

    public static ObservableCollection<MenuButtonInfo> BankOpertionsRightBar(IFormCommands vm, Dictionary<MenuGeneratorItemVisibleEnum, bool> menuVisibleSettings = null)
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

    public static ObservableCollection<MenuButtonInfo> KontragentCardRightBar(IFormCommands vm,
        Dictionary<MenuGeneratorItemVisibleEnum, bool> menuVisibleSettings = null)
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
        IFormCommands vm, Dictionary<MenuGeneratorItemVisibleEnum, bool> menuVisibleSettings = null)
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

    public static ObservableCollection<MenuButtonInfo> DocWithRowsLeftBar(IFormCommands vm,
        Dictionary<MenuGeneratorItemVisibleEnum, bool> menuVisibleSettings = null)
    {
        var ret = BaseLeftBar(vm);
        return ret;
    }

    public static ObservableCollection<MenuButtonInfo> DocWithCreateLinkDocumentLeftBar(IFormCommands vm,
        Dictionary<MenuGeneratorItemVisibleEnum, bool> menuVisibleSettings = null)
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

    public static ObservableCollection<MenuButtonInfo> DocWithCustomizeFormDocumentLeftBar(IFormCommands vm,
        Dictionary<MenuGeneratorItemVisibleEnum, bool> menuVisibleSettings = null)
    {
        var ret = BaseLeftBar(vm);
        ret[0].SubMenu.Add(
            new MenuButtonInfo
            {
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Caption = "Форматирование/просмотр формы",
                Image = Application.Current.Resources["CheckStadartDrawingImage"] as DrawingImage,
                ToolTip = "Создать связанный приходно/расходный документ",
                Command = vm.SetCustomizeFormDocumentCommand
            });
        ret[0].SubMenu.Add(
            new MenuButtonInfo
            {
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Caption = "Сохранить разметку",
                Image = Application.Current.Resources["DocumentOptionsDrawingImage"] as DrawingImage,
                ToolTip = "Сохранить новый формат разметки",
                Command = vm.SaveCustomizedFormDocumentCommand
            });
        return ret;
    }
    //ExportDrawingImage

    public static ObservableCollection<MenuButtonInfo> BaseLeftBar(IFormCommands vm,
        Dictionary<MenuGeneratorItemVisibleEnum, bool> menuVisibleSettings = null)
    {
        if (menuVisibleSettings is null)
        {
            menuVisibleSettings = new Dictionary<MenuGeneratorItemVisibleEnum, bool>
                {
                    [MenuGeneratorItemVisibleEnum.AddSearchlist] = false
                };
        }

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
        if (IsMenuItemVisible(MenuGeneratorItemVisibleEnum.AddSearchlist, menuVisibleSettings))
        {
            ret.Add(new MenuButtonInfo
            {
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Content = Application.Current.Resources["menuAddSearchList"] as ControlTemplate,
                ToolTip = "Создать дубликат поиска",
                Command = vm.AddSearchListCommand
            });
        }

        return ret;
    }

    public static ObservableCollection<MenuButtonInfo> BaseLeftBar(IFormSearchCommands vm,
        Dictionary<MenuGeneratorItemVisibleEnum, bool> menuVisibleSettings = null)
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

    public static ObservableCollection<MenuButtonInfo> RightBarOfKontragentReferens(
        IFormCommands vm, Dictionary<MenuGeneratorItemVisibleEnum, bool> menuVisibleSettings = null)
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

    public static ObservableCollection<MenuButtonInfo> StandartSearchRightBar(IFormCommands vm,
        Dictionary<MenuGeneratorItemVisibleEnum, bool> menuVisibleSettings = null)
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

    public static ObservableCollection<MenuButtonInfo> StandartSearchRightBar(IFormSearchCommands vm,
        Dictionary<MenuGeneratorItemVisibleEnum, bool> menuVisibleSettings = null)
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
        IFormCommands vm, Dictionary<MenuGeneratorItemVisibleEnum, bool> menuVisibleSettings = null)
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
    public static ObservableCollection<MenuButtonInfo> StandartInfoRightBar(IFormCommands vm,
        Dictionary<MenuGeneratorItemVisibleEnum, bool> menuVisibleSettings = null)
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

    public static ObservableCollection<MenuButtonInfo> StandartInfoRightBar(IFormSearchCommands vm,
        Dictionary<MenuGeneratorItemVisibleEnum, bool> menuVisibleSettings = null)
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

    public static ObservableCollection<MenuButtonInfo> TableEditRightBar(IFormCommands vm,
        Dictionary<MenuGeneratorItemVisibleEnum, bool> menuVisibleSettings = null)
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

    public static ObservableCollection<MenuButtonInfo> ExitOnlyRightBar(IFormCommands vm,
        Dictionary<MenuGeneratorItemVisibleEnum, bool> menuVisibleSettings = null)
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

    public static ObservableCollection<MenuButtonInfo> RefreshOnlyRightBar(IFormCommands vm,
        Dictionary<MenuGeneratorItemVisibleEnum, bool> menuVisibleSettings = null)
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
    public static ObservableCollection<MenuButtonInfo> RefreshExitOnlyRightBar(IFormCommands vm,
        Dictionary<MenuGeneratorItemVisibleEnum, bool> menuVisibleSettings = null)
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

    public static ObservableCollection<MenuButtonInfo> CardRightBar(IFormCommands vm,
        Dictionary<MenuGeneratorItemVisibleEnum, bool> menuVisibleSettings = null)
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

    public static ObservableCollection<MenuButtonInfo> ReferenceRightBar(IFormCommands vm,
        Dictionary<MenuGeneratorItemVisibleEnum, bool> menuVisibleSettings = null)
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

    public static ObservableCollection<MenuButtonInfo> DialogRightBar(IFormCommands vm,
        Dictionary<MenuGeneratorItemVisibleEnum, bool> menuVisibleSettings = null)
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

    public static ObservableCollection<MenuButtonInfo> DialogStandartBar(IDialogOperation vm,
        Dictionary<MenuGeneratorItemVisibleEnum, bool> menuVisibleSettings = null)
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


    public static ObservableCollection<MenuButtonInfo> NomenklCardRightBar(IFormCommands vm,
        Dictionary<MenuGeneratorItemVisibleEnum, bool> menuVisibleSettings = null)
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
