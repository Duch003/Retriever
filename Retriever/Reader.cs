using System;
using System.Collections.Generic;
using ExcelDataReader;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml;
using System.Windows;

namespace Retriever
{
    public class Reader
    {
        FileStream                  stream;
        IExcelDataReader            excelReader;
        DataSet                     result;
        public Computer             Komputer { get; set; }
        public RAM                  Ram { get; private set; }
        public Storage[]            Dyski { get; private set; }
        public Mainboard            PlytaGlowna { get; private set; }
        public SWM                  Swm { get; private set; }
        public BiosVer              WersjaBios { get; private set; }
        public IEnumerable<Model>   listaModeli { get; private set; }
        public string               BiosZBazy { get; private set; }
        string                      Plik = @"/NoteBookiRef_v2.xlsx";

        //TODO Dokończyć usprawnianie klasy w zależności od decyzji przełożonych
        //TODO zHASZOWAC CALY PLIK xml crc32
        //Konstruktor pobierający listę modeli
        public Reader()
        {
            int ilosc_modeli;
            //TEST I - Istnienie bazy danych notebooków
            if (Open())
            {
                SetDataTables();
                //TEST II - Istnienie pliku staticdata.txt
                //Jeżeli nie istnieje plik staticdata, utwórz go
                if (!File.Exists(Environment.CurrentDirectory + @"\staticdata.txt"))
                {
                    FileStream fs = (File.Create(Environment.CurrentDirectory + @"\staticdata.txt"));
                    ilosc_modeli = 0;
                    //StreamWriter sw = new StreamWriter(fs);
                }
                else
                {
                    FileInfo fi = new FileInfo(Environment.CurrentDirectory + @"\staticdata.txt");
                    StreamReader sr = fi.OpenText();
                    ilosc_modeli = Convert.ToInt32(sr.ReadLine());
                }
                    
                //Jeżeli nie istnieje plik Model.xml, utwórz go
                if(!File.Exists(Environment.CurrentDirectory + @"\Model.xml"))
                    //Jeżeli nie udało się otworzyć pliku, zakończ działanie konstruktora
                    if(!CreateXMLFile())
                        return;

                Close();
            }
            else return;
        }

