using ExcelDataReader;
using NativeWifi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes
{
    //--------------------------------------------------Interfejs z danymi z bazy danych---------------------------------------------------------
    public interface IDBData
    {
        Computer Komputer { get; set; }
        RAM[] Ram { get; set; }
        Storage[] Dyski { get; set; }
        Mainboard PlytaGlowna { get; set; }
        Bios WersjaBios { get; set; }
        SWM[] Swm { get; set; } 
    }

    //--------------------------------------------------Interfejs z danymi lokalnymi---------------------------------------------------------
    public interface IDeviceData
    {
        DeviceManager[] MenedzerUrzadzen { get; set; }
        NetDevice[] UrzadzeniaSieciowe { get; set; }
        GraphicCard[] KartyGraficzne { get; set; }
    }

    //--------------------------------------------------Interfejs z danymi lokalnymi---------------------------------------------------------
    public interface IFileSystemManager
    {
        Settings Set { get; set; }
    }

    public interface IDBManager
    {
        ObservableCollection<Model> ListaModeli { get; set; }
        FileStream stream { get; set; }
        IExcelDataReader excelReader { get; set; }
        DataSet result { get; set; }

        DataPack ReadModel(Model model);
    }
}
