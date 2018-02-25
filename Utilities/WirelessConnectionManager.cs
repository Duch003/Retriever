using NativeWifi;
using System;
using DataTypes;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Threading;

namespace Utilities
{
    public static class WirelessConnectionManager
    {
        private static WlanClient _client;
        private static WlanClient.WlanInterface _wlanIface;
        private static string _name;

        //Metoda łącząca z siecią WiFi
        public static void Connect()
        {
            try
            { 
                //Utworzenie klienta sieciowego
                _client = new WlanClient();

                //Wczytanie pierwszego urządzenia sieciowego - domyślnie jest to karta sieciowa
                _wlanIface = _client.Interfaces.First();

                //TODO Jak można zmusić kartę do natychmiastowego odświeżenia listy wykrywanych sieci? Błąd - karta nie zawsze nadązą odświeżać listę sieci i zwraca błąd mimo działania karty
                
                ////Sprawdzenie czy karta sieciowa wykrywa jakiekolwiek sieci
                //var network = _wlanIface.GetAvailableNetworkList(0);
                //if (network.Length == 0)
                //    throw new EmptyNetworkListException("Lista dostępnych sieci WiFi jest pusta - podłącz antenty do karty sieciowej.");

                //Dodanie profilu połączenia z daną siecią
                var stream = new FileStream(Environment.CurrentDirectory + @"\WlanProfile.xml", FileMode.Open);

                //Zapisanie nazwy danej sieci z profilu
                var profile = from z in XElement.Load(stream).DescendantNodes().OfType<XElement>() select z.Value;
                _name = profile.First();
                stream.Close();

                //Otwarcie strumienia danych do profilu na nowo (wyzerowanie odczytu - aby móc odczytać od samego początku
                //Inaczej sypie błędem
                stream = new FileStream(Environment.CurrentDirectory + @"\WlanProfile.xml", FileMode.Open);
                var sr = new StreamReader(stream);

                //Odczytanie profilu
                var profileXml = sr.ReadToEnd();
                sr.Close();
                stream.Close();

                //Próba połączenia z siecią
                _wlanIface.SetProfile(Wlan.WlanProfileFlags.AllUser, profileXml, true);
                _wlanIface.Connect(Wlan.WlanConnectionMode.Profile, Wlan.Dot11BssType.Any, _name);
            }
            catch(Exception e)
            {
                var message = $"Wystąpił błąd podczas próby połączenia z siecią WiFi:\n{e.Message}";
                throw new Exception(message);
            }
        }

        //Metoda rozłączajaca z siecią WiFi
        public static void Disconnect()
        {
            _wlanIface.DeleteProfile(_name);
            _wlanIface = null;
            _client = null;
        }
    }
}
