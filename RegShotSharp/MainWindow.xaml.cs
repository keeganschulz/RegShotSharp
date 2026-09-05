using Microsoft.Win32;
using RegShotSharp.Extensions;
using RegShotSharp.Tools;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace RegShotSharp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
      InitializeComponent();
      MainPage mp = new MainPage();
      mp.Show();
      this.Close();
    }    
}