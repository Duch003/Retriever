using System;
using System.Text.RegularExpressions;

namespace DataTypes
{
    //--------------------------------------------------Kontener na podstawowe dane o modelach---------------------------------------------------------
    public class Model
    {
        public int WierszModel { get; set; } //Nr linii danego modelu
        public int WierszBios { get; set; } //Nr linii z danymi o BIOSie
        public string Msn { get; set; }
        public string Md { get; set; }
        public string BiosZeszyt { get; set; }

        public Model(int wierszModel, int wierszBios, string biosZeszyt, string msn, string md)
        {
            WierszModel = wierszModel;
            WierszBios = wierszBios;
            Msn = msn;
            Md = md;
            BiosZeszyt = biosZeszyt;
        }

        //Konstruktor na potrzeby serializowania listy modeli
        public Model()
        { }
    }

    //--------------------------------------------------Kontener na ogólne i różne dane o komputerze---------------------------------------------------
    public class Computer
    {
        public string Md { get; set; }                  //Model komputera
        public string Msn { get; set; }                 //MSN komputera
        public string System { get; set; }              //System operacyjny   
        public double[] WearLevel { get; set; }         //Przegrzanie baterii
        public string Wskazowki { get; set; }           //Dodatkowe informacje odnośnie komputera
        public string Obudowa { get; set; }             //Model obudowy
        public bool ShippingMode { get; set; }          //ShippingMode
        public string StaryMsn { get; set; }             //NowyMSN
        public string Lcd { get; set; }                 //Rodzaj matrycy
        public string Kolor { get; set; }               //Kolor obudowy

        public Computer(string md = "-", string msn = "-", string system = "-", double[] wearLevel = null,
            string wskazowki = "-", string obudowa = "-", string lcd = "-", string kolor = "-", bool shipp = false, string staryMsn = "-")
        {
            Md = md;
            Msn = msn;
            System = system;
            WearLevel = wearLevel ?? new double[] { -999 };
            Wskazowki = wskazowki;
            Obudowa = obudowa;
            ShippingMode = shipp;
            StaryMsn = staryMsn;
            Lcd = lcd;
            Kolor = kolor;
        }
    }

    //--------------------------------------------------Kontener na dane związane z płyta główną-------------------------------------------------------
    public class Mainboard
    {
        public string Model { get; set; }       //Model płyty głównej
        public string Producent { get; set; }   //Producent płyty
        public string Cpu { get; set; }         //Procesor
        public string Taktowanie { get; set; }  //Taktowanie
        public string Id { get; set; }          //Pełna nazwa procesora

        public Mainboard(string model = "-", string producent = "-", string cpu = "-", string taktowanie = "-")
        {
            Model = model;
            Producent = producent;
            Cpu = cpu;
            Taktowanie = taktowanie;
            Id = string.Format($"{Cpu}" + " @ " + $"{Taktowanie}");
        }
    }

    //--------------------------------------------------Kontener na dane o pamięci RAM-----------------------------------------------------------------
    public class Ram
    {
        public double Pojemnosc { get; set; } //Pojemność

        //Konstruktor standardowy
        public Ram(double size = 0)
        {
            Pojemnosc = size;
        }

        //Konstruktor na potrzeby bazy danych w pliku excel
        public Ram(string info)
        {
            var size = new[]
            {
                new Regex(@"[0-9][0-9]"),
                new Regex(@"[0-9]")
            };
            foreach (var rgx in size)
            {
                var result = rgx.Match(info);
                if (result.Success)
                {
                    Pojemnosc = Convert.ToDouble(result.Value);
                    break;
                }

                Pojemnosc = -999;
            }
        }
    }

    //--------------------------------------------------Kontener na dane o dyskach twardych------------------------------------------------------------
    public class Storage
    {
        public string Nazwa { get; set; }       //Nazwa dysku
        public double Pojemnosc { get; set; }   //Pojemność
        //Konstruktor standardowy
        public Storage(double size = 0, string nazwa = "Brak")
        {
            Nazwa = nazwa;
            Pojemnosc = size;
        }

        //Konstruktor na potrzeby bazy w pliku excel
        public Storage(string info)
        {
            //Wzory dla odnajdywania wartości pojemności
            var size = new[]
            {
                new Regex(@"\d{4}"),
                new Regex(@"\d{3}"),
                new Regex(@"\d{2}"),
                new Regex(@"\d\W|_\d"),
                new Regex(@"\d{1}")
                
            };
            //Wzory dla odnajdywania typu dysku
            var type = new[]
            {
                new Regex(@"[A-Za-z]{3}\s[A-Za-z]\d"),
                new Regex(@"[A-Za-z]{5}"),
                new Regex(@"[A-Za-z]{4}"),
                new Regex(@"[A-Za-z]{3}")
            };
            //Test i wynik
            Match result;
            
            foreach (var rgx in size)
            {
                result = rgx.Match(info);
                if(result.Success && info.ToLower().Contains("tb"))
                {
                    Pojemnosc = Convert.ToDouble(result.Value.Replace(",", ".")) * 1000;
                    break;
                }
                else if (result.Success)
                {
                    Pojemnosc = Convert.ToDouble(result.Value);
                    break;
                }
                else Pojemnosc = 0;
            }
            foreach (var rgx in type)
            {
                result = rgx.Match(info);
                if (result.Success)
                {
                    Nazwa = result.Value;
                    break;
                }
                else Nazwa = "-";
            }
        }

        //Nadpisana metoda ToString na potrzeby wyświetlania w tabeli wartości z bazy danych
        public override string ToString()
        {
            return $"{Nazwa}: {Pojemnosc}";
        }
    }

