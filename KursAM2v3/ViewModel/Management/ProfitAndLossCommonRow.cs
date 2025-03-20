using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Core.Helper;
using Core.ViewModel.Base;
using DevExpress.Mvvm.DataAnnotations;

namespace KursAM2.ViewModel.Management;

[MetadataType(typeof(DataAnnotationProfitAndLossCommonRow))]
public class ProfitAndLossCommonRow : RSViewModelBase
{
    //'35c9783e-e19f-452b-8479-d6f022444552'		'e7da6232-caa0-4358-9bae-5d96c2ee248a'		'Балансовые операции' 21
    public readonly Guid BalansOpeeration = Guid.Parse("{FB303EF4-7C98-48EE-902E-2D1727891AC3}");

    //'0ad95635-a46d-49f2-ae78-cbdf52bd6e27'		'16524d17-451c-4149-8813-bdb9875f759a'	'Банковские счета'			2
    // '920b2ab6-1984-49a9-b5e7-18407221f620'	'2f1fd81f-a7be-4830-93eb-9b3a9a362b2b'		'Банковские счета'
    public readonly Guid Bank = Guid.Parse("{2F241929-886F-4877-979E-84D384E03169}");

    //'a084b37a-d942-4b7f-9ae9-3c3aaa0f4475'		'16524d17-451c-4149-8813-bdb9875f759a'	'Касса'								3
    // 'c0aedec3-15cd-4d5b-8e37-0fe1edb5c6ff'		'2f1fd81f-a7be-4830-93eb-9b3a9a362b2b'		'Касса'
    public readonly Guid Cash = Guid.Parse("{08DCD892-297D-44E3-9452-A3444BDE8950}");

    //'836cc414-bef4-4371-a253-47d2e8f4535f'		'a938446c-ab19-45c3-8dfd-0f24ab08df49'		'Проценты по кассовым ордерам'          15
    // 'e9d63300-829b-4cb1-aa05-68ec3a73c459'	'f59d3232-ddf2-4f15-b7ae-e3691f385ddd'		'Проценты по кассовым ордерам'
    public readonly Guid CashOrderPercent = Guid.Parse("{FC1A57BA-4262-403E-9AF7-CE1647E14721}");

    //'b6f2540a-9593-42e3-b34f-8c0983bc39a2'		'a938446c-ab19-45c3-8dfd-0f24ab08df49'		'Валютная конвертация'			12
    // '35ebabec-eac3-4c3c-8383-6326c5d64c8c'		'f59d3232-ddf2-4f15-b7ae-e3691f385ddd'		'Валютная конвертация'
    public readonly Guid CurrencyConvert = Guid.Parse("{6465832D-ECA2-468C-AB40-7C249246A321}");

    //'564db69c-6dad-4b16-8bf5-118f5af2d07f'		'35c9783e-e19f-452b-8479-d6f022444552'		'Валютный перевод товаров'  7
    public readonly Guid CurrencyNomenklTransfer = Guid.Parse("{AE929291-10D2-465B-80FD-4902D48D2F1A}");

    //'ba628f86-6ae4-4cf3-832b-c6f7388dd01b'		'f59d3232-ddf2-4f15-b7ae-e3691f385ddd'		'Дилерские'							8
    public readonly Guid DilerNomenkl = Guid.Parse("{714C83A0-11F7-4717-84C9-86CF8D4F2CA8}");

    //'7338b45d-b137-4b53-b641-554deed3f1b3'	'a938446c-ab19-45c3-8dfd-0f24ab08df49'		'Прямые услуги для клиентов'             16
    // '4cea8d09-030e-49e8-b6f7-eb83d5e0d6ea'		'f59d3232-ddf2-4f15-b7ae-e3691f385ddd'		'Прямые затраты'
    public readonly Guid DirectCosts = Guid.Parse("{6FB1948D-49BA-4503-8E20-39BA45FF8EE0}");

