using DataTypes;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Retriever
{
    public class ReaderInfo : INotifyPropertyChanged, IDBData
    {
        IDBManager DBManager;
        Computer _Komputer;
        public Computer Komputer
        {
            get
            {
                return _Komputer;
            }
            set
            {
                if (_Komputer != value)
                {
                    _Komputer = value;
                    OnPropertyChanged("Komputer");
                }
            }
        }
        RAM[] _Ram;
        public RAM[] Ram
        {
            get
            {
                return _Ram;
            }
            set
            {
                if (_Ram != value)
                {
                    _Ram = value;
                    OnPropertyChanged("Ram");
                }
            }
        }
        public Storage[] Dyski { get; set; }
        Mainboard _PlytaGlowna;
        public Mainboard PlytaGlowna
        {
            get
            {
                return _PlytaGlowna;
            }
            set
            {
                if (_PlytaGlowna != value)
                {
                    _PlytaGlowna = value;
                    OnPropertyChanged("PlytaGlowna");
                }
            }
        }
        SWM[] _Swm;
        public SWM[] Swm
        {
            get
            {
                return _Swm;
            }
            set
            {
                if (_Swm != value)
                {
                    _Swm = value;
                    OnPropertyChanged("Swm");
                }
            }
        }
        Bios _WersjaBios;
        public Bios WersjaBios
        {
            get
            {
                return _WersjaBios;
            }
            set
            {
                if (_WersjaBios != value)
                {
                    _WersjaBios = value;
                    OnPropertyChanged("WersjaBios");
                }
            }
        }
        public ObservableCollection<Model> ListaModeli { get; private set; }

        //Konstruktor główny
        public ReaderInfo(IDBManager dbManager)
        {
            DBManager = dbManager;
            ListaModeli = dbManager.ListaModeli;
        }

        //Odczytanie informacji o danym modelu z bazy
        public void ReadData(Model model)
        {
            if(ListaModeli.Contains(model))
            {
                DataPack readerPack = DBManager.ReadModel(model);
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
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

    }
}