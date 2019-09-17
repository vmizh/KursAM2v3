namespace Calculates.Materials
{
    public enum NomenklMoveOperationType
    {
        /// <summary>
        ///     Приход от внешнего балансового контрагента
        /// </summary>
        ReceiptKontragent = 0,

        /// <summary>
        ///     Приход от внешнего внебалансового контрагента
        /// </summary>
        ReceiptOutBalansKontragent = 1,

        /// <summary>
        ///     Приход по инвентаризационной ведомости
        /// </summary>
        ReceiptInventory = 2,

        /// <summary>
        ///     приход по акту приемки продукции
        /// </summary>
        ReceiptAssemblyProduct = 3,

        /// <summary>
        ///     приход по акту разборке продукции (разукомплектация)
        /// </summary>
        ReceiptDisassemblyProduct = 4,

        /// <summary>
        ///     Приход по накладной внутреннего перемещения
        /// </summary>
        ReceiptInnerMove = 5,

        /// <summary>
        ///     Приход по продаже за наличный расчет
        /// </summary>
        ReceiptSpotSale = 6,

        /// <summary>
        ///     отгрузка товара балансовому иклиенту
        /// </summary>
        ExpenseKontragent = 100,

        /// <summary>
        ///     Отгрузка внебелансовому контрагенту
        /// </summary>
        ExpenseOutBalansKontragent = 101,

        /// <summary>
        ///     Списание по инвентаризационной ведомости
        /// </summary>
        ExpenseInventory = 102,

        /// <summary>
        ///     Списание товара по акту приемки готовой продукции
        /// </summary>
        ExpenseAssemblyProduct = 103,

        /// <summary>
        ///     Списание по акту разукомплектации продукции
        /// </summary>
        ExpenseDisassemblyProduct = 104,

        /// <summary>
        ///     Расход по накладной на внутреннее перемещение
        /// </summary>
        ExpenseInnerMove = 105,

        /// <summary>
        ///     Списание по акту списания
        /// </summary>
        ExpenseWriteOff = 106,

        /// <summary>
        ///     Списание по продаже за наличный расчет
        /// </summary>
        ExpenseSpotSale = 106,
        /// <summary>
        /// Перевод товара
        /// </summary>
        Transfer = 107
    }
}