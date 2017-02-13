using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NUnit;
using NUnit.Framework;
namespace LOUV.Torp.MonProtocol
{
    [TestFixture]
    public class MonProtocolTest
    {
        string app = System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName);
        List<byte[]> GpsArray = null;
        List<byte[]> RangeArray = null;
        List<byte[]> TeleRangeArray = null;
        [SetUp]
        public void Setup()
        {
            GpsArray = new List<byte[]>();
            RangeArray = new List<byte[]>();
            TeleRangeArray = new List<byte[]>();
            string data = app + ".\\..\\..\\TestData\\接收UDP包.bin";
            using (FileStream fs = new FileStream(data, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader br = new BinaryReader(fs, new ASCIIEncoding()))
                {
                    byte[] chunk;

                    while (true)
                    {
                        chunk = br.ReadBytes(1032);
                        if (chunk.Length < 1032)
                            break;
                        switch(BitConverter.ToInt16(chunk,0))
                        {
                            case 0x0128:
                                RangeArray.Add(chunk);
                                break;
                            case 0x0129:
                                TeleRangeArray.Add(chunk);
                                break;
                            case 0x012A:
                                GpsArray.Add(chunk);
                                break;
                            default:
                                Assert.Fail("ID is not supported!");
                                break;
                        }
                    }
                }
            }
            
            
        }
        [Test]
        public void ParseGpsTest()
        {
            int i = 0;
            foreach(var buf in GpsArray)
            {
                
                Console.WriteLine("Process {0} Gps\n",i);
                var info = MonProtocol.ParseGps(buf);
                Assert.IsNotNull(info);
                Console.WriteLine("GPS: time:{0};lat:{1};long:{2}\n", info.UTCTime, info.Latitude, info.Longitude);
                i++;
            }
            
        }

        [Test]
        public void ParseRangeTest()
        {
            foreach (var buf in RangeArray)
            {
                Assert.IsNotNull(MonProtocol.ParseRange(buf));
            }
        }
        [Test]
        public void ParseTeleRangeTest()
        {
            foreach (var buf in TeleRangeArray)
            {
                Assert.IsNotNull(MonProtocol.ParseTeleRange(buf));
            }
        }
        [TearDown]
        public void ShutDown()
        {

        }
    }
}
