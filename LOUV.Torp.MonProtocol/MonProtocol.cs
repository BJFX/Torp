﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using TinyMetroWpfLibrary.Utility;
namespace LOUV.Torp.MonP
{
    public class MonProtocol
    {
        private static readonly Hashtable ACNWebHashtableID = new Hashtable();
        private static readonly Hashtable ACNCommandID = new Hashtable();

        private static BitArray data;//将数据转换成bit数组，低位在前。
        public static BitArray packdata;//将打包数据转换成bit数组

        private static Hashtable CmdTable = new Hashtable();//命令哈希表
        private static List<BitArray> CmdForSend = new List<BitArray>();//命令数据
        private static List<int> CmdNode = new List<int>();//命令地址
        private static List<int> NodeFindOut = new List<int>();//数据分组后的节点名集合

        private static int index = 0;//累进解析器的下标位置。
        private static int packindex = 0;//累进打包器的下标位置
        public static List<string[]> parselist = new List<string[]>();
        private static string[] str;
        public static string Errormessage { get; private set; }
        public static string BuoyID { get; set; }
        public static int Port { get; set; }

        public static bool bUseTrack { get; set; }
        public static List<string> TrackNodeList = new List<string>();
        private static bool _Initialed = false;
        public static bool Initialed
        {
            get { return _Initialed; }
            set { _Initialed = value; }

        }

        public static UInt16 SourceID { get; set; }
        public static int BlockIndex { get; set; }//块标识
        #region 成员函数
        /// <summary>
        /// 初始化协议参数
        /// </summary>
        static public void Init(int ID)
        {
            SourceID = (ushort)ID;
            BuoyID = "00";
            bUseTrack = false;
            Port = 2;
            BlockIndex = 0;
            ACNWebHashtableID.Clear();
            ACNCommandID.Clear();
            CmdForSend.Clear();
            CmdNode.Clear();
            NodeFindOut.Clear();
            CmdTable.Clear();
            //命令hash
            ACNWebHashtableID.Clear();
            ACNWebHashtableID.Add(0, "结束标识");
            ACNWebHashtableID.Add(1, "分段标识");
            ACNWebHashtableID.Add(2, "节点信息表");
            ACNWebHashtableID.Add(3, "邻节点表");
            ACNWebHashtableID.Add(4, "网络表");
            ACNWebHashtableID.Add(5, "网络简表");
            ACNWebHashtableID.Add(6, "路由表");
            ACNWebHashtableID.Add(7, "路径记录");
            ACNWebHashtableID.Add(8, "路径安排");
            ACNWebHashtableID.Add(9, "路径中断");
            ACNWebHashtableID.Add(10, "转发失败");
            ACNWebHashtableID.Add(61, "设备数据");
            ACNWebHashtableID.Add(62, "设备状态");
            ACNWebHashtableID.Add(63, "节点状态");
            ACNWebHashtableID.Add(101, "回环测试");
            ACNWebHashtableID.Add(102, "回环测试应答");
            ACNWebHashtableID.Add(103, "索取节点信息");
            ACNWebHashtableID.Add(104, "索取节点信息应答");
            ACNWebHashtableID.Add(105, "索取节点信息表");
            ACNWebHashtableID.Add(106, "索取节点信息表应答");
            ACNWebHashtableID.Add(107, "索取网络表");
            ACNWebHashtableID.Add(108, "索取网络表应答");
            ACNWebHashtableID.Add(109, "索取网络简表");
            ACNWebHashtableID.Add(110, "索取网络简表应答");
            ACNWebHashtableID.Add(111, "索取节点邻节点表");
            ACNWebHashtableID.Add(112, "索取节点邻节点表应答");
            ACNWebHashtableID.Add(113, "索取节点路由表");
            ACNWebHashtableID.Add(114, "索取节点路由表应答");
            ACNWebHashtableID.Add(115, "索取节点设备数据");
            ACNWebHashtableID.Add(116, "索取节点设备数据应答");
            ACNWebHashtableID.Add(117, "索取节点设备状态");
            ACNWebHashtableID.Add(118, "索取节点设备状态应答");
            ACNWebHashtableID.Add(119, "设备参数设置命令");
            ACNWebHashtableID.Add(120, "设备参数设置命令应答");
            ACNWebHashtableID.Add(121, "索取通信机状态");
            ACNWebHashtableID.Add(122, "索取通信机状态应答");
            //收发控制命令定义
            ACNWebHashtableID.Add(140, "设备数据定时回传开关");
            ACNWebHashtableID.Add(141, "收发自动调节开关");
            ACNWebHashtableID.Add(142, "通信制式开关");

            ACNWebHashtableID.Add(200, "全网复位");
            ACNWebHashtableID.Add(201, "网络路由命令");
            ACNWebHashtableID.Add(202, "网络路由命令");
            ACNWebHashtableID.Add(203, "网络路由命令");
            ACNWebHashtableID.Add(204, "网络路由命令");
            ACNWebHashtableID.Add(205, "网络路由命令");
            ACNWebHashtableID.Add(206, "扩展网络路由命令");
            /////////////////////////////////////////////////////
            //MSP特殊命令
            ACNCommandID.Clear();
            ACNCommandID.Add(255, "设置AD门限");
            ACNCommandID.Add(254, "将430能量数据写入FLASH");
            ACNCommandID.Add(253, "串口2、3，GPS，DSP配置命令");
            ACNCommandID.Add(252, "430校时命令");
            ACNCommandID.Add(251, "430休眠命令");
            ACNCommandID.Add(250, "调试状态，DSP开网络调试");
            ACNCommandID.Add(249, "430上电复位，为loader做准备");
            ACNCommandID.Add(248, "设置串口2和3定时唤醒时间");
            ACNCommandID.Add(247, "读取430实时状态");
            ACNCommandID.Add(246, "DSP喂狗开关");
            ACNCommandID.Add(245, "关DSP");
            ACNCommandID.Add(244, "给DSP上电");
            ACNCommandID.Add(243, "清零单片机重启次数");
            ACNCommandID.Add(242, "DSP进入Loader模式");

            //MSP上传数据命令
            ACNCommandID.Add(2, "DSP故障命令");
            ACNCommandID.Add(6, "上位机发布430休眠命令");
            ACNCommandID.Add(7, "上位机发布DSP关机命令");
            ACNCommandID.Add(8, "上位机读取通信机工作状态");
            ACNCommandID.Add(9, "通信机芯漏水命令");
            ACNCommandID.Add(10, "通信机电量低报警");
            ACNCommandID.Add(11, "温度过高报警命令");
            ACNCommandID.Add(12, "返回430工作状态数据");
            ACNCommandID.Add(13, "上位机读取430程序版本信息");
            ACNCommandID.Add(14, "返回430程序版本信息");
            ACNCommandID.Add(15, "休眠时间错误命令");
            ACNCommandID.Add(16, "浮标工作状态命令");
            ACNCommandID.Add(17, "DSP进入loader模式");
            ACNCommandID.Add(18, "DSP进入调试模式");
            ACNCommandID.Add(19, "关闭DSP");
            ACNCommandID.Add(20, "读取MSP430状态");
            ACNCommandID.Add(170, "DSP回传数据命令");
            ACNCommandID.Add(171, "下发DSP命令");
            _Initialed = true;
        }
        static public void GetDataForParse(byte[] d)
        {
            Clear();
            data = new BitArray(d);
        }
        static public void GetDataForParse(BitArray d)
        {
            Clear();
            data = new BitArray(d);
        }

