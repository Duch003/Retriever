using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.IO;
using System.Runtime.InteropServices;

namespace Retriever
{
    //--------------------------------------------------Klasa obsługująca WMI do wydobywania danych--------------------------------------------------
    public static class WMI
    {
        //Metoda zwracająca pojedyńczą właściwość z określonej klasy
        public static IEnumerable<Win32HardwareData> GetSingleProperty(Win32Hardware hardwareClass, string property, string scope = "root/cimv2")
        {
            string query = string.Format("SELECT {0} FROM {1}", property, hardwareClass.ToString());
            ManagementObjectSearcher search = new ManagementObjectSearcher(scope, query);
            foreach (ManagementObject mo in search.Get())
            {
                yield return new Win32HardwareData(property, (mo[property] == null) ? "" : mo[property].ToString());
            }
        }

        //Metoda zwracająca całą zawartość określonej klasy
        public static IEnumerable<Win32HardwareData> GetAll(Win32Hardware hardwareClass, string scope = "root/cimv2")
        {
            string query = string.Format("SELECT * FROM {0}", hardwareClass.ToString());
            ManagementObjectSearcher search = new ManagementObjectSearcher(scope, query);
            foreach (ManagementObject mo in search.Get())
            {
                foreach (PropertyData prop in mo.Properties)
                {
                    yield return new Win32HardwareData(prop.Name, (prop.Value == null) ? "-" : prop.Value.ToString());
                }
            }
        }

        //Metoda zwracająca zużycie baterii
        public static IEnumerable<double> WearLevel()
        {
            //Zmienne dla obliczeń
            double[] designed = new double[0];
            double[] full = new double[0];
            int i = 0;
            //Pobieranie informacji o zaprojektowanej pojemności baterii
            IEnumerable<Win32HardwareData> designedCapacity = GetSingleProperty(Win32Hardware.BatteryStaticData, "DesignedCapacity", "root/wmi");
            //Z każdej znalezionej baterii wyciągnij i przelicz na Wh pojemnosć zaprojektowaną
            foreach (Win32HardwareData z in designedCapacity)
            {
                designed = ExpandArr.Expand(designed);
                designed[i] = Math.Round((double.Parse(z.Wartosc) / 1000), 2); //[Wh]
            }
            //Wyciągniej informacje o rzeczywistym maksymalnym możliwym naładowaniu baterii
            IEnumerable<Win32HardwareData> fullChargeCapacity = GetSingleProperty(Win32Hardware.BatteryFullChargedCapacity, "FullChargedCapacity", "root/wmi");
            i = 0;
            //Z każdej znalezionej baterii wyciągniej i przelicz na Wh pojemność rzeczywistą
            foreach (Win32HardwareData z in fullChargeCapacity)
            {
                full = ExpandArr.Expand(designed);
                full[i] = Math.Round((double.Parse(z.Wartosc) / 1000), 2);
            }
            //Zwracaj WearLevele każdej baterii
            for (i = 0; i < designed.Length; i++)
            {
                yield return Math.Round(((1 - (full[i] / designed[i])) * 100), 2);
            }
        }
    }

    //--------------------------------------------------Klasa obsługująca odnajdywanie i wydobywanie numerów SWM-------------------------------------
    public static class SWMSearcher
    {
        public static string[] SWM { get; private set; }
        public static string[] Drive { get; private set; }
        static DriveInfo[] allDrives = DriveInfo.GetDrives(); //Pobranie informacji o dyskach logicznych an komputerze

        //Pobieranie SWM z plików swconf.dat
        public static IEnumerable<SWM> GetSWM()
        {
            int i = 0; //Iterator słuzący do poruszania się po tablicy
            SWM = new string[0]; //Początkowa wartość tablicy
            Drive = new string[0];
            foreach (DriveInfo d in allDrives)
            {
                //Sprawdza gotowość dysku do odczytu
                if (d.IsReady == true)
                {
                    //Utwórz instancję pliku swconf.dat na danym dysku
                    FileInfo fInfo = new FileInfo(d.Name + "swconf.dat");
                    //Jeżeli pliku nie ma na dysku, przejdź do następnego
                    if (!fInfo.Exists)
                    {
                        continue;
                    }
                    //W innym wypadku odczytaj 3 linię z pliku i dodaj do smiennej SWM
                    else
                    {
                        Expand();
                        //Daną wyjściową jest np: D:\12345678
                        SWM[i] = string.Format($"{File.ReadLines(d.Name + "swconf.dat").Skip(2).Take(1).First()}");
                        Drive[i] = string.Format($"{d.Name}");
                        yield return new SWM(Drive[i], SWM[i]);
                        i++;
                    }
                }
            }
            if (SWM.Length == 0)
            {
                Expand();
                SWM[0] = "Brak SWM w plikach";
                Drive[0] = "-";
            }
        }

        //Metoda powiększająca tablice SWM i Drive
        static void Expand()
        {
            SWM = ExpandArr.Expand(SWM);
            Drive = ExpandArr.Expand(Drive);
        }
    }

    //--------------------------------------------------Klasa powiększająca tablice------------------------------------------------------------------
    public static class ExpandArr
    {
        //DOUBLE
        public static double[] Expand(double[] arr)
        {
            double[] temp = arr;
            arr = new double[temp.Length + 1];
            for (int i = 0; i < temp.Length; i++)
            {
                arr[i] = temp[i];
            }
            return arr;
        }

        //STRING
        public static string[] Expand(string[] arr)
        {
            string[] temp = arr;
            arr = new string[temp.Length + 1];
            for (int i = 0; i < temp.Length; i++)
            {
                arr[i] = temp[i];
            }
            return arr;
        }

        //RAM
        public static RAM[] Expand(RAM[] arr)
        {
            RAM[] temp = arr;
            arr = new RAM[temp.Length + 1];
            for (int i = 0; i < temp.Length; i++)
            {
                arr[i] = temp[i];
            }
            return arr;
        }

        //STORAGE
        public static Storage[] Expand(Storage[] arr)
        {
            Storage[] temp = arr;
            arr = new Storage[temp.Length + 1];
            for (int i = 0; i < temp.Length; i++)
            {
                arr[i] = temp[i];
            }
            return arr;
        }

        //SWM
        public static SWM[] Expand(SWM[] arr)
        {
            SWM[] temp = arr;
            arr = new SWM[temp.Length + 1];
            for (int i = 0; i < temp.Length; i++)
            {
                arr[i] = temp[i];
            }
            return arr;
        }
    }

    //--------------------------------------------------Klasa dla obsługi metody zmiany daty i czasu w systemie--------------------------------------
    public static class SystemDateTimeChanger
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetSystemTime(ref SYSTEMTIME st);
    }

}
