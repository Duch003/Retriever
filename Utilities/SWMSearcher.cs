using DataTypes;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Utilities
{
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
}
