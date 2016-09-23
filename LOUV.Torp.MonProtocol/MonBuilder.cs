using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using TinyMetroWpfLibrary.Utility;
using LOUV.Torp.BaseType;
namespace LOUV.Torp.MonP
{
    /// <summary>
    /// 构建Monitor命令类
    /// </summary>
    public class MonBuilder
    {
        public static void Pack002(int ID, Hashtable NodeInfo)
        {
            int nodenum = NodeInfo.Keys.Count;
            int[] dat = new int[1];
            MonProtocol.InitForPack(nodenum * 115 + 6 + 20);
            dat[0] = 2;
            MonProtocol.OutPutIntBit(dat, 8);
            dat[0] = nodenum * 115 + 6 + 20;
            MonProtocol.OutPutIntBit(dat, 12);
            dat[0] = nodenum;
            MonProtocol.OutPutIntBit(dat, 6);//节点数
            foreach (string nodename in NodeInfo.Keys)
            {
                MonProtocol.OutPutArrayBit((BitArray)NodeInfo[nodename]);
            }
            MonProtocol.AddPool(ID);
            List<string> IDLst = new List<string>();
            IDLst.Add(MonProtocol.SourceID.ToString());
            if (MonProtocol.bUseTrack)
            {
                
                if (MonProtocol.TrackNodeList.Count > 0)
                {
                    IDLst.AddRange(MonProtocol.TrackNodeList); 
                }

            }
            IDLst.Add(ID.ToString());
            Pack008(ID, IDLst);
        }

        public static void Pack003(int ID, List<NeiborNodeinfo> NeiborNodeLst)
        {
            int[] dat = new int[1];
            int nodenum = NeiborNodeLst.Count;
            MonProtocol.InitForPack(nodenum * 24 + 4 + 20);
            dat[0] = 3;
            MonProtocol.OutPutIntBit(dat, 8);
            dat[0] = nodenum * 24 + 4 + 20;
            MonProtocol.OutPutIntBit(dat, 12);
            dat[0] = nodenum;
            MonProtocol.OutPutIntBit(dat, 4);//邻节点数
            for (int i = 0; i < nodenum; i++)
            {
                dat[0] = NeiborNodeLst[i].NodeID;
                MonProtocol.OutPutIntBit(dat, 6);//邻节点
                dat[0] = (int)NeiborNodeLst[i].Distance*5;
                MonProtocol.OutPutIntBit(dat, 16);//距离
                dat[0] = NeiborNodeLst[i].ChannelEstimate;
                MonProtocol.OutPutIntBit(dat, 2);//评价
            }
            MonProtocol.AddPool(ID);
            List<string> IDLst = new List<string>();
            IDLst.Add(MonProtocol.SourceID.ToString());
            if (MonProtocol.bUseTrack)
            {

                if (MonProtocol.TrackNodeList.Count > 0)
                {
                    IDLst.AddRange(MonProtocol.TrackNodeList);
                }

            }
            IDLst.Add(ID.ToString());
            Pack008(ID, IDLst);
        }

        public static void Pack004(int ID, List<NetworkList> NetLst)//ID=0 广播
        {
            int[] dat = new int[1];
            int nodenum = NetLst.Count;
            MonProtocol.InitForPack(nodenum * 24 + 4 + 20);
            dat[0] = 3;
            MonProtocol.OutPutIntBit(dat, 8);
            dat[0] = nodenum * 30 + 4 + 20;
            MonProtocol.OutPutIntBit(dat, 12);
            dat[0] = nodenum;
            MonProtocol.OutPutIntBit(dat, 4);
            for (int i = 0; i < nodenum; i++)
            {
                dat[0] = NetLst[i].SourceNodeID;
                MonProtocol.OutPutIntBit(dat, 6);
                dat[0] = NetLst[i].DestinationNodeID;
                MonProtocol.OutPutIntBit(dat, 6);
                dat[0] = (int)NetLst[i].Distance*5;
                MonProtocol.OutPutIntBit(dat, 16);//距离
                dat[0] = NetLst[i].ChannelEstimate;
                MonProtocol.OutPutIntBit(dat, 2);//评价
            }
            MonProtocol.AddPool(ID);
            if(ID==0)
                return;
            List<string> IDLst = new List<string>();
            IDLst.Add(MonProtocol.SourceID.ToString());
            if (MonProtocol.bUseTrack)
            {

                if (MonProtocol.TrackNodeList.Count > 0)
                {
                    IDLst.AddRange(MonProtocol.TrackNodeList);
                }

            }
            IDLst.Add(ID.ToString());
            Pack008(ID, IDLst);
        }

