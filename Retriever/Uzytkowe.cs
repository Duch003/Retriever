using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.IO;
using System.Windows;

namespace Retriever
{
    //--------------------------------------------------Klasa obsługująca WMI do wydobywania danych--------------------------------------------------
    public static class WMI
    {

        //Metoda zwracająca pojedyńczą właściwość z określonej klasy z warunkiem
        public static IEnumerable<Win32HardwareData> GetSingleProperty(Win32Hardware hardwareClass, string property, string condition, string scope = "root/cimv2")
        {
            string query = string.Format("SELECT {0} FROM {1} WHERE {2}", property, hardwareClass.ToString(), condition);
            ManagementObjectSearcher search = new ManagementObjectSearcher(scope, query);
            foreach (ManagementObject mo in search.Get())
            {
                yield return new Win32HardwareData(property, (mo[property] == null) ? "" : mo[property].ToString());
            }
            
        }

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

        //Metoda zwracająca całą zawartość określonej klasy z warunkiem
        public static IEnumerable<Win32HardwareData> GetAll(Win32Hardware hardwareClass, string condition, string scope = "root/cimv2")
        {
            string query = string.Format("SELECT * FROM {0} WHERE {1}", hardwareClass.ToString(), condition);
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
                i++;
            }
            //Wyciągniej informacje o rzeczywistym maksymalnym możliwym naładowaniu baterii
            IEnumerable<Win32HardwareData> fullChargeCapacity = GetSingleProperty(Win32Hardware.BatteryFullChargedCapacity, "FullChargedCapacity", "root/wmi");
            i = 0;
            //Z każdej znalezionej baterii wyciągniej i przelicz na Wh pojemność rzeczywistą
            foreach (Win32HardwareData z in fullChargeCapacity)
            {
                full = ExpandArr.Expand(full);
                full[i] = Math.Round((double.Parse(z.Wartosc) / 1000), 2);
                i++;
            }
            //Zwracaj WearLevele każdej baterii
            for (i = 0; i < designed.Length; i++)
            {
                yield return Math.Round(((1 - (full[i] / designed[i])) * 100), 2);
            }
        }

        //Metoda zwracająca klucz Windows
        public static string GetOriginalProductKey()
        {
            ManagementScope scope = new ManagementScope("root/cimv2");
            ObjectQuery query = new ObjectQuery("SELECT * FROM SoftwareLicensingService Where OA3xOriginalProductKey != null");
            ManagementObjectSearcher search = new ManagementObjectSearcher(scope, query);
            ManagementObjectCollection ans = search.Get();
            foreach (ManagementObject m in ans)
            {
                return m["OA3xOriginalProductKey"].ToString();
            }
            return null;
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
        public static T[] Expand<T>(T[] arr)
        {
            var temp = arr;
            arr = new T[temp.Length + 1];
            for (int i = 0; i < temp.Length; i++)
            {
                arr[i] = temp[i];
            }
            return arr;
        }
    }

    //--------------------------------------------------Klasa obsługująca zapisywanie i wyświetlanie błędów------------------------------------------
    public static class ErrorWriter
    {
        //Metoda zapisująca loga
        public static bool WriteErrorLog(Exception e)
        {
            bool logCreated = true;
            FileInfo errorInfo;
            StreamWriter sw = null;
            try
            {
                errorInfo = new FileInfo(File.Create(Environment.CurrentDirectory + $"\\{DateTime.Now.ToLongTimeString()}.log").Name);
                sw = new StreamWriter(new FileStream(errorInfo.DirectoryName, FileMode.Open));
                sw.WriteLine("Obiekt, który wyrzucił wyjątek: {0}", e.Source);
                sw.WriteLine("Metoda która wyrzuciła wyjątek: {0}", e.TargetSite);
                sw.WriteLine("Wywołania stosu: {0}", e.StackTrace);
                sw.WriteLine("Pary klucz-wartość: {0}", e.Data);
                sw.WriteLine("Opis: {0}", e.Message);
                sw.Close();
                sw.Dispose();
            }
            catch (Exception logEx)
            {
                MessageBox.Show($"Nie można utworzyć loga błędu. Treść błędu:\n{logEx.Message}", "Błąd przy tworzeniu loga błędu.", MessageBoxButton.OK, MessageBoxImage.Error);
                logCreated = false;
            }
            return logCreated;
        }

        //Metoda działająca w wypadku kiedy nie można zapisać loga
        public static void ShowErrorLog(Exception e, string naglowek, string opis)
        {
            string mess = string.Format("\n\nObiekt, który wyrzucił wyjątek: {0}\n" +
                "Metoda która wyrzuciła wyjątek: {1}\n" +
                "Wywołania stosu: {2}\n" +
                "Pary klucz-wartość: {3}\n" +
                "Opis: {4}\n)", 
                e.Source, e.TargetSite, e.StackTrace, e.Data, e.Message);
            MessageBox.Show(opis + mess, naglowek,  MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static T BinarySearch<T, TKey>(this IList<T> list, Func<T, TKey> keySelector, TKey key)
        where TKey : IComparable<TKey>
        {
            if (list.Count == 0)
                throw new InvalidOperationException("Item not found");

            int min = 0;
            int max = list.Count;
            while (min < max)
            {
                int mid = min + ((max - min) / 2);
                T midItem = list[mid];
                TKey midKey = keySelector(midItem);
                int comp = midKey.CompareTo(key);
                if (comp < 0)
                {
                    min = mid + 1;
                }
                else if (comp > 0)
                {
                    max = mid - 1;
                }
                else
                {
                    return midItem;
                }
            }
            if (min == max &&
                min < list.Count &&
                keySelector(list[min]).CompareTo(key) == 0)
            {
                return list[min];
            }
            throw new InvalidOperationException("Item not found");
        }
    }
}
