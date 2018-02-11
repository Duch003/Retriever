using System;
using System.IO;
using System.Windows;

namespace Utilities
{
    //--------------------------------------------------Klasa obsługująca zapisywanie i wyświetlanie błędów------------------------------------------
    public static class ErrorWriter
    {
        //Metoda zapisująca loga
        public static bool WriteErrorLog(Exception e)
        {
            bool logCreated = true;
            FileInfo errorInfo;
            StreamWriter sw = null;
            try
            {
                string name = string.Format(@"{0}.{1}.log", DateTime.Now.ToLongDateString(), DateTime.Now.ToLongTimeString());
                errorInfo = new FileInfo(name);
                sw = new StreamWriter(new FileStream(errorInfo.DirectoryName, FileMode.OpenOrCreate));
                sw.WriteLine("Obiekt, który wyrzucił wyjątek: {0}", e.Source);
                sw.WriteLine("Metoda która wyrzuciła wyjątek: {0}", e.TargetSite);
                sw.WriteLine("Wywołania stosu: {0}", e.StackTrace);
                sw.WriteLine("Pary klucz-wartość: {0}", e.Data);
                sw.WriteLine("Opis: {0}", e.Message);
                sw.Close();
                sw.Dispose();
            }
            catch (Exception logEx)
            {
                MessageBox.Show($"Nie można utworzyć loga błędu. Treść błędu:\n{logEx.Message}", "Błąd przy tworzeniu loga błędu.", MessageBoxButton.OK, MessageBoxImage.Error);
                logCreated = false;
            }
            return logCreated;
        }

        //Metoda działająca w wypadku kiedy nie można zapisać loga
        public static void ShowErrorLog(Exception e, string naglowek, string opis)
        {
            string mess = string.Format("\n\nObiekt, który wyrzucił wyjątek: {0}\n" +
                "Metoda która wyrzuciła wyjątek: {1}\n" +
                "Wywołania stosu: {2}\n" +
                "Pary klucz-wartość: {3}\n" +
                "Opis: {4}\n)",
                e.Source, e.TargetSite, e.StackTrace, e.Data, e.Message);
            MessageBox.Show(opis + mess, naglowek, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
