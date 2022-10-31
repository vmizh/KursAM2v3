using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Core.Menu
{
    public class KursButton : Button
    {
        public static readonly DependencyProperty IdProperty = DependencyProperty.Register(
            "IsNewDocument", typeof(int), typeof(KursButton), new PropertyMetadata(default(int),
                // ReSharper disable once RedundantDelegateCreation
                new PropertyChangedCallback(IdChangedCallback)));

        //В get и set нельзя ничего дописывать, нужно все реакции 
        //на изменения свойства писать в колбеке (IdChangedCallback).
        //Инфраструктура WPF иногда обращается к свойству напряму методами GetValue и SetValue
        //минуя это свойство. Поэтому нет смысла туда что-то дописывать
        public bool IsNewDocument
        {
            get => (bool) GetValue(IdProperty);
            set => SetValue(IdProperty, value);
        }

        //Срабатывает, когда меняется значение свойства Id
        private static void IdChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // ReSharper disable once UnusedVariable
            var button = (KursButton) d;
            Debug.WriteLine("New IsNewDocument = {0}; Old IsNewDocument = {1}", e.NewValue, e.OldValue);
        }
    }
}