using System;
using System.Linq;

namespace Retriever
{
    public class Gatherer
    {
        public Computer Komputer { get; set; }
        public RAM[] Ram { get; private set; }
        public Storage[] Dyski { get; private set; }
        public Mainboard PlytaGlowna { get; private set; }
        public SWM[] Swm { get; private set; }
        public double PamiecRamSuma { get; private set; }

        public Gatherer()
        {
            int i = 0; //Iterator dla pętli foreach
            #region Tworzenie instancji Computer

            //Pobranie modelu
            var temp = WMI.GetSingleProperty(Win32Hardware.Win32_ComputerSystem, "Model");
            string model = GatherModel((temp.First() as Win32HardwareData).Wartosc, true);

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
            i = 0;
            var temp3 = WMI.WearLevel();
            double[] wearLevel = new double[temp3.Count()];
            foreach (double value in temp3)
            {
                wearLevel[i] = value;
                i++;
            }

            //Pobieranie obudowy - jest to zamienne z biosem
            temp = WMI.GetSingleProperty(Win32Hardware.Win32_ComputerSystem, "Model");
            string obudowa = GatherModel((temp.First() as Win32HardwareData).Wartosc, false);
            #endregion
            Komputer = new Computer(md: model, msn: msn, system: system, wearLevel: wearLevel, obudowa: obudowa);

            #region Tworzenie instancji Mainboard
            //Pobranie modelu płyty głównej
            temp = WMI.GetSingleProperty(Win32Hardware.Win32_BaseBoard, "Product");
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

            //Pobierane informacji o wersji BIOS
            temp = WMI.GetSingleProperty(Win32Hardware.Win32_BIOS, "SMBIOSBIOSVersion");
            string mbWersjaBios = (temp.First() as Win32HardwareData).Wartosc;
            #endregion
            PlytaGlowna = new Mainboard(model: mbModel, producent: mbProducent, cpu: mbCpu, taktowanie: mbTaktowanie, bios: mbWersjaBios);

            #region Tworzenie instancji RAM
            //Pobieranie informacji o bankach
            i = 0;
            Ram = new RAM[0];
            string[] bank = new string[0];
            foreach (Win32HardwareData z in WMI.GetSingleProperty(Win32Hardware.Win32_PhysicalMemory, "BankLabel"))
            {
                bank = ExpandArr.Expand(bank);
                bank[i] = z.Wartosc;
                i++;
            }
            //Pobieranie informacji o poejmności
            //Pojemność wyrażona w GiB
            //1073741824 B = 1 GiB
            i = 0;
            double[] pojemnosc = new double[0];
            PamiecRamSuma = 0;
            foreach (Win32HardwareData z in WMI.GetSingleProperty(Win32Hardware.Win32_PhysicalMemory, "Capacity"))
            {
                pojemnosc = ExpandArr.Expand(pojemnosc);
                pojemnosc[i] = Math.Round(double.Parse(z.Wartosc) / 1073741824, 1);
                //Tworzenie instancji
                Ram = ExpandArr.Expand(Ram);
                Ram[i] = new RAM(size: pojemnosc[i], bank: bank[i]);
                PamiecRamSuma = PamiecRamSuma + Ram[i].Pojemnosc;
                i++;
            }
            #endregion

            #region Tworzenie instancji Storage
            //Pobieranie informacji o nazwach dysków
            i = 0;
            Dyski = new Storage[0];
            string[] nazwa = new string[0];
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
                Dyski = ExpandArr.Expand(Dyski);
                Dyski[i] = new Storage(size: size[i], nazwa: nazwa[i]);
                i++;
            }
            #endregion

            #region Tworzenie instancji SWM
            i = 0;
            Swm = new SWM[0];
            foreach (SWM z in SWMSearcher.GetSWM())
            {
                Swm = ExpandArr.Expand(Swm);
                Swm[i] = new SWM(z.Dysk, z.Swm);
                i++;
            }
            #endregion
        }



        //Metoda poszukująca modelu
        string GatherModel(string modelString, bool modelFlag)
        {
            bool[] flags = new bool[5]; //Flagi znaków
            int j = 0; //Iterator do poruszania się po flagach
            string ans = ""; //String z odpowiedzią
            if (modelFlag)
            {
                if ("GE72 6QD" == modelString) return "GE72 6QD";
                //W tym przypadku poszukiwany jest MD. Wszystkie 5 flag oznacza cyfry
                for (int i = 0; i < modelString.Length; i++)
                {
                    //Jeżeli znak jest cyfrą
                    if (char.IsDigit(modelString[i]))
                    {
                        //Oznacz flagę cyfry
                        flags[j] = true;
                        ans = ans + modelString[i];
                        j++;
                    }
                    //W innym przypadku
                    else
                    {
                        //Zerowanie
                        flags = new bool[5];
                        ans = "";
                        j = 0;
                    }

                    if (flags[0] == true && flags[1] == true && flags[2] == true && flags[3] == true && flags[4] == true)
                        return ans;
                }
            }
            else
            {
                //W tym przypadku poszukiwany jest model obudowy. Pierwsza flaga to litera, pozostałe to cyfry
                for (int i = 0; i < modelString.Length; i++)
                {
                    //Jeżeli znak jest liczbą i znaleziono już pierwszą literę
                    if (char.IsDigit(modelString[i]) && flags[0] == true)
                    {
                        //Oznacz flagę cyfry
                        flags[j] = true;
                        ans = ans + modelString[i];
                        j++;
                    }
                    //Jeżeli znaleziono literę i nie ma pierwszej flagi litery
                    else if (char.IsLetter(modelString[i]) && flags[0] == false)
                    {
                        flags[0] = true;
                        ans = ans + modelString[i];
                        j++;
                    }
                    //W innym przypadku (np.: kolejna litera)
                    else
                    {
                        //Zerowanie
                        flags = new bool[5];
                        ans = "";
                        j = 0;
                    }

                    if (flags[0] == true && flags[1] == true && flags[2] == true && flags[3] == true && flags[4] == true)
                        return ans;
                }
            }
            return "-";
        }
    }
}
