using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows.Threading;

namespace Retriever
{
    //--------------------------------------------------Kontener na podstawowe dane o modelach---------------------------------------------------------
    public class Model
    {
        public int Wiersz { get; set; } //Nr linii danego modelu
        public int Zeszyt { get; set; } //Nr zeszytu na ktorym jest model (MD/PQ)
        public string MSN { get; set; }
        public string MD { get; set; }
        public string NazwaZeszytu { get; set; }
        public Model(int wiersz, int zeszyt, string msn, string md, string sheetName)
        {
            Wiersz = wiersz;
            Zeszyt = zeszyt;
            MSN = msn;
            MD = md;
            NazwaZeszytu = (sheetName == "MD") ? "Medion" : "Peaq";
        }
    }

    //--------------------------------------------------Kontener na ogólne i różne dane o komputerze---------------------------------------------------
    public class Computer
    {
        public string MD { get; set; }                          //Model komputera
        public string MSN { get; private set; }                 //MSN komputera
        public string System { get; private set; }              //System operacyjny   
        public double[] WearLevel { get; private set; }         //Przegrzanie baterii
        public string Wskazowki { get; private set; }           //Dodatkowe informacje odnośnie komputera
        public string Obudowa { get; private set; }             //Model obudowy
        public bool ShippingMode { get; private set; }          //ShippingMode
        public string NowyMSN { get; private set; }             //NowyMSN
        public string LCD { get; private set; }                 //Rodzaj matrycy
        public string PelnyModel { get; private set; }          //Dotyczy peaq - pełny model z dodatkowymi informacjami
        public string Kolor { get; private set; }               //Kolor obudowy

        public Computer(string md = "-", string msn = "-", string system = "-", double[] wearLevel = null,
            string wskazowki = "-", string obudowa = "-", string lcd = "-", string kolor = "-", bool shipp = false, string nowyMsn = "-",
            string pelnyModel = "-")
        {
            MD = md;
            MSN = msn;
            System = system;
            WearLevel = (wearLevel == null) ? new double[1] { -999 } : wearLevel;
            Wskazowki = wskazowki;
            Obudowa = obudowa;
            ShippingMode = shipp;
            NowyMSN = nowyMsn;
            LCD = lcd;
            PelnyModel = pelnyModel;
            Kolor = kolor;
        }
    }

    //--------------------------------------------------Kontener na dane związane z płyta główną-------------------------------------------------------
    public class Mainboard
    {
        public string Model { get; private set; }       //Model płyty głównej
        public string Producent { get; private set; }   //Producent płyty
        public string CPU { get; private set; }         //Procesor
        public string Taktowanie { get; private set; }  //Taktowanie
        public string WersjaBios { get; private set; }  //Wersja bios płyty
        public string ID { get; private set; }          //Pełna nazwa procesora

        public Mainboard(string model = "-", string producent = "-", string cpu = "-", string taktowanie = "-", string bios = "-")
        {
            Model = model;
            Producent = producent;
            CPU = cpu;
            Taktowanie = taktowanie;
            WersjaBios = bios;
            ID = string.Format($"{CPU}" + " @ " + $"{Taktowanie}");
        }
    }

    //--------------------------------------------------Kontener na dane o pamięci RAM-----------------------------------------------------------------
    public class RAM
    {
        public string Bank { get; private set; } //Slot
        public double Pojemnosc { get; private set; } //Pojemność
        public string Info { get; private set; }

        //Konstruktor standardowy
        public RAM(double size = 0, string bank = "-")
        {
            Bank = bank;
            Pojemnosc = size;
            Info = string.Format($"{bank}: {Pojemnosc}");
        }

