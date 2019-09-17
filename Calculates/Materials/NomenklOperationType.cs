namespace Calculates.Materials
{
    public enum NomenklOperationType
    {
        /// <summary>
        ///     Инвентаризационная ведомость
        /// </summary>
        InventarVedomost = 0,

        /// <summary>
        ///     Приходны складско ордер
        /// </summary>
        PrihodOrder = 1,

        /// <summary>
        ///     накладная на внутреннее перемещение
        /// </summary>
        InnerNaklad = 2,

        /// <summary>
        ///     Акт разукомплектации готовой продукции(приход)
        /// </summary>
        RazukomplektAct = 3,

        /// <summary>
        ///     Акт приемки готовой продукции(приход)
        /// </summary>
        PriemGotovyProduct = 4
    }
}