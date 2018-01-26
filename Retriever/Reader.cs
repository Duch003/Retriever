using System;
using System.Collections.Generic;
using ExcelDataReader;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml;
using System.Windows;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.Text.RegularExpressions;

namespace Retriever
{
    public class Reader
    {
        FileStream stream;
        IExcelDataReader excelReader;
        DataSet result;
        public Computer Komputer { get; set; }
        public RAM Ram { get; private set; }
        public Storage[] Dyski { get; private set; }
        public Mainboard PlytaGlowna { get; private set; }
        public SWM Swm { get; private set; }
        public BiosVer WersjaBios { get; private set; }
        public ObservableCollection<Model> listaModeli { get; private set; }
        public string BiosZBazy { get; private set; }
        string Plik = @"/NoteBookiRef_v3.xlsx";
        string AktualnyHash;
        
        //Konstruktor główny
        // - sprawdza czy wszystkie pliki kontrolne są na swoim miejscu
        // - sprawdza czy baza danych jest osiągalna
        // - utworzenie obiektu oznacza że baza istnieje i jest aktualna i można dowoi odświezaćrekordy
        public Reader()
        {
            //TEST I - Istnienie bazy danych notebooków
            if(!Open()) return;
            else
            {
                //TEST II - Istnienie pliku staticdata.txt
                //Jeżeli nie istnieje plik hash, utwórz go
                if (!File.Exists(Environment.CurrentDirectory + @"\sha1.txt"))
                {
                    //Utworzenie pliku i zapisanie hasha
                    FileStream fs = new FileStream(Environment.CurrentDirectory + @"\sha1.txt", FileMode.Create);
                    StreamWriter sw = new StreamWriter(fs);
                    sw.WriteLine(AktualnyHash);
                    sw.Close();
                    //Utworzenie lub uaktualnienie bazy danych
                    SaveModelList();
                }
                //Jeżeli plik istnieje, porównaj hashe
                else
                {
                    //Pobranie hasha z pliku
                    FileStream fs = new FileStream(Environment.CurrentDirectory + @"\sha1.txt", FileMode.Open);
                    StreamReader sr = new StreamReader(fs);
                    var temp = sr.ReadLine().ToString();
                    //Jeżeli hashe są różne, pobierz na nowo listę modeli
                    if (temp != AktualnyHash)
                        SaveModelList();
                    else
                        LoadModelList();
                    //Jeżeli są takie same, nie rób nic
                }
                //Pozamykaj pliki, można pobierać dane z bazy danych
                Close();
            } 
        }

        //Metoda otwierająca plik bazy danych
        //Zapewnia że baza danych istnieje, jest otwarta i utworzono hashcode
        bool Open()
        {
            //Zmienna zwracająca końcowy wynik
            bool isOpened = true;
            //Spróbuj otworzyć plik bazy danych
            try
            {
                //Otwarcie pliku
                stream = new FileStream(Environment.CurrentDirectory + Plik, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                result = excelReader.AsDataSet();
                isOpened = true;
                ComputeHashCode();
            }
            catch (Exception e)
            {
                //Próba zapisania loga błędu. Jeżeli się uda, wyświetla komunikat bez loga.
                if(ErrorWriter.WriteErrorLog(e))
                {
                    string opis = string.Format("Wystąpił błąd podczas próby otwarcia bazy danych komputerów. Program zostanie załadowany bez zestawienia bazy danych.\n");
                    MessageBox.Show(opis + "\n\n" + e.Message, "Nie można otworzyć bazy danych", MessageBoxButton.OK, MessageBoxImage.Error);
                    isOpened = false;
                }
                //Jeżeli się nie uda, wyświetla log w wiadomości o błędzie.
                else
                {
                    string opis = "Wystąpił błąd podczas próby otwarcia bazy danych komputerów. Program zostanie załadowany bez zestawienia bazy danych.";
                    ErrorWriter.ShowErrorLog(e, "Nie można otworzyć bazy danych", opis);
                    isOpened = false;
                }
                //Czyszczenie końcowe. Nie można użyć w finally, bo zmienne utworzone w razie powodzenia mają zostać.
                stream = null;
                excelReader = null;
                result = null;
            }
            return isOpened;
        }

        //Czyszczenie końcowe po zakończeniu używania programu
        void Close()
        {
            stream.Close();
            stream.Dispose();
            excelReader.Close();
            excelReader.Dispose();
            result.Dispose();
        }

        //Metoda obliczająca i zapisująca w pliku sha1.txt HashCode
        //Można użyć tylko w metodzie Open()
        void ComputeHashCode()
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
            AktualnyHash = hashhex.ToString();
        }

