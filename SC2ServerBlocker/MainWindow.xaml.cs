using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using SC2ServerBlocker.Properties;

namespace SC2ServerBlocker
{
    public partial class MainWindow : Window
    {
        private readonly List<Server> _servers = new List<Server>();
        private readonly RegionBlockingService _regionService;
        private StartupValidationResult _validationResult = StartupValidationResult.Ok();
        private int _blockedStateRefreshGeneration;
        private bool _suppressSelectionRefresh;

        public MainWindow()
            : this(CreateDefaultRegionBlockingService())
        {
        }

        internal MainWindow(RegionBlockingService regionService)
        {
            _regionService = regionService;
            InitializeComponent();
            RunStartupValidation();
            InitialiseServers();
            RefreshBlockedStateAsync();
        }

        private static RegionBlockingService CreateDefaultRegionBlockingService()
        {
            var repository = new ServerRepository(ServerFactory.GetServersDirectory());
            return new RegionBlockingService(new SC2FirewallManager(), repository);
        }

        private void RunStartupValidation()
        {
            _validationResult = _regionService.ValidateEnvironment();
            ApplyValidationBanner();
        }

        private void ApplyValidationBanner()
        {
            if (_validationResult.Severity == StartupValidationSeverity.None)
            {
                validationBanner.Visibility = Visibility.Collapsed;
                return;
            }

            validationBanner.Visibility = Visibility.Visible;
            validationBannerText.Text = _validationResult.Message;

            if (_validationResult.Severity == StartupValidationSeverity.Error)
            {
                validationBanner.Background = new SolidColorBrush(UIColors.BannerErrorBackground);
                validationBanner.BorderBrush = new SolidColorBrush(UIColors.BannerErrorBorder);
                validationBanner.BorderThickness = new Thickness(1);
                validationBannerText.Foreground = new SolidColorBrush(UIColors.BannerErrorForeground);
                SetStatus("Blocking is disabled until this issue is resolved.", isError: true);
            }
            else
            {
                validationBanner.Background = new SolidColorBrush(UIColors.BannerWarningBackground);
                validationBanner.BorderBrush = new SolidColorBrush(UIColors.BannerWarningBorder);
                validationBanner.BorderThickness = new Thickness(1);
                validationBannerText.Foreground = new SolidColorBrush(UIColors.BannerWarningForeground);
                SetStatus("Blocking may not take effect until the warning is resolved.", isError: false);
            }
        }

        private void InitialiseServers()
        {
            _suppressSelectionRefresh = true;

            try
            {
                _servers.Clear();
                _servers.AddRange(_regionService.LoadServers());
                serverList.ItemsSource = _servers;

                var savedRegion = Settings.Default.LastSelectedRegion;
                var selectedServer = _servers.FirstOrDefault(server =>
                    string.Equals(server.Name, savedRegion, StringComparison.OrdinalIgnoreCase));

                serverList.SelectedItem = selectedServer ?? _servers.FirstOrDefault();
            }
            finally
            {
                _suppressSelectionRefresh = false;
            }
        }

        private void RefreshBlockedState()
        {
            if (!_validationResult.AllowsBlocking)
            {
                _regionService.ApplyBlockedState(_servers, new HashSet<string>());
                blockedSummaryText.Text = BlockedRegionsFormatter.FormatSummary(new HashSet<string>());
                UpdateActionButtons();
                return;
            }

            var query = _regionService.RefreshBlockedState(_servers);
            ApplyBlockedStateQueryResult(query);
        }

