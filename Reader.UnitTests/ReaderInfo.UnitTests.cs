using DataTypes;
using ExcelDataReader;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reader.UnitTests
{
	[TestFixture]
    class ReaderInfo_UnitTests
    {
		[Test]
		public void DatabaseManager_ConstrutorTests_VariousPossibilities()
        {
            
        }
    }

    class FakeFileSystemManager : IFileSystemManager
    {
        public string DbPath { get; set; }
        public string SSID { get; set; }
        public string WifiPassword { get; set; }
        public string Encryption { get; set; }
        public string Authetication { get; set; }
        public string HashCode { get; set; }
    }

	class FakeFSManagerFactory
    {
		static IFileSystemManager GetManager()
        {
			var temp = new FakeFileSystemManager();
            temp.DbPath = Environment.CurrentDirectory + @"\NoteBookiRef_v3.xlsx";
            temp.SSID = "Duch003";
            temp.WifiPassword = "Killer003";
            temp.HashCode = "44756368303033";
            temp.Encryption = "AES";
            temp.Authetication = "WPA2PSK";
            return temp;
        }
    }

    class FakeDatabaseManager : IDBManager
    {
        public ObservableCollection<Model> ListaModeli { get; set; }
        public FileStream stream { get; set; }
        public IExcelDataReader excelReader { get; set; }
        public DataSet result { get; set; }

        public DataPack ReadModel(Model model)
        {
            throw new NotImplementedException();
        }
    }
}
