using Microsoft.Win32;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Retriever
{
    //--------------------------------------------------Kontener na podstawowe dane o modelach---------------------------------------------------------
    public class Model
    {
        public int WierszModel { get; set; } //Nr linii danego modelu
        public int WierszBios { get; set; } //Nr linii z danymi o BIOSie
        public string MSN { get; set; }
        public string MD { get; set; }
        public string BiosZeszyt { get; set; }

        public Model(int wierszModel, int wierszBios, string biosZeszyt, string msn, string md)
        {
            WierszModel = wierszModel;
            WierszBios = wierszBios;
            MSN = msn;
            MD = md;
            BiosZeszyt = biosZeszyt;
        }

        //Konstruktor na potrzeby serializowania listy modeli
        public Model()
        { }
    }

    //--------------------------------------------------Kontener na ogólne i różne dane o komputerze---------------------------------------------------
    public class Computer
    {
        public string MD { get; set; }                  //Model komputera
        public string MSN { get; set; }                 //MSN komputera
        public string System { get; set; }              //System operacyjny   
        public double[] WearLevel { get; set; }         //Przegrzanie baterii
        public string Wskazowki { get; set; }           //Dodatkowe informacje odnośnie komputera
        public string Obudowa { get; set; }             //Model obudowy
        public bool ShippingMode { get; set; }          //ShippingMode
        public string StaryMSN { get; set; }             //NowyMSN
        public string LCD { get; set; }                 //Rodzaj matrycy
        public string Kolor { get; set; }               //Kolor obudowy

        public Computer(string md = "-", string msn = "-", string system = "-", double[] wearLevel = null,
            string wskazowki = "-", string obudowa = "-", string lcd = "-", string kolor = "-", bool shipp = false, string staryMsn = "-")
        {
            MD = md;
            MSN = msn;
            System = system;
            WearLevel = (wearLevel == null) ? new double[1] { -999 } : wearLevel;
            Wskazowki = wskazowki;
            Obudowa = obudowa;
            ShippingMode = shipp;
            StaryMSN = staryMsn;
            LCD = lcd;
            Kolor = kolor;
        }
    }

    //--------------------------------------------------Kontener na dane związane z płyta główną-------------------------------------------------------
    public class Mainboard
    {
        public string Model { get; set; }       //Model płyty głównej
        public string Producent { get; set; }   //Producent płyty
        public string CPU { get; set; }         //Procesor
        public string Taktowanie { get; set; }  //Taktowanie
        public string ID { get; set; }          //Pełna nazwa procesora

        public Mainboard(string model = "-", string producent = "-", string cpu = "-", string taktowanie = "-", string bios = "-")
        {
            Model = model;
            Producent = producent;
            CPU = cpu;
            Taktowanie = taktowanie;
            ID = string.Format($"{CPU}" + " @ " + $"{Taktowanie}");
        }
    }

    //--------------------------------------------------Kontener na dane o pamięci RAM-----------------------------------------------------------------
    public class RAM
    {
        public double Pojemnosc { get; set; } //Pojemność

        //Konstruktor standardowy
        public RAM(double size = 0, string bank = "-")
        {
            Pojemnosc = size;
        }

        //Konstruktor na potrzeby bazy danych w pliku excel
        public RAM(string info)
        {
            Regex[] size = new Regex[2]
            {
                new Regex(@"\d\d"),
                new Regex(@"\d")
            };
            Match result;
            foreach(Regex rgx in size)
            {
                result = rgx.Match(info);
                if (result.Success)
                {
                    Pojemnosc = Convert.ToDouble(result.Value);
                    break;
                }
                else Pojemnosc = -999;        
            }
        }
    }

    //--------------------------------------------------Kontener na dane o dyskach twardych------------------------------------------------------------
    public class Storage
    {
        public string Nazwa { get; set; }       //Nazwa dysku
        public double Pojemnosc { get; set; }   //Pojemność
        //Konstruktor standardowy
        public Storage(double size = 0, string nazwa = "Brak")
        {
            Nazwa = nazwa;
            Pojemnosc = size;
        }

        //Konstruktor na potrzeby bazy w pliku excel
        public Storage(string info)
        {
            //Wzory dla odnajdywania wartości pojemności
            Regex[] size = new Regex[3]
            {
                new Regex(@"\d{4}"),
                new Regex(@"\d{3}"),
                new Regex(@"\d{2}")
            };
            //Wzory dla odnajdywania typu dysku
            Regex[] type = new Regex[4]
            {
                new Regex(@"[A-Za-z]{3}\s[A-Za-z]\d"),
                new Regex(@"[A-Za-z]{5}"),
                new Regex(@"[A-Za-z]{4}"),
                new Regex(@"[A-Za-z]{3}")
            };
            //Test i wynik
            Match result;
            foreach(Regex rgx in size)
            {
                result = rgx.Match(info);
                if (result.Success)
                {
                    Pojemnosc = Convert.ToDouble(result.Value);
                    break;
                }
                else Pojemnosc = 0;
            }
            foreach (Regex rgx in type)
            {
                result = rgx.Match(info);
                if (result.Success)
                {
                    Nazwa = result.Value;
                    break;
                }
                else Nazwa = "-";
            }
        }

        //Nadpisana metoda ToString na potrzeby wyświetlania w tabeli wartości z bazy danych
        public override string ToString()
        {
            return string.Format("{0}: {1}", Nazwa, Pojemnosc);
        }
    }

    //--------------------------------------------------Kontener na dane o IMAGE'u komputera-----------------------------------------------------------
    public class SWM
    {
        public string Dysk { get; set; } //Dysk na którym zlokalizowano swconf.dat
        public string Swm { get; set; }  //Nr SWM z pliku swconf.dat (3 linijka)

        public SWM(string swm, string dysk = "-")
        {
            Dysk = dysk;
            Swm = swm;
        }
    }

    //--------------------------------------------------Kontener danych porównwczych-------------------------------------------------------------------
    public class RetrieverInfo
    {
        public Reader ReaderInfo{ get; set; }
        public Gatherer GathererInfo { get; set; }
        public Status Statusy { get; set; }

        public RetrieverInfo(Reader reader, Gatherer gatherer, Status statusy)
        {
            ReaderInfo = reader;
            GathererInfo = gatherer;
            Statusy = statusy;
        }
    }

    //--------------------------------------------------Kontener na wyniki z klasy WMI-----------------------------------------------------------------
    public class Win32HardwareData
    {
        public string Wlasciwosc { get; set; }
        public string Wartosc { get; set; }

        public Win32HardwareData(string property, string value)
        {
            Wlasciwosc = property;
            Wartosc = value;
        }
    }

    //--------------------------------------------------Typ wyliczeniowy wszystkich klas WIN32_Hardware------------------------------------------------
    public enum Win32Hardware : byte
    {
        //Chłodzenie
        Win32_Fan,
        Win32_HeatPipe,
        Win32_Refrigeration,
        Win32_TemperatureProbe,
        //Urządzenia wejściowe
        Win32_Keyboard,
        Win32_PointingDevice,
        //Urządzenia pamięci masowej
        Win32_AutochkSetting,
        Win32_CDROMDrive,
        Win32_DiskDrive,
        Win32_FloppyDrive,
        Win32_PhysicalMedia,
        Win32_TapeDrive,
        //Płyta główna, Kontrolery i Porty
        Win32_1394Controller,
        Win32_1394ControllerDevice,
        Win32_AllocatedResource,
        Win32_AssociatedProcessorMemory,
        Win32_BaseBoard,
        Win32_BIOS,
        Win32_Bus,
        Win32_CacheMemory,
        Win32_ControllerHasHub,
        Win32_DeviceBus,
        Win32_DeviceMemoryAddress,
        Win32_DeviceSettings,
        Win32_DMAChannel,
        Win32_FloppyController,
        Win32_IDEController,
        Win32_IDEControllerDevice,
        Win32_InfraredDevice,
        Win32_IRQResource,
        Win32_MemoryArray,
        Win32_MemoryArrayLocation,
        Win32_MemoryDevice,
        Win32_MemoryDeviceArray,
        Win32_MemoryDeviceLocation,
        Win32_MotherboardDevice,
        Win32_OnBoardDevice,
        Win32_ParallelPort,
        Win32_PCMCIAController,
        Win32_PhysicalMemory,
        Win32_PhysicalMemoryArray,
        Win32_PhysicalMemoryLocation,
        Win32_PNPAllocatedResource,
        Win32_PNPDevice,
        Win32_PNPEntity,
        Win32_PortConnector,
        Win32_PortResource,
        Win32_Processor,
        Win32_SCSIController,
        Win32_SCSIControllerDevice,
        Win32_SerialPort,
        Win32_SerialPortConfiguration,
        Win32_SerialPortSetting,
        Win32_SMBIOSMemory,
        Win32_SoundDevice,
        Win32_SystemBIOS,
        Win32_SystemDriverPNPEntity,
        Win32_SystemEnclosure,
        Win32_SystemMemoryResource,
        Win32_SystemSlot,
        Win32_USBController,
        Win32_USBControllerDevice,
        Win32_USBHub,
        //Urządzenia sieciowe
        Win32_NetworkAdapter,
        Win32_NetworkAdapterConfiguration,
        Win32_NetworkAdapterSetting,
        //Zasilanie
        Win32_Battery,
        Win32_CurrentProbe,
        Win32_PortableBattery,
        Win32_PowerManagementEvent,
        Win32_VoltageProbe,
        BatteryStaticData,
        BatteryFullChargedCapacity,
        //Drukarki
        Win32_DriverForDevice,
        Win32_Printer,
        Win32_PrinterConfiguration,
        Win32_PrinterController,
        Win32_PrinterDriver,
        Win32_PrinterDriverDll,
        Win32_PrinterSetting,
        Win32_PrintJob,
        Win32_TCPIPPrinterPort,
        //Telefony
        Win32_POTSModem,
        Win32_POTSModemToSerialPort,
        //Karty graficzne i monitor
        Win32_DesktopMonitor,
        Win32_DisplayControllerConfiguration,
        Win32_VideoController,
        Win32_VideoSettings,
        //Inne
        Win32_OperatingSystem,
        Win32_ComputerSystem,
        SoftwareLicensingProduct,
        Win32_WindowsProductActivation,
        SoftwareLicensingService
    }

    //--------------------------------------------------Typ wyliczeniowy na błędy Menedżera urządzeń---------------------------------------------------
    public enum ConfigManagerErrorCode : byte
    {
        working_properly,
        not_configured_correctly,
        cannot_load_the_driver,
        might_be_corrupted_or_low_on_memory_or_other_resources,
        one_of_its_drivers_or_your_registry_might_be_corrupted,
        needs_a_resource_that_Windows_cannot_manage,
        boot_configuration_conflicts_with_other_devices,
        cannot_filter,
        driver_loader_is_missing,
        controlling_firmware_is_reporting_the_resources_for_the_device_incorrectly,
        cannot_start,
        failed,
        cannot_find_enough_free_resources_that_it_can_use,
        cannot_verify_this_devices_resources,
        cannot_work_properly_until_you_restart_your_computer,
        reenumeration_problem,
        cannot_identify_all_the_resources_this_device_uses,
        device_is_asking_for_an_unknown_resource_type,
        reinstall_the_drivers_for_this_device,
        failure_using_the_VxD_loader,
        your_registry_might_be_corrupted,
        system_failure_Try_changing_the_driver_for_this_device,
        disabled,
        system_failure_state2_Try_changing_the_driver_for_this_device,
        not_present_or_not_working_properly_or_does_not_have_all_its_drivers_installed,
        windows_is_still_setting_up_this_device,
        windows_is_still_setting_up_this_device_2,
        not_valid_log_configuration,
        drivers_are_not_installed,
        disabled_because_the_firmware_of_the_device_did_not_give_it_the_required_resources,
        Interrupt_Request_IRQ_resource_that_another_device_is_using,
        not_working_properly_because_Windows_cannot_load_the_drivers_required_for_this_device,
    }

    //--------------------------------------------------Klasa obsługująca opisy błędów z menedżera urządzeń--------------------------------------------
    public class ConfigManagerErrorDescription
    {
        int index;
        public ConfigManagerErrorDescription(ConfigManagerErrorCode Code)
        {
            index = (int)Code;
        }
        string ReturnDescription(int index)
        {
            switch (index)
            {
                case 0:
                    return "This device is working properly.";
                case 1:
                    return "This device is not configured correctly.";
                case 2:
                    return "Windows cannot load the driver for this device.";
                case 3:
                    return "The driver for this device might be corrupted, or your system may be running low on memory or other resources.";
                case 4:
                    return "This device is not working properly. One of its drivers or your registry might be corrupted.";
                case 5:
                    return "The driver for this device needs a resource that Windows cannot manage. ";
                case 6:
                    return "The boot configuration for this device conflicts with other devices.";
                case 7:
                    return "Cannot filter.";
                case 8:
                    return "The driver loader for the device is missing.";
                case 9:
                    return "This device is not working properly because the controlling firmware is reporting the resources for the device incorrectly.";
                case 10:
                    return "This device cannot start.";
                case 11:
                    return "This device failed.";
                case 12:
                    return "This device cannot find enough free resources that it can use.";
                case 13:
                    return "Windows cannot verify this device's resources.";
                case 14:
                    return "This device cannot work properly until you restart your computer.";
                case 15:
                    return "This device is not working properly because there is probably a re-enumeration problem.";
                case 16:
                    return "Windows cannot identify all the resources this device uses.";
                case 17:
                    return "This device is asking for an unknown resource type";
                case 18:
                    return "Reinstall the drivers for this device.";
                case 19:
                    return "Failure using the VxD loader.";
                case 20:
                    return "Your registry might be corrupted.";
                case 21:
                    return "System failure: Try changing the driver for this device. If that does not work, see your hardware documentation. Windows is removing this device. ";
                case 22:
                    return "This device is disabled.";
                case 23:
                    return "System failure: Try changing the driver for this device. If that doesn't work, see your hardware documentation.";
                case 24:
                    return "This device is not present, is not working properly, or does not have all its drivers installed.";
                case 25:
                    return "Windows is still setting up this device.";
                case 26:
                    return "Windows is still setting up this device.";
                case 27:
                    return "This device does not have valid log configuration.";
                case 28:
                    return "The drivers for this device are not installed.";
                case 29:
                    return "This device is disabled because the firmware of the device did not give it the required resources.";
                case 30:
                    return "This device is using an Interrupt Request (IRQ) resource that another device is using.";
                case 31:
                    return "This device is not working properly because Windows cannot load the drivers required for this device.";
                default:
                    return "The cake is a lie.";
            }
        }

        public override string ToString()
        {
            return string.Format(ReturnDescription(index));
        }
    }

    //--------------------------------------------------Kontener na dane o błędach z menedżera urządzeń------------------------------------------------
    public class DeviceManager
    {
        public string Nazwa { get; set; }
        public string TrescBledu { get; set; }
        ConfigManagerErrorDescription Descriptor;

        public DeviceManager(string nazwa, int kod)
        {
            Nazwa = nazwa;
            Descriptor = new ConfigManagerErrorDescription((ConfigManagerErrorCode)kod);
            TrescBledu = Descriptor.ToString();
        }
    }

    //--------------------------------------------------Kontener na dane o adresach MAC urządzeń sieciowych--------------------------------------------
    public class NetDevice
    {
        public string Nazwa { get; set; }
        public string AdresMAC { get; set; }

        public NetDevice(string nazwa, string adres)
        {
            Nazwa = nazwa;
            AdresMAC = adres;
        }
    }

    //--------------------------------------------------Kontener na dane o wersji BIOS która powinna być-----------------------------------------------
    public class Bios
    {
        public string Wersja { get; set; }
        public string Opis { get; set; }

        public Bios(string ver, string opis = "-")
        {
            Wersja = ver;
            Opis = opis;
        }
    }

    //--------------------------------------------------Typ wyliczeniowy ze statusami aktywacji systemu------------------------------------------------
    public enum WindowsActivationStatus : byte
    {
        Unlicensed,
        Licensed,
        OOBGRace,
        OOTGrace,
        NonGeniueGrace,
        Notification,
        ExtendedGrace
    }

    //--------------------------------------------------Typ wyliczeniowy dla statusów SecureBoot-------------------------------------------------------
    public enum SecureBootStatus : sbyte
    {
        None = -1,
        Disabled = 0,
        Enabled = 1
    }

    //--------------------------------------------------Kontener na poszczególne statusy systemu-------------------------------------------------
    public class Status
    {
        public string WinStatus { get; private set; }
        public string SecureStatus { get; private set; }
        public string KluczWindows { get; private set; }
        public int PortyUSB { get; private set; }
        public double[] StanBaterii { get; private set; }

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
            var ans = WMI.GetSingleProperty(Win32Hardware.Win32_USBController, "Availability");
            PortyUSB = ans.Where(z => z.Wlasciwosc == "Availability").Count();

        }

        public void RefreshBatteriesState()
        {
            StanBaterii = new double[0];
            int i = 0;
            var ans = WMI.GetSingleProperty(Win32Hardware.Win32_Battery, "EstimatedChargeRemaining");
            foreach(Win32HardwareData z in ans)
            {
                StanBaterii = ExpandArr.Expand(StanBaterii);
                StanBaterii[i] = Convert.ToInt64(z.Wartosc);
            }
        }

        public void RefreshBatteriesState(object sender, EventArgs e)
        {
            RefreshBatteriesState();
        }
    }

    //--------------------------------------------------Kontener na dane o kartach graficznych----------------------------------------------------------
    public class GraphicCard
    {
        public string Nazwa { get; set; }
        public string Opis { get; set; }

        public GraphicCard(string nazwa, string opis)
        {
            Nazwa = nazwa;
            Opis = opis;
        }
    }

}
