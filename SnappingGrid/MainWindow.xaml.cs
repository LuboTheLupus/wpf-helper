using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace SnappingGrid;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    #region INotifyPropertyChanged
    public event PropertyChangedEventHandler? PropertyChanged;
    void NotifyPropertyChanged([CallerMemberName] string? propName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    #endregion INotifyPropertyChanged

    #region constructors
    public MainWindow()
    {
        DataContext = this;
        InitializeComponent();
    }
    #endregion constructors
}