        public static void Pack006(int ID,List<RourteList> rourte)
        {
            int nodenum = rourte.Count;
            int[] dat = new int[1];
            if (rourte.Count > 0)
            {
                MonProtocol.InitForPack(nodenum*33 + 6 + 20);
                dat[0] = 6;
                MonProtocol.OutPutIntBit(dat, 8);
                dat[0] = nodenum*33 + 6 + 20;
                MonProtocol.OutPutIntBit(dat, 12);
                dat[0] = nodenum;
                MonProtocol.OutPutIntBit(dat, 6); //路由条数
                for (int i = 0; i < nodenum; i++)
                {
                    dat[0] = rourte[i].DestinationNodeID;
                    MonProtocol.OutPutIntBit(dat, 6); //目标节点
                    dat[0] = rourte[i].NextNodeID;
                    MonProtocol.OutPutIntBit(dat, 6); //下一跳地址
                    dat[0] = rourte[i].Hop; //跳数
                    MonProtocol.OutPutIntBit(dat, 4); //跳数
                    dat[0] = rourte[i].DestSerial;
                    MonProtocol.OutPutIntBit(dat, 15);
                    dat[0] = rourte[i].RouteStatus;
                    MonProtocol.OutPutIntBit(dat, 2);
                }
                MonProtocol.AddPool(ID);
                List<string> IDLst = new List<string>();
                IDLst.Add(MonProtocol.SourceID.ToString());
                if (MonProtocol.bUseTrack)
                {

                    if (MonProtocol.TrackNodeList.Count > 0)
                    {
                        IDLst.AddRange(MonProtocol.TrackNodeList);
                    }

                }
                IDLst.Add(ID.ToString());
                Pack008(ID, IDLst);
            }
        }

        //路径安排
        public static void Pack008(int ID,List<string> nodename)
        {
            int[] dat = new int[1];
            int nodenum = nodename.Count;
            MonProtocol.InitForPack(nodenum * 6 + 20);
            dat[0] = 8;
            MonProtocol.OutPutIntBit(dat, 8);
            dat[0] = nodenum * 6 + 20;
            MonProtocol.OutPutIntBit(dat, 12);
            for (int i = 0; i < nodenum; i++)
            {
                dat[0] = int.Parse(nodename[i]);
                MonProtocol.OutPutIntBit(dat, 6);//节点
            }
            MonProtocol.AddPool(ID);
        }

