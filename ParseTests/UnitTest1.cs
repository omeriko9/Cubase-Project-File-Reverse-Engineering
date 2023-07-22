using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parse;
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
            
            Assert.IsTrue(cpr.FoundSections.Count > 0);
            Assert.IsTrue(cpr.FoundSections.Where(x => x.Name == "FMemoryStream").Count() > 0);
            
            var fms = cpr.FoundSections.Where(x => x.Name == "FMemoryStream").First();
            Assert.IsTrue(fms.SubSections.Count > 0);
            
            var smixer = fms.SubSections.Where(x => x.Name.Equals("VST Mixer"));            
            var vstMixer = smixer.FirstOrDefault();
            
            Assert.IsNotNull(vstMixer);
            Assert.IsTrue(vstMixer.SubSections.Count > 0);
            
            var table = DataItem.ToDataTable(vstMixer.SubSections);
            Assert.IsTrue(table.Rows[3].ItemArray[6].ToString() == "Native Reverb Plus");
        }
    }
}
