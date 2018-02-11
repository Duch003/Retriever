using NativeWifi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public static class WirelessConnectionManager
    {
        static string Ssid;
        static WlanClient client;
        static WlanClient.WlanInterface wlanIface;
        //Metoda łącząca z siecią WiFi
        public static void Connect(string SSID, string password, string encrypt, string auth)
        {
            try
            {
                client = new WlanClient();
                wlanIface = client.Interfaces.First();
                string profileName = Ssid = SSID;
                string mac = ConvertToHex(profileName);
                string key = password;
                string encryption = encrypt;
                string authentication = auth;
                string profileXml = string.Format("<?xml version=\"1.0\"?>" +
                    "<WLANProfile xmlns=\"http://www.microsoft.com/networking/WLAN/profile/v1\">" +
                    "<name>{0}</name>" +
                    "<SSIDConfig>" +
                    "<SSID>" +
                    "<hex>{1}</hex>" + //SSID hex
                    "<name>{0}</name>" + //SSID
                    "</SSID>" +
                    "</SSIDConfig>" +
                    "<connectionType>ESS</connectionType>" +
                    "<connectionMode>auto</connectionMode>" +
                    "<MSM>" +
                    "<security>" +
                    "<authEncryption>" +
                    "<authentication>{4}</authentication>" + //Zabezpieczenie sieci
                    "<encryption>{3}</encryption>" + //Szyfrowanie
                    "<useOneX>false</useOneX>" +
                    "</authEncryption>" +
                    "<sharedKey>" +
                    "<keyType>passPhrase</keyType>" +
                    "<protected>false</protected>" +
                    "<keyMaterial>{2}</keyMaterial>" + //Hasło
                    "</sharedKey>" +
                    "</security>" +
                    "</MSM>" +
                    "</WLANProfile>", profileName, mac, key, encryption, authentication);
                wlanIface.SetProfile(Wlan.WlanProfileFlags.AllUser, profileXml, true);
                wlanIface.Connect(Wlan.WlanConnectionMode.Profile, Wlan.Dot11BssType.Any, profileName);
            }
            catch (Exception e)
            {
                string message = string.Format("Wystąpił błąd podczas próby połączenia z siecią WiFi:\n{0}", e.Message);
                throw new Exception(message);
            }
        }

        //Metoda rozłączajaca z siecią WiFi
        public static void Disconnect()
        {
            wlanIface.DeleteProfile(Ssid);
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
