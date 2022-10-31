namespace Core.EntityViewModel
{
    internal interface ICheckViewModel
    {
        /// <summary>
        ///     Проверяет корректность заполнения полей viewmodel
        /// </summary>
        /// <returns></returns>
        bool Check();
    }
}