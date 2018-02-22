using DataTypes;
using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Serialization;
using Utilities;

namespace Reader
{
    public class DatabaseManager : IDBManager
    {
        public ObservableCollection<Model> ListaModeli { get; set; }
        public FileStream stream { get; set; }
        public IExcelDataReader excelReader { get; set; }
        public DataSet result { get; set; }
        IFileSystemManager Manager;

        public DatabaseManager(IFileSystemManager FSManager)
        {
            //Zapisanie informacjio z pliku settings.xml
            Manager = FSManager;

            //Połączenie z siecią
            WirelessConnectionManager.Connect();

            //Odczekanie 5 sekund aby ustanowić połączenie
            Thread.Sleep(3000);

            //Otwrcie pliku we wskazanym miejscu
            Open(Manager.Set.DBPath);

            //SPRAWDZANIE WERSJI BAZY DANYCH
            //Utworzenie aktualnego hasha pliku bazy danych
            var aktualnyHash = ComputeHashCode();
            
            //Hasze są takie same, istnieje już lista modeli - załadować listę do pamięci
            if (FSManager.Set.SHA1 == aktualnyHash && File.Exists(Environment.CurrentDirectory + @"\Model.xml"))
            {
                ListaModeli = LoadModelList();
            }
            //Hasze są różne, istnieje już lista modeli - trzeba napisać listę od nowa
            else if (FSManager.Set.SHA1 != aktualnyHash && File.Exists(Environment.CurrentDirectory + @"\Model.xml"))
            {
                ListaModeli = SaveAndLoadModelList(result);
                FileStream stream = new FileStream(Environment.CurrentDirectory + @"\SHA1.txt", FileMode.Open, FileAccess.Write);
                StreamWriter sr = new StreamWriter(stream);
                sr.WriteLine(aktualnyHash);
                sr.Close();
            }
            //Hasze są różne, nie istnieje lista modeli - trzeba utworzyć listę modeli
            //Hasze są takie same, nie istnieje lista modeli - trzeba utworzyć listę modeli
            else if (!File.Exists(Environment.CurrentDirectory + @"\Model.xml"))
            {
                //Utworzenie nowego pliku listy modeli
                File.Create(Environment.CurrentDirectory + @"\Model.xml").Close(); 
                ListaModeli = SaveAndLoadModelList(result);
                FileStream stream = new FileStream(Environment.CurrentDirectory + @"\SHA1.txt", FileMode.Open, FileAccess.Write);
                StreamWriter sr = new StreamWriter(stream);
                sr.WriteLine(aktualnyHash);
                sr.Close();
            }
            Close();
        }  

        //Metoda otwierająca plik bazy danych
        void Open(string FilePath)
        {
            try
            {
                stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                result = excelReader.AsDataSet();
            }
            catch (Exception e)
            {
                var message = string.Format("Wystąpił błąd podczas próby otwarcia bazy danych:\n{0}", e.Message);
                Close();
                throw new Exception(message);
            }
        }

        //TODO Zweryfikowac problem dla 99525 - brak biosu
        public DataPack ReadModel(Model model)
        {
            DataPack ans;

            //Połączenie z siecią
            WirelessConnectionManager.Connect();

            //Otwarcie pliku bazy danych
            Open(Manager.Set.DBPath);

            //Sprawdzenie argumentu - w razie braku argumentu wyrzucić pusty kontener ans
            if (model != null)
            {
                //Ustawienie zmiennych na kontkretne arkusze w bazie
                DataTable table = result.Tables["MD"];
                DataTable biosTable = result.Tables["BIOS"];

                #region Tworzenie instancji Mainboard
                ans.PlytaGlowna = new Mainboard(
                    model: table.Rows[model.WierszModel][14].ToString() == "-" ? table.Rows[model.WierszModel][17].ToString() : table.Rows[model.WierszModel][14].ToString(),
                    producent: table.Rows[model.WierszModel][15].ToString(),
                    cpu: table.Rows[model.WierszModel][7].ToString(),
                    taktowanie: table.Rows[model.WierszModel][8].ToString());
                #endregion

                #region Tworzenie instancji Bios
                //Dla MEDION
                if (model.BiosZeszyt == "BIOS")
                    ans.WersjaBios = new Bios(
                      ver: model.WierszBios == -1 ? "Nie znaleziono" : biosTable.Rows[model.WierszBios][3].ToString(),
                      opis: model.WierszBios == -1 ? "Nie znaleziono" : biosTable.Rows[model.WierszBios][4].ToString());

                //Dla PEAQ
                else
                    ans.WersjaBios = new Bios(
                      ver: model.WierszBios == -1 ? "Nie znaleziono" : table.Rows[model.WierszBios][18].ToString(),
                      opis: "");

                #endregion

                #region Tworzenie instancji Computer
                ans.Komputer = new Computer(
                    md: table.Rows[model.WierszModel][0].ToString(),
                    //msn:      table.Rows[model.Wiersz][1].ToString(), Opcjonalne wyświetlanie MSN, odkomentować jeżeli potrzeba + dorobić wiersze dla msn w xaml
                    //staryMsn: table.Rows[model.Wiersz][2].ToString(),
                    system: table.Rows[model.WierszModel][9].ToString(),
                    wearLevel: new double[1] { 12 },
                    wskazowki: table.Rows[model.WierszModel][12].ToString(),
                    obudowa: table.Rows[model.WierszModel][13].ToString(),
                    lcd: table.Rows[model.WierszModel][4].ToString(),
                    kolor: table.Rows[model.WierszModel][3].ToString(),
                    shipp: table.Rows[model.WierszModel][16].ToString() == "Tak" ? true : false);
                #endregion

                #region Tworzenie instancji SWM, RAM i Storage
                ans.Swm = new SWM[1] { new SWM(swm: table.Rows[model.WierszModel][10].ToString()) };

                ans.Ram = new RAM[1] { new RAM(info: table.Rows[model.WierszModel][6].ToString()) };

                var tempMem = table.Rows[model.WierszModel][5].ToString().Replace("+", ";").Split(';');
                ans.Dyski = new Storage[tempMem.Length];
                for (int j = 0; j < tempMem.Length; j++)
                {
                    ans.Dyski[j] = new Storage(tempMem[j]);
                }
                #endregion
            }
            Close();
            return ans;
        }
        //Czyszczenie końcowe po zakończeniu używania programu
        void Close()
        {
            if(stream != null)
            {
                stream.Close();
                stream.Dispose();
            }
            if(excelReader != null)
            {
                excelReader.Close();
                excelReader.Dispose();
            }
            if(result != null)
                result.Dispose();
            WirelessConnectionManager.Disconnect();
        }