        public static void Ping(int ID,string txt)
        {
            int[] dat = new int[1];
            byte[] bstr = System.Text.Encoding.Default.GetBytes(txt);
            BitArray bta = new BitArray(bstr);
            MonProtocol.InitForPack(bta.Length + 20);
            int[] b = new int[1];
            b[0] = 101;
            MonProtocol.OutPutIntBit(b, 8);
            b[0] = bta.Length + 20;
            MonProtocol.OutPutIntBit(b, 12);
            MonProtocol.OutPutArrayBit(bta);
            MonProtocol.AddPool(ID);
            List<string> IDLst = new List<string>();
            IDLst.Add(MonProtocol.SourceID.ToString());
            if (MonProtocol.bUseTrack)
            {

                if (MonProtocol.TrackNodeList.Count > 0)
                {
                    IDLst.AddRange(MonProtocol.TrackNodeList);
                }

            }
            IDLst.Add(ID.ToString());
            Pack008(ID, IDLst);
        }
        public static void Pack103(int ID)
        {
            int[] dat = new int[1];
            MonProtocol.InitForPack(20);
            dat[0] = 103;
            MonProtocol.OutPutIntBit(dat, 8);
            dat[0] = 20;
            MonProtocol.OutPutIntBit(dat, 12);
            MonProtocol.AddPool(ID);
            List<string> IDLst = new List<string>();
            IDLst.Add(MonProtocol.SourceID.ToString());
            if (MonProtocol.bUseTrack)
            {

                if (MonProtocol.TrackNodeList.Count > 0)
                {
                    IDLst.AddRange(MonProtocol.TrackNodeList);
                }

            }
            IDLst.Add(ID.ToString());
            Pack008(ID, IDLst);
        }
        public static void Pack105(int ID)
        {
            int[] dat = new int[1];
            MonProtocol.InitForPack(20);
            dat[0] = 105;
            MonProtocol.OutPutIntBit(dat, 8);
            dat[0] = 20;
            MonProtocol.OutPutIntBit(dat, 12);
            MonProtocol.AddPool(ID);
            List<string> IDLst = new List<string>();
            IDLst.Add(MonProtocol.SourceID.ToString());
            if (MonProtocol.bUseTrack)
            {

                if (MonProtocol.TrackNodeList.Count > 0)
                {
                    IDLst.AddRange(MonProtocol.TrackNodeList);
                }

            }
            IDLst.Add(ID.ToString());
            Pack008(ID, IDLst);
        }
        public static void Pack107(int ID, bool Rebuild)
        {
            int[] dat = new int[1];
            MonProtocol.InitForPack(21);
            dat[0] = 107;
            MonProtocol.OutPutIntBit(dat, 8);
            dat[0] = 21;
            MonProtocol.OutPutIntBit(dat, 12);
            dat[0] = 0;//默认值
            if (Rebuild)
                dat[0] = 1;
            MonProtocol.OutPutIntBit(dat, 1);
            MonProtocol.AddPool(ID);
            List<string> IDLst = new List<string>();
            IDLst.Add(MonProtocol.SourceID.ToString());
            if (MonProtocol.bUseTrack)
            {

                if (MonProtocol.TrackNodeList.Count > 0)
                {
                    IDLst.AddRange(MonProtocol.TrackNodeList);
                }

            }
            IDLst.Add(ID.ToString());
            Pack008(ID, IDLst);
        }
        public static void Pack109(int ID)
        {
            int[] dat = new int[1];
            MonProtocol.InitForPack(20);
            dat[0] = 109;
            MonProtocol.OutPutIntBit(dat, 8);
            dat[0] = 20;
            MonProtocol.OutPutIntBit(dat, 12);
            MonProtocol.AddPool(ID);
            List<string> IDLst = new List<string>();
            IDLst.Add(MonProtocol.SourceID.ToString());
            if (MonProtocol.bUseTrack)
            {

                if (MonProtocol.TrackNodeList.Count > 0)
                {
                    IDLst.AddRange(MonProtocol.TrackNodeList);
                }

            }
            IDLst.Add(ID.ToString());
            Pack008(ID, IDLst);
        }
        public static void Pack111(int ID, bool Rebuild)
        {
            int[] dat = new int[1];
            MonProtocol.InitForPack(21);
            dat[0] = 111;
            MonProtocol.OutPutIntBit(dat, 8);
            dat[0] = 21;
            MonProtocol.OutPutIntBit(dat, 12);
            dat[0] = 0;//默认值
            if (Rebuild)
                dat[0] = 1;
            MonProtocol.OutPutIntBit(dat, 1);
            MonProtocol.AddPool(ID);
            List<string> IDLst = new List<string>();
            IDLst.Add(MonProtocol.SourceID.ToString());
            if (MonProtocol.bUseTrack)
            {

                if (MonProtocol.TrackNodeList.Count > 0)
                {
                    IDLst.AddRange(MonProtocol.TrackNodeList);
                }

            }
            IDLst.Add(ID.ToString());
            Pack008(ID, IDLst);
        }
        public static void Pack113(int ID)
        {
            int[] dat = new int[1];
            MonProtocol.InitForPack(20);
            dat[0] = 113;
            MonProtocol.OutPutIntBit(dat, 8);
            dat[0] = 20;
            MonProtocol.OutPutIntBit(dat, 12);
            MonProtocol.AddPool(ID);
            List<string> IDLst = new List<string>();
            IDLst.Add(MonProtocol.SourceID.ToString());
            if (MonProtocol.bUseTrack)
            {

                if (MonProtocol.TrackNodeList.Count > 0)
                {
                    IDLst.AddRange(MonProtocol.TrackNodeList);
                }

            }
            IDLst.Add(ID.ToString());
            Pack008(ID, IDLst);
        }
        public static void Pack115(int ID,int CommIndex)
        {
            int[] dat = new int[1];
            MonProtocol.InitForPack(28);
            dat[0] = 115;
            MonProtocol.OutPutIntBit(dat, 8);
            dat[0] = 28;
            MonProtocol.OutPutIntBit(dat, 12);
            dat[0] = 2;//默认值
            if (CommIndex != -1)
                dat[0] = CommIndex;
            MonProtocol.OutPutIntBit(dat, 8);
            MonProtocol.AddPool(ID);
            List<string> IDLst = new List<string>();
            IDLst.Add(MonProtocol.SourceID.ToString());
            if (MonProtocol.bUseTrack)
            {

                if (MonProtocol.TrackNodeList.Count > 0)
                {
                    IDLst.AddRange(MonProtocol.TrackNodeList);
                }

            }
            IDLst.Add(ID.ToString());
            Pack008(ID, IDLst);
        }
        public static void Pack117(int ID, int CommIndex)
        {
            int[] dat = new int[1];
            MonProtocol.InitForPack(28);
            dat[0] = 117;
            MonProtocol.OutPutIntBit(dat, 8);
            dat[0] = 28;
            MonProtocol.OutPutIntBit(dat, 12);
            dat[0] = 2;//默认值
            if (CommIndex != -1)
                dat[0] = CommIndex;
            MonProtocol.OutPutIntBit(dat, 8);
            MonProtocol.AddPool(ID);
            List<string> IDLst = new List<string>();
            IDLst.Add(MonProtocol.SourceID.ToString());
            if (MonProtocol.bUseTrack)
            {

                if (MonProtocol.TrackNodeList.Count > 0)
                {
                    IDLst.AddRange(MonProtocol.TrackNodeList);
                }

            }
            IDLst.Add(ID.ToString());
            Pack008(ID, IDLst);
        }

