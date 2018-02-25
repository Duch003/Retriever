using DataTypes;
using System;
using System.IO;
using System.Xml.Serialization;

namespace Retriever
{
    internal class FileSystemManager : IFileSystemManager
    {
        public Settings Set { get; set; }

        public FileSystemManager()
        {
            LoadSettings();
        }

        private void LoadSettings()
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
                        var sr = new StreamReader(stream);
                        temp = sr.ReadLine();
                        temp = temp ?? "";
                        sr.Close();
                        stream.Close();
                    }
                    else
                    {
                        temp = "";
                        File.Create(Environment.CurrentDirectory + @"\SHA1.txt").Close();
                    }
                    var xml = new XmlSerializer(typeof(Settings));
                    stream = new FileStream(Environment.CurrentDirectory + @"\Settings.xml", FileMode.Open);
                    Set = (Settings)xml.Deserialize(stream);
                    Set.DbPath = string.IsNullOrEmpty(Set.DbPath) ? Environment.CurrentDirectory + @"\NoteBookiRef_v3.xlsx" : Set.DbPath;
                    Set.Sha1 = temp;
                    stream.Close();
                }
                catch (UnauthorizedAccessException ex)
                {
                    var message = $"Brak uprawnień dostępu do pliku konfiguracyjnego.\n{ex.Message}";
                    throw new UnauthorizedAccessException(message);
                }
                catch (Exception e)
                {
                    var message = $"Wystąpił błąd przy próbie otwarcia pliku kofiguracyjnego:\n{e.Message}";
                    throw new Exception(message);
                }
            }
            else
            {
                const string message = "Nie odnaleziono pliku konficuracyjnego settings.txt w katalogu.";
                const string file = @"\Settings.xml";
                throw new FileNotFoundException(message, file);
            }
        }
    }

    
}
