using DataTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace Retriever
{
    class FileSystemManager /*: IFileSystemManager*/
    {
        public Settings set { get; set; }
        string HashCode { get; set; }

        public FileSystemManager()
        {
            LoadSettings();
            ReadSHA1();
        }

        void LoadSettings()
        {
            if (File.Exists(@"..\.." + @"\Settings.xml"))
            {
                try
                {
                    set = new Settings();
                    XmlSerializer xml = new XmlSerializer(typeof(Settings));
                    FileStream stream = new FileStream(@"..\.." + @"\Settings.xml", FileMode.Open);
                    set = (Settings)xml.Deserialize(stream);
                    set.DBPath = set.DBPath == "" || set.DBPath == null ? @"..\.." + @"\NoteBookiRef_v3.xlsx" : set.DBPath;
                    stream.Close();
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
                var file = @"..\.." + @"\Settings.xml";
                throw new FileNotFoundException(message, file);
            }
        }

        void ReadSHA1()
        {
            if (File.Exists(@"..\.." + @"\HashCode.xml"))
            {
                try
                {
                    XmlSerializer xml = new XmlSerializer(typeof(Settings));
                    FileStream stream = new FileStream(@"..\.." + @"\HashCode.xml", FileMode.Open);
                    HashCode = (string)xml.Deserialize(stream);
                    stream.Close();
                }
                catch (Exception e)
                {
                    File.Delete(@"..\.." + @"\HashCode.xml");
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
