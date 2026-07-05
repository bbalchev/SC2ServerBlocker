using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SC2ServerBlocker
{
    public class Server : INotifyPropertyChanged
    {
        private bool _isBlocked;

        public Server(String name, List<String> ipAddresses)
        {
            Name = name;
            IpAddressList = ipAddresses;
        }

        public String Name { get; private set; }

        public List<String> IpAddressList { get; private set; }

        public int IpCount
        {
            get { return IpAddressList.Count; }
        }

        public bool IsBlocked
        {
            get { return _isBlocked; }
            set
            {
                if (_isBlocked == value)
                {
                    return;
                }

                _isBlocked = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayName));
            }
        }

        public string DisplayName
        {
            get { return IsBlocked ? Name + " (blocked)" : Name; }
        }

        public void ReplaceIpAddresses(IEnumerable<string> ipAddresses)
        {
            IpAddressList = ipAddresses.ToList();
            OnPropertyChanged(nameof(IpCount));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