        public static void ClearParseList()
        {
            if (parselist != null)
                parselist.Clear();
            index = 0;


        }
        public static void Clear()
        {
            CmdForSend.Clear();
            CmdNode.Clear();
            NodeFindOut.Clear();
            CmdTable.Clear();
            ClearParseList();
        }
        static public void InitForPack(int bitlen)
        {
            ClearParseList();
            packdata = new BitArray(bitlen);
            packindex = 0;
        }

        private static void AddtoList(string level, string typename, string datastring, string description)
        {
            str = new string[4];
            str[0] = level;
            str[1] = typename;
            str[2] = datastring;
            str[3] = description;
            parselist.Add(str);

        }

        //将相同目标节点通信网命令加入命令池，调用顺序：ACNBuilder.PackXXX()(AddPool)->Package
        public static void AddPool(int ID)
        {
            CmdNode.Add(ID);
            CmdForSend.Add(packdata);
        }
        //将通信网命令打包成网络或是串口数据包(171) 
        static public byte[] Package(bool bViaComm)
        {
            int total = 0;//数据总长度，不包括包头

            NodeFindOut.Clear();
            CmdTable.Clear();//每次打包前清空。
            for (int i = 0; i < CmdNode.Count; i++)
            {
                if (CmdTable.ContainsKey(CmdNode[i]))
                {
                    BitArray ba = (BitArray)CmdTable[CmdNode[i]]; //取出已有的数据
                    BitArray newba = new BitArray(ba.Length + CmdForSend[i].Length);
                    for (int a = 0; a < ba.Length; a++)
                    {
                        newba[a] = ba[a];
                    }
                    for (int j = 0; j < CmdForSend[i].Length; j++)
                    {
                        newba[ba.Length + j] = CmdForSend[i][j];
                    }
                    CmdTable[CmdNode[i]] = newba; //将新的数据放进哈希表


                }
                else
                {
                    CmdTable.Add(CmdNode[i], CmdForSend[i]);
                    NodeFindOut.Add(CmdNode[i]);
                }
                total += CmdForSend[i].Length;
            }

            //start encode!
            //打包协议
            int blocknum = CmdTable.Keys.Count;//块数
            total += blocknum * 34 + 6;

            InitForPack(total);
            int[] b = new int[1];
            b[0] = blocknum;
            OutPutIntBit(b, 6);
            for (int i = 0; i < blocknum; i++)
            {
                BitArray ba = (BitArray)CmdTable[NodeFindOut[i]];//数据区集合
                int blocklen = ba.Length + 34;
                BitArray blockba = new BitArray(blocklen);
                b[0] = BlockIndex;
                BlockIndex++;
                OutPutIntBit(b, 10);
                b[0] = blocklen;
                OutPutIntBit(b, 12);
                b[0] = SourceID;
                OutPutIntBit(b, 6);
                b[0] = NodeFindOut[i];
                OutPutIntBit(b, 6);
                OutPutArrayBit(ba);
            }
            CmdForSend.Clear();
            CmdNode.Clear();
            byte[] cmd = new byte[(int)Math.Ceiling(((double)total) / 8)];
            packdata.CopyTo(cmd, 0);
            bUseTrack = false;//reset track flag after pack data every time
            if (bViaComm)
            {
                return CommPackage(171, cmd);
            }
            else
            {
                return NetPackage(cmd);
            }

        }


        #endregion

        #region 串口打包解包

        //串口打包函数，加0xAA，校验，浮标协议包头
        public static byte[] CommPackage(int id, byte[] outcmd)
        {
            string head;
            byte type;
            var time = DateTime.Now.Year.ToString().TrimStart('2', '0') + "," + DateTime.Now.Month.ToString("00")
                       + "," + DateTime.Now.Day.ToString("00") + "," + DateTime.Now.Hour.ToString("00") + "," +
                       DateTime.Now.Minute.ToString("00")
                       + "," + DateTime.Now.Second.ToString("00") + ",";
            const string tail = ",END";
            var timelen = 0;
            if ((id >= 240) && (id <= 255)) //特殊命令
            {
                head = "EB90,10,";
                type = (byte)id;
                timelen = 0;
            }
            else //转发命令或浮标命令
            {
                head = "EB90,01,";
                type = 171;
                if ((id <= 22) && (id >= 16))
                    type = (byte)id;
                timelen = 23; //加上长度域
            }
            var aaHeadCmd = PackageAAHead(type, (byte)Port, outcmd); //加AA协议
            if (aaHeadCmd == null) throw new ArgumentNullException("协议无效！");
            var headbyte = Encoding.Default.GetBytes(head);
            var tailbyte = Encoding.Default.GetBytes(tail);
            var buoyid = BuoyID.ToString(CultureInfo.InvariantCulture) + ",";
            var bytebuoyid = Encoding.Default.GetBytes(buoyid);
            var cmd = new byte[headbyte.Length + aaHeadCmd.Length + 4 + timelen];
            Buffer.BlockCopy(headbyte, 0, cmd, 0, headbyte.Length);
            Buffer.BlockCopy(bytebuoyid, 0, cmd, headbyte.Length, 3);
            if (type == 171)
            {
                var total = headbyte.Length + aaHeadCmd.Length + 4 + timelen + 6;
                var lenstr = total.ToString("0000");
                time = lenstr + "," + time;
                var timechar = StringHexConverter.ConvertHexToChar(StringHexConverter.ConvertStrToHex(time));
                Buffer.BlockCopy(timechar, 0, cmd, headbyte.Length + 3, timechar.Length);
            }
            Buffer.BlockCopy(aaHeadCmd, 0, cmd, headbyte.Length + 3 + timelen, aaHeadCmd.Length);
            cmd[headbyte.Length + 3 + timelen + aaHeadCmd.Length] = 0x2C; //逗号
            var crc = CRCHelper.CRC16Byte(cmd);
            var bytecrc = BitConverter.GetBytes((ushort)crc);
            //长度高低位转换，for dsp
            var temp = bytecrc[0];
            bytecrc[0] = bytecrc[1];
            bytecrc[1] = temp;
            var fulcmd = new byte[headbyte.Length + aaHeadCmd.Length + tailbyte.Length + 6 + timelen];
            Buffer.BlockCopy(cmd, 0, fulcmd, 0, headbyte.Length + aaHeadCmd.Length + 4 + timelen);
            Buffer.BlockCopy(bytecrc, 0, fulcmd, headbyte.Length + 4 + timelen + aaHeadCmd.Length, 2);
            Buffer.BlockCopy(tailbyte, 0, fulcmd, headbyte.Length + 4 + timelen + aaHeadCmd.Length + 2, tailbyte.Length);
            return fulcmd;
        }

