using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SC2ServerBlocker
{
    public partial class ServerIpEditorWindow : Window
    {
        private readonly ServerIpEditorSession _session;
        private bool _suppressRegionChange;

        public ServerIpEditorWindow(IReadOnlyList<Server> servers, string initialRegionName)
        {
            if (servers == null)
            {
                throw new ArgumentNullException(nameof(servers));
            }

            InitializeComponent();
            _session = new ServerIpEditorSession(servers, initialRegionName);
            InitializeRegionSelector();
            UpdateRegionDescription();
            RefreshList();
        }

        public string EditedRegionName
        {
            get { return _session.SelectedRegionName; }
        }

        public IReadOnlyList<string> SavedIpAddresses
        {
            get { return _session.IpAddresses.ToList(); }
        }

        public bool WasSaved { get; private set; }

        private void InitializeRegionSelector()
        {
            _suppressRegionChange = true;

            try
            {
                regionCombo.ItemsSource = _session.AvailableRegions;
                regionCombo.SelectedItem = _session.SelectedRegionName;
            }
            finally
            {
                _suppressRegionChange = false;
            }
        }

        private void OnRegionSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_suppressRegionChange)
            {
                return;
            }

            var selectedRegion = regionCombo.SelectedItem as string;
            if (selectedRegion == null)
            {
                return;
            }

            string errorMessage;
            if (!_session.TrySelectRegion(selectedRegion, out errorMessage))
            {
                MessageBox.Show(
                    errorMessage,
                    "Region",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            UpdateRegionDescription();
            RefreshList();
            newIpTextBox.Clear();
        }

        private void UpdateRegionDescription()
        {
            regionLabel.Text = _session.GetDescriptionText();
        }

        private void RefreshList()
        {
            ipList.ItemsSource = null;
            ipList.ItemsSource = _session.IpAddresses.ToList();
        }

        private void OnAddIp(object sender, RoutedEventArgs e)
        {
            AddIpFromTextBox();
        }

        private void OnNewIpKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddIpFromTextBox();
                e.Handled = true;
            }
        }

        private void AddIpFromTextBox()
        {
            string errorMessage;
            if (!_session.TryAddAddress(newIpTextBox.Text, out errorMessage))
            {
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    MessageBox.Show(
                        errorMessage,
                        string.Equals(errorMessage, "That address is already in the list.", StringComparison.Ordinal)
                            ? "Duplicate address"
                            : "Invalid address",
                        MessageBoxButton.OK,
                        string.Equals(errorMessage, "That address is already in the list.", StringComparison.Ordinal)
                            ? MessageBoxImage.Information
                            : MessageBoxImage.Warning);
                }

                return;
            }

            newIpTextBox.Clear();
            RefreshList();
            ipList.SelectedItem = _session.IpAddresses.Last();
        }

        private void OnRemoveSelected(object sender, RoutedEventArgs e)
        {
            var selected = ipList.SelectedItem as string;
            if (selected == null)
            {
                return;
            }

            _session.TryRemoveAddress(selected);
            RefreshList();
        }

        private void OnOpenFolder(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", ServerFactory.GetServersDirectory());
        }

        private void OnSave(object sender, RoutedEventArgs e)
        {
            if (!_session.CanSave())
            {
                MessageBox.Show(
                    "Add at least one IP address before saving.",
                    "No addresses",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            WasSaved = true;
            DialogResult = true;
            Close();
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
