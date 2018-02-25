using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataTypes;

namespace Utilities
{
    //--------------------------------------------------Klasa obsługująca odnajdywanie i wydobywanie numerów SWM-------------------------------------
    public static class SwmSearcher
    {
        public static string[] Swm { get; private set; }
        public static string[] Drive { get; private set; }
        private static readonly DriveInfo[] AllDrives = DriveInfo.GetDrives(); //Pobranie informacji o dyskach logicznych an komputerze

        //Pobieranie SWM z plików swconf.dat
        public static IEnumerable<SWM> GetSwm()
        {
            var i = 0; //Iterator słuzący do poruszania się po tablicy
            Swm = new string[0]; //Początkowa wartość tablicy
            Drive = new string[0];
            foreach (var d in AllDrives)
            {
                //Sprawdza gotowość dysku do odczytu
                if (d.IsReady)
                {
                    //Utwórz instancję pliku swconf.dat na danym dysku
                    var fInfo = new FileInfo(d.Name + "swconf.dat");
                    //Jeżeli pliku nie ma na dysku, przejdź do następnego
                    if (!fInfo.Exists)
                    {
                    }
                    //W innym wypadku odczytaj 3 linię z pliku i dodaj do smiennej SWM
                    else
                    {
                        Expand();
                        //Daną wyjściową jest np: D:\12345678
                        Swm[i] = string.Format($"{File.ReadLines(d.Name + "swconf.dat").Skip(2).Take(1).First()}");
                        Drive[i] = string.Format($"{d.Name}");
                        yield return new SWM(Drive[i], Swm[i]);
                        i++;
                    }
                }
            }
            if (Swm.Length == 0)
            {
                Expand();
                Swm[0] = "Brak SWM w plikach";
                Drive[0] = "-";
            }
        }

        //Metoda powiększająca tablice SWM i Drive
        private static void Expand()
        {
            Swm = ExpandArr.Expand(Swm);
            Drive = ExpandArr.Expand(Drive);
        }
    }
}
