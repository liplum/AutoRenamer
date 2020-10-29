using System.Collections.ObjectModel;
using System.Windows;

namespace AutoRenamerWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            RuleBoxList.ItemsSource = AllRuleBoxs;
            //allRule.Add(new RuleBox("123"));
            //AllPresetListBox.DisplayMemberPath = "Name";
        }
        public readonly ObservableCollection<RuleBox> AllRuleBoxs = new ObservableCollection<RuleBox>();

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void NewRulePart_Click(object sender, RoutedEventArgs e)
        {
            AllRuleBoxs.Add(new RuleBox());
        }
    }
}
