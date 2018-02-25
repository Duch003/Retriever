using System;
using System.Linq;
using System.Text.RegularExpressions;
using DataTypes;
using Utilities;

namespace Gatherer
{
    public class GathererInfo : IDbData, IDeviceData
    {
        public Computer Komputer { get; set; }
        public Storage[] Dyski { get; set; }
        public Mainboard PlytaGlowna { get; set; }
        public SWM[] Swm { get; set; }
        public DeviceManager[] MenedzerUrzadzen { get; set; }
        public NetDevice[] UrzadzeniaSieciowe { get; set; }
        public double PamiecRamSuma { get; set; }
        public Ram[] Ram { get; set; }
        public GraphicCard[] KartyGraficzne { get; set; }
        public Bios WersjaBios { get; set; }

        public GathererInfo()
        {
            try
            {
                Komputer = GatherComputerInfo();
                PlytaGlowna = GatherMainboardInfo();
                WersjaBios = GatherBiosInfo();
                Ram = GatherRamSize();
                PamiecRamSuma = 0;
                foreach (var t in Ram)
                {
                    PamiecRamSuma = PamiecRamSuma + t.Pojemnosc;
                }
                Dyski = GatherStorageInfo();
                Swm = GatherSwmNumbers();
                MenedzerUrzadzen = GatherDeviceManagerInfo();
                UrzadzeniaSieciowe = GatherNetDevicesLanAdresses();
                KartyGraficzne = GatherGraphicCardInfo();
            }
            catch(Exception e)
            {
                var message =
                    $"Nie można pobrać informacji o sprzęcie - prawdopdobnie\nzainstalowany zły image//złe sterowniki.\n{e.Message}";
                throw new DriversNotInstalledException(message);
            }
            
        }

        //------------------------------------------Model, MSN, OS, WearLevel, Model obudowy------------------------------------------
        private static Computer GatherComputerInfo()
        {
            //Pobranie modelu
            var temp = Wmi.GetSingleProperty(Win32Hardware.Win32_ComputerSystem, "Model");
            var model = GatherModel(temp.First().Wartosc);

            //Pobranie MSN
            temp = Wmi.GetSingleProperty(Win32Hardware.Win32_BaseBoard, "SKU");
            var msn = temp.First().Wartosc;

            //Pobranie informacji o systemie operacyjnym
            temp = Wmi.GetSingleProperty(Win32Hardware.Win32_OperatingSystem, "Caption");
            var temp2 = Wmi.GetSingleProperty(Win32Hardware.Win32_OperatingSystem, "OSArchitecture");
            var system = string.Format(
                temp.First().Wartosc + " " +
                temp2.First().Wartosc);

            //Pobranie wartości WearLevel
            var temp3 = Wmi.WearLevel();
            var wearLevel = temp3.ToArray();

            //Pobieranie obudowy
            temp = Wmi.GetSingleProperty(Win32Hardware.Win32_ComputerSystem, "Model");
            var obudowa = GatherCase(temp.First().Wartosc);

            return new Computer(model, msn, system, wearLevel, obudowa: obudowa);
        }

        //------------------------------------------Model MB, Producent procesora, Model CPU, Taktowanie maksymalne-------------------
        private static Mainboard GatherMainboardInfo()
        {
            //Pobranie modelu płyty głównej
            var temp = Wmi.GetSingleProperty(Win32Hardware.Win32_BaseBoard, "Product");
            var mbModel = temp.First().Wartosc;

            //Pobranie producenta procesora
            temp = Wmi.GetSingleProperty(Win32Hardware.Win32_BaseBoard, "Manufacturer");
            var mbProducent = temp.First().Wartosc;

            //Pobranie nazwy CPU
            temp = Wmi.GetSingleProperty(Win32Hardware.Win32_Processor, "Name");
            var mbCpu = temp.First().Wartosc;

            //Pobranie informacji o taktowaniu (maksymalne)
            temp = Wmi.GetSingleProperty(Win32Hardware.Win32_Processor, "MaxClockSpeed");
            var tempTaktowanie = double.Parse(temp.First().Wartosc);
            var mbTaktowanie = Math.Round((tempTaktowanie / 1000), 2) + " GHz";

            return new Mainboard(mbModel, mbProducent, mbCpu, mbTaktowanie);
        }

        //------------------------------------------Aktualna wersja BIOS--------------------------------------------------------------
        private static Bios GatherBiosInfo()
        {
            //Pobierane informacji o wersji BIOS
            var temp = Wmi.GetSingleProperty(Win32Hardware.Win32_BIOS, "SMBIOSBIOSVersion");
            var mbWersjaBios = temp.First().Wartosc;
            return new Bios(mbWersjaBios);
        }

        //------------------------------------------Pojemność RAM---------------------------------------------------------------------
        private static Ram[] GatherRamSize()
        {
            var i = 0;
            var temp = new Ram[0];

            //Pobieranie informacji o poejmności
            //Pojemność wyrażona w GiB
            //1073741824 B = 1 GiB
            var pojemnosc = new double[0];
            foreach (var z in Wmi.GetSingleProperty(Win32Hardware.Win32_PhysicalMemory, "Capacity"))
            {
                pojemnosc = ExpandArr.Expand(pojemnosc);
                pojemnosc[i] = Math.Round(double.Parse(z.Wartosc) / 1073741824, 1);
                //Tworzenie instancji
                temp = ExpandArr.Expand(temp);
                temp[i] = new Ram(pojemnosc[i]);
                i++;
            }
            return temp;
        }