    //'30e9bd73-9bda-4d75-b897-332f9210b9b1'	'a938446c-ab19-45c3-8dfd-0f24ab08df49'		'Финансовые операции'          11
    // '459937df-085f-4825-9ae9-810b054d0276'	'f59d3232-ddf2-4f15-b7ae-e3691f385ddd'		'Финансовые операции'
    public readonly Guid FinanceOperations = Guid.Parse("{85141396-032F-4109-A710-99BA7D9B5E13}");

    //'6e8f2011-49f5-4ec7-a4cb-248da8059a79'		'334973b4-1652-4473-9ded-fd4b31b31fc1'		'Приход товара (инвентаризация)'    20
    // 'c8acebbd-704c-41cc-99d8-1e97b09e8f2c'		'd89b1e18-074e-4a7d-a0ee-9537dc1585d8'	'Списание товара (инвентаризация)'
    public readonly Guid Inventory = Guid.Parse("{C88CFA6B-635C-4406-B78E-1E19A4788206}");

    //'2d07127b-72a8-4018-b9a8-62c7a78cb9c3'	'16524d17-451c-4149-8813-bdb9875f759a'	'Балансы контрагентов'      4
    // '15df4d79-d608-412a-87a8-1560714a706a'	'2f1fd81f-a7be-4830-93eb-9b3a9a362b2b'		'Балансы контрагентов'
    public readonly Guid KontragentBalans = Guid.Parse("{7695E245-4A05-4C92-8529-5999AA70F8B8}");

    //'297d2727-5161-48ed-969e-651811906526'	'a938446c-ab19-45c3-8dfd-0f24ab08df49'		'Операции с внебалансовыми контрагентами'  13
    // 'e47b2338-c42f-4b2a-8865-1024fc84f020'		'f59d3232-ddf2-4f15-b7ae-e3691f385ddd'		'Операции с внебалансовыми контрагентами'
    public readonly Guid KontragentOutBalans = Guid.Parse("{3AF995DA-CF22-49D6-B29B-A852F5782AB9}");

    //'c5c36299-fdef-4251-b525-3df10c0e8cb9'		'f59d3232-ddf2-4f15-b7ae-e3691f385ddd'		'Возврат товара'					9
    // '04a7b6bb-7b3c-49f1-8e10-f1ae5f5582e4'		'a938446c-ab19-45c3-8dfd-0f24ab08df49'		'Возврат товара'
    public readonly Guid NomenklReturn = Guid.Parse("{46A5812A-4187-4EE5-9A0B-DFB33D01B5AD}");

    //'334973b4-1652-4473-9ded-fd4b31b31fc1'		'a938446c-ab19-45c3-8dfd-0f24ab08df49'		'Товары'								6
    // 'd89b1e18-074e-4a7d-a0ee-9537dc1585d8'	'f59d3232-ddf2-4f15-b7ae-e3691f385ddd'		'Товары'
    public readonly Guid Nomenkls = Guid.Parse("{EE3E54A3-E60E-468F-AA41-0C364A1BE49F}");

    //'8fc37bda-4509-4dc8-bc53-7a8869d17364'		'f59d3232-ddf2-4f15-b7ae-e3691f385ddd'		'Выплаты акционерам'						18
    public readonly Guid PaymentsToShareholders = Guid.Parse("{41B942CD-9796-45B9-9103-9B1F16D854AE}");

    //'b96b2906-c5aa-4566-b77f-f3e4b912e72e'		'f59d3232-ddf2-4f15-b7ae-e3691f385ddd'		'Заработная плата'											14
    public readonly Guid PersonalEmployee = Guid.Parse("{E49923D3-3EF8-4B78-A707-69EBAB97D01F}");

    //'2fa1dd9f-6842-4209-b0cc-ddef3b920496'		'a938446c-ab19-45c3-8dfd-0f24ab08df49'		'Услуги'									10
    // 'e47ef726-3bea-4b18-9773-e564d624fdf6'		'f59d3232-ddf2-4f15-b7ae-e3691f385ddd'		'Услуги'
    public readonly Guid Services = Guid.Parse("{B22CE4BE-630C-4949-AB9C-3A40A403B254}");

