using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parse;
using Parse.DataItems;
using System;
using System.IO;
using System.Linq;

namespace ParseTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            CPR2 cpr = new CPR2();
            cpr.Parse(File.ReadAllBytes(@"Data\chill-01.cpr"));

            var smixer = cpr.GetSection(DataItemFactory.sVSTMixer); 
            
            var table = DataItem.ToDataTable(smixer.SubSections);
            Assert.IsTrue(table.Rows[3].ItemArray[6].ToString() == "Native Reverb Plus");
        }
    }
}
