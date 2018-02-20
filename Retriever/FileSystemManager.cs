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
    class FileSystemManager : IFileSystemManager
    {
        public Settings Set { get; set; }

        public FileSystemManager()
        {
            LoadSettings();
        }

        void LoadSettings()
        {
            if (File.Exists(Environment.CurrentDirectory + @"\Settings.xml"))
            {
                try
                {
                    FileStream stream;
                    string temp;
                    if (File.Exists(Environment.CurrentDirectory + @"\SHA1.txt"))
                    {
                        stream = new FileStream(Environment.CurrentDirectory + @"\SHA1.txt", FileMode.Open);
                        StreamReader sr = new StreamReader(stream);
                        temp = sr.ReadLine();
                        temp = temp == null ? "" : temp;
                        sr.Close();
                        stream.Close();
                    }
                    else
                    {
                        temp = "";
                        File.Create(Environment.CurrentDirectory + @"\SHA1.txt").Close();
                    }
                    XmlSerializer xml = new XmlSerializer(typeof(Settings));
                    stream = new FileStream(Environment.CurrentDirectory + @"\Settings.xml", FileMode.Open);
                    Set = (Settings)xml.Deserialize(stream);
                    Set.DBPath = Set.DBPath == "" || Set.DBPath == null ? Environment.CurrentDirectory + @"\NoteBookiRef_v3.xlsx" : Set.DBPath;
                    Set.SHA1 = temp;
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
                var file = @"\Settings.xml";
                throw new FileNotFoundException(message, file);
            }
        }
    }

    
}