        ////Utworzenie pliku XML dla bazy danych
        //bool CreateXMLFile()
        //{
        //    bool isCreated = true;
        //    XmlTextWriter writer = null;
        //    try
        //    {
        //        //Utworz obiekt do zapisywania tekstu
        //        writer = new XmlTextWriter("Model.xml", System.Text.Encoding.UTF8);
        //    }
        //    catch(Exception e)
        //    {
        //        if(ErrorWriter.WriteErrorLog(e))
        //        {
        //            string opis = string.Format("Wystąpił błąd podczas próby utworzenia spisu modeli. Program zostanie załadowany bez zestawienia bazy danych.\n");
        //            MessageBox.Show(opis + "\n\n" + e.Message, "Błąd utworzenia pliku", MessageBoxButton.OK, MessageBoxImage.Error);
        //            isCreated = false;
        //        }
        //        else
        //        {
        //            string opis = "Wystąpił błąd podczas próby utworzenia spisu modeli. Program zostanie załadowany bez zestawienia bazy danych.";
        //            ErrorWriter.ShowErrorLog(e, "Błąd utworzenia pliku", opis);
        //            isCreated = false;
        //        }
        //    }
        //    if(writer != null)
        //    {
        //        //Parametry zapisu
        //        writer.WriteStartDocument(true);
        //        writer.Formatting = Formatting.Indented;
        //        writer.Indentation = 2;
        //        //Utworzenie bazowego znacznika
        //        writer.WriteStartElement("BAZA");
        //        //Zapisanie bazy
        //        foreach (Model z in listaModeli)
        //        {
        //            CreateNode(z.WierszModel, z.WierszBios, z.MSN, z.MD, writer);
        //        }
        //        //Zamknięcie znacznika
        //        writer.WriteEndElement();
        //        //Zamknięcie dokumentu
        //        writer.WriteEndDocument();
        //        writer.Close();
        //    }
        //    return isCreated;           
        //}

        ////Metoda zapisująca dane Modelu w pliku XML
        //void CreateNode(int wierszModel, int wierszBios, string msn, string md, XmlTextWriter writer)
        //{
        //    //Utworzenie roota MODEL
        //    writer.WriteStartElement("Model"); //Dodaj pierwotną gałąź
        //    //Utworzenie gałęzi Wiersz
        //    writer.WriteStartElement("WierszModel");     //Dodaj subgałąź
        //    writer.WriteString(wierszModel.ToString());  //Wpisz wartość
        //    writer.WriteEndElement();               //Zamknij gałąź
        //    //Utworzenie gałęzi Zeszyt
        //    writer.WriteStartElement("WierszBios");
        //    writer.WriteString(wierszBios.ToString());
        //    writer.WriteEndElement();
        //    //Utworzenie gałęzi MSN
        //    writer.WriteStartElement("MSN");
        //    writer.WriteString(msn);
        //    writer.WriteEndElement();
        //    //Utworzenie gałęzi MD
        //    writer.WriteStartElement("MD");
        //    writer.WriteString(md);
        //    writer.WriteEndElement();
        //    writer.WriteEndElement();
        //}

        //Metoda wczytująca listę modeli do pamięci
        void LoadModelList()
        {
            XmlSerializer xs = new XmlSerializer(typeof(ObservableCollection<Model>));
            StreamReader sr = new StreamReader(Environment.CurrentDirectory + @"\Model.xml");
            listaModeli = xs.Deserialize(sr) as ObservableCollection<Model>;
            sr.Close();
        }