    //'16524d17-451c-4149-8813-bdb9875f759a'	'a938446c-ab19-45c3-8dfd-0f24ab08df49'		'Начальные остатки'          1
    // '2f1fd81f-a7be-4830-93eb-9b3a9a362b2b'		'f59d3232-ddf2-4f15-b7ae-e3691f385ddd'		'Начальные остатки'
    public readonly Guid StartRest = Guid.Parse("{622E2602-63DE-4DEE-8A4D-70056AB63F0F}");

    //'d21d9a76-5c9d-4c7b-bc2b-7e2d59452cbd'	'f59d3232-ddf2-4f15-b7ae-e3691f385ddd'		'Перевод за баланс'							19
    public readonly Guid TransferOutBalans = Guid.Parse("{026EE35C-25D8-4136-BD96-077AD30B9383}");

    //'52ea160e-27dc-47e1-9006-70df349943f6'		'f59d3232-ddf2-4f15-b7ae-e3691f385ddd'		'Выдано под отчет'				5
    // '7550849b-1d51-445b-b692-ce3ff7ab11b0'		'a938446c-ab19-45c3-8dfd-0f24ab08df49'		'Возврат с под отчета'
    public readonly Guid UnderReport = Guid.Parse("{1BE4CB93-7DDE-4FC5-9F4F-490755C93F26}");

    //'7e2ab2e0-b2b2-495d-a403-e746647ca99d'	'f59d3232-ddf2-4f15-b7ae-e3691f385ddd'		'Списание '											17
    public readonly Guid WriteOff = Guid.Parse("{CC5B1F33-20E4-45C3-98DD-6E6678CF5517}");

    private decimal myLossCHF;
    private decimal myLossCNY;
    private decimal myLossEUR;
    private decimal myLossGBP;
    private decimal myLossRUB;
    private decimal myLossSEK;
    private decimal myLossUSD;
    private decimal myProfitCHF;
    private decimal myProfitCNY;
    private decimal myProfitEUR;
    private decimal myProfitGBP;
    private decimal myProfitRUB;
    private decimal myProfitSEK;
    private decimal myProfitUSD;
    private decimal myResultCHF;
    private decimal myResultCNY;
    private decimal myResultEUR;
    private decimal myResultGBP;
    private decimal myResultRUB;
    private decimal myResultSEK;
    private decimal myResultUSD;

    public Guid? ProfitId { set; get; }
    public Guid? LossId { set; get; }

    public int Order { set; get; }