        //给数据打成AA头的包
        private static byte[] PackageAAHead(int id, byte port, byte[] outcmd)
        {
            const byte head = 0xAA;
            UInt16 crc = 0;
            byte[] package;
            byte bSourcePort = port;
            byte dest = 0;
            if ((id <= 22) && (id >= 16))
                dest = 1;
            var type = (byte)id;
            var length = (UInt16)outcmd.Length;
            if (type != outcmd[0]) //数据域非空
            {
                package = new byte[length + 6]; //不带校验的命令
                package[0] = head;
                package[1] = dest;
                package[2] = bSourcePort;
                package[3] = type;
                Buffer.BlockCopy(BitConverter.GetBytes(length), 0, package, 4, 2);
                //长度高低位转换，for dsp
                byte temp = package[4];
                package[4] = package[5];
                package[5] = temp;
                Buffer.BlockCopy(outcmd, 0, package, 6, length);
                crc = CRCHelper.CRC16Byte(package);
            }
            else //空数据
            {
                package = new byte[6]; //不带校验的命令
                length = 0;
                package[0] = head;
                package[1] = dest;
                package[2] = bSourcePort;
                package[3] = type;
                package[4] = 0;
                package[5] = 0;
                crc = CRCHelper.CRC16Byte(package);
                ;
            }
            var fulpackage = new byte[length + 8]; //带校验的命令
            Buffer.BlockCopy(package, 0, fulpackage, 0, length + 6);
            Buffer.BlockCopy(BitConverter.GetBytes(crc), 0, fulpackage, length + 6, 2);
            var crctemp = fulpackage[length + 6];
            fulpackage[length + 6] = fulpackage[length + 7];
            fulpackage[length + 7] = crctemp;
            return fulpackage;
        }

        #region 协议拆包

        //将串口回传数据拆包，返回数据记录时间，数据id，数据内容，信源混合包拆到ID为171为止，后面调用parse
        public static bool DepackCommData(byte[] cmd, out string time, out int id, out byte[] data)
        {
            try
            {
                string oldstr = Encoding.ASCII.GetString(cmd);
                var shortcmd = new byte[cmd.Length - 6];
                Buffer.BlockCopy(cmd, 0, shortcmd, 0, cmd.Length - 6);
                var crcnew = CRCHelper.CRC16Byte(shortcmd);
                var crcchar = new byte[2];
                Buffer.BlockCopy(cmd, cmd.Length - 6, crcchar, 0, 2);
                var tmp = crcchar[1];
                crcchar[1] = crcchar[0];
                crcchar[0] = tmp;
                var crcold = BitConverter.ToUInt16(crcchar, 0);
                if (crcold == crcnew)
                {
                    string[] str = oldstr.Split(',');
                    time = "20" + str[4] + " " + str[5] + "." + str[6] + " " + str[7] + ":" + str[8] + ":" + str[9];
                    var cmslen = Int32.Parse(str[3]) - 41;
                    var aadata = new byte[cmslen];
                    Buffer.BlockCopy(cmd, 34, aadata, 0, cmslen);
                    var cmdnocrc = new byte[aadata.Length - 2];
                    Array.Copy(aadata, cmdnocrc, aadata.Length - 2);
                    int len = (int)(aadata[4] << 8) + (int)aadata[5];
                    data = new byte[len];
                    Buffer.BlockCopy(aadata, 6, data, 0, len);
                    var aacrc = CRCHelper.CRC16Byte(cmdnocrc);
                    var newcrc = (UInt16)((int)(aadata[aadata.Length - 2] << 8) + (int)aadata[aadata.Length - 1]);
                    id = cmdnocrc[3];
                    if (aacrc == newcrc)
                    {
                        return true;
                    }
                    else //里层校验错误
                    {
                        return false;
                    }
                }
                else
                {
                    time = DateTime.Now.ToShortTimeString();
                    data = new byte[1];
                    id = 0;
                    return false;
                }
            }
            catch (Exception e)
            {
                time = DateTime.Now.ToShortTimeString();
                data = new byte[1];
                id = 0;
                return false;
            }
        }

        #endregion

        #endregion

        #region 网络打包
        //网络打包函数，加0xAA，校验，网络包头
        static public byte[] NetPackage(byte[] outcmd)
        {
            var fullpackage = new byte[outcmd.Length + 4];//完整命令 
            UInt16 uid = 0xEE01;
            Buffer.BlockCopy(BitConverter.GetBytes(uid), 0, fullpackage, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(outcmd.Length), 0, fullpackage, 2, 2);
            Buffer.BlockCopy(outcmd, 0, fullpackage, 4, outcmd.Length);
            return fullpackage;

        }
        #endregion

