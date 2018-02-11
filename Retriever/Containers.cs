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
    public class Status : INotifyPropertyChanged
    {
        public string WinStatus
        {
            get
            {
                return _WinStatus;
            }
            private set
            {
                if (_WinStatus != value)
                {
                    _WinStatus = value;
                    OnPropertyChanged("WinStatus");
                }
            }
        }
        string _WinStatus;
        public string SecureStatus
        {
            get
            {
                return _SecureStatus;
            }
            private set
            {
                if (_SecureStatus != value)
                {
                    _SecureStatus = value;
                    OnPropertyChanged("SecureStatus");
                }
            }

        }
        string _SecureStatus;
        public string KluczWindows
        {
            get
            {
                return _KluczWindows;
            }
            set
            {
                if (_KluczWindows != value)
                {
                    _KluczWindows = value;
                    OnPropertyChanged("KluczWindows");
                }
            }
        }
        string _KluczWindows;
        public int PortyUSB { get; private set; }
        public double[] StanBaterii
        {
            get
            {
                return _StanBaterii;
            }
            set
            {
                if (_StanBaterii != value)
                {
                    _StanBaterii = value;
                    OnPropertyChanged("StanBaterii");
                }
            }
        }
        double[] _StanBaterii;
        public event PropertyChangedEventHandler PropertyChanged;

        public Status()
        {
            RenewWindowsActivationStatusInfo();
            RefreshBatteriesState();
            RenewSecureBootInfo();
            RenewWindowsKeyInfo();
            CountUSB();
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
            KluczWindows = WMI.GetOriginalProductKey() == null ? "Brak klucza" : "Znaleziono klucz";
        }

        public void CountUSB()
        {
            PortyUSB = WMI.CountUSB();
        }

        public void RefreshBatteriesState()
        {
            StanBaterii = new double[0];
            int i = 0;
            var ans = WMI.GetSingleProperty(Win32Hardware.Win32_Battery, "EstimatedChargeRemaining");
            foreach (Win32HardwareData z in ans)
            {
                StanBaterii = ExpandArr.Expand(StanBaterii);
                StanBaterii[i] = Convert.ToInt64(z.Wartosc);
            }
        }

        public void RefreshBatteriesState(object sender, EventArgs e)
        {
            var ans = WMI.GetSingleProperty(Win32Hardware.Win32_Battery, "EstimatedChargeRemaining");
            int i = 0;
            foreach (Win32HardwareData z in ans)
            {
                StanBaterii[i] = Convert.ToInt64(z.Wartosc);
                i++;
            }
        }

        private void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
    }
}
