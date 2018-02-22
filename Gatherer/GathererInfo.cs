using DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Utilities;

namespace Gatherer
{
    public class GathererInfo : IDBData, IDeviceData
    {
        public Computer Komputer { get; set; }
        public Storage[] Dyski { get; set; }
        public Mainboard PlytaGlowna { get; set; }
        public SWM[] Swm { get; set; }
        public DeviceManager[] MenedzerUrzadzen { get; set; }
        public NetDevice[] UrzadzeniaSieciowe { get; set; }
        public double PamiecRamSuma { get; set; }
        public RAM[] Ram { get; set; }
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
                for (int i = 0; i < Ram.Length; i++)
                {
                    PamiecRamSuma = PamiecRamSuma + Ram[i].Pojemnosc;
                }
                Dyski = GatherStorageInfo();
                Swm = GatherSwmNumbers();
                MenedzerUrzadzen = GatherDeviceManagerInfo();
                UrzadzeniaSieciowe = GatherNetDevicesLanAdresses();
                KartyGraficzne = GatherGraphicCardInfo();
            }
            catch(Exception e)
            {
                string message = string.Format("Nie można pobrać informacji o sprzęcie - prawdopdobnie\nzainstalowany zły image//złe sterowniki.\n{0}", e.Message);
                throw new DriversNotInstalledException(message);
            }
            
        }

        //------------------------------------------Model, MSN, OS, WearLevel, Model obudowy------------------------------------------
        Computer GatherComputerInfo()
        {
            int i = 0;
            //Pobranie modelu
            var temp = WMI.GetSingleProperty(Win32Hardware.Win32_ComputerSystem, "Model");
            string model = GatherModel((temp.First() as Win32HardwareData).Wartosc);

            //Pobranie MSN
            temp = WMI.GetSingleProperty(Win32Hardware.Win32_BaseBoard, "SKU");
            string msn = ((temp.First() as Win32HardwareData).Wartosc);

            //Pobranie informacji o systemie operacyjnym
            temp = WMI.GetSingleProperty(Win32Hardware.Win32_OperatingSystem, "Caption");
            var temp2 = WMI.GetSingleProperty(Win32Hardware.Win32_OperatingSystem, "OSArchitecture");
            string system = string.Format(
                (temp.First() as Win32HardwareData).Wartosc + " " +
                (temp2.First() as Win32HardwareData).Wartosc);

            //Pobranie wartości WearLevel
            var temp3 = WMI.WearLevel();
            double[] wearLevel = new double[temp3.Count()];
            foreach (double value in temp3)
            {
                wearLevel[i] = value;
                i++;
            }

            //Pobieranie obudowy
            temp = WMI.GetSingleProperty(Win32Hardware.Win32_ComputerSystem, "Model");
            string obudowa = GatherCase((temp.First() as Win32HardwareData).Wartosc);

            return new Computer(md: model, msn: msn, system: system, wearLevel: wearLevel, obudowa: obudowa);
        }

        //------------------------------------------Model MB, Producent procesora, Model CPU, Taktowanie maksymalne-------------------
        Mainboard GatherMainboardInfo()
        {
            //Pobranie modelu płyty głównej
            var temp = WMI.GetSingleProperty(Win32Hardware.Win32_BaseBoard, "Product");
            string mbModel = (temp.First() as Win32HardwareData).Wartosc;

            //Pobranie producenta procesora
            temp = WMI.GetSingleProperty(Win32Hardware.Win32_BaseBoard, "Manufacturer");
            string mbProducent = (temp.First() as Win32HardwareData).Wartosc;

            //Pobranie nazwy CPU
            temp = WMI.GetSingleProperty(Win32Hardware.Win32_Processor, "Name");
            string mbCpu = (temp.First() as Win32HardwareData).Wartosc;

            //Pobranie informacji o taktowaniu (maksymalne)
            temp = WMI.GetSingleProperty(Win32Hardware.Win32_Processor, "MaxClockSpeed");
            double tempTaktowanie = double.Parse((temp.First() as Win32HardwareData).Wartosc);
            string mbTaktowanie = Math.Round((tempTaktowanie / 1000), 2).ToString() + " GHz";

            return new Mainboard(model: mbModel, producent: mbProducent, cpu: mbCpu, taktowanie: mbTaktowanie);
        }

        //------------------------------------------Aktualna wersja BIOS--------------------------------------------------------------
        Bios GatherBiosInfo()
        {
            //Pobierane informacji o wersji BIOS
            var temp = WMI.GetSingleProperty(Win32Hardware.Win32_BIOS, "SMBIOSBIOSVersion");
            string mbWersjaBios = (temp.First() as Win32HardwareData).Wartosc;
            return new Bios(mbWersjaBios);
        }

        //------------------------------------------Pojemność RAM---------------------------------------------------------------------
        RAM[] GatherRamSize()
        {
            int i = 0;
            var temp = new RAM[0];

            //Pobieranie informacji o poejmności
            //Pojemność wyrażona w GiB
            //1073741824 B = 1 GiB
            double[] pojemnosc = new double[0];
            foreach (Win32HardwareData z in WMI.GetSingleProperty(Win32Hardware.Win32_PhysicalMemory, "Capacity"))
            {
                pojemnosc = ExpandArr.Expand(pojemnosc);
                pojemnosc[i] = Math.Round(double.Parse(z.Wartosc) / 1073741824, 1);
                //Tworzenie instancji
                temp = ExpandArr.Expand(temp);
                temp[i] = new RAM(size: pojemnosc[i]);
                i++;
            }
            return temp;
        }

        //------------------------------------------Dyski twarde----------------------------------------------------------------------
        Storage[] GatherStorageInfo()
        {
            //Pobieranie informacji o nazwach dysków
            int i = 0;
            var temp = new Storage[0];
            var nazwa = new string[0];
            foreach (Win32HardwareData z in WMI.GetSingleProperty(Win32Hardware.Win32_DiskDrive, "Caption"))
            {
                nazwa = ExpandArr.Expand(nazwa);
                nazwa[i] = z.Wartosc;
                i++;
            }
            //Pobieranie informacji o poejmności
            i = 0;
            double[] size = new double[0];
            foreach (Win32HardwareData z in WMI.GetSingleProperty(Win32Hardware.Win32_DiskDrive, "Size"))
            {
                size = ExpandArr.Expand(size);
                size[i] = Math.Round(double.Parse(z.Wartosc) / 1073741824, 1);
                //Tworzenie instancji
                temp = ExpandArr.Expand(temp);
                temp[i] = new Storage(size: size[i], nazwa: nazwa[i]);
                i++;
            }
            return temp;
        }

        //------------------------------------------SWM-------------------------------------------------------------------------------
        SWM[] GatherSwmNumbers()
        {
            int i = 0;
            var temp = new SWM[0];
            foreach (SWM z in SWMSearcher.GetSWM())
            {
                temp = ExpandArr.Expand(temp);
                temp[i] = new SWM(z.Dysk, z.Swm);
                i++;
            }
            return temp;
        }

        //------------------------------------------Menedżer urządzeń-----------------------------------------------------------------
        DeviceManager[] GatherDeviceManagerInfo()
        {
            int i = 0;
            var temp = new DeviceManager[0];
            var nazwaUrzadzenia = new string[0];
            //Pobieranie nazw urządzeń
            foreach (Win32HardwareData z in WMI.GetSingleProperty(Win32Hardware.Win32_PNPEntity, "Caption", condition: "ConfigManagerErrorCode != 0"))
            {
                nazwaUrzadzenia = ExpandArr.Expand(nazwaUrzadzenia);
                nazwaUrzadzenia[i] = z.Wartosc;
                i++;
            }

            i = 0;
            //Pobieranie błędów
            foreach (Win32HardwareData z in WMI.GetSingleProperty(Win32Hardware.Win32_PNPEntity, "ConfigManagerErrorCode", condition: "ConfigManagerErrorCode != 0"))
            {
                temp = ExpandArr.Expand(temp);
                temp[i] = new DeviceManager(nazwaUrzadzenia[i], Convert.ToInt32(z.Wartosc));
                i++;
            }
            return temp;
        }

        //------------------------------------------Urządzenia sieciowe---------------------------------------------------------------
        NetDevice[] GatherNetDevicesLanAdresses()
        {
            int i = 0;
            var temp = new NetDevice[0];
            var nazwaUrzadzenia = new string[0];
            foreach (Win32HardwareData z in WMI.GetSingleProperty(Win32Hardware.Win32_NetworkAdapter, "Description", condition: "MACAddress != null AND ServiceName != 'vwifimp' AND ServiceName != 'NdisWan'"))
            {
                nazwaUrzadzenia = ExpandArr.Expand(nazwaUrzadzenia);
                nazwaUrzadzenia[i] = z.Wartosc;
                i++;
            }

            i = 0;
            foreach (Win32HardwareData z in WMI.GetSingleProperty(Win32Hardware.Win32_NetworkAdapter, "MACAddress", condition: "MACAddress != null AND ServiceName != 'vwifimp' AND ServiceName != 'NdisWan'"))
            {
                temp = ExpandArr.Expand(temp);
                temp[i] = new NetDevice(nazwaUrzadzenia[i], z.Wartosc);
                i++;
            }
            return temp;
        }

        //------------------------------------------Karty graficzne-------------------------------------------------------------------
        GraphicCard[] GatherGraphicCardInfo()
        {
            int i = 0;
            var temp = new GraphicCard[0];
            var nazwaUrzadzenia = new string[0];
            foreach (Win32HardwareData z in WMI.GetSingleProperty(Win32Hardware.Win32_VideoController, "AdapterCompatibility"))
            {
                nazwaUrzadzenia = ExpandArr.Expand(nazwaUrzadzenia);
                nazwaUrzadzenia[i] = z.Wartosc;
                i++;
            }

            i = 0;
            foreach (Win32HardwareData z in WMI.GetSingleProperty(Win32Hardware.Win32_VideoController, "Caption"))
            {
                temp = ExpandArr.Expand(temp);
                temp[i] = new GraphicCard(nazwaUrzadzenia[i], z.Wartosc);
                i++;
            }
            return temp;
        }

        //Metoda poszukująca modelu komputera
        string GatherModel(string modelString)
        {
            Regex rgx = new Regex(@"\d{5}");
            Match m = rgx.Match(modelString);
            if (m.Success)
                return m.Value;
            else
                return "";
        }

        //Metoda poszukujaca modelu obudowy komputera
        string GatherCase(string caseString)
        {
            Regex rgx = new Regex(@"[A-Za-z]{1}\d{4}");
            Match m = rgx.Match(caseString);
            if (m.Success)
                return m.Value.ToString();
            else
                return "";
        }
    }
}