        //Metoda pobierajaca i układająca listę modeli
        //Po jej użyciu lista jest już w pamięci
        void SaveModelList()
        {
            IEnumerable<Model> temp = GatherModels(result.Tables["MD"], result.Tables["BIOS"]);
            temp = temp.OrderBy(z => z.MD);
            listaModeli = new ObservableCollection<Model>(temp);
            XmlSerializer xs = new XmlSerializer(typeof(ObservableCollection<Model>));
            StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\Model.xml");
            xs.Serialize(sw, listaModeli);
            sw.Close();
        }

        //TODO Poprawić listę bios - wyszukiwanie po modelu płyty, nie obudowy
        //TODO Usprawnić szukanie: dla konkretnych producentów konkretne wiersze lub ignorowanie wierszy do póki nie odnajdzie się producent
        //TODO Jeżeli nie znajdzie biosu w tabeli, może to być peaq, wtedy zapisać bios peaq
        //TODO Testowane do tego momentu
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

                if (!Regex.IsMatch(modelTable.Rows[i][18].ToString(), @"^\d{5}$"))
                    biosRow = modelTable.Rows[i][18].ToString();
                //Na podstawie modelu obudowy przeszukaj tabelę z biosami i znajdź 
                int biosRow = -1;
                string temp = modelTable.Rows[i][13].ToString();
                //Jeżeli jest zapisany model, zacznij poszukiwania
                if(temp != "-")
                {
                    for (int j = 0; j < biosTable.Rows.Count; j++)
                    {
                        if (biosTable.Rows[j][0].ToString().Contains(temp))
                        {
                            biosRow = j;
                            break;
                        }   
                        else if()
                        else if (j == biosTable.Rows.Count - 1)
                            biosRow = -1;
                    }
                }
                yield return new Model(mdRow, biosRow, msn, md);
            }
        }

        //Odczytanie informacji o danym modelu z bazy
        public void ReadData(Model model)
        {
            //Utworzenie tymaczowego obiektu zeszytu
            if (model != null)
            {
                Open();
                DataTable table = result.Tables["MD"];
                DataTable biosTable = result.Tables["BIOS"];

                #region Tworzenie instancji Mainboard
                PlytaGlowna = new Mainboard(
                    model:      table.Rows[model.WierszModel][14].ToString(),
                    producent:  table.Rows[model.WierszModel][15].ToString(),
                    cpu:        table.Rows[model.WierszModel][7].ToString(),
                    taktowanie: table.Rows[model.WierszModel][8].ToString());
                #endregion

                #region Tworzenie instancji BiosVer
                WersjaBios = new BiosVer(
                    ver: model.WierszBios == -1 ? "Nie znaleziono" : biosTable.Rows[model.WierszBios][3].ToString(),
                    opis: model.WierszBios == -1 ? "Nie znaleziono" : biosTable.Rows[model.WierszBios][4].ToString());
                #endregion

                #region Tworzenie instancji Computer
                Komputer = new Computer(
                    md:         table.Rows[model.WierszModel][0].ToString(),
                    //msn:      table.Rows[model.Wiersz][1].ToString(), Opcjonalne wyświetlanie MSN, odkomentować jeżeli potrzeba + dorobić wiersze dla msn w xaml
                    //staryMsn: table.Rows[model.Wiersz][2].ToString(),
                    wearLevel: new double[1] { 12 },
                    wskazowki:  table.Rows[model.WierszModel][12].ToString(),
                    obudowa:    table.Rows[model.WierszModel][13].ToString(),
                    lcd:        table.Rows[model.WierszModel][4].ToString(),
                    kolor:      table.Rows[model.WierszModel][3].ToString(),
                    shipp:      table.Rows[model.WierszModel][16].ToString() == "Tak" ? true : false,
                    pelnyModel: table.Rows[model.WierszModel][17].ToString());
                #endregion

                #region Tworzenie instancji SWM, RAM i Storage
                Swm = new SWM(swm: table.Rows[model.WierszModel][10].ToString());

                Ram = new RAM(table.Rows[model.WierszModel][6].ToString());

                var tempMem = table.Rows[model.WierszModel][5].ToString().Split(';');
                Dyski = new Storage[tempMem.Length];
                for (int j = 0; j < tempMem.Length; j++)
                {
                    Dyski[j] = new Storage(tempMem[j]);
                }
                #endregion

                Close();
            }
        }  
    }
}