        public static void Pack119(int ID,int CommIndex, bool bHex, string str)
        {
            int[] dat = new int[1];
            if (bHex)
            {
                byte[] end = StringHexConverter.ConvertHexToChar(str);
                int len = 20 + 8 + end.Length * 8;
                MonProtocol.InitForPack(len);
                dat[0] = 119;
                MonProtocol.OutPutIntBit(dat, 8);
                dat[0] = len;
                MonProtocol.OutPutIntBit(dat, 12);
                dat[0] = 2;//默认值
                if (CommIndex == 0)
                    dat[0] = 2;
                if (CommIndex == 1)
                    dat[0] = 3;
                MonProtocol.OutPutIntBit(dat, 8);
                for (int i = 0; i < end.Length; i++)
                {
                    dat[0] = end[i];
                    MonProtocol.OutPutIntBit(dat, 8);
                }   
            }
            else
            {
                int arraylen = str.Length;//int[] 长度
                int len = 20 + 8 + arraylen * 8;
                MonProtocol.InitForPack(len);
                dat[0] = 119;
                MonProtocol.OutPutIntBit(dat, 8);
                dat[0] = len;
                MonProtocol.OutPutIntBit(dat, 12);
                dat[0] = 2;//默认值
                if (CommIndex == 0)
                    dat[0] = 2;
                if (CommIndex == 1)
                    dat[0] = 3;
                MonProtocol.OutPutIntBit(dat, 8);
                byte[] para = Encoding.Default.GetBytes(str);
                for (int i = 0; i < arraylen; i++)
                {
                    dat[0] = para[i];
                    MonProtocol.OutPutIntBit(dat, 8);
                }
            }
            MonProtocol.AddPool(ID);
            List<string> IDLst = new List<string>();
            IDLst.Add(MonProtocol.SourceID.ToString());
            if (MonProtocol.bUseTrack)
            {

                if (MonProtocol.TrackNodeList.Count > 0)
                {
                    IDLst.AddRange(MonProtocol.TrackNodeList);
                }

            }
            IDLst.Add(ID.ToString());
            Pack008(ID, IDLst);
        }
        public static void Pack121(int ID)
        {
            int[] dat = new int[1];
            MonProtocol.InitForPack(20);
            dat[0] = 121;
            MonProtocol.OutPutIntBit(dat, 8);
            dat[0] = 20;
            MonProtocol.OutPutIntBit(dat, 12);
            MonProtocol.AddPool(ID);
            List<string> IDLst = new List<string>();
            IDLst.Add(MonProtocol.SourceID.ToString());
            if (MonProtocol.bUseTrack)
            {

                if (MonProtocol.TrackNodeList.Count > 0)
                {
                    IDLst.AddRange(MonProtocol.TrackNodeList);
                }

            }
            IDLst.Add(ID.ToString());
            Pack008(ID, IDLst);
        }
        //通信制式开关
        public static void Pack142(int ID, BitArray baCommType)
        {
            int[] dat = new int[1];
            MonProtocol.Clear();
            MonProtocol.InitForPack(20 + 16);
            dat[0] = 142;
            MonProtocol.OutPutIntBit(dat, 8);
            dat[0] = 36;
            MonProtocol.OutPutIntBit(dat, 12);
            BitArray a = new BitArray(16);
            MonProtocol.OutPutArrayBit(baCommType);
            MonProtocol.AddPool(ID);
            List<string> IDLst = new List<string>();
            IDLst.Add(MonProtocol.SourceID.ToString());
            if (MonProtocol.bUseTrack)
            {

                if (MonProtocol.TrackNodeList.Count > 0)
                {
                    IDLst.AddRange(MonProtocol.TrackNodeList);
                }

            }
            IDLst.Add(ID.ToString());
            Pack008(ID, IDLst);
        }

