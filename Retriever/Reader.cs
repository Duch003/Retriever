using System;
using System.Collections.Generic;
using ExcelDataReader;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml;

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
        public IEnumerable<Model> listaModeli { get; private set; }
        public string BiosZBazy { get; private set; }
        string Plik = @"/NoteBookiRef_v2.xlsx";

        //Kontruktor wyciągający dane o modelu z bazy danych
        public Reader(Model model)
        {
            Open();
            ReadData(model);
            Close();
        }

        //Konstruktor pobierający listę modeli
        public Reader()
        {
            Open();
            SetDataTables();
            CreateXMLFile();
            Close();
        }

        //Konstruktor Pobierajacy aktualny bios z drugiej bazy
        public Reader(string plik, Computer komp)
        {
            var temp = Plik;
            Plik = plik;
            Open();
            LookForBios(komp);
            Close();
            Plik = temp;
        }

        //Metoda otwierająca plik bazy danych
        void Open()
        {

            stream = new FileStream(Environment.CurrentDirectory + Plik, FileMode.Open, FileAccess.Read);
            excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            result = excelReader.AsDataSet();
            FileInfo fInfo = new FileInfo("staticdata.txt");
            FileStream fs;
            if (!fInfo.Exists)
            {
                fs = File.Create(Environment.CurrentDirectory);
                


            }
                
            else
                fs = fInfo.OpenRead();

        }

        //Utworzenie pliku XML dla bazy danych
        void CreateXMLFile()
        {
            //Utworz obiekt do zapisywania tekstu
            XmlTextWriter writer = new XmlTextWriter("Model.xml", System.Text.Encoding.UTF8);
            //Parametry zapisu
            writer.WriteStartDocument(true);
            writer.Formatting = Formatting.Indented;
            writer.Indentation = 2;
            //Utworzenie bazowego znacznika
            writer.WriteStartElement("BAZA");
            //Zapisanie bazy
            foreach(Model z in listaModeli)
            {
                CreateNode(z.Wiersz, z.Zeszyt, z.MSN, z.MD, writer);
            }
            //Zamknięcie znacznika
            writer.WriteEndElement();
            //Zamknięcie dokumentu
            writer.WriteEndDocument();
            
            writer.Close();
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
                XmlDocument doc = new XmlDocument();
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
            PlytaGlowna = new Mainboard(ModelPlyty, ProducentPlyty, CPU, Taktowanie, BIOS);
            Komputer = new Computer(MD, MSN, System, new double[1] { 12 }, Wskazowki, Obudowa, LCD, Kolor, ShippingMode, NowyMSN, PelnyModel);
            Swm = new SWM(swm: swm);
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
    }
}


//BLAD LOGICZNY
//PO USUNIECIU W WYSZUKIWARCE BLAD