    //--------------------------------------------------Kontener na dane o IMAGE'u komputera-----------------------------------------------------------
    public class SWM
    {
        public string Dysk { get; set; } //Dysk na którym zlokalizowano swconf.dat
        public string Swm { get; set; }  //Nr SWM z pliku swconf.dat (3 linijka)

        public SWM(string swm, string dysk = "-")
        {
            Dysk = dysk;
            Swm = swm;
        }
    }

    //--------------------------------------------------Kontener na wyniki z klasy WMI-----------------------------------------------------------------
    public class Win32HardwareData
    {
        public string Wlasciwosc { get; set; }
        public string Wartosc { get; set; }

        public Win32HardwareData(string property, string value)
        {
            Wlasciwosc = property;
            Wartosc = value;
        }
    }

    //--------------------------------------------------Klasa obsługująca opisy błędów z menedżera urządzeń--------------------------------------------
    public class ConfigManagerErrorDescription
    {
        private readonly int _index;
        public ConfigManagerErrorDescription(ConfigManagerErrorCode code)
        {
            _index = (int)code;
        }

        private static string ReturnDescription(int index)
        {
            switch (index)
            {
                case 0:
                    return "This device is working properly.";
                case 1:
                    return "This device is not configured correctly.";
                case 2:
                    return "Windows cannot load the driver for this device.";
                case 3:
                    return "The driver for this device might be corrupted, or your system may be running low on memory or other resources.";
                case 4:
                    return "This device is not working properly. One of its drivers or your registry might be corrupted.";
                case 5:
                    return "The driver for this device needs a resource that Windows cannot manage. ";
                case 6:
                    return "The boot configuration for this device conflicts with other devices.";
                case 7:
                    return "Cannot filter.";
                case 8:
                    return "The driver loader for the device is missing.";
                case 9:
                    return "This device is not working properly because the controlling firmware is reporting the resources for the device incorrectly.";
                case 10:
                    return "This device cannot start.";
                case 11:
                    return "This device failed.";
                case 12:
                    return "This device cannot find enough free resources that it can use.";
                case 13:
                    return "Windows cannot verify this device's resources.";
                case 14:
                    return "This device cannot work properly until you restart your computer.";
                case 15:
                    return "This device is not working properly because there is probably a re-enumeration problem.";
                case 16:
                    return "Windows cannot identify all the resources this device uses.";
                case 17:
                    return "This device is asking for an unknown resource type";
                case 18:
                    return "Reinstall the drivers for this device.";
                case 19:
                    return "Failure using the VxD loader.";
                case 20:
                    return "Your registry might be corrupted.";
                case 21:
                    return "System failure: Try changing the driver for this device. If that does not work, see your hardware documentation. Windows is removing this device. ";
                case 22:
                    return "This device is disabled.";
                case 23:
                    return "System failure: Try changing the driver for this device. If that doesn't work, see your hardware documentation.";
                case 24:
                    return "This device is not present, is not working properly, or does not have all its drivers installed.";
                case 25:
                    return "Windows is still setting up this device.";
                case 26:
                    return "Windows is still setting up this device.";
                case 27:
                    return "This device does not have valid log configuration.";
                case 28:
                    return "The drivers for this device are not installed.";
                case 29:
                    return "This device is disabled because the firmware of the device did not give it the required resources.";
                case 30:
                    return "This device is using an Interrupt Request (IRQ) resource that another device is using.";
                case 31:
                    return "This device is not working properly because Windows cannot load the drivers required for this device.";
                default:
                    return "The cake is a lie.";
            }
        }

        public override string ToString()
        {
            return string.Format(ReturnDescription(_index));
        }
    }

    //--------------------------------------------------Kontener na dane o błędach z menedżera urządzeń------------------------------------------------
    public class DeviceManager
    {
        public string Nazwa { get; set; }
        public string TrescBledu { get; set; }

        public DeviceManager(string nazwa, int kod)
        {
            Nazwa = nazwa;
            var descriptor = new ConfigManagerErrorDescription((ConfigManagerErrorCode)kod);
            TrescBledu = descriptor.ToString();
        }
    }

    //--------------------------------------------------Kontener na dane o adresach MAC urządzeń sieciowych--------------------------------------------
    public class NetDevice
    {
        public string Nazwa { get; set; }
        public string AdresMac { get; set; }

        public NetDevice(string nazwa, string adres)
        {
            Nazwa = nazwa;
            AdresMac = adres;
        }
    }

    //--------------------------------------------------Kontener na dane o wersji BIOS która powinna być-----------------------------------------------
    public class Bios
    {
        public string Wersja { get; set; }
        public string Opis { get; set; }

        public Bios(string ver, string opis = "-")
        {
            Wersja = ver;
            Opis = opis;
        }
    } 

    //--------------------------------------------------Kontener na dane o kartach graficznych----------------------------------------------------------
    public class GraphicCard
    {
        public string Nazwa { get; set; }
        public string Opis { get; set; }

        public GraphicCard(string nazwa, string opis)
        {
            Nazwa = nazwa;
            Opis = opis;
        }
    }

    public class Settings
    {
        public string DbPath;
        public string Sha1 { get; set; }
    }

    //--------------------------------------------------Kontener na dane pobrane już z bazy lub WMI----------------------------------------------------------
    public struct DataPack : IDbData, IDeviceData
    {
        public Computer Komputer { get; set; }
        public Ram[] Ram { get; set; }
        public Storage[] Dyski { get; set; }
        public Mainboard PlytaGlowna { get; set; }
        public Bios WersjaBios { get; set; }
        public SWM[] Swm { get; set; }
        public DeviceManager[] MenedzerUrzadzen { get; set; }
        public NetDevice[] UrzadzeniaSieciowe { get; set; }
        public GraphicCard[] KartyGraficzne { get; set; }
    }
}
