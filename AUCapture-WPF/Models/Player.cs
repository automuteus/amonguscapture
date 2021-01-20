using System.CodeDom;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AmongUsCapture;

namespace AUCapture_WPF.Models
{
    public class Player : INotifyPropertyChanged
    {
        private bool _alive;

        public bool Alive
        {
            get => _alive;
            set
            {
                _alive = value;
                OnPropertyChanged();

            }
        }
        private uint _hatID;
        public uint HatID
        {
            get => _hatID;
            set
            {
                _hatID = value;
                OnPropertyChanged();

            }
        }
        private uint _pantsID;
        public uint PantsID
        {
            get => _pantsID;
            set
            {
                _pantsID = value;
                OnPropertyChanged();

            }
        }
        private uint _petID;
        public uint PetID
        {
            get => _petID;
            set
            {
                _petID = value;
                OnPropertyChanged();

            }
        }
        private PlayerColor _color;

        public PlayerColor Color
        {
            get => _color;
            set
            {
                _color = value;
                OnPropertyChanged();

            }
        }

        private string _name;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public Player(string name, PlayerColor color, bool alive, uint PantsID, uint HatID)
        {
            Name = name;
            Color = color;
            Alive = alive;
            this.PantsID = PantsID;
            this.HatID = HatID;
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
