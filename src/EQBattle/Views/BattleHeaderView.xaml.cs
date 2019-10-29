using System.Windows;
using System.Windows.Controls;

namespace EQBattle.Views
{
    /// <summary>
    /// Interaction logic for BattleHeaderView.xaml
    /// </summary>
    public partial class BattleHeaderView : UserControl
    {
        public BattleHeaderView()
        {
            InitializeComponent();
        }

        // https://stackoverflow.com/a/1051264
        private void ToolBar_Loaded(object sender, RoutedEventArgs e)
        {
            ToolBar toolBar = sender as ToolBar;

            if (toolBar.Template.FindName("OverflowGrid", toolBar) is FrameworkElement overflowGrid)
                overflowGrid.Visibility = Visibility.Collapsed;

            if (toolBar.Template.FindName("MainPanelBorder", toolBar) is FrameworkElement mainPanelBorder)
                mainPanelBorder.Margin = new Thickness();
        }
    }
}
