using System;
using System.Collections.Generic;
using System.Management;
using DataTypes;

namespace Utilities
{
    //--------------------------------------------------Klasa obsługująca WMI do wydobywania danych--------------------------------------------------
    public static class Wmi
    {

        //Metoda zwracająca pojedyńczą właściwość z określonej klasy z warunkiem
        public static IEnumerable<Win32HardwareData> GetSingleProperty(Win32Hardware hardwareClass, string property, string condition, string scope = "root/cimv2")
        {
            var query = $"SELECT {property} FROM {hardwareClass.ToString()} WHERE {condition}";
            var search = new ManagementObjectSearcher(scope, query);
            foreach (var mo in search.Get())
                yield return new Win32HardwareData(property, (mo[property] == null) ? "" : mo[property].ToString());
        }

        //Metoda zwracająca pojedyńczą właściwość z określonej klasy
        public static IEnumerable<Win32HardwareData> GetSingleProperty(Win32Hardware hardwareClass, string property, string scope = "root/cimv2")
        {
            var query = $"SELECT {property} FROM {hardwareClass.ToString()}";
            var search = new ManagementObjectSearcher(scope, query);
            foreach (var mo in search.Get())
                yield return new Win32HardwareData(property, (mo[property] == null) ? "" : mo[property].ToString());
        }

        //Metoda zwracająca całą zawartość określonej klasy
        public static IEnumerable<Win32HardwareData> GetAll(Win32Hardware hardwareClass, string scope = "root/cimv2")
        {
            var query = $"SELECT * FROM {hardwareClass.ToString()}";
            var search = new ManagementObjectSearcher(scope, query);
            foreach (var mo in search.Get())
                foreach (var prop in mo.Properties)
                    yield return new Win32HardwareData(prop.Name, prop.Value?.ToString() ?? "-");
        }

        //Metoda zwracająca całą zawartość określonej klasy z warunkiem
        public static IEnumerable<Win32HardwareData> GetAll(Win32Hardware hardwareClass, string condition, string scope = "root/cimv2")
        {
            var query = $"SELECT * FROM {hardwareClass.ToString()} WHERE {condition}";
            var search = new ManagementObjectSearcher(scope, query);
            foreach (var mo in search.Get())
                foreach (var prop in mo.Properties)
                    yield return new Win32HardwareData(prop.Name, prop.Value?.ToString() ?? "-");
        }

        //Metoda zwracająca zużycie baterii
        public static IEnumerable<double> WearLevel()
        {
            //Zmienne dla obliczeń
            var designed = new double[0];
            var full = new double[0];
            var i = 0;
            //Pobieranie informacji o zaprojektowanej pojemności baterii
            var designedCapacity = GetSingleProperty(Win32Hardware.BatteryStaticData, "DesignedCapacity", "root/wmi");
            //Z każdej znalezionej baterii wyciągnij i przelicz na Wh pojemnosć zaprojektowaną
            foreach (var z in designedCapacity)
            {
                designed = ExpandArr.Expand(designed);
                designed[i] = Math.Round((double.Parse(z.Wartosc) / 1000), 2); //[Wh]
                i++;
            }
            //Wyciągniej informacje o rzeczywistym maksymalnym możliwym naładowaniu baterii
            var fullChargeCapacity = GetSingleProperty(Win32Hardware.BatteryFullChargedCapacity, "FullChargedCapacity", "root/wmi");
            i = 0;
            //Z każdej znalezionej baterii wyciągniej i przelicz na Wh pojemność rzeczywistą
            foreach (var z in fullChargeCapacity)
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
            var scope = new ManagementScope("root/cimv2");
            var query = new ObjectQuery("SELECT * FROM SoftwareLicensingService Where OA3xOriginalProductKey != null");
            var search = new ManagementObjectSearcher(scope, query);
            var ans = search.Get();
            foreach (var m in ans)
            {
                return m["OA3xOriginalProductKey"].ToString();
            }
            return null;
        }

        public static int CountUsb()
        {
            var temp = 0;
            var scope = new ManagementScope("root/cimv2");
            var query = new ObjectQuery("SELECT * FROM Win32_USBControllerDevice");
            var searcher = new ManagementObjectSearcher(scope, query);

            foreach (var obj in searcher.Get())
            {
                foreach (var prop in obj.Properties)
                {
                    switch (prop.Name)
                    {
                        case "Antecedent":
                            Console.WriteLine("Antecedent:");
                            var antecendent = new ManagementObject();
                            var aPath = new ManagementPath((string)obj["Antecedent"]);
                            antecendent.Path = aPath;
                            antecendent.Get();
                            foreach (var inner in antecendent.Properties)
                            {
                                Console.WriteLine("\t{0}: {1}", inner.Name.PadRight(30), inner.Value);
                            }
                            continue;
                        case "Dependent":
                            bool flag1 = false, flag2 = false;
                            Console.WriteLine("Dependent:");
                            var dependent = new ManagementObject();
                            var dPath = new ManagementPath((string)obj["Dependent"]);
                            dependent.Path = dPath;
                            dependent.Get();

                            foreach (var inner in dependent.Properties)
                            {
                                //Console.WriteLine("\t{0}: {1}", inner.Name.PadRight(30), inner.Value);
                                switch (inner.Name)
                                {
                                    case "Caption" when inner.Value.ToString().ToLower().Contains("wejściowe"):
                                    case "Caption" when inner.Value.ToString().ToLower().Contains("input"):
                                        flag1 = true;
                                        break;
                                    case "ConfigManagerErrorCode" when Convert.ToUInt32(inner.Value) == 0:
                                        flag2 = true;
                                        break;
                                }

                                if (!flag1 || !flag2) continue;
                                flag1 = flag2 = false;
                                temp++;
                            }

                            break;
                        case "AccessState":
                            Console.WriteLine("\n\nNOWE URZĄDZENIE\n");
                            break;
                    }

                    Console.WriteLine("{0}: {1}", prop.Name.PadRight(30), prop.Value);
                }
            }
            return temp;
            
        }
    }
}