        //Metoda tworząca SHA1
        string ComputeHashCode()
        {
            //Utworzenie hasha do porównania z zapisanym
            BufferedStream bs = new BufferedStream(stream);
            SHA1Managed sha1 = new SHA1Managed();
            byte[] hash = sha1.ComputeHash(bs);
            StringBuilder hashhex = new StringBuilder(2 * hash.Length);
            foreach (byte b in hash)
            {
                hashhex.AppendFormat("{0:X2}", b);
            }
            return hashhex.ToString();
        }

        //Metoda wczytująca listę modeli do pamięci
        ObservableCollection<Model> LoadModelList()
        {
            //Utwórz obiekt deserializera o typie listy modeli
            XmlSerializer xs = new XmlSerializer(typeof(ObservableCollection<Model>));

            //Utworzenie streamu danych na lokalnym pliku modeli
            StreamReader sr = new StreamReader(Environment.CurrentDirectory + @"\Model.xml");

            //Deserializacja
            ObservableCollection<Model> ListaModeli = xs.Deserialize(sr) as ObservableCollection<Model>;

            sr.Close();
            return ListaModeli;
        }

        //Metoda pobierajaca i układająca listę modeli
        ObservableCollection<Model> SaveAndLoadModelList(DataSet result)
        {
            //Wczytanie surowej listy modeli z bazy
            IEnumerable<Model> temp = GatherModels(result.Tables["MD"], result.Tables["BIOS"]);

            //Sortowanie listy
            temp = temp.OrderBy(z => z.MD);

            //Utworzenie prawidłowej listy na podstawie posortowanej surowej listy
            ObservableCollection<Model> lista = new ObservableCollection<Model>(temp);

            //Utworzenie obiektu do serializacji danych
            XmlSerializer xs = new XmlSerializer(typeof(ObservableCollection<Model>));

            //Utworzenie nowego streamu na plik lokalny w którym będzie zapisana lista modeli
            StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\Model.xml");

            //Serializacja
            xs.Serialize(sw, lista);

            sw.Close();
            return lista;
        }

        // Metoda zwracająca listę modeli z pliku excel.
        IEnumerable<Model> GatherModels(DataTable modelTable, DataTable biosTable)
        {
            for (int i = 1; i < modelTable.Rows.Count; i++)
            {
                //Zapisz daną linię
                int mdRow = i;
                //Zapisz Model i MSN z danej linii
                string msn = modelTable.Rows[i][1].ToString();
                string md = modelTable.Rows[i][0].ToString();

                int biosRow = -1;
                string biosSheet = "";

                //Jeżeli komputer jest PEAQ
                if (!Regex.IsMatch(modelTable.Rows[i][17].ToString(), @"^\d{5}$"))
                {
                    biosRow = i;
                    biosSheet = "MD";
                }
                //Jeżeli jest to MEDION
                else
                {
                    //Zapisz model obudowy w pamięci
                    string temp = modelTable.Rows[i][13].ToString().ToLower();
                    //Przeszukuj tabelę BIOS
                    for (int j = 0; j < biosTable.Rows.Count; j++)
                    {
                        //Jeżeli komórka zawiera model obudowy, zapisz
                        if (biosTable.Rows[j][0].ToString().ToLower().Contains(temp))
                        {
                            biosRow = j;
                            biosSheet = "BIOS";
                            break;
                        }
                        //Jeżeli nie znaleziono w ogóle, zapisz -1
                        else if (j == biosTable.Rows.Count - 1)
                            biosRow = -1;
                    }
                }
                yield return new Model(mdRow, biosRow, biosSheet, msn, md);
            }
        }
    }
}
