using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes
{
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
}