        #region 网络信源包解包
        static public bool Parse()
        {

            Clear();
            index = 0;
            try
            {
                //解块
                int blocknum = GetIntValueFromBit(6);
                AddtoList("0", "块数", blocknum.ToString(), "");
                for (int i = 0; i < blocknum; i++)
                {
                    string num = "块" + (i + 1).ToString();
                    AddtoList("0", num, (i + 1).ToString(), "");
                    //块定义
                    int blockid = GetIntValueFromBit(10);
                    AddtoList("1", "块标识", blockid.ToString(), "");
                    int blocklen = GetIntValueFromBit(12);
                    AddtoList("1", "块长", blocklen.ToString(), "");
                    int StartId = GetIntValueFromBit(6);
                    AddtoList("1", "起始源地址", StartId.ToString(), "");


                    int EndId = GetIntValueFromBit(6);
                    AddtoList("1", "目的地址", EndId.ToString(), "");
                    int j = 34;//长度加两个地址长度
                    int Sector = 1;
                    while (j < blocklen)
                    {

                        num = "数据区" + Sector.ToString();
                        AddtoList("1", num, "", "");

                        Sector++;
                        //解析数据区
                        int sectorId = GetIntValueFromBit(8);
                        if (sectorId == 0)//结束标识
                        {
                            AddtoList("2", (string)ACNWebHashtableID[sectorId], sectorId.ToString(), "结束");
                            j += 8;//只有8bit长
                        }
                        else
                        {
                            //
                            AddtoList("2", "ID", sectorId.ToString(), (string)ACNWebHashtableID[sectorId]);

                            int len = GetIntValueFromBit(12);

                            j += len;
                            AddtoList("2", "数据区长", len.ToString(), "");
                            //AddtoList("2", "数据体", "", "");
                            if (len == 20)//命令
                                continue;
                            //有相同处理方法但ID不同的命令
                            switch (sectorId)
                            {
                                case 103:
                                    sectorId = 2;
                                    break;
                                case 104:
                                    sectorId = 2;
                                    break;
                                case 105:
                                    sectorId = 2;
                                    break;
                                case 106:
                                    sectorId = 2;
                                    break;
                                //case 107:
                                //    sectorId = 4;
                                //    break;
                                //case 108:
                                //    sectorId = 4;
                                //    break;
                                case 109:
                                    sectorId = 5;
                                    break;
                                case 110:
                                    sectorId = 5;
                                    break;
                                case 112:
                                    sectorId = 3;
                                    break;
                                case 113:
                                    sectorId = 6;
                                    break;
                                case 114:
                                    sectorId = 6;
                                    break;
                                case 116:
                                    sectorId = 61;
                                    break;
                                case 118:
                                    sectorId = 62;
                                    break;
                                case 122:
                                    sectorId = 63;
                                    break;
                                default:
                                    break;
                            }

                            int key;
                            switch (sectorId)
                            {

                                case 1://分段
                                    len = GetIntValueFromBit(4);
                                    AddtoList("3", "总段数", len.ToString(), "");
                                    len = GetIntValueFromBit(4);
                                    AddtoList("3", "当前段号", len.ToString(), "");
                                    break;
                                case 2:
                                    int nodes = GetIntValueFromBit(6);//节点数
                                    AddtoList("3", "节点数", nodes.ToString(), "");

                                    for (int n = 0; n < nodes; n++)
                                    {
                                        Nodeinfo ni = new Nodeinfo(GetIntValueFromBit(6), GetIntValueFromBit(1), GetIntValueFromBit(3), GetIntValueFromBit(8),
                                            GetIntValueFromBit(8), GetIntValueFromBit(3), GetIntValueFromBit(16),
                                            GetIntValueFromBit(28), GetIntValueFromBit(28), GetIntValueFromBit(14));
                                        AddtoList("3", "节点信息" + (n + 1).ToString(), "", "");

                                        AddtoList("4", "节点ID", ni.NodeId.ToString(), ni.NodeID);

                                        AddtoList("4", "节点类型", ni.NodeType.ToString(), ni.MoveType);

                                        AddtoList("4", "节点接收换能器个数", ni.RecvNum.ToString(), ni.Receiver);

                                        AddtoList("4", "节点外挂设备1类型", ni.Set1type.ToString(), ni.Set1Type);

                                        AddtoList("4", "节点外挂设备2类型", ni.Set2type.ToString(), ni.Set2Type);

                                        AddtoList("4", "剩余能量", ni.NodePower.ToString(), ni.NodePW);

                                        AddtoList("4", "通信制式", ni.CommType.ToString(), ni.Type);

                                        AddtoList("4", "经度", ni.Lang.ToString(), ni.Langtude);

                                        AddtoList("4", "纬度", ni.Lat.ToString(), ni.Latitude);

                                        AddtoList("4", "深度", ni.depth.ToString(), ni.Depth);


                                    }
                                    break;
                                case 3:
                                    nodes = GetIntValueFromBit(4);//邻节点数
                                    AddtoList("3", "邻节点数", nodes.ToString(), "");
                                    for (int n = 0; n < nodes; n++)
                                    {
                                        NeiborNodeinfo ni = new NeiborNodeinfo(GetIntValueFromBit(6), GetIntValueFromBit(16), GetIntValueFromBit(2));
                                        AddtoList("3", "邻节点信息" + (n + 1).ToString(), "", "");
                                        AddtoList("4", "邻节点ID", ni.NodeId.ToString(), ni.NodeID.ToString());
                                        AddtoList("4", "距离", ni.Nodedist.ToString(), ni.Distance.ToString());
                                        AddtoList("4", "评价", ni.ChannelEstimate.ToString(), ni.ChanEsti);
                                    }
                                    break;
                                case 4://主动上报网络表，重新路由
                                    int routers = GetIntValueFromBit(8);//路径条数
                                    Hashtable nodelist = new Hashtable();
                                    Hashtable distlist = new Hashtable();
                                    AddtoList("3", "路径条数", routers.ToString(), "");
                                    for (int n = 0; n < routers; n++)
                                    {
                                        NetworkList ni = new NetworkList(GetIntValueFromBit(6), GetIntValueFromBit(6),
                                            GetIntValueFromBit(16), GetIntValueFromBit(2));
                                        AddtoList("3", "路径信息" + (n + 1).ToString(), "", "");
                                        AddtoList("4", "源节点", ni.SourceNodeId.ToString(), ni.SourceNodeID.ToString());
                                        AddtoList("4", "目标节点", ni.DestinNodeId.ToString(), ni.DestinationNodeID.ToString());
                                        AddtoList("4", "路径距离", ni.Nodedist.ToString(), ni.Distance.ToString());
                                        AddtoList("4", "信道评价", ni.ChannelEstimate.ToString(), ni.ChanEsti);
                                    }
                                    break;
                                case 108://被动返回网络表
                                    routers = GetIntValueFromBit(8);//路径条数
                                    AddtoList("3", "路径条数", routers.ToString(), "");
                                    for (int n = 0; n < routers; n++)
                                    {
                                        NetworkList ni = new NetworkList(GetIntValueFromBit(6), GetIntValueFromBit(6), GetIntValueFromBit(16), GetIntValueFromBit(2));
                                        AddtoList("3", "路径信息" + (n + 1).ToString(), "", "");
                                        AddtoList("4", "源节点", ni.SourceNodeId.ToString(), ni.SourceNodeID.ToString());
                                        AddtoList("4", "目标节点", ni.DestinNodeId.ToString(), ni.DestinationNodeID.ToString());
                                        AddtoList("4", "路径距离", ni.Nodedist.ToString(), ni.Distance.ToString());
                                        AddtoList("4", "信道评价", ni.ChannelEstimate.ToString(), ni.ChanEsti);
                                    }
                                    break;
                                case 5://网络简表
                                    routers = GetIntValueFromBit(8);//路由条数
                                    AddtoList("3", "路由条数", routers.ToString(), "");
                                    for (int n = 0; n < routers; n++)
                                    {
                                        var SourceNodeId = GetIntValueFromBit(6);
                                        var DestNodeId = GetIntValueFromBit(6);
                                        AddtoList("3", "路由信息" + (n + 1).ToString(), "", "");
                                        AddtoList("4", "源节点", SourceNodeId.ToString(), SourceNodeId.ToString());
                                        AddtoList("4", "目标节点", DestNodeId.ToString(), DestNodeId.ToString());

                                    }
                                    break;
                                case 6:
                                    routers = GetIntValueFromBit(6);//路由条数
                                    AddtoList("3", "路由条数", routers.ToString(), "");
                                    for (int n = 0; n < routers; n++)
                                    {
                                        RourteList ni = new RourteList(GetIntValueFromBit(6), GetIntValueFromBit(6), GetIntValueFromBit(4),
                                            GetIntValueFromBit(15), GetIntValueFromBit(2));
                                        AddtoList("3", "路由信息" + (n + 1).ToString(), "", "");
                                        AddtoList("4", "目标节点", ni.DNodeId.ToString(), ni.DestinationNodeID.ToString());
                                        AddtoList("4", "下一跳地址", ni.NextNodeId.ToString(), ni.NextNodeID.ToString());
                                        AddtoList("4", "跳数", ni.Hops.ToString(), ni.Hop.ToString());
                                        AddtoList("4", "目标节点序列号", ni.DestSerial.ToString(), ni.Serial.ToString());
                                        AddtoList("4", "路由状态", ni.RouteStatus.ToString(), ni.Status);
                                    }
                                    break;
                                case 7:
                                    nodes = (len - 26) / 6;//记录条数
                                    routers = GetIntValueFromBit(6);
                                    AddtoList("3", "起始地址", routers.ToString(), "");

                                    for (int n = 0; n < nodes; n++)
                                    {
                                        routers = GetIntValueFromBit(6);
                                        AddtoList("3", "节点ID" + (n + 1).ToString(), routers.ToString(), "");

                                    }

                                    break;
                                case 8:
                                    nodes = (len - 32) / 6;//记录条数
                                    int sourceID = GetIntValueFromBit(6);
                                    AddtoList("3", "起始地址", sourceID.ToString(), "节点" + sourceID.ToString());
                                    for (int n = 0; n < nodes; n++)
                                    {
                                        AddtoList("3", "节点ID" + (n + 1).ToString(), "节点" + GetIntValueFromBit(6).ToString(), "");
                                    }
                                    int dest = GetIntValueFromBit(6);
                                    AddtoList("3", "目的地址", dest.ToString(), "节点" + dest.ToString());
                                    break;
                                case 9:
                                    //nodes = (len - 20) / 12;//记录条数
                                    int sourceid, destid;
                                    AddtoList("3", "路径中断信息", "", "");
                                    sourceid = GetIntValueFromBit(6);
                                    destid = GetIntValueFromBit(6);
                                    AddtoList("4", "起始地址", sourceid.ToString(), sourceid.ToString());
                                    AddtoList("4", "目的地址", destid.ToString(), destid.ToString());
                                    //for (int n = 0; n < nodes; n++)
                                    //{
                                    //    AddtoList("3", "路径中断信息" + (n + 1).ToString(), "", "");
                                    //    sourceid = GetIntValueFromBit(6);
                                    //    destid = GetIntValueFromBit(6);
                                    //    AddtoList("4", "起始地址", sourceid.ToString(), sourceid.ToString());
                                    //    AddtoList("4", "目的地址", destid.ToString(), destid.ToString());
                                    //}

                                    break;
                                case 10:
                                    //nodes = (len - 20) / 16;//失败记录条数
                                    int blockd, source, errnum;

                                    AddtoList("3", "转发失败信息", "", "");
                                    blockd = GetIntValueFromBit(10);
                                    source = GetIntValueFromBit(6);
                                    errnum = GetIntValueFromBit(3);
                                    AddtoList("4", "块标识", blockd.ToString(), blockd.ToString());
                                    AddtoList("4", "起始源地址", source.ToString(), source.ToString());
                                    AddtoList("4", "失败次数", errnum.ToString(), errnum.ToString());
                                    break;
                                case 61:
                                    if (len > 20)
                                    {
                                        int ID = GetIntValueFromBit(8);
                                        AddtoList("3", "设备类型", ID.ToString(), Enum.GetName(typeof(DeviceAddr), ID));

                                        int comm = GetIntValueFromBit(8);
                                        AddtoList("3", "COM端口", comm.ToString(), "");

                                        switch (ID)
                                        {
                                            case 16:
                                                int Type = GetIntValueFromBit(8);
                                                switch (Type)
                                                {
                                                    case 0x54://"T"
                                                        AddtoList("3", "状态", "TimeOut", "设备超时");
                                                        GetIntValueFromBit(32);
                                                        GetIntValueFromBit(16);//跳过48个bit
                                                        break;
                                                    case 0x4E://"N"
                                                        AddtoList("3", "状态", "No Device", "无设备");
                                                        GetIntValueFromBit(32);
                                                        GetIntValueFromBit(32);//跳过64个bit
                                                        break;
                                                    default:
                                                        index -= 8;//后退8个bit
                                                        byte[] b1 = GetByteValueFromBit(len - 36);
                                                        AddtoList("3", "数据回复", StringHexConverter.ConvertCharToHex(b1, b1.Length), Encoding.Default.GetString(b1));

                                                        break;
                                                }
                                                break;
                                            case 17:
                                                Type = GetIntValueFromBit(8);
                                                switch (Type)
                                                {
                                                    case 0x54://"T"
                                                        AddtoList("3", "状态", "TimeOut", "设备超时");

                                                        GetIntValueFromBit(32);
                                                        GetIntValueFromBit(16);//跳过48个bit
                                                        break;
                                                    case 0x4E://"N"
                                                        AddtoList("3", "状态", "No Device", "无设备");

                                                        GetIntValueFromBit(32);
                                                        GetIntValueFromBit(32);//跳过64个bit
                                                        break;
                                                    default:
                                                        index -= 8;//后退8个bit
                                                        byte[] b2 = GetByteValueFromBit(len - 36);
                                                        AddtoList("3", "设备数据", StringHexConverter.ConvertCharToHex(b2, b2.Length), Encoding.Default.GetString(b2));

                                                        break;
                                                }
                                                break;
                                            case 18:
                                                Type = GetIntValueFromBit(8);
                                                switch (Type)
                                                {
                                                    case 0x54://"T"
                                                        AddtoList("3", "状态", "TimeOut", "设备超时");

                                                        GetIntValueFromBit(32);
                                                        GetIntValueFromBit(16);//跳过48个bit
                                                        break;
                                                    case 0x4E://"N"
                                                        AddtoList("3", "状态", "No Device", "无设备");

                                                        GetIntValueFromBit(32);
                                                        GetIntValueFromBit(32);//跳过64个bit
                                                        break;
                                                    default:
                                                        index -= 8;//后退8个bit
                                                        AddtoList("3", "AUV_CTD采样数据", "", "");

                                                        AddtoList("3", "数据体", "", "");
                                                        var temp = (Int16)((GetIntValueFromBit(8) << 8) + GetIntValueFromBit(8));
                                                        AddtoList("4", "温度", temp.ToString(), (((double)temp) / 1000).ToString());

                                                        int daolv = (GetIntValueFromBit(8) << 8) + GetIntValueFromBit(8);
                                                        AddtoList("4", "电导率", daolv.ToString(), daolv.ToString() + "μS/cm");

                                                        int pressure = (GetIntValueFromBit(8) << 8) + GetIntValueFromBit(8);
                                                        AddtoList("4", "压力", pressure.ToString(), (((double)pressure) / 1000).ToString() + "MPa");


                                                        break;
                                                }
                                                break;
                                            case 19:
                                                Type = GetIntValueFromBit(8);
                                                switch (Type)
                                                {
                                                    case 0x54://"T"
                                                        AddtoList("3", "状态", "TimeOut", "设备超时");

                                                        GetIntValueFromBit(32);
                                                        GetIntValueFromBit(16);//跳过48个bit
                                                        break;
                                                    case 0x4E://"N"
                                                        AddtoList("3", "状态", "No Device", "无设备");

                                                        GetIntValueFromBit(32);
                                                        GetIntValueFromBit(32);//跳过64个bit
                                                        break;
                                                    default:
                                                        index -= 8;//后退8个bit

                                                        AddtoList("3", "ADCP数据", "", "");
                                                        string timestr = "20" + GetIntValueFromBit(8) + " " + GetIntValueFromBit(8) + "-" + GetIntValueFromBit(8)
                                                             + " " + GetIntValueFromBit(8) + ":" + GetIntValueFromBit(8) + ":" + GetIntValueFromBit(8);
                                                        AddtoList("4", "系统时间", timestr, "");
                                                        int layers = GetIntValueFromBit(8);
                                                        AddtoList("4", "层数", layers.ToString(), layers.ToString());
                                                        int thickness = GetIntValueFromBit(16);
                                                        AddtoList("4", "层厚", thickness.ToString(), thickness.ToString());
                                                        for (int a = 0; a < layers; a++)
                                                        {
                                                            AddtoList("4", "层" + (a + 1).ToString() + "流速", "", "");
                                                            for (int layer = 0; layer < 4; layer++)
                                                            {
                                                                AddtoList("5", "流速" + (layer + 1).ToString(), GetIntValueFromBit(16).ToString(), "");
                                                            }
                                                        }
                                                        for (int a = 0; a < layers; a++)
                                                        {
                                                            AddtoList("4", "层" + (a + 1).ToString() + "回波强度", "", "");
                                                            for (int layer = 0; layer < 4; layer++)
                                                            {
                                                                AddtoList("5", "回波强度" + (layer + 1).ToString(), GetIntValueFromBit(8).ToString(), "");
                                                            }
                                                        }
                                                        for (int layer = 0; layer < 4; layer++)
                                                        {
                                                            AddtoList("4", "测底距离" + (layer + 1).ToString(), GetIntValueFromBit(16).ToString(), "");
                                                        }
                                                        for (int layer = 0; layer < 4; layer++)
                                                        {
                                                            AddtoList("4", "底速" + (layer + 1).ToString(), GetIntValueFromBit(16).ToString(), "");
                                                        }
                                                        break;
                                                }
                                                break;
                                            default:
                                                byte[] b = GetByteValueFromBit(len - 36);
                                                AddtoList("3", "数据内容", StringHexConverter.ConvertCharToHex(b, b.Length), Encoding.Default.GetString(b));//减去头和长度域

                                                break;
                                        }
                                    }
                                    break;
                                case 62:
                                    int deviceid = GetIntValueFromBit(8);
                                    AddtoList("3", "设备类型", deviceid.ToString(), Enum.GetName(typeof(DeviceAddr), deviceid));

                                    int com = GetIntValueFromBit(8);
                                    AddtoList("3", "COM端口", com.ToString(), "COM" + com.ToString());

                                    switch (deviceid)
                                    {
                                        case 101:
                                            AddtoList("3", "AUV运行状态", "", "");
                                            int Type = GetIntValueFromBit(8);
                                            switch (Type)
                                            {
                                                case 0x54://"T"
                                                    AddtoList("3", "状态", "TimeOut", "设备超时");
                                                    GetIntValueFromBit(32);
                                                    GetIntValueFromBit(16);//跳过48个bit
                                                    break;
                                                case 0x4E://"N"
                                                    AddtoList("3", "状态", "No Device", "无设备");
                                                    GetIntValueFromBit(32);
                                                    GetIntValueFromBit(32);//跳过64个bit
                                                    break;
                                                default:
                                                    int depth = (Type << 8) + GetIntValueFromBit(8);

                                                    AddtoList("4", "深度", depth.ToString(), ((double)depth / 10).ToString() + "米");
                                                    depth = (GetIntValueFromBit(8) << 8) + GetIntValueFromBit(8);
                                                    AddtoList("4", "高度", depth.ToString(), ((double)depth / 100).ToString() + "米");
                                                    AddtoList("4", "位置", "", "");
                                                    int byte1 = GetIntValueFromBit(8);
                                                    int byte2 = GetIntValueFromBit(8);
                                                    int byte3 = GetIntValueFromBit(8);
                                                    int byte4 = GetIntValueFromBit(8);
                                                    int lat = (byte1 << 24) + (byte2 << 16) + (byte3 << 8) + byte4;
                                                    if (lat >= 0)
                                                        AddtoList("5", "北纬", lat.ToString(), ((double)lat / 10000 / 60).ToString() + "度");
                                                    else
                                                        AddtoList("5", "南纬", lat.ToString(), ((double)-lat / 10000 / 60).ToString() + "度");
                                                    ///
                                                    int lng = (GetIntValueFromBit(8) << 24) + (GetIntValueFromBit(8) << 16) + (GetIntValueFromBit(8) << 8) + GetIntValueFromBit(8);
                                                    if (lng >= 0)
                                                        AddtoList("5", "东经", lng.ToString(), ((double)lng / 10000 / 60).ToString() + "度");
                                                    else
                                                        AddtoList("5", "西经", lng.ToString(), ((double)-lng / 10000 / 60).ToString() + "度");

                                                    int v = (GetIntValueFromBit(8) << 8) + GetIntValueFromBit(8);
                                                    Int16 velocity = (Int16)v;
                                                    if (velocity >= 0)
                                                        AddtoList("4", "航速", velocity.ToString(), "前进" + (((double)velocity) / 1000).ToString() + "节");
                                                    else
                                                        AddtoList("4", "航速", velocity.ToString(), "后退" + (-((double)velocity) / 1000).ToString() + "节");
                                                    AddtoList("4", "电池数据", "", "");
                                                    int auvvol = GetIntValueFromBit(8);
                                                    AddtoList("5", "AUV动力电压", auvvol.ToString(), auvvol.ToString() + "V");
                                                    int AuvI = (GetIntValueFromBit(8) << 8) + GetIntValueFromBit(8);

                                                    AddtoList("5", "电流", AuvI.ToString(), AuvI.ToString() + "mA");
                                                    int vleft = GetIntValueFromBit(8);
                                                    AddtoList("5", "剩余电量", vleft.ToString(), vleft.ToString() + "%");
                                                    int alarm = GetIntValueFromBit(8);
                                                    if (alarm == 0)
                                                        AddtoList("4", "无报警", "", "");
                                                    else
                                                    {
                                                        AddtoList("4", "AUV报警", "", "");
                                                        if ((alarm & 0x01) == 1)
                                                        {
                                                            AddtoList("5", "漏水报警", "", "");
                                                        }
                                                        if ((alarm & 0x02) == 1)
                                                        {
                                                            AddtoList("5", "温度报警", "", "");
                                                        }
                                                        if ((alarm & 0x04) == 1)
                                                        {
                                                            AddtoList("5", "低压报警", "", "");
                                                        }
                                                        if ((alarm & 0x08) == 1)
                                                        {
                                                            AddtoList("5", "高度报警", "", "");
                                                        }
                                                        if ((alarm & 0x10) == 1)
                                                        {
                                                            AddtoList("5", "深度报警", "", "");
                                                        }
                                                        if ((alarm & 0x20) == 1)
                                                        {
                                                            AddtoList("5", "障碍报警", "", "");
                                                        }
                                                        if ((alarm & 0x40) == 1)
                                                        {
                                                            AddtoList("5", "保留", "", "");
                                                        }
                                                        if ((alarm & 0x80) == 1)
                                                        {
                                                            AddtoList("5", "保留", "", "");
                                                        }

                                                    }
                                                    ///attitude
                                                    int Roll = (GetIntValueFromBit(8) << 8) + GetIntValueFromBit(8);
                                                    AddtoList("4", "姿态角", "", "");
                                                    AddtoList("5", "横滚角", Roll.ToString(), ((double)Roll / 100).ToString() + "度");
                                                    int pitch = (GetIntValueFromBit(8) << 8) + GetIntValueFromBit(8);

                                                    AddtoList("5", "纵倾角", pitch.ToString(), ((double)pitch / 100).ToString() + "度");
                                                    int head = (GetIntValueFromBit(8) << 8) + GetIntValueFromBit(8);

                                                    AddtoList("5", "航向角", head.ToString(), ((double)head / 100).ToString() + "度");
                                                    //转速

                                                    int fanspeed = (GetIntValueFromBit(8) << 8) + GetIntValueFromBit(8);
                                                    if (fanspeed >= 0)
                                                        AddtoList("4", "正转", fanspeed.ToString(), fanspeed.ToString() + "转");
                                                    else
                                                        AddtoList("4", "反转", fanspeed.ToString(), (-fanspeed).ToString() + "转");
                                                    ///
                                                    break;
                                            }
                                            break;

                                        default:
                                            byte[] statusb = GetByteValueFromBit(len - 36);
                                            AddtoList("3", "状态数据", StringHexConverter.ConvertCharToHex(statusb, statusb.Length), Encoding.Default.GetString(statusb));

                                            break;

                                    }

                                    break;
                                case 63:
                                    int id = GetIntValueFromBit(8);
                                    AddtoList("3", "通信机类型", id.ToString(), Enum.GetName(typeof(NodeType), id));

                                    AddtoList("3", "状态数据", "", "");

                                    if (id == 0)
                                    {
                                        string timestr = "20" + GetIntValueFromBit(8) + " " + GetIntValueFromBit(8) + "-" + GetIntValueFromBit(8)
                                                            + " " + GetIntValueFromBit(8) + ":" + GetIntValueFromBit(8) + ":" + GetIntValueFromBit(8);
                                        AddtoList("4", "系统时间", timestr, "");


                                        int nn = GetIntValueFromBit(16);
                                        AddtoList("4", "3.3V电压", nn.ToString(), ((double)nn / 100).ToString() + "V");

                                        nn = GetIntValueFromBit(16);
                                        AddtoList("4", "48V电压", nn.ToString(), ((double)nn / 100).ToString() + "V");

                                        nn = GetIntValueFromBit(32);
                                        AddtoList("4", "3.3V剩余电量", nn.ToString(), ((double)nn / 100).ToString() + "mA*h");

                                        nn = GetIntValueFromBit(32);
                                        AddtoList("4", "48V剩余电量", nn.ToString(), ((double)nn / 100).ToString() + "mA*h");

                                        nn = GetIntValueFromBit(16);
                                        AddtoList("4", "温度", nn.ToString(), ((double)nn / 100).ToString() + "°C");

                                        nn = GetIntValueFromBit(8);
                                        AddtoList("4", "漏水", nn.ToString(), (nn == 1) ? "漏水啦！" : "无漏水");

                                        if (len == 212)
                                        {
                                            nn = GetIntValueFromBit(1);
                                            AddtoList("4", "发射自动调节开关", nn.ToString(), (nn == 1) ? "自动调节" : "固定");

                                            nn = GetIntValueFromBit(7);
                                            AddtoList("4", "发射幅度设置", nn.ToString(), ((double)nn / 100).ToString());

                                            nn = GetIntValueFromBit(1);
                                            AddtoList("4", "接收自动调节开关", nn.ToString(), (nn == 1) ? "自动调节" : "固定");

                                            nn = GetIntValueFromBit(7);
                                            AddtoList("4", "接收增益设置", nn.ToString(), nn.ToString() + "dB");


                                        }
                                    }
                                    else
                                    {
                                        byte[] b = GetByteValueFromBit(len - 28);
                                        AddtoList("4", "通信机状态", StringHexConverter.ConvertCharToHex(b, b.Length), "");//减去头和长度域

                                    }

                                    break;
                                case 101:
                                    byte[] tb = GetByteValueFromBit(len - 20);
                                    AddtoList("3", "回环数据（16进制）", StringHexConverter.ConvertCharToHex(tb, tb.Length), Encoding.Default.GetString(tb));
                                    break;
                                case 102:
                                    byte[] bb = GetByteValueFromBit(len - 20);

                                    AddtoList("3", "应答回环数据（16进制）", StringHexConverter.ConvertCharToHex(bb, bb.Length), Encoding.Default.GetString(bb));

                                    break;
                                case 107:
                                    AddtoList("3", "命令", (string)ACNWebHashtableID[sectorId], "");
                                    key = GetIntValueFromBit(1);
                                    AddtoList("3", "索取网络表开关", key.ToString(), (key.ToString() == "1") ? "重建网络表" : "直接返回当前网络表");
                                    break;
                                case 111:
                                    AddtoList("3", "命令", (string)ACNWebHashtableID[sectorId], "");
                                    key = GetIntValueFromBit(1);
                                    AddtoList("3", "重建邻节点表开关", key.ToString(), (key.ToString() == "1") ? "重建邻节点表" : "直接返回当前邻节点表");
                                    break;

                                case 115://比一般命令多一个comm
                                    //AddtoList("3", "命令", (string)WebId[sectorId], "");
                                    AddtoList("3", "COM端口", GetIntValueFromBit(8).ToString(), "");
                                    break;
                                case 117:
                                    //AddtoList("3", "命令", (string)WebId[sectorId], "");
                                    AddtoList("3", "COM端口", GetIntValueFromBit(8).ToString(), "");
                                    break;
                                case 119:
                                    //AddtoList("3", "命令", (string)WebId[sectorId], "");
                                    AddtoList("3", "COM端口", GetIntValueFromBit(8).ToString(), "");
                                    byte[] parab = GetByteValueFromBit(len - 28);
                                    AddtoList("3", "设置参数", StringHexConverter.ConvertCharToHex(parab, parab.Length), Encoding.Default.GetString(parab));
                                    break;
                                case 120:
                                    //AddtoList("3", "命令", (string)WebId[sectorId], "");
                                    int DeviceId = GetIntValueFromBit(8);
                                    AddtoList("3", "设备类型", DeviceId.ToString(), Enum.GetName(typeof(DeviceAddr), DeviceId));

                                    int mn = GetIntValueFromBit(8);
                                    AddtoList("3", "COM端口", mn.ToString(), "");

                                    byte[] devicestatus = GetByteValueFromBit(len - 20 - 8 - 8);
                                    AddtoList("3", "响应", StringHexConverter.ConvertCharToHex(devicestatus, devicestatus.Length), Encoding.Default.GetString(devicestatus));

                                    break;
                                case 140:
                                    //AddtoList("3", "命令", (string)WebId[sectorId], "");
                                    mn = GetIntValueFromBit(32);
                                    AddtoList("3", "设备数据定时回传间隔", mn.ToString(), "");

                                    break;
                                case 141:
                                    AddtoList("3", "发射自动调节开关", GetIntValueFromBit(1).ToString(), "");
                                    AddtoList("3", "发射幅度设置", GetIntValueFromBit(7).ToString(), "");
                                    AddtoList("3", "接收自动调节开关", GetIntValueFromBit(1).ToString(), "");
                                    AddtoList("3", "接收增益设置", GetIntValueFromBit(7).ToString(), "");
                                    break;
                                case 142:
                                    int CommType = GetIntValueFromBit(16);
                                    string comstr = "";
                                    byte[] bbb = BitConverter.GetBytes((short)CommType);
                                    BitArray ba = new BitArray(bbb);
                                    for (int ij = 0; ij < ba.Count; ij++)
                                    {
                                        comstr += ba[ij] ? "1" : "0";
                                    }
                                    comstr.PadRight(16, '0');
                                    AddtoList("3", "通信制式开关", comstr, "");

                                    break;
                                default:
                                    if (len > 20)
                                    {
                                        byte[] b = GetByteValueFromBit(len - 20);
                                        AddtoList("3", "数据", StringHexConverter.ConvertCharToHex(b, b.Length), Encoding.Default.GetString(b));//减去头和长度域

                                    }
                                    else
                                    {
                                        AddtoList("3", "命令", (string)ACNWebHashtableID[sectorId], "");

                                    }
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Errormessage = e.Message + ":" + e.StackTrace;
                return false;
            }
            return true;
        }

        #region 读取bit流方法
        /// <summary>
        /// 从数组中读取一定长度的bit转成32bit的整型数
        /// </summary>
        /// <param name="bitlen">读取的bit长度</param>
        /// <returns>返回32bit的整型数</returns>
        static public int GetIntValueFromBit(int bitlen)
        {
            int[] value = new int[1];
            BitArray ba = new BitArray(bitlen);
            for (int i = 0; i < bitlen; i++)
            {
                ba[i] = data[index + i];
            }
            index += bitlen;

            ba.CopyTo(value, 0);
            return value[0];
        }

        /// <summary>
        /// 从数组中读取一定长度的bit转成byte数组
        /// </summary>
        /// <param name="bitlen">读取的bit长度</param>
        /// <returns>返回byte数组</returns>
        static public Byte[] GetByteValueFromBit(int bitlen)
        {
            Byte[] value = new Byte[(int)Math.Ceiling((double)bitlen / 8)];
            Array.Clear(value, 0, (int)Math.Ceiling((double)bitlen / 8));
            BitArray ba = new BitArray(bitlen);
            for (int i = 0; i < bitlen; i++)
            {
                ba[i] = data[index + i];
            }
            index += bitlen;

            ba.CopyTo(value, 0);
            return value;
        }
        /// <summary>
        /// 从数组中读取一定长度的bit转成32bit的整型数16进制字符串
        /// </summary>
        /// <param name="bitlen">读取的bit长度</param>
        /// <returns>返回8bit的整型数16进制字符串</returns>
        static public string GetHexValueFromBit(int bitlen)
        {
            string s = Convert.ToString(GetIntValueFromBit(bitlen), 16);
            if (s.Length == 1)
            {
                s = "0" + s;
            }
            return s;
        }

        /// <summary>
        /// 从数组中读取一定长度的bit转成字符串
        /// </summary>
        /// <param name="bitlen">读取的bit长度</param>
        /// <returns>返回字符串</returns>
        static public string GetASCValueFromBit(int bitlen)
        {
            byte[] b = GetByteValueFromBit(bitlen);
            string str = Encoding.Default.GetString(b);
            return str;
        }
        #endregion

        #region 写入比特流方法

        /// <summary>
        /// 将int型数据写入当前bit流
        /// </summary>
        /// <param name="?"></param>
        static public void OutPutIntBit(int[] dat, int bitlen)
        {
            BitArray ba;

            ba = new BitArray(dat);
            for (int j = 0; j < bitlen; j++)
            {
                packdata[packindex + j] = ba[j];
            }
            packindex += bitlen;
        }

        /// <summary>
        /// 将bit流写入当前比特流之后
        /// </summary>
        static public void OutPutArrayBit(BitArray ba)
        {
            for (int j = 0; j < ba.Length; j++)
            {
                packdata[packindex + j] = ba[j];
            }
            packindex += ba.Length;
        }

        #endregion
        #endregion
    }
}