        //Metoda otwierająca plik bazy danych
        bool Open()
        {
            //Zmienna zwracająca końcowy wynik
            bool isOpened = true;
            //Spróbuj otworzyć plik bazy danych
            try
            {
                stream = new FileStream(Environment.CurrentDirectory + Plik, FileMode.Open, FileAccess.Read);
                excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                result = excelReader.AsDataSet();
                isOpened = true;
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

        //Utworzenie pliku XML dla bazy danych
        bool CreateXMLFile()
        {
            bool isCreated = true;
            XmlTextWriter writer = null;
            try
            {
                //Utworz obiekt do zapisywania tekstu
                writer = new XmlTextWriter("Model.xml", System.Text.Encoding.UTF8);
            }
            catch(Exception e)
            {
                if(ErrorWriter.WriteErrorLog(e))
                {
                    string opis = string.Format("Wystąpił błąd podczas próby utworzenia spisu modeli. Program zostanie załadowany bez zestawienia bazy danych.\n");
                    MessageBox.Show(opis + "\n\n" + e.Message, "Błąd utworzenia pliku", MessageBoxButton.OK, MessageBoxImage.Error);
                    isCreated = false;
                }
                else
                {
                    string opis = "Wystąpił błąd podczas próby utworzenia spisu modeli. Program zostanie załadowany bez zestawienia bazy danych.";
                    ErrorWriter.ShowErrorLog(e, "Błąd utworzenia pliku", opis);
                    isCreated = false;
                }
            }
            if(writer != null)
            {
                //Parametry zapisu
                writer.WriteStartDocument(true);
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 2;
                //Utworzenie bazowego znacznika
                writer.WriteStartElement("BAZA");
                //Zapisanie bazy
                foreach (Model z in listaModeli)
                {
                    CreateNode(z.Wiersz, z.Zeszyt, z.MSN, z.MD, writer);
                }
                //Zamknięcie znacznika
                writer.WriteEndElement();
                //Zamknięcie dokumentu
                writer.WriteEndDocument();
                writer.Close();
            }
            return isCreated;           
        }

        //Metoda zapisująca dane Modelu w pliku XML
        void CreateNode(int wiersz, int zeszyt, string msn, string md, XmlTextWriter writer)
        {
            //Utworzenie roota MODEL
            writer.WriteStartElement("Model"); //Dodaj pierwotną gałąź
            //Utworzenie gałęzi Wiersz
            writer.WriteStartElement("Wiersz");     //Dodaj subgałąź
            writer.WriteString(wiersz.ToString());  //Wpisz wartość
            writer.WriteEndElement();               //Zamknij gałąź
            //Utworzenie gałęzi Zeszyt
            writer.WriteStartElement("Zeszyt");
            writer.WriteString(zeszyt.ToString());
            writer.WriteEndElement();
            //Utworzenie gałęzi MSN
            writer.WriteStartElement("MSN");
            writer.WriteString(msn);
            writer.WriteEndElement();
            //Utworzenie gałęzi MD
            writer.WriteStartElement("MD");
            writer.WriteString(md);
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        //Metoda wyszukująca bios któy powinien być zainstalowany na podstawie porównywania modelu obudowy
        void LookForBios(Computer komp)
        {
            DataTable table = result.Tables[0];
            for (int i = 0; i < table.Rows.Count; i++)
            {
                if (table.Rows[i][0].ToString().ToLower().Replace(" ", "").Contains(komp.Obudowa.ToLower().Replace(" ", "")) || table.Rows[i][0].ToString().ToLower().Replace(" ", "").Contains("e221xt") || table.Rows[i][0].ToString().ToLower().Replace(" ", "").Contains("e222xt"))
                {
                    WersjaBios = new BiosVer(table.Rows[i][3].ToString(), table.Rows[i][4].ToString());
                    break;
                }
            }
        }

        // Metoda przeszukująca plik excela w celu znalezienia odpowiednich zeszytów z danymi
        void SetDataTables()
        {
            IEnumerable<Model> tempPQ = null;
            IEnumerable<Model> tempMD = null;
            //Dodatkowe przełączniki
            bool Medion = false, Peaq = false;
            //Przeszukuj zeszyty w pliku do póki nie znajdziesz MD i PQ
            for (int i = 0; i < result.Tables.Count; i++)
            {
                //Pobierz bazę z MD
                if (result.Tables[i].TableName.ToString() == "MD")
                {
                    tempMD = GatherModels(result.Tables[i], i, 0, 2);
                    Medion = true; //Pobrano bazę MD
                }
                //Pobierz bazę z PQ
                else if (result.Tables[i].TableName.ToString() == "PQ")
                {
                    tempPQ = GatherModels(result.Tables[i], i, 1, 2);
                    Peaq = true; //Pobrano bazę PQ
                }
                //Przełączniki przyspieszają zakończenie wykonywania pętli
                if (Medion == true && Peaq == true)
                {
                    listaModeli = tempMD.Concat(tempPQ);
                    return;
                }
            }
        }

        // Metoda zwracająca listę modeli z pliku excel.
        IEnumerable<Model> GatherModels(DataTable table, int sheet, int mdColumn, int msnColumn)
        {
            for (int i = 1; i < table.Rows.Count; i++)
            {
                //Odrzuca rekordy w których brakuje modelu i msn
                if (string.IsNullOrEmpty(table.Rows[i][msnColumn].ToString()) && (string.IsNullOrEmpty(table.Rows[i][mdColumn].ToString()))) continue;
                //Argumenty: (Wiersz w którym znajduje się dany model, numer zeszytu w którym jest rekord, MSN, Model)
                yield return new Model(i, sheet, table.Rows[i][msnColumn].ToString(), table.Rows[i][mdColumn].ToString(), table.TableName.ToString());
            }
        }

        //Odczytanie informacji o danym modelu z bazy
        void ReadData(Model model)
        {
            //Zmienne potrzebne do utworzenia obiektów opisujących komputer
            string MD = null, Kolor = null, MSN = null, NowyMSN = null, PelnyModel = null, LCD = null, CPU = null, Taktowanie = null, System = null,
                swm = null, Wskazowki = null, Obudowa = null, ModelPlyty = null, ProducentPlyty = null, BIOS = null;
            bool ShippingMode = false;

            //Utworzenie tymaczowego obiektu zeszytu
            if (model != null)
            {
                DataTable table = result.Tables[model.Zeszyt];
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    #region Pobieranie i wpisywanie wartości dla konstruktorów
                    string variable = "";
                    switch (variable = table.Rows[0][i].ToString().ToLower().Replace(" ", ""))
                    {
                        case "model":
                            MD = table.Rows[model.Wiersz][i].ToString();
                            break;
                        case "farba":
                            Kolor = table.Rows[model.Wiersz][i].ToString();
                            break;
                        case "msn":
                        case "starymsn":
                            MSN = table.Rows[model.Wiersz][i].ToString();
                            break;
                        case "nowymsn":
                            NowyMSN = table.Rows[model.Wiersz][i].ToString();
                            break;
                        case "model3":
                            PelnyModel = table.Rows[model.Wiersz][i].ToString();
                            break;
                        case "rodzajpanela":
                            LCD = table.Rows[model.Wiersz][i].ToString();
                            break;
                        case "hdd":
                        case "hdd ":
                            string[] tempMem = table.Rows[model.Wiersz][i].ToString().Split(';', '+');
                            Dyski = new Storage[tempMem.Length];
                            for (int j = 0; j < tempMem.Length; j++)
                            {
                                Dyski[j] = new Storage(tempMem[j]);
                            }
                            break;
                        case "ram ":
                        case "ram":
                            Ram = new RAM(table.Rows[model.Wiersz][i].ToString());
                            break;
                        case "cpu":
                            CPU = table.Rows[model.Wiersz][i].ToString();
                            break;
                        case "tak":
                            Taktowanie = table.Rows[model.Wiersz][i].ToString();
                            break;
                        case "systemoperacyjny":
                            System = table.Rows[model.Wiersz][i].ToString();
                            break;
                        case "nrswm":
                            swm = table.Rows[model.Wiersz][i].ToString();
                            break;
                        case "wskazowki":
                        case "wskazówki":
                        case "uwagi":
                            Wskazowki = table.Rows[model.Wiersz][i].ToString();
                            break;
                        case "modelbudy":
                            Obudowa = table.Rows[model.Wiersz][i].ToString();
                            break;
                        case "modelpłyty":
                        case "modelplyty":
                            ModelPlyty = table.Rows[model.Wiersz][i].ToString();
                            break;
                        case "producentplyty":
                        case "producentpłyty":
                            ProducentPlyty = table.Rows[model.Wiersz][i].ToString();
                            break;
                        case "shipping":
                            ShippingMode = false;
                            if (table.Rows[model.Wiersz][i].ToString().ToLower() == "tak") ShippingMode = true;
                            break;
                        case "bios":
                            BIOS = table.Rows[model.Wiersz][i].ToString();
                            break;
                    }
                    #endregion
                }
            }
            //W przypadku kiedy nie ma modelu, wpisz wartości null, konstruktory utworzą wartości domyślne
            PlytaGlowna = new Mainboard(ModelPlyty, ProducentPlyty, CPU, Taktowanie, BIOS);
            Komputer = new Computer(MD, MSN, System, new double[1] { 12 }, Wskazowki, Obudowa, LCD, Kolor, ShippingMode, NowyMSN, PelnyModel);
            Swm = new SWM(swm: swm);
        }

        //Metoda odświeżajaca wartości Readera bez tworzenia nowego obiektu Reader
        public void RefreshData(Model model, Computer komp)
        {
            #region Wyszukiwanie wersji BIOS
            Plik = @"\Medion_NB_Bios.xlsx";
            if (Open())
            {
                LookForBios(komp);
                Close();
            }
            #endregion

            #region Wyszukiwanie pozostałych parametrów
            Plik = @"/NoteBookiRef_v2.xlsx";
            if (Open())
            {
                ReadData(model);
                Close();
            }
            #endregion
        }

        
    }

    
}