        private void RefreshBlockedStateAsync()
        {
            if (!_validationResult.AllowsBlocking)
            {
                RefreshBlockedState();
                return;
            }

            var refreshGeneration = ++_blockedStateRefreshGeneration;
            SetBusyState(true);

            Task.Run(() => _regionService.QueryBlockedRegionNames(_servers))
                .ContinueWith(
                    task =>
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (refreshGeneration != _blockedStateRefreshGeneration)
                            {
                                return;
                            }

                            SetBusyState(false);

                            if (task.IsFaulted)
                            {
                                SetStatus(
                                    "Unable to refresh blocked regions. Block/unblock state may be inaccurate.",
                                    isError: true);
                                UpdateActionButtons();
                                return;
                            }

                            ApplyBlockedStateQueryResult(task.Result);
                        }));
                    });
        }

        private void ApplyBlockedStateQueryResult(BlockedRegionsQueryResult query)
        {
            if (query == null || !query.Succeeded)
            {
                var message = query == null || string.IsNullOrWhiteSpace(query.ErrorMessage)
                    ? "Unable to refresh blocked regions. Block/unblock state may be inaccurate."
                    : query.ErrorMessage;
                SetStatus(message, isError: true);
                UpdateActionButtons();
                return;
            }

            _regionService.ApplyBlockedState(_servers, query.BlockedRegionNames);
            blockedSummaryText.Text = BlockedRegionsFormatter.FormatSummary(query.BlockedRegionNames);
            UpdateActionButtons();
        }

        private void SetBusyState(bool isBusy)
        {
            Mouse.OverrideCursor = isBusy ? Cursors.Wait : null;
            serverList.IsEnabled = !isBusy;

            if (isBusy)
            {
                blockButton.IsEnabled = false;
                unblockButton.IsEnabled = false;
                unblockAllButton.IsEnabled = false;
                editIpsButton.IsEnabled = false;
                return;
            }

            UpdateActionButtons();
        }

        private void UpdateActionButtons()
        {
            var selectedServer = serverList.SelectedItem as Server;
            var states = _regionService.GetActionStates(selectedServer, _servers, _validationResult);

            blockButton.IsEnabled = states.CanBlock;
            unblockButton.IsEnabled = states.CanUnblock;
            unblockAllButton.IsEnabled = states.CanUnblockAll;
            editIpsButton.IsEnabled = states.CanEditIps;
            openFolderButton.IsEnabled = true;
        }

        private void SetStatus(string message, bool isError)
        {
            statusText.Text = message;
            statusText.Foreground = new SolidColorBrush(isError ? UIColors.StatusError : UIColors.StatusNormal);
        }

        private void OnServerSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_suppressSelectionRefresh)
            {
                return;
            }

            var selectedServer = serverList.SelectedItem as Server;
            if (selectedServer != null)
            {
                Settings.Default.LastSelectedRegion = selectedServer.Name;
                Settings.Default.Save();
            }

            UpdateActionButtons();
        }

        private void OnWindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && blockButton.IsEnabled && !serverList.IsDropDownOpen)
            {
                OnServerBlocked(blockButton, new RoutedEventArgs());
                e.Handled = true;
                return;
            }

            if (e.Key != Key.U || (Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
            {
                return;
            }

            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                if (unblockAllButton.IsEnabled)
                {
                    OnUnblockAll(unblockAllButton, new RoutedEventArgs());
                    e.Handled = true;
                }

                return;
            }

            if (unblockButton.IsEnabled)
            {
                OnServerUnblocked(unblockButton, new RoutedEventArgs());
                e.Handled = true;
            }
        }

        private void OnServerBlocked(object sender, RoutedEventArgs e)
        {
            var server = serverList.SelectedItem as Server;
            var result = _regionService.BlockSelectedServer(server);
            RefreshBlockedState();
            SetStatus(result.Message, isError: !result.Succeeded);
        }

        private void OnServerUnblocked(object sender, RoutedEventArgs e)
        {
            var server = serverList.SelectedItem as Server;
            var result = _regionService.UnblockSelectedServer(server);
            RefreshBlockedState();
            SetStatus(result.Message, isError: !result.Succeeded);
        }

        private void OnUnblockAll(object sender, RoutedEventArgs e)
        {
            var blockedCount = _servers.Count(server => server.IsBlocked);
            if (blockedCount == 0)
            {
                SetStatus("No regions are currently blocked.", isError: false);
                return;
            }

            var confirm = MessageBox.Show(
                "Remove firewall blocks for all StarCraft 2 regions?",
                "Unblock all regions",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes)
            {
                return;
            }

            var result = _regionService.UnblockAllRegions(_servers);
            RefreshBlockedState();
            SetStatus(result.Message, isError: !result.Succeeded);
        }

        private void OnEditIps(object sender, RoutedEventArgs e)
        {
            var server = serverList.SelectedItem as Server;
            if (server == null)
            {
                return;
            }

            var editor = new ServerIpEditorWindow(_servers, server.Name)
            {
                Owner = this
            };

            if (editor.ShowDialog() != true || !editor.WasSaved)
            {
                return;
            }

            var result = _regionService.SaveServerAddressesForRegion(
                editor.EditedRegionName,
                _servers,
                editor.SavedIpAddresses);

            var editedServer = RegionBlockingService.FindServer(_servers, editor.EditedRegionName);
            if (editedServer != null && !ReferenceEquals(serverList.SelectedItem, editedServer))
            {
                serverList.SelectedItem = editedServer;
            }

            RefreshBlockedState();
            SetStatus(result.Message, isError: !result.Succeeded);
        }

        private void OnOpenServersFolder(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", _regionService.GetServersDirectory());
        }

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var selectedServer = serverList.SelectedItem as Server;
            if (selectedServer != null)
            {
                Settings.Default.LastSelectedRegion = selectedServer.Name;
            }

            Settings.Default.Save();
        }
    }
}
