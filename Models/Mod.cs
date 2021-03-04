using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace launher
{
    public class Mod : INotifyPropertyChanged
    {
        private string buttonText;

        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageLink { get; set; }
        public string ModLink { get; set; }

        public string Description { get; set; }
        public string ButtonText
        {
            get => buttonText;
            set
            {
                buttonText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ButtonText)));
            }
        }

        public byte IsRoot { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