        //Konstruktor na potrzeby bazy danych w pliku excel
        public RAM(string info)
        {
            //Przeszukuje zakres możliwych wartości pamięci ram
            for (int i = 33; i > 0; i--)
            {
                //Dodatkowa zmienna na prawidlowe wydobycie pamieci ram
                string temp;
                //Zabezpieczenie przed kolejnymi testami temp
                if (info.Length < 2) continue;
                //Przypadek 2048 GB - aby nie wykryło 20
                else if (info[1] == '0') continue;
                //W innym przypadku zbuduj zmienną do testów
                else temp = info[0].ToString() + info[1].ToString();

                //Testowanie w zależności o i
                if (temp.Contains(i.ToString()))
                {
                    Pojemnosc = i;
                    Bank = "0";
                    return;
                }
            }
            //W innym wypadku
            Pojemnosc = 0;
            Bank = "-";
        }
    }

    //--------------------------------------------------Kontener na dane o dyskach twardych------------------------------------------------------------
    public class Storage
    {
        public string Nazwa { get; private set; }       //Nazwa dysku
        public double Pojemnosc { get; private set; }   //Pojemność
        List<string> size = new List<string>            //Lista potencjalnych pojemności dysków
        {
            "1",
            "1,5",
            "1.5",
            "2",
            "16",
            "32",
            "64",
            "128",
            "240",
            "256",
            "500",
            "512",
            "750"
        };
        //Konstruktor standardowy
        public Storage(double size = 0, string nazwa = "Brak")
        {
            Nazwa = nazwa;
            Pojemnosc = size;
        }

        //Konstruktor na potrzeby bazy w pliku excel
        public Storage(string info)
        {
            info.ToLower();
            for (int i = size.Count - 1; i >= 0; i--)
            {
                //Badanie czy info zawiera w sobie którąś z wartości w liście
                if (info.Contains(size[i]))
                {
                    //Dodatkowy warunek uwzględniający pamięć typu M.2
                    if (size[i] == "2" && (info.Contains("m2") || info.Contains("m.2"))) continue;
                    else
                    {
                        Pojemnosc = double.Parse(size[i]) <= 2 ? double.Parse(size[i]) * 1000 : double.Parse(size[i]);
                        Nazwa = info.Replace(size[i], "").Replace("GB", "").Replace("TB", "").Replace(" ", ""); //Usuwa wartość oraz jednostkę, pozozstawia sam typ pamięci.
                        return;
                    }
                }
            }
            //W innym wypadku
            Pojemnosc = 0;
            Nazwa = info;
        }
    }

    //--------------------------------------------------Kontener na dane o IMAGE'u komputera-----------------------------------------------------------
    public class SWM
    {
        public string Dysk { get; private set; } //Dysk na którym zlokalizowano swconf.dat
        public string Swm { get; private set; }  //Nr SWM z pliku swconf.dat (3 linijka)

        public SWM(string dysk = "-", string swm = "00000000")
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

        public RetrieverInfo(Reader reader, Gatherer gatherer)
        {
            ReaderInfo = reader;
            GathererInfo = gatherer;
        }
    }

    //--------------------------------------------------Kontener na wyniki z klasy WMI-----------------------------------------------------------------
    public class Win32HardwareData
    {
        public string Wlasciwosc { get; private set; }
        public string Wartosc { get; private set; }

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
        Win32_ComputerSystem
    }

    //--------------------------------------------------Kontener przetrzymujący listę modeli-----------------------------------------------------------
    public class ModelListSource
    {
        public IEnumerable<Model> ListaModeli { get; private set; }
        public ModelListSource(Reader reader)
        {
            ListaModeli = reader.listaModeli;
        }
    }

    //--------------------------------------------------Kontener na informacje o dacie i czasie--------------------------------------------------------
    public class SystemDateTime
    { 
        public string Now { get; private set; }

        public SystemDateTime()
        {
            Now = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
        }   
    }

    //--------------------------------------------------Struktura dla nazędzia ustawiajacego datę i czas---------------------------------------------
    [StructLayout(LayoutKind.Sequential)]
    public struct SYSTEMTIME
    {
        public short wYear;
        public short wMonth;
        public short wDayOfWeek;
        public short wDay;
        public short wHour;
        public short wMinute;
        public short wSecond;
        public short wMilliseconds;
    }
}
