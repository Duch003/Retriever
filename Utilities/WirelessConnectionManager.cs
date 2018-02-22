using NativeWifi;
using System;
using DataTypes;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using static NativeWifi.Wlan;

namespace Utilities
{
    public static class WirelessConnectionManager
    {
        static WlanClient client;
        static WlanClient.WlanInterface wlanIface;
        static string name;

        //Metoda łącząca z siecią WiFi
        public static void Connect()
        {
            try
            { 
                //Utworzenie klienta sieciowego
                client = new WlanClient();

                //Wczytanie pierwszego urządzenia sieciowego - domyślnie jest to karta sieciowa
                wlanIface = client.Interfaces.First();

                //Sprawdzenie czy karta sieciowa wykrywa jakiekolwiek sieci
                WlanAvailableNetwork[] network = wlanIface.GetAvailableNetworkList(0);
                if (network.Length == 0)
                    throw new EmptyNetworkListException("Lista dostępnych sieci WiFi jest pusta - podłącz antenty do karty sieciowej.");

                //Dodanie profilu połączenia z daną siecią
                FileStream stream = new FileStream(Environment.CurrentDirectory + @"\WlanProfile.xml", FileMode.Open);

                //Zapisanie nazwy danej sieci z profilu
                var profile = from z in XElement.Load(stream).DescendantNodes().OfType<XElement>() select z.Value;
                name = profile.First().ToString();
                stream.Close();

                //Otwarcie strumienia danych do profilu na nowo (wyzerowanie odczytu - aby móc odczytać od samego początku
                //Inaczej sypie błędem
                stream = new FileStream(Environment.CurrentDirectory + @"\WlanProfile.xml", FileMode.Open);
                StreamReader sr = new StreamReader(stream);

                //Odczytanie profilu
                string profileXml = sr.ReadToEnd();
                sr.Close();
                stream.Close();

                //Próba połączenia z siecią
                wlanIface.SetProfile(Wlan.WlanProfileFlags.AllUser, profileXml, true);
                wlanIface.Connect(Wlan.WlanConnectionMode.Profile, Wlan.Dot11BssType.Any, name);
            }
            catch(Exception e)
            {
                string message = string.Format("Wystąpił błąd podczas próby połączenia z siecią WiFi:\n{0}", e.Message);
                throw new Exception(message);
            }
        }

        //Metoda rozłączajaca z siecią WiFi
        public static void Disconnect()
        {
            wlanIface.DeleteProfile(name);
            wlanIface = null;
            client = null;
        }
    }
}
