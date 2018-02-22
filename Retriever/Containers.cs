using DataTypes;
using Gatherer;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Linq;
using Utilities;

namespace Retriever
{
    //--------------------------------------------------Kontener danych porównwczych-------------------------------------------------------------------
    public class RetrieverInfo
    {
        public ReaderInfo ReaderData { get; set; }
        public GathererInfo GathererData { get; set; }
        public Status Statusy { get; set; }

        public RetrieverInfo(ReaderInfo reader, GathererInfo gatherer, Status statusy)
        {
            ReaderData = reader;
            GathererData = gatherer;
            Statusy = statusy;
        }
    }
    //--------------------------------------------------Kontener na poszczególne statusy systemu-------------------------------------------------
    public class Status
    {
        public string WinStatus { get; set; }
        public string SecureStatus { get; set; }
        public string KluczWindows { get; set; }
        public string[] StanBaterii { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public Status()
        {
            RenewWindowsActivationStatusInfo();
            RefreshBatteriesState();
            RenewSecureBootInfo();
            RenewWindowsKeyInfo();
        }

        public void RenewWindowsActivationStatusInfo()
        {
            WindowsActivationStatus result = (WindowsActivationStatus)Convert.ToInt16(WMI.GetSingleProperty
                (Win32Hardware.SoftwareLicensingProduct, property: "LicenseStatus",
                condition: "ApplicationID = '55c92734-d682-4d71-983e-d6ec3f16059f' AND PartialProductKey != null")
                .First().Wartosc);
            WinStatus = result.ToString();
        }

        public void RenewSecureBootInfo()
        {
            SecureBootStatus secure = (SecureBootStatus)Convert.ToInt16(Registry.GetValue(
               @"HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Control\SecureBoot\State",
               "UEFISecureBootEnabled", -1));
            SecureStatus = secure.ToString();
        }

        public void RenewWindowsKeyInfo()
        {
            KluczWindows = WMI.GetOriginalProductKey();
        }

        public void RefreshBatteriesState()
        {
            StanBaterii = new string[0];
            double temp;
            int i = 0;
            var ans = WMI.GetSingleProperty(Win32Hardware.Win32_Battery, "EstimatedChargeRemaining");
            foreach (Win32HardwareData z in ans)
            {
                StanBaterii = ExpandArr.Expand(StanBaterii);
                if (double.TryParse(z.Wartosc, out temp))
                    StanBaterii[i] = z.Wartosc;
                else
                    StanBaterii[i] = "Wymiana baterii!";
            }
        }
    }
}
