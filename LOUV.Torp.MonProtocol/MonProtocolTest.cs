using System;
using System.CodeDom;
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
                var gpsbuf = new byte[1030];
                Buffer.BlockCopy(buf, 2, gpsbuf, 0, 1030);
                Console.WriteLine("Process {0} Gps\n",i);
                var info = MonProtocol.ParseGps(gpsbuf);
                Assert.IsNotNull(info);
                Console.WriteLine("GPS: time:{0};lat:{1};long:{2}\n", info.UTCTime, info.Latitude, info.Longitude);
                i++;
            }
            
        }

        [Test]
        public void ParsePulseRangeTest()
        {
            foreach (var buf in RangeArray)
            {
                var litebuf = new byte[14];
                Buffer.BlockCopy(buf,2,litebuf,0,14);
                var range = MonProtocol.ParsePulseRange(litebuf);
                Assert.IsNotNull(range);
                Console.WriteLine("PulseRange: RelativePara1:{0};RelativePara2:{1};RecvGain:{2};PeakPosition:{3}\n", range.RelativePara1, range.RelativePara2, range.RecvGain, range.PeakPosition);
                var gpsbuf = new byte[1030];
                Buffer.BlockCopy(buf, 16, gpsbuf, 0, 1032-16);
                var info = MonProtocol.ParseGps(gpsbuf);
                Assert.IsNotNull(info);
                Console.WriteLine("GPS: time:{0};lat:{1};long:{2}\n", info.UTCTime, info.Latitude, info.Longitude);
            }
        }
        [Test]
        public void ParseTeleRangeTest()
        {
            foreach (var buf in TeleRangeArray)
            {
                var litebuf = new byte[14];
                Buffer.BlockCopy(buf, 2, litebuf, 0, 14);
                var range = MonProtocol.ParsePulseRange(litebuf);
                Assert.IsNotNull(range);
                Console.WriteLine("PulseRange: RelativePara1:{0};RelativePara2:{1};RecvGain:{2};PeakPosition:{3}\n", range.RelativePara1, range.RelativePara1, range.RecvGain, range.PeakPosition);
                var length = BitConverter.ToUInt16(buf, 31);
                var combuf = new byte[17+length];
                Buffer.BlockCopy(buf, 16, combuf, 0, 17 + length);
                var telerange = MonProtocol.ParseTeleRange(combuf,length);
                Assert.IsNotNull(telerange);
                Console.WriteLine("Telerange: SamplingStart:{0};RecvDelay:{1};ModemStyle:{2};Dopple:{3},CRC:{4};Message:{5}\n", telerange.SamplingStart,
                    telerange.RecvDelay, telerange.ModemStyle, telerange.Dopple, telerange.Crc,telerange.Message );
                var gpsbuf = new byte[1030];
                Buffer.BlockCopy(buf, 33+length, gpsbuf, 0, 1032 - 33-length);
                var info = MonProtocol.ParseGps(gpsbuf);
                Assert.IsNotNull(info);
                Console.WriteLine("GPS: time:{0};lat:{1};long:{2}\n", info.UTCTime, info.Latitude, info.Longitude);
            }
        }
        [TearDown]
        public void ShutDown()
        {

        }
    }
}