        //------------------------------------------Dyski twarde----------------------------------------------------------------------
        private static Storage[] GatherStorageInfo()
        {
            //Pobieranie informacji o nazwach dysków
            var i = 0;
            var temp = new Storage[0];
            var nazwa = new string[0];
            foreach (var z in Wmi.GetSingleProperty(Win32Hardware.Win32_DiskDrive, "Caption"))
            {
                nazwa = ExpandArr.Expand(nazwa);
                nazwa[i] = z.Wartosc;
                i++;
            }
            //Pobieranie informacji o poejmności
            i = 0;
            var size = new double[0];
            foreach (var z in Wmi.GetSingleProperty(Win32Hardware.Win32_DiskDrive, "Size"))
            {
                size = ExpandArr.Expand(size);
                size[i] = Math.Round(double.Parse(z.Wartosc) / 1073741824, 1);
                //Tworzenie instancji
                temp = ExpandArr.Expand(temp);
                temp[i] = new Storage(size[i], nazwa[i]);
                i++;
            }
            return temp;
        }

        //------------------------------------------SWM-------------------------------------------------------------------------------
        private static SWM[] GatherSwmNumbers()
        {
            var i = 0;
            var temp = new SWM[0];
            foreach (var z in SwmSearcher.GetSwm())
            {
                temp = ExpandArr.Expand(temp);
                temp[i] = new SWM(z.Dysk, z.Swm);
                i++;
            }
            return temp;
        }

        //------------------------------------------Menedżer urządzeń-----------------------------------------------------------------
        private static DeviceManager[] GatherDeviceManagerInfo()
        {
            var i = 0;
            var temp = new DeviceManager[0];
            var nazwaUrzadzenia = new string[0];
            //Pobieranie nazw urządzeń
            foreach (var z in Wmi.GetSingleProperty(Win32Hardware.Win32_PNPEntity, "Caption", condition: "ConfigManagerErrorCode != 0"))
            {
                nazwaUrzadzenia = ExpandArr.Expand(nazwaUrzadzenia);
                nazwaUrzadzenia[i] = z.Wartosc;
                i++;
            }

            i = 0;
            //Pobieranie błędów
            foreach (var z in Wmi.GetSingleProperty(Win32Hardware.Win32_PNPEntity, "ConfigManagerErrorCode", condition: "ConfigManagerErrorCode != 0"))
            {
                temp = ExpandArr.Expand(temp);
                temp[i] = new DeviceManager(nazwaUrzadzenia[i], Convert.ToInt32(z.Wartosc));
                i++;
            }
            return temp;
        }

        //------------------------------------------Urządzenia sieciowe---------------------------------------------------------------
        private static NetDevice[] GatherNetDevicesLanAdresses()
        {
            var i = 0;
            var temp = new NetDevice[0];
            var nazwaUrzadzenia = new string[0];
            foreach (var z in Wmi.GetSingleProperty(Win32Hardware.Win32_NetworkAdapter, "Description", condition: "MACAddress != null AND ServiceName != 'vwifimp' AND ServiceName != 'NdisWan'"))
            {
                nazwaUrzadzenia = ExpandArr.Expand(nazwaUrzadzenia);
                nazwaUrzadzenia[i] = z.Wartosc;
                i++;
            }

            i = 0;
            foreach (var z in Wmi.GetSingleProperty(Win32Hardware.Win32_NetworkAdapter, "MACAddress", condition: "MACAddress != null AND ServiceName != 'vwifimp' AND ServiceName != 'NdisWan'"))
            {
                temp = ExpandArr.Expand(temp);
                temp[i] = new NetDevice(nazwaUrzadzenia[i], z.Wartosc);
                i++;
            }
            return temp;
        }

        //------------------------------------------Karty graficzne-------------------------------------------------------------------
        private static GraphicCard[] GatherGraphicCardInfo()
        {
            var i = 0;
            var temp = new GraphicCard[0];
            var nazwaUrzadzenia = new string[0];
            foreach (var z in Wmi.GetSingleProperty(Win32Hardware.Win32_VideoController, "AdapterCompatibility"))
            {
                nazwaUrzadzenia = ExpandArr.Expand(nazwaUrzadzenia);
                nazwaUrzadzenia[i] = z.Wartosc;
                i++;
            }

            i = 0;
            foreach (var z in Wmi.GetSingleProperty(Win32Hardware.Win32_VideoController, "Caption"))
            {
                temp = ExpandArr.Expand(temp);
                temp[i] = new GraphicCard(nazwaUrzadzenia[i], z.Wartosc);
                i++;
            }
            return temp;
        }

        //Metoda poszukująca modelu komputera
        private static string GatherModel(string modelString)
        {
            var rgx = new Regex(@"\d{5}");
            var m = rgx.Match(modelString);
            return m.Success ? m.Value : "";
        }

        //Metoda poszukujaca modelu obudowy komputera
        private static string GatherCase(string caseString)
        {
            var rgx = new Regex(@"[A-Za-z]{1}\d{4}");
            var m = rgx.Match(caseString);
            return m.Success ? m.Value : "";
        }
    }
}
