using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Core.ViewModel.Base;

namespace KursDomain.WindowsManager.WindowsManager;

public class KursDialogViewModel : INotifyPropertyChanged
{
    #region Constructors

    // public Brush TitleTextColor { get; set; }

    public KursDialogViewModel(string text, string titleText, Brush titleTextColor,
        WindowManager.KursDialogResult result,
        Dictionary<WindowManager.KursDialogResult, string> buttonNames)
    {
        bNames = buttonNames;
        TitleText = titleText;
        Text = text;
        TitleTextColor = titleTextColor ?? Brushes.Black;

        dialog = new KursDialog();

        if ((result & WindowManager.KursDialogResult.Yes) == WindowManager.KursDialogResult.Yes)
            Buttons.Add(new Button
            {
                Content = buttonNames[WindowManager.KursDialogResult.Yes],
                Command = ButtonClickCommand,
                CommandParameter = WindowManager.KursDialogResult.Yes,
                Width = buttonWidth,
                Margin = buttonMargin
            });
        if ((result & WindowManager.KursDialogResult.No) == WindowManager.KursDialogResult.No)
            Buttons.Add(new Button
            {
                Content = buttonNames[WindowManager.KursDialogResult.No],
                Command = ButtonClickCommand,
                CommandParameter = WindowManager.KursDialogResult.No,
                Width = buttonWidth,
                Margin = buttonMargin
            });

        if ((result & WindowManager.KursDialogResult.Save) == WindowManager.KursDialogResult.Save)
            Buttons.Add(new Button
            {
                Content = buttonNames[WindowManager.KursDialogResult.Save],
                Command = ButtonClickCommand,
                CommandParameter = WindowManager.KursDialogResult.Save,
                Width = buttonWidth,
                Margin = buttonMargin
            });
        if ((result & WindowManager.KursDialogResult.NotSave) == WindowManager.KursDialogResult.NotSave)
            Buttons.Add(new Button
            {
                Content = buttonNames[WindowManager.KursDialogResult.NotSave],
                Command = ButtonClickCommand,
                CommandParameter = WindowManager.KursDialogResult.NotSave,
                Width = buttonWidth,
                Margin = buttonMargin
            });
        if ((result & WindowManager.KursDialogResult.Cancel) == WindowManager.KursDialogResult.Cancel)
            Buttons.Add(new Button
            {
                Content = buttonNames[WindowManager.KursDialogResult.Cancel],
                Command = ButtonClickCommand,
                CommandParameter = WindowManager.KursDialogResult.Cancel,
                Width = buttonWidth,
                Margin = buttonMargin
            });

        if ((result & WindowManager.KursDialogResult.Confirm) == WindowManager.KursDialogResult.Confirm)
            Buttons.Add(new Button
            {
                Content = buttonNames[WindowManager.KursDialogResult.Confirm],
                Command = ButtonClickCommand,
                CommandParameter = WindowManager.KursDialogResult.Confirm,
                Width = buttonWidth + 60,
                Margin = buttonMargin
            });

        if (Buttons.Count > 0)
            foreach (var button1 in Buttons)
            {
                button1.MouseEnter += (s, e) =>
                {
                    BorderBrush = button1.Foreground;
                    button1.Foreground = Brushes.White;
                    button1.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#0067c0");
                };
                button1.MouseLeave += (s, e) =>
                {
                    button1.Background = Brushes.White;
                    button1.Foreground = BorderBrush;
                };
                button1.PreviewMouseLeftButtonDown += (s, e) =>
                {
                    button1.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#d4d0c6");
                    button1.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#477bd5");
                };
            }
    }

    #endregion

    public event PropertyChangedEventHandler PropertyChanged;

    #region Methods

    public void Show()
    {
        dialog.Topmost = true;
        dialog.DataContext = this;
        Buttons.First().Focus();
        dialog.ShowDialog();
        OnPropertyChanged(nameof(Buttons));
        OnPropertyChanged(nameof(Text));
    }

    #endregion

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    #region Fields

    private Dictionary<WindowManager.KursDialogResult, string> bNames;
    private readonly KursDialog dialog;
    private string text;
    private const double buttonWidth = 120;
    private readonly Thickness buttonMargin = new Thickness(0, 0, 20, 0);

    #endregion

    #region Properties

    public Brush BorderBrush { set; get; }
    public Brush TitleTextColor { set; get; }
    public List<Button> Buttons { set; get; } = new List<Button>();

    public string Text
    {
        set
        {
            text = value;
            OnPropertyChanged();
        }
        get => text;
    }

    public string TitleText { set; get; }


    public WindowManager.KursDialogResult DialogResult;

    #endregion

    #region Commands

    private ICommand ButtonClickCommand
    {
        get { return new Command(ButtonClick, _ => true); }
    }

    private void ButtonClick(object obj)
    {
        if (obj is string p)
            switch (p)
            {
                case "Enter":
                    DialogResult = WindowManager.KursDialogResult.Yes;
                    break;
                case "Cancel":
                    DialogResult = WindowManager.KursDialogResult.No;
                    break;
            }
        else
            DialogResult = (WindowManager.KursDialogResult)obj;

        dialog?.Close();
    }

    #endregion
}
