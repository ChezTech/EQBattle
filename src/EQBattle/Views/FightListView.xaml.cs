using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EQBattle.Views
{
    /// <summary>
    /// Interaction logic for FightListView.xaml
    /// </summary>
    public partial class FightListView : UserControl
    {
        public FightListView()
        {
            InitializeComponent();
        }

        private void lvFights_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count < 1)
                return;

            // We only select one item at a time. When we set it from the code, we need to ensure the item is scrolled into view
            // This seems like a View responsibility, not necessarily a ViewModel's. I'm not entirely sure, but it is a lot easier here.
            lvFights.ScrollIntoView(e.AddedItems[0]);
        }
    }
}