        //设备数据定时回传开关
        public static void Pack140(int ID, int iTimePeriod)
        {
            int[] dat = new int[1];
            MonProtocol.InitForPack(20 + 32);
            dat[0] = 140;
            MonProtocol.OutPutIntBit(dat, 8);

            dat[0] = 52;
            MonProtocol.OutPutIntBit(dat, 12);

            dat[0] = iTimePeriod;
            MonProtocol.OutPutIntBit(dat, 32);
            MonProtocol.AddPool(ID);
            List<string> IDLst = new List<string>();
            IDLst.Add(MonProtocol.SourceID.ToString());
            if (MonProtocol.bUseTrack)
            {

                if (MonProtocol.TrackNodeList.Count > 0)
                {
                    IDLst.AddRange(MonProtocol.TrackNodeList);
                }

            }
            IDLst.Add(ID.ToString());
            Pack008(ID, IDLst);
        }
        //收发自动调节开关
        public static void Pack141(int ID, int EmitAmp, int ReceGain)
        {
            int[] dat = new int[1];
            MonProtocol.InitForPack(20 + 16);

            dat[0] = 141;
            MonProtocol.OutPutIntBit(dat, 8);
            dat[0] = 36;
            MonProtocol.OutPutIntBit(dat, 12);
            if (EmitAmp!=0)
            {
                dat[0] = 0;
                MonProtocol.OutPutIntBit(dat, 1);
                dat[0] = EmitAmp;
                MonProtocol.OutPutIntBit(dat, 7);
            }
            else
            {
                dat[0] = 1;
                MonProtocol.OutPutIntBit(dat, 1);
                dat[0] = 0;
                MonProtocol.OutPutIntBit(dat, 7);
            }
            if (ReceGain!=0)
            {
                dat[0] = 0;
                MonProtocol.OutPutIntBit(dat, 1);
                dat[0] = ReceGain;
                MonProtocol.OutPutIntBit(dat, 7);
            }
            else
            {
                dat[0] = 1;
                MonProtocol.OutPutIntBit(dat, 1);
                dat[0] = 0;
                MonProtocol.OutPutIntBit(dat, 7);
            }
            //加入列表
            MonProtocol.AddPool(ID);
            List<string> IDLst = new List<string>();
            IDLst.Add(MonProtocol.SourceID.ToString());
            if (MonProtocol.bUseTrack)
            {

                if (MonProtocol.TrackNodeList.Count > 0)
                {
                    IDLst.AddRange(MonProtocol.TrackNodeList);
                }

            }
            IDLst.Add(ID.ToString());
            Pack008(ID, IDLst);
        }

        public static void Pack200()
        {
            int[] dat = new int[1];
            MonProtocol.InitForPack(20);
            dat[0] = 200;
            MonProtocol.OutPutIntBit(dat, 8);
            dat[0] = 20;
            MonProtocol.OutPutIntBit(dat, 12);
            MonProtocol.Clear();//delete all cmd in cmd list
            MonProtocol.AddPool(0);

        }
        /// <summary>
        /// 任务包打包
        /// </summary>
        /// <param name="task">任务包类型115</param>

