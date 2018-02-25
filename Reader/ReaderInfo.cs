using DataTypes;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Retriever
{
    public class ReaderInfo : INotifyPropertyChanged, IDbData
    {
        private readonly IDbManager _dbManager;
        private Computer _komputer;
        public Computer Komputer
        {
            get => _komputer;
            set
            {
                if (_komputer == value) return;
                _komputer = value;
                OnPropertyChanged("Komputer");
            }
        }

        private Ram[] _ram;
        public Ram[] Ram
        {
            get => _ram;
            set
            {
                if (_ram == value) return;
                _ram = value;
                OnPropertyChanged("Ram");
            }
        }

        public Storage[] Dyski { get; set; }

        private Mainboard _plytaGlowna;
        public Mainboard PlytaGlowna
        {
            get => _plytaGlowna;
            set
            {
                if (_plytaGlowna == value) return;
                _plytaGlowna = value;
                OnPropertyChanged("PlytaGlowna");
            }
        }

        private SWM[] _swm;
        public SWM[] Swm
        {
            get => _swm;
            set
            {
                if (_swm == value) return;
                _swm = value;
                OnPropertyChanged("Swm");
            }
        }

        private Bios _wersjaBios;
        public Bios WersjaBios
        {
            get => _wersjaBios;
            set
            {
                if (_wersjaBios == value) return;
                _wersjaBios = value;
                OnPropertyChanged("WersjaBios");
            }
        }

        public ObservableCollection<Model> ListaModeli { get; }

        //Konstruktor główny
        public ReaderInfo(IDbManager dbManager)
        {
            _dbManager = dbManager;
            ListaModeli = dbManager.ListaModeli;
        }

        //Odczytanie informacji o danym modelu z bazy
        public void ReadData(Model model)
        {
            if(ListaModeli.Contains(model))
            {
                var readerPack = _dbManager.ReadModel(model);
                PlytaGlowna = readerPack.PlytaGlowna;
                Komputer = readerPack.Komputer;
                Ram = readerPack.Ram;
                Dyski = readerPack.Dyski;
                Swm = readerPack.Swm;
                WersjaBios = readerPack.WersjaBios;
            }
            else
            {
                PlytaGlowna = null;
                Komputer = null;
                Ram = null;
                Dyski = null;
                Swm = null;
                WersjaBios = null;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}