using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace launher
{
    public class Server : INotifyPropertyChanged
    {
        string address;
        ushort port;
        string name;
        int currentPlayers;
        int maxPlayers;

        public string Address
        {
            get => address;

            set
            {
                address = value;
                OnPropertyChanged(nameof(Address));
            }
        }
        public ushort Port
        {
            get => port;

            set
            {
                port = value;
                OnPropertyChanged(nameof(Port));
            }
        }
        public string Name
        {
            get => name;

            set
            {
                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        public int CurrentPlayers
        {
            get => currentPlayers;

            set
            {
                currentPlayers = value;
                OnPropertyChanged(nameof(CurrentPlayers));
            }
        }
        public int MaxPlayers
        {
            get => maxPlayers;

            set
            {
                maxPlayers = value;
                OnPropertyChanged(nameof(MaxPlayers));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string caller)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
        }
    }
}
