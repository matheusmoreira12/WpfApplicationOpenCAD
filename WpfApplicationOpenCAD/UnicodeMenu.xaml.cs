using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApplicationOpenCAD
{
    class CategoryListBoxItem : ListBoxItem
    {
        private string Representation;
        private string HexadecimalCode;

        private Grid LayoutGrid;
        private TextBlock RepresentationTextBlock;
        private TextBlock HexadecimalCodeTextBlock;

        private void populateInfo()
        {
            LayoutGrid = new Grid();

            RepresentationTextBlock = new TextBlock();
            RepresentationTextBlock.Text = Representation;
            RepresentationTextBlock.Margin = new Thickness(0);

            HexadecimalCodeTextBlock = new TextBlock();
            HexadecimalCodeTextBlock.Text = HexadecimalCode;
            HexadecimalCodeTextBlock.Opacity = .5;
            HexadecimalCodeTextBlock.Margin = new Thickness(50, 0, 0, 0);

            LayoutGrid.Children.Add(RepresentationTextBlock);
            LayoutGrid.Children.Add(HexadecimalCodeTextBlock);

            AddChild(LayoutGrid);
        }

        public CategoryListBoxItem(string representation, string hexadecimalCode)
        {
            Representation = representation;
            HexadecimalCode = hexadecimalCode;

            populateInfo();
        }
    }

    /// <summary>
    /// Interaction logic for UnicodeContextMenu.xaml
    /// </summary>
    public partial class UnicodeMenu : ContentControl
    {
        private void populateControl()
        {
            foreach (var category in OpenCAD.Unicode.UnicodeCategory.AllCategories)
            {
                var titleItem = new ListBoxItem();
                titleItem.IsEnabled = false;
                titleItem.Content = category.Name;

                ResultsListBox.Items.Add(titleItem);

                foreach (var codePoint in category.CodePoints)
                {
                    var codePointItem = new CategoryListBoxItem(codePoint.ToString(), 
                        "U+" + Convert.ToString((int)codePoint, 16).ToUpper());

                    ResultsListBox.Items.Add(codePointItem);
                }
            }
        }

        public UnicodeMenu()
        {
            InitializeComponent();

            populateControl();
        }
    }
}
