using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parse;
using Parse.DataItems;
using System;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;

namespace ParseTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestLoad()
        {
            CPR2 cpr = new CPR2();
            cpr.Parse(File.ReadAllBytes(@"Data\chill-01.cpr"));

            MakeSureChannel(cpr);
        }

        [TestMethod]
        public void TestSave()
        {
            CPR2 cpr = new CPR2();
            cpr.Parse(File.ReadAllBytes(@"Data\chill-01.cpr"));
            cpr.Save("tmp.sav");
            CPR2 cpr2 = new CPR2();
            cpr2.Parse(File.ReadAllBytes("tmp.sav"));
            MakeSureChannel(cpr2);
        }

        public void MakeSureChannel(CPR2 cpr)
        {
            Assert.IsTrue(cpr.VSTMixer.SubSections[5].SubSections[3].Name == "Native Reverb Plus");
        }
    }
}