        public static void PackTask(BDTask task,bool bNew, int lastpkgid)
        {
            if (bNew)
            {
                int[] dat = new int[4];
                int length = 20 + 8 + 8 + 64;//不包括参数
                if (task.CommID == 2 || task.CommID == 5)
                    length += 10*8;
                if (task.CommID == 3)
                    length += 6*8;
                MonProtocol.InitForPack(length);
                dat[0] = 115;
                MonProtocol.OutPutIntBit(dat, 8);
                dat[0] = length;
                MonProtocol.OutPutIntBit(dat, 12);
                dat[0] = task.DestPort;
                MonProtocol.OutPutIntBit(dat, 8);
                dat[0] = task.CommID;
                MonProtocol.OutPutIntBit(dat, 8);
                var idstring = task.TaskID.ToString();
                string year = idstring.Substring(0, 4);
                dat[0] = Int16.Parse(year);
                MonProtocol.OutPutIntBit(dat, 16);
                string month = idstring.Substring(4, 2);
                dat[0] = Int16.Parse(month);
                MonProtocol.OutPutIntBit(dat, 8);
                string day = idstring.Substring(6, 2);
                dat[0] = Int16.Parse(day);
                MonProtocol.OutPutIntBit(dat, 8);
                string hour = idstring.Substring(8, 2);
                dat[0] = Int16.Parse(hour);
                MonProtocol.OutPutIntBit(dat, 8);
                string minute = idstring.Substring(10, 2);
                dat[0] = Int16.Parse(minute);
                MonProtocol.OutPutIntBit(dat, 8);
                string second = idstring.Substring(12, 2);
                dat[0] = Int16.Parse(second);
                MonProtocol.OutPutIntBit(dat, 8);
                string dest = idstring.Substring(14, 2);
                dat[0] = Int16.Parse(dest);
                MonProtocol.OutPutIntBit(dat, 8);
                
                if (task.CommID == 2)
                {
                    var buf = new int[4];
                    Buffer.BlockCopy(task.ParaBytes,0,buf,0,10);
                    MonProtocol.OutPutIntBit(buf, 80);
                    MonProtocol.AddPool(task.DestID);
                }

                if (task.CommID == 3)
                {
                    var buf = new int[3];
                    Buffer.BlockCopy(task.ParaBytes, 0, buf, 0, 6);
                    MonProtocol.OutPutIntBit(buf, 48);
                    MonProtocol.AddPool(task.DestID);
                }
                if (task.CommID == 5)
                {
                    var buf = new int[4];
                    Buffer.BlockCopy(task.ParaBytes, 0, buf, 0, 10);
                    MonProtocol.OutPutIntBit(buf, 80);
                    MonProtocol.AddPool(task.DestID);
                }
                
            }
            else//继续
            {
                int[] dat = new int[4];
                int length = 20 + 64;//不包括参数
                if (task.ErrIdxStr != "")
                {
                    string[] split = task.ErrIdxStr.Split(';');
                    length += split.Count()*16;
                }
                MonProtocol.InitForPack(length);
                dat[0] = 64;
                MonProtocol.OutPutIntBit(dat, 8);
                dat[0] = length;
                MonProtocol.OutPutIntBit(dat, 12);
                var idstring = task.TaskID.ToString();
                string year = idstring.Substring(0, 4);
                dat[0] = Int16.Parse(year);
                MonProtocol.OutPutIntBit(dat, 16);
                string month = idstring.Substring(4, 2);
                dat[0] = Int16.Parse(month);
                MonProtocol.OutPutIntBit(dat, 8);
                string day = idstring.Substring(6, 2);
                dat[0] = Int16.Parse(day);
                MonProtocol.OutPutIntBit(dat, 8);
                string hour = idstring.Substring(8, 2);
                dat[0] = Int16.Parse(hour);
                MonProtocol.OutPutIntBit(dat, 8);
                string minute = idstring.Substring(10, 2);
                dat[0] = Int16.Parse(minute);
                MonProtocol.OutPutIntBit(dat, 8);
                string second = idstring.Substring(12, 2);
                dat[0] = Int16.Parse(second);
                MonProtocol.OutPutIntBit(dat, 8);
                string dest = idstring.Substring(14, 2);
                dat[0] = Int16.Parse(dest);
                MonProtocol.OutPutIntBit(dat, 8);
                if (lastpkgid == -1) //尚未接收到数据，不需要添加重传包
                {
                    MonProtocol.AddPool(task.DestID);
                }
                else
                {
                    if (task.ErrIdxStr != "")
                    {
                        string[] split = task.ErrIdxStr.Split(';');
                        foreach (var strid in split)
                        {
                            dat[0] = int.Parse(strid);
                            if(dat[0]!=-1)
                                MonProtocol.OutPutIntBit(dat, 16);    
                        }
                        
                    }
                    MonProtocol.AddPool(task.DestID);
                }
                
            }
            List<string> IDLst = new List<string>() { MonProtocol.SourceID.ToString(), task.DestID.ToString() };
            Pack008(task.DestID, IDLst);
        }
        
    }
}
