using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DataTypes
{
    //--------------------------------------------------Kontener na podstawowe dane o modelach---------------------------------------------------------
    public class Model
    {
        public int WierszModel { get; set; } //Nr linii danego modelu
        public int WierszBios { get; set; } //Nr linii z danymi o BIOSie
        public string MSN { get; set; }
        public string MD { get; set; }
        public string BiosZeszyt { get; set; }

        public Model(int wierszModel, int wierszBios, string biosZeszyt, string msn, string md)
        {
            WierszModel = wierszModel;
            WierszBios = wierszBios;
            MSN = msn;
            MD = md;
            BiosZeszyt = biosZeszyt;
        }

        //Konstruktor na potrzeby serializowania listy modeli
        public Model()
        { }
    }

    //--------------------------------------------------Kontener na ogólne i różne dane o komputerze---------------------------------------------------
    public class Computer
    {
        public string MD { get; set; }                  //Model komputera
        public string MSN { get; set; }                 //MSN komputera
        public string System { get; set; }              //System operacyjny   
        public double[] WearLevel { get; set; }         //Przegrzanie baterii
        public string Wskazowki { get; set; }           //Dodatkowe informacje odnośnie komputera
        public string Obudowa { get; set; }             //Model obudowy
        public bool ShippingMode { get; set; }          //ShippingMode
        public string StaryMSN { get; set; }             //NowyMSN
        public string LCD { get; set; }                 //Rodzaj matrycy
        public string Kolor { get; set; }               //Kolor obudowy

        public Computer(string md = "-", string msn = "-", string system = "-", double[] wearLevel = null,
            string wskazowki = "-", string obudowa = "-", string lcd = "-", string kolor = "-", bool shipp = false, string staryMsn = "-")
        {
            MD = md;
            MSN = msn;
            System = system;
            WearLevel = (wearLevel == null) ? new double[1] { -999 } : wearLevel;
            Wskazowki = wskazowki;
            Obudowa = obudowa;
            ShippingMode = shipp;
            StaryMSN = staryMsn;
            LCD = lcd;
            Kolor = kolor;
        }
    }

    //--------------------------------------------------Kontener na dane związane z płyta główną-------------------------------------------------------
    public class Mainboard
    {
        public string Model { get; set; }       //Model płyty głównej
        public string Producent { get; set; }   //Producent płyty
        public string CPU { get; set; }         //Procesor
        public string Taktowanie { get; set; }  //Taktowanie
        public string ID { get; set; }          //Pełna nazwa procesora

        public Mainboard(string model = "-", string producent = "-", string cpu = "-", string taktowanie = "-", string bios = "-")
        {
            Model = model;
            Producent = producent;
            CPU = cpu;
            Taktowanie = taktowanie;
            ID = string.Format($"{CPU}" + " @ " + $"{Taktowanie}");
        }
    }

    //--------------------------------------------------Kontener na dane o pamięci RAM-----------------------------------------------------------------
    public class RAM
    {
        public double Pojemnosc { get; set; } //Pojemność

        //Konstruktor standardowy
        public RAM(double size = 0, string bank = "-")
        {
            Pojemnosc = size;
        }

        //Konstruktor na potrzeby bazy danych w pliku excel
        public RAM(string info)
        {
            Regex[] size = new Regex[2]
            {
                new Regex(@"\d\d"),
                new Regex(@"\d")
            };
            Match result;
            foreach (Regex rgx in size)
            {
                result = rgx.Match(info);
                if (result.Success)
                {
                    Pojemnosc = Convert.ToDouble(result.Value);
                    break;
                }
                else Pojemnosc = -999;
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
            Regex[] size = new Regex[3]
            {
                new Regex(@"\d{4}"),
                new Regex(@"\d{3}"),
                new Regex(@"\d{2}")
            };
            //Wzory dla odnajdywania typu dysku
            Regex[] type = new Regex[4]
            {
                new Regex(@"[A-Za-z]{3}\s[A-Za-z]\d"),
                new Regex(@"[A-Za-z]{5}"),
                new Regex(@"[A-Za-z]{4}"),
                new Regex(@"[A-Za-z]{3}")
            };
            //Test i wynik
            Match result;
            foreach (Regex rgx in size)
            {
                result = rgx.Match(info);
                if (result.Success)
                {
                    Pojemnosc = Convert.ToDouble(result.Value);
                    break;
                }
                else Pojemnosc = 0;
            }
            foreach (Regex rgx in type)
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
            return string.Format("{0}: {1}", Nazwa, Pojemnosc);
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
        int index;
        public ConfigManagerErrorDescription(ConfigManagerErrorCode Code)
        {
            index = (int)Code;
        }
        string ReturnDescription(int index)
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
            return string.Format(ReturnDescription(index));
        }
    }

    //--------------------------------------------------Kontener na dane o błędach z menedżera urządzeń------------------------------------------------
    public class DeviceManager
    {
        public string Nazwa { get; set; }
        public string TrescBledu { get; set; }
        ConfigManagerErrorDescription Descriptor;

        public DeviceManager(string nazwa, int kod)
        {
            Nazwa = nazwa;
            Descriptor = new ConfigManagerErrorDescription((ConfigManagerErrorCode)kod);
            TrescBledu = Descriptor.ToString();
        }
    }

    //--------------------------------------------------Kontener na dane o adresach MAC urządzeń sieciowych--------------------------------------------
    public class NetDevice
    {
        public string Nazwa { get; set; }
        public string AdresMAC { get; set; }

        public NetDevice(string nazwa, string adres)
        {
            Nazwa = nazwa;
            AdresMAC = adres;
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
        public string DBPath;
        public string SHA1 { get; set; }

        public Settings() { }
    }

    //--------------------------------------------------Kontener na dane pobrane już z bazy lub WMI----------------------------------------------------------
    public struct DataPack : IDBData, IDeviceData
    {
        public Computer Komputer { get; set; }
        public RAM[] Ram { get; set; }
        public Storage[] Dyski { get; set; }
        public Mainboard PlytaGlowna { get; set; }
        public Bios WersjaBios { get; set; }
        public SWM[] Swm { get; set; }
        public DeviceManager[] MenedzerUrzadzen { get; set; }
        public NetDevice[] UrzadzeniaSieciowe { get; set; }
        public GraphicCard[] KartyGraficzne { get; set; }
    }
}
