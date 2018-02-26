using ExcelDataReader;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;

namespace DataTypes
{
    //--------------------------------------------------Interfejs z danymi z bazy danych---------------------------------------------------------
    public interface IDbData
    {
        Computer Komputer { get; set; }
        Ram[] Ram { get; set; }
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

    public interface IDbManager
    {
        ObservableCollection<Model> ListaModeli { get; set; }
        FileStream Stream { get; set; }
        IExcelDataReader ExcelReader { get; set; }
        DataSet Result { get; set; }

        DataPack ReadModel(Model model);
    }
}
