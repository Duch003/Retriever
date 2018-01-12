using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using NativeWifi;

namespace Retriever
{
    public class AktwacjaWindows
    {
        WlanClient Client;
        string Key;

        public AktwacjaWindows()
        {
            if(GetWindowsKey())
            {
                Connect();
            }
            else
            {
                var mess = string.Format("Brak klucza Windows w systemie.");
                MessageBox.Show(mess, "Nie można aktywować systemu", MessageBoxButton.OK, MessageBoxImage.Information);
            }
                
        }

        static string GetStringForSSID(Wlan.Dot11Ssid ssid)
        {
            return Encoding.ASCII.GetString(ssid.SSID, 0, (int)ssid.SSIDLength);
        }

        void Connect()
        {
            Client = new WlanClient();
            string profileName = "WindowsActivation";
            string mac = "57696E646F777341637469766174696F6E";
            string profileXml = string.Format("<?xml version=\"1.0\"?><WLANProfile xmlns=\"http://www.microsoft.com/networking/WLAN/profile/v1\"><name>{0}</name><SSIDConfig><SSID><hex>{1}</hex><name>{0}</name></SSID></SSIDConfig><connectionType>ESS</connectionType><connectionMode>manual</connectionMode><MSM><security><authEncryption><authentication>open</authentication><encryption>none</encryption><useOneX>false</useOneX></authEncryption></security></MSM></WLANProfile>", profileName, mac);

            foreach (WlanClient.WlanInterface wlanIface in Client.Interfaces)
            {
                wlanIface.SetProfile(Wlan.WlanProfileFlags.AllUser, profileXml, true);
                wlanIface.Connect(Wlan.WlanConnectionMode.Profile, Wlan.Dot11BssType.Any, profileName);
            }
        }
        
        bool GetWindowsKey()
        {
            var temp = WMI.GetSingleProperty(Win32Hardware.SoftwareLicensingProduct, property: "OA3xOriginalProductKey");
            if (temp.Count() > 0)
            {
                Key = temp.First().Wartosc;
                return true;
            }               
            else
                return false;
        }
    }
}
