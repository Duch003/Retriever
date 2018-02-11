using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Retriever.UnitTests
{
    class ReaderTests
    {
        [Test]
        public void Open_DatabaseDoesntExists_ReturnMessage()
        {
            Reader temp = new Reader();
        }

    }
}
