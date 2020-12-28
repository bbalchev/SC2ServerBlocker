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

namespace SC2ServerBlocker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Server> Servers = new List<Server>();
        SC2FirewallManager Firewall = new SC2FirewallManager();

        public MainWindow()
        {
            InitializeComponent();
            InitialiseServers();
        }

        private void InitialiseServers()
        {
            Servers = ServerFactory.GetServers();
            serverList.ItemsSource = Servers;
            serverList.SelectedIndex = 0;
        }

        private void OnServerBlocked(object sender, RoutedEventArgs e)
        {
            var server = serverList.SelectedItem as Server;
            Firewall.BlockServer(server);
            MessageBox.Show(String.Format("{0} game servers are blocked.", server.Name), "Server blocked",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnServerUnblocked(object sender, RoutedEventArgs e)
        {
            var server = serverList.SelectedItem as Server;
            Firewall.UnblockServer(server);
            MessageBox.Show(String.Format("{0} game servers are unblocked.", server.Name), "Server unblocked",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
