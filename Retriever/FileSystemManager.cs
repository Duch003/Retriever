using DataTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Retriever
{
    class FileSystemManager : IFileSystemManager
    {
        FileStream fs;
        StreamReader sr;
        public string DbPath { get; set; }
        public string SSID { get; set; }
        public string WifiPassword { get; set; }
        public string Encryption { get; set; }
        public string Authetication { get; set; }
        public string HashCode { get; set; }

        public FileSystemManager()
        {
            LoadSettings();
            ReadSHA1();
        }

        void LoadSettings()
        {
            if (File.Exists(Environment.CurrentDirectory + @"\settings.txt"))
            {
                sr = null;
                try
                {
                    fs = new FileStream(Environment.CurrentDirectory + @"\settings.txt", FileMode.Open);
                    sr = new StreamReader(fs);
                    DbPath = sr.ReadLine();
                    SSID = sr.ReadLine();
                    WifiPassword = sr.ReadLine();
                    Encryption = sr.ReadLine();
                    Authetication = sr.ReadLine();
                    sr.Close();
                }
                catch (UnauthorizedAccessException ex)
                {
                    var message = string.Format("Brak uprawnień dostępu do pliku konfiguracyjnego.\n{0}", ex.Message);
                    throw new UnauthorizedAccessException(message);
                }
                catch (Exception e)
                {
                    var message = string.Format("Wystąpił błąd przy próbie otwarcia pliku kofiguracyjnego:\n{0}", e.Message);
                    throw new Exception(message);
                }
            }
            else
            {
                var message = string.Format("Nie odnaleziono pliku konficuracyjnego settings.txt w katalogu.");
                var file = Environment.CurrentDirectory + @"\settings.txt";
                throw new FileNotFoundException(message, file);
            }
        }

        void ReadSHA1()
        {
            sr = null;
            if (File.Exists(Environment.CurrentDirectory + @"\sha1.txt"))
            {
                try
                {
                    fs = new FileStream(Environment.CurrentDirectory + @"\sha1.txt", FileMode.Open);
                    sr = new StreamReader(fs);
                    HashCode = sr.ReadLine();
                    sr.Close();
                }
                catch (UnauthorizedAccessException ex)
                {
                    var message = string.Format("Brak uprawnień dostępu do hasza bazy danych.\n{0}", ex.Message);
                    MessageBox.Show(message, "Błąd dostępu do pliku", MessageBoxButton.OK, MessageBoxImage.Information);
                    HashCode = "";
                }
                catch (Exception e)
                {
                    var message = string.Format("Wystąpił błąd przy próbie odczytu hasza bazy danych:\n{0}", e.Message);
                    MessageBox.Show(message, "Błąd dostępu do pliku", MessageBoxButton.OK, MessageBoxImage.Information);
                    HashCode = "";
                }
            }
            else
            {
                HashCode = "";
            }
        }
    }
}
