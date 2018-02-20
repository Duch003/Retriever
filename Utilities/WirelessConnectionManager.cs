using NativeWifi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

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
                client = new WlanClient();
                wlanIface = client.Interfaces.First();
                FileStream stream = new FileStream(Environment.CurrentDirectory + @"..\..\..\WlanProfile.xml", FileMode.Open);
                var profile = from z in XElement.Load(stream).DescendantNodes().OfType<XElement>() select z.Value;
                name = profile.First().ToString();
                stream.Close();
                stream = new FileStream(Environment.CurrentDirectory + @"..\..\..\WlanProfile.xml", FileMode.Open);
                StreamReader sr = new StreamReader(stream);
                string profileXml = sr.ReadToEnd();
                sr.Close();
                stream.Close();
                wlanIface.SetProfile(Wlan.WlanProfileFlags.AllUser, profileXml, true);
                wlanIface.Connect(Wlan.WlanConnectionMode.Profile, Wlan.Dot11BssType.Any, name); //Ustawione na sztywno, zmienić
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
