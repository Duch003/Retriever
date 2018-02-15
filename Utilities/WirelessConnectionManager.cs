using NativeWifi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Utilities
{
    public static class WirelessConnectionManager
    {
        static WlanClient client;
        static WlanClient.WlanInterface wlanIface;


        //Metoda łącząca z siecią WiFi
        public static void Connect(bool Security)
        {
            if (Security)
            {
                try
                {
                    client = new WlanClient();
                    wlanIface = client.Interfaces.First();
                    XmlSerializer xml = new XmlSerializer(typeof(string));
                    FileStream stream = new FileStream(@"C:\Users\Duch003\Documents\GitHub\Retriever\Retriever\WlanProfile_Password.xml", FileMode.Open);
                    string profileXml = (string)xml.Deserialize(stream);
                    wlanIface.SetProfile(Wlan.WlanProfileFlags.AllUser, profileXml, true);
                    wlanIface.Connect(Wlan.WlanConnectionMode.Profile, Wlan.Dot11BssType.Any, "WindowsActivation"); //Ustawione na sztywno, zmienić
                }
                catch(Exception e)
                {
                    string message = string.Format("Wystąpił błąd podczas próby połączenia z siecią WiFi:\n{0}", e.Message);
                    throw new Exception(message);
                }
            }
            else
            {
                try
                {
                    client = new WlanClient();
                    wlanIface = client.Interfaces.First();
                    XmlSerializer xml = new XmlSerializer(typeof(string));
                    FileStream stream = new FileStream(Environment.CurrentDirectory + @"..\..\..\XMLFile1.xml", FileMode.Open);
                    StreamReader sr = new StreamReader(stream);
                    string profileXml = sr.ReadToEnd()/*.Replace("\n", "").Replace(" ", "").Replace("\r", "");*/;
                    //string profileXml = (string)xml.Deserialize(stream);
                    wlanIface.SetProfile(Wlan.WlanProfileFlags.AllUser, profileXml, true);
                    wlanIface.Connect(Wlan.WlanConnectionMode.Profile, Wlan.Dot11BssType.Any, "WindowsActivation"); //Ustawione na sztywno, zmienić
                }
                catch (Exception e)
                {
                    string message = string.Format("Wystąpił błąd podczas próby połączenia z siecią WiFi:\n{0}", e.Message);
                    throw new Exception(message);
                }
            }
            //try
            //{
            //    client = new WlanClient();
            //    wlanIface = client.Interfaces.First();
            //    XmlSerializer xml = new XmlSerializer(typeof(string));
            //    FileStream stream = new FileStream(@"C:\Users\Duch003\Documents\GitHub\Retriever\Retriever\WlanProfile_Password.xml", FileMode.Open);
            //    //string profileName = Ssid = SSID;
            //    //string mac = ConvertToHex(profileName);
            //    //string key = password;
            //    //string encryption = encrypt;
            //    //string authentication = auth;
            //    string profileXml = (string)xml.Deserialize(stream);
                    
            //        //string.Format("<?xml version=\"1.0\"?>" +
            //        //"<WLANProfile xmlns=\"http://www.microsoft.com/networking/WLAN/profile/v1\">" +
            //        //"<name>{0}</name>" +
            //        //"<SSIDConfig>" +
            //        //"<SSID>" +
            //        //"<hex>{1}</hex>" + //SSID hex
            //        //"<name>{0}</name>" + //SSID
            //        //"</SSID>" +
            //        //"</SSIDConfig>" +
            //        //"<connectionType>ESS</connectionType>" +
            //        //"<connectionMode>auto</connectionMode>" +
            //        //"<MSM>" +
            //        //"<security>" +
            //        //"<authEncryption>" +
            //        //"<authentication>{4}</authentication>" + //Zabezpieczenie sieci
            //        //"<encryption>{3}</encryption>" + //Szyfrowanie
            //        //"<useOneX>false</useOneX>" +
            //        //"</authEncryption>" +
            //        //"<sharedKey>" +
            //        //"<keyType>passPhrase</keyType>" +
            //        //"<protected>false</protected>" +
            //        //"<keyMaterial>{2}</keyMaterial>" + //Hasło
            //        //"</sharedKey>" +
            //        //"</security>" +
            //        //"</MSM>" +
            //        //"</WLANProfile>", profileName, mac, key, encryption, authentication);
            //    wlanIface.SetProfile(Wlan.WlanProfileFlags.AllUser, profileXml, true);
            //    wlanIface.Connect(Wlan.WlanConnectionMode.Profile, Wlan.Dot11BssType.Any, "WindowsActivation"); //Ustawione na sztywno, zmienić
            //}
            //catch (Exception e)
            //{
            //    string message = string.Format("Wystąpił błąd podczas próby połączenia z siecią WiFi:\n{0}", e.Message);
            //    throw new Exception(message);
            //}
        }

        //Metoda rozłączajaca z siecią WiFi
        public static void Disconnect()
        {
            wlanIface.DeleteProfile("WindowsActivation");
            wlanIface = null;
            client = null;
        }

        //Konwerter ciągu znaków do wartości hexadecymalnej
        static string ConvertToHex(string name)
        {
            byte[] temp = Encoding.Default.GetBytes(name);
            var hexString = BitConverter.ToString(temp);
            hexString = hexString.Replace("-", "");
            return hexString;
        }
    }
}
