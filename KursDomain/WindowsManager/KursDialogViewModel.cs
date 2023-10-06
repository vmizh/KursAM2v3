using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Core.ViewModel.Base;
using static Core.WindowsManager.WindowManager;

namespace KursDomain.WindowsManager;

public class KursDialogViewModel : INotifyPropertyChanged
{

    #region Fields

    private Dictionary<KursDialogResult, string> bNames;
    private KursDialog dialog;
    private string text;
    private const double  buttonWidth = 120;
    private readonly Thickness buttonMargin = new Thickness(0, 0, 20, 0);
   
    #endregion

    #region Properties

    public Brush BorderBrush { set; get; } = Brushes.Blue;
    public List<Button> Buttons { set; get; } = new List<Button>();
    public string Text {
        set
        {
            text = value;
            OnPropertyChanged();
        }
        get => text;
    }

    public KursDialogResult DialogResult;
    
    #endregion

    #region Constructors
    
    public KursDialogViewModel(string text, KursDialogResult result, Dictionary<KursDialogResult, string> buttonNames)
    {
        bNames = buttonNames;
        Text = text;
        dialog = new KursDialog();

        if ((result & KursDialogResult.Yes) == KursDialogResult.Yes)
        {
            Buttons.Add(new Button
            {
                Content = buttonNames[KursDialogResult.Yes],
                Command = ButtonClickCommand,
                CommandParameter = KursDialogResult.Yes,
                Width = buttonWidth,
                Margin = buttonMargin
            });
        }
        if ((result & KursDialogResult.No) == KursDialogResult.No)
        {
            Buttons.Add(new Button
            {
                Content = buttonNames[KursDialogResult.No],
                Command = ButtonClickCommand,
                CommandParameter = KursDialogResult.No,
                Width = buttonWidth,
                Margin = buttonMargin
            });
        }
        
        if ((result & KursDialogResult.Save) == KursDialogResult.Save)
        {
            Buttons.Add(new Button
            {
                Content = buttonNames[KursDialogResult.Save],
                Command = ButtonClickCommand,
                CommandParameter = KursDialogResult.Save,
                Width = buttonWidth,
                Margin = buttonMargin
            });
        }
        if ((result & KursDialogResult.NotSave) == KursDialogResult.NotSave)
        {
            Buttons.Add(new Button
            {
                Content = buttonNames[KursDialogResult.NotSave],
                Command = ButtonClickCommand,
                CommandParameter = KursDialogResult.NotSave,
                Width = buttonWidth,
                Margin = buttonMargin
            });
        }
        if ((result & KursDialogResult.Cancel) == KursDialogResult.Cancel)
        {
            Buttons.Add(new Button
            {
                Content = buttonNames[KursDialogResult.Cancel],
                Command = ButtonClickCommand,
                CommandParameter = KursDialogResult.Cancel,
                Width = buttonWidth,
                Margin = buttonMargin
            });
        }

        if ((result & KursDialogResult.Confirm) == KursDialogResult.Confirm)
        {
            Buttons.Add(new Button
            {
                Content = buttonNames[KursDialogResult.Confirm],
                Command = ButtonClickCommand,
                CommandParameter = KursDialogResult.Confirm,
                Width = buttonWidth,
                Margin = buttonMargin
            });
        }
    } 

    #endregion

    #region Methods

    public void Show()
    {
        dialog.Topmost = true; 
        dialog.DataContext = this;
        dialog.ShowDialog();
        OnPropertyChanged(nameof(Buttons));
        OnPropertyChanged(nameof(Text));
    }

    #endregion

    #region Commands

    ICommand ButtonClickCommand
    {
        get { return new Command(ButonClick, _ => true); }
    }

    private void ButonClick(object obj)
    {
        DialogResult = (KursDialogResult)obj;
        dialog?.Close();
    }

    #endregion

    public event PropertyChangedEventHandler PropertyChanged;

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
}