    /// <summary>
    ///     Шведская Крона
    /// </summary>
    public decimal LossSEK
    {
        get => myLossSEK;
        set
        {
            if (myLossSEK == value) return;
            myLossSEK = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    ///     Шведская Крона
    /// </summary>
    public decimal ProfitSEK
    {
        get => myProfitSEK;
        set
        {
            if (myProfitSEK == value) return;
            myProfitSEK = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    ///     Шведская Крона
    /// </summary>
    public decimal ResultSEK
    {
        get => myResultSEK;
        set
        {
            if (myResultSEK == value) return;
            myResultSEK = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    ///     Швейцарский франк
    /// </summary>
    public decimal LossCHF
    {
        get => myLossCHF;
        set
        {
            if (myLossCHF == value) return;
            myLossCHF = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    ///     Швейцарский франк
    /// </summary>
    public decimal ProfitCHF
    {
        get => myProfitCHF;
        set
        {
            if (myProfitCHF == value) return;
            myProfitCHF = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    ///     Швейцарский франк
    /// </summary>
    public decimal ResultCHF
    {
        get => myResultCHF;
        set
        {
            if (myResultCHF == value) return;
            myResultCHF = value;
            RaisePropertyChanged();
        }
    }

    public decimal LossGBP
    {
        get => myLossGBP;
        set
        {
            if (myLossGBP == value) return;
            myLossGBP = value;
            RaisePropertyChanged();
        }
    }

    public decimal ProfitGBP
    {
        get => myProfitGBP;
        set
        {
            if (myProfitGBP == value) return;
            myProfitGBP = value;
            RaisePropertyChanged();
        }
    }

    public decimal ResultGBP
    {
        get => myResultGBP;
        set
        {
            if (myResultGBP == value) return;
            myResultGBP = value;
            RaisePropertyChanged();
        }
    }

    public decimal ProfitRUB
    {
        set
        {
            if (Equals(value, myProfitRUB)) return;
            myProfitRUB = value;
            RaisePropertyChanged();
        }
        get => myProfitRUB;
    }

    public decimal LossRUB
    {
        set
        {
            if (Equals(value, myLossRUB)) return;
            myLossRUB = value;
            RaisePropertyChanged();
        }
        get => myLossRUB;
    }

    public decimal ResultRUB
    {
        set
        {
            if (Equals(value, myResultRUB)) return;
            myResultRUB = value;
            RaisePropertyChanged();
        }
        get => myResultRUB;
    }

    public decimal ProfitUSD
    {
        set
        {
            if (Equals(value, myProfitUSD)) return;
            myProfitUSD = value;
            RaisePropertyChanged();
        }
        get => myProfitUSD;
    }

    public decimal LossUSD
    {
        set
        {
            if (Equals(value, myLossUSD)) return;
            myLossUSD = value;
            RaisePropertyChanged();
        }
        get => myLossUSD;
    }

    public decimal ResultUSD
    {
        set
        {
            if (Equals(value, myResultUSD)) return;
            myResultUSD = value;
            RaisePropertyChanged();
        }
        get => myResultUSD;
    }

    public decimal ProfitEUR
    {
        set
        {
            if (Equals(value, myProfitEUR)) return;
            myProfitEUR = value;
            RaisePropertyChanged();
        }
        get => myProfitEUR;
    }

    public decimal LossEUR
    {
        set
        {
            if (Equals(value, myLossEUR)) return;
            myLossEUR = value;
            RaisePropertyChanged();
        }
        get => myLossEUR;
    }

    public decimal ResultEUR
    {
        set
        {
            if (Equals(value, myResultEUR)) return;
            myResultEUR = value;
            RaisePropertyChanged();
        }
        get => myResultEUR;
    }

    /// <summary>
    ///     Китайский юань
    /// </summary>
    public decimal LossCNY
    {
        get => myLossCNY;
        set
        {
            if (myLossCNY == value) return;
            myLossCNY = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    ///     Китайский юань
    /// </summary>
    public decimal ProfitCNY
    {
        get => myProfitCNY;
        set
        {
            if (myProfitCNY == value) return;
            myProfitCNY = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    ///     Китайский юань
    /// </summary>
    public decimal ResultCNY
    {
        get => myResultCNY;
        set
        {
            if (myResultCNY == value) return;
            myResultCNY = value;
            RaisePropertyChanged();
        }
    }


    public static IEnumerable<ProfitAndLossCommonRow> GetBaseStructure()
    {
        var res = new List<ProfitAndLossCommonRow>();
        var newItem = new ProfitAndLossCommonRow();
        newItem.Id = newItem.StartRest;
        newItem.ProfitId = Guid.Parse("16524d17-451c-4149-8813-bdb9875f759a");
        newItem.LossId =  Guid.Parse("2f1fd81f-a7be-4830-93eb-9b3a9a362b2b");
        newItem.Order = 1;
        newItem.Name = "Начальные остатки";
        res.Add(newItem);

        newItem = new ProfitAndLossCommonRow();
        newItem.Id = newItem.Bank;
        newItem.ParentId = newItem.StartRest;
        newItem.ProfitId = Guid.Parse("0ad95635-a46d-49f2-ae78-cbdf52bd6e27");
        newItem.LossId =  Guid.Parse("920b2ab6-1984-49a9-b5e7-18407221f620");
        newItem.Order = 2;
        newItem.Name = "Банковские счета";
        res.Add(newItem);

        newItem = new ProfitAndLossCommonRow();
        newItem.Id = newItem.Cash;
        newItem.ParentId = newItem.StartRest;
        newItem.ProfitId = Guid.Parse("a084b37a-d942-4b7f-9ae9-3c3aaa0f4475");
        newItem.LossId =  Guid.Parse("c0aedec3-15cd-4d5b-8e37-0fe1edb5c6ff");
        newItem.Order = 3;
        newItem.Name = "Касса";
        res.Add(newItem);

        newItem = new ProfitAndLossCommonRow();
        newItem.Id = newItem.KontragentBalans;
        newItem.ParentId = newItem.StartRest;
        newItem.ProfitId = Guid.Parse("2d07127b-72a8-4018-b9a8-62c7a78cb9c3");
        newItem.LossId =  Guid.Parse("15df4d79-d608-412a-87a8-1560714a706a");
        newItem.Order = 4;
        newItem.Name = "Балансы контрагентов";
        res.Add(newItem);

        newItem = new ProfitAndLossCommonRow();
        newItem.Id = newItem.UnderReport;
        newItem.ProfitId = Guid.Parse("52ea160e-27dc-47e1-9006-70df349943f6");
        newItem.LossId =  Guid.Parse("7550849b-1d51-445b-b692-ce3ff7ab11b0");
        newItem.Order = 5;
        newItem.Name = "Деньги под отчетом";
        res.Add(newItem);

        newItem = new ProfitAndLossCommonRow();
        newItem.Id = newItem.Nomenkls;
        newItem.ProfitId = Guid.Parse("334973b4-1652-4473-9ded-fd4b31b31fc1");
        newItem.LossId =  Guid.Parse("d89b1e18-074e-4a7d-a0ee-9537dc1585d8");
        newItem.Order = 6;
        newItem.Name = "Товары";
        res.Add(newItem);

        newItem = new ProfitAndLossCommonRow();
        newItem.Id = newItem.CurrencyNomenklTransfer;
        newItem.ProfitId = Guid.Parse("564db69c-6dad-4b16-8bf5-118f5af2d07f");
        newItem.LossId =  null;
        newItem.Order = 7;
        newItem.Name = "Валютный перевод товаров";
        res.Add(newItem);

        newItem = new ProfitAndLossCommonRow();
        newItem.Id = newItem.DilerNomenkl;
        newItem.ProfitId = null;
        newItem.LossId =  Guid.Parse("ba628f86-6ae4-4cf3-832b-c6f7388dd01b");
        newItem.Order = 8;
        newItem.Name = "Дилерские";
        res.Add(newItem);

        newItem = new ProfitAndLossCommonRow();
        newItem.Id = newItem.NomenklReturn;
        newItem.ProfitId = Guid.Parse("c5c36299-fdef-4251-b525-3df10c0e8cb9");
        newItem.LossId =  Guid.Parse("04a7b6bb-7b3c-49f1-8e10-f1ae5f5582e4");
        newItem.Order = 9;
        newItem.Name = "Возврат товара";
        res.Add(newItem);

        newItem = new ProfitAndLossCommonRow();
        newItem.Id = newItem.Services;
        newItem.ProfitId = Guid.Parse("2fa1dd9f-6842-4209-b0cc-ddef3b920496");
        newItem.LossId =  Guid.Parse("e47ef726-3bea-4b18-9773-e564d624fdf6");
        newItem.Order = 10;
        newItem.Name = "Услуги";
        res.Add(newItem);

        newItem = new ProfitAndLossCommonRow();
        newItem.Id = newItem.FinanceOperations;
        newItem.ProfitId = Guid.Parse("30e9bd73-9bda-4d75-b897-332f9210b9b1");
        newItem.LossId =  Guid.Parse("459937df-085f-4825-9ae9-810b054d0276");
        newItem.Order = 11;
        newItem.Name = "Финансовые операции";
        res.Add(newItem);

        newItem = new ProfitAndLossCommonRow();
        newItem.Id = newItem.CurrencyConvert;
        newItem.ProfitId = Guid.Parse("b6f2540a-9593-42e3-b34f-8c0983bc39a2");
        newItem.LossId =  Guid.Parse("35ebabec-eac3-4c3c-8383-6326c5d64c8c");
        newItem.Order = 12;
        newItem.Name = "Валютная конвертация";
        res.Add(newItem);

        newItem = new ProfitAndLossCommonRow();
        newItem.Id = newItem.KontragentOutBalans;
        newItem.ProfitId = Guid.Parse("b6f2540a-9593-42e3-b34f-8c0983bc39a2");
        newItem.LossId =  Guid.Parse("35ebabec-eac3-4c3c-8383-6326c5d64c8c");
        newItem.Order = 13;
        newItem.Name = "Операции с внебалансовыми контрагентами";
        res.Add(newItem);

        newItem = new ProfitAndLossCommonRow();
        newItem.Id = newItem.PersonalEmployee;
        newItem.ProfitId =null;
        newItem.LossId =  Guid.Parse("b96b2906-c5aa-4566-b77f-f3e4b912e72e");
        newItem.Order = 14;
        newItem.Name = "Заработная плата";
        res.Add(newItem);

        newItem = new ProfitAndLossCommonRow();
        newItem.Id = newItem.CashOrderPercent;
        newItem.ProfitId = Guid.Parse("836cc414-bef4-4371-a253-47d2e8f4535f");
        newItem.LossId =  Guid.Parse("e9d63300-829b-4cb1-aa05-68ec3a73c459");
        newItem.Order = 15;
        newItem.Name = "Проценты по кассовым ордерам";
        res.Add(newItem);

        newItem = new ProfitAndLossCommonRow();
        newItem.Id = newItem.DirectCosts;
        newItem.ProfitId = Guid.Parse("7338b45d-b137-4b53-b641-554deed3f1b3");
        newItem.LossId =  Guid.Parse("4cea8d09-030e-49e8-b6f7-eb83d5e0d6ea");
        newItem.Order = 16;
        newItem.Name = "Прямые затраты/услуги";
        res.Add(newItem);

        newItem = new ProfitAndLossCommonRow();
        newItem.Id = newItem.WriteOff;
        newItem.ProfitId = null;
        newItem.LossId =  Guid.Parse("7e2ab2e0-b2b2-495d-a403-e746647ca99d");
        newItem.Order = 17;
        newItem.Name = "Списание";
        res.Add(newItem);

        newItem = new ProfitAndLossCommonRow();
        newItem.Id = newItem.PaymentsToShareholders;
        newItem.ProfitId = null;
        newItem.LossId =  Guid.Parse("8fc37bda-4509-4dc8-bc53-7a8869d17364");
        newItem.Order = 18;
        newItem.Name = "Выплаты акционерам";
        res.Add(newItem);

        newItem = new ProfitAndLossCommonRow();
        newItem.Id = newItem.TransferOutBalans;
        newItem.ProfitId = null;
        newItem.LossId =  Guid.Parse("d21d9a76-5c9d-4c7b-bc2b-7e2d59452cbd");
        newItem.Order = 19;
        newItem.Name = "Перевод за баланс";
        res.Add(newItem);

        newItem = new ProfitAndLossCommonRow();
        newItem.Id = newItem.Inventory;
        newItem.ProfitId = Guid.Parse("6e8f2011-49f5-4ec7-a4cb-248da8059a79");
        newItem.LossId =  Guid.Parse("c8acebbd-704c-41cc-99d8-1e97b09e8f2c");
        newItem.Order = 20;
        newItem.Name = "Инвентаризация";
        res.Add(newItem);
        
        return res;
    }

     public class DataAnnotationProfitAndLossCommonRow : DataAnnotationForFluentApiBase,
        IMetadataProvider<ProfitAndLossCommonRow>
    {
        void IMetadataProvider<ProfitAndLossCommonRow>.BuildMetadata(
            MetadataBuilder<ProfitAndLossCommonRow> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(_ => _.Name).LocatedAt(0).AutoGenerated().DisplayName("Наименование").ReadOnly();
            builder.Property(_ => _.ProfitRUB).AutoGenerated().DisplayName("Доход").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.LossRUB).AutoGenerated().DisplayName("Расход").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.ResultRUB).AutoGenerated().DisplayName("Результат").DisplayFormatString("n2")
                .ReadOnly();

            builder.Property(_ => _.ProfitUSD).AutoGenerated().DisplayName("Доход").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.LossUSD).AutoGenerated().DisplayName("Расход").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.ResultUSD).AutoGenerated().DisplayName("Результат").DisplayFormatString("n2")
                .ReadOnly();

            builder.Property(_ => _.ProfitEUR).AutoGenerated().DisplayName("Доход").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.LossEUR).AutoGenerated().DisplayName("Расход").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.ResultEUR).AutoGenerated().DisplayName("Результат").DisplayFormatString("n2")
                .ReadOnly();

            builder.Property(_ => _.ProfitCNY).AutoGenerated().DisplayName("Доход").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.LossCNY).AutoGenerated().DisplayName("Расход").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.ResultCNY).AutoGenerated().DisplayName("Результат").DisplayFormatString("n2")
                .ReadOnly();

            builder.Property(_ => _.ProfitGBP).AutoGenerated().DisplayName("Доход").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.LossGBP).AutoGenerated().DisplayName("Расход").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.ResultGBP).AutoGenerated().DisplayName("Результат").DisplayFormatString("n2")
                .ReadOnly();

            builder.Property(_ => _.ProfitCHF).AutoGenerated().DisplayName("Доход").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.LossCHF).AutoGenerated().DisplayName("Расход").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.ResultCHF).AutoGenerated().DisplayName("Результат").DisplayFormatString("n2")
                .ReadOnly();

            builder.Property(_ => _.ProfitSEK).AutoGenerated().DisplayName("Доход").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.LossSEK).AutoGenerated().DisplayName("Расход").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.ResultSEK).AutoGenerated().DisplayName("Результат").DisplayFormatString("n2")
                .ReadOnly();

            
            // @formatter:off
            builder.TableLayout()
                .Group("Основные данные")
                    .ContainsProperty(_ => _.Name)
                .EndGroup()
                .Group("RUB")
                    .ContainsProperty(_ => _.ProfitRUB)
                    .ContainsProperty(_ => _.LossRUB)
                    .ContainsProperty(_ => _.ResultRUB)
                .EndGroup()
                .Group("USD")
                    .ContainsProperty(_ => _.ProfitUSD)
                    .ContainsProperty(_ => _.LossUSD)
                    .ContainsProperty(_ => _.ResultUSD)
                .EndGroup()
                .Group("EUR")
                    .ContainsProperty(_ => _.ProfitEUR)
                    .ContainsProperty(_ => _.LossEUR)
                    .ContainsProperty(_ => _.ResultEUR)
                .EndGroup()
                .Group("CNY")
                    .ContainsProperty(_ => _.ProfitCNY)
                    .ContainsProperty(_ => _.LossCNY)
                    .ContainsProperty(_ => _.ResultCNY)
                .EndGroup()
                .Group("GBP")
                    .ContainsProperty(_ => _.ProfitGBP)
                    .ContainsProperty(_ => _.LossGBP)
                    .ContainsProperty(_ => _.ResultGBP)
                .EndGroup()
                .Group("CHF")
                    .ContainsProperty(_ => _.ProfitCHF)
                    .ContainsProperty(_ => _.LossCHF)
                    .ContainsProperty(_ => _.ResultCHF)
                .EndGroup()
                .Group("SEK")
                    .ContainsProperty(_ => _.ProfitSEK)
                    .ContainsProperty(_ => _.LossSEK)
                    .ContainsProperty(_ => _.ResultSEK)
                .EndGroup();
            // @formatter:on
        }
    }
}
