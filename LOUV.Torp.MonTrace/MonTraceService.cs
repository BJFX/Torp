using System;
using LOUV.Torp.TraceService;
//using LOUV.Torp.ACMP;
using System.Text;
namespace LOUV.Torp.MonTrace
{
    
    /// <summary>
    /// 调用tracefile类生成需要的记录文件，
    /// </summary>
    public class MonTraceService
    {
        private TraceFile _traceFile = TraceFile.GetInstance();
        
        private bool IsCreate = false;
        public string Error
        {
            get { return _traceFile.Errormsg; }
        }

        
        public MonTraceService()
        {

        }


        /// <summary>
        /// 根据运行模式生成不同的记录文件
        /// </summary>
        /// <returns>生成结果</returns>
        public bool CreateService()
        {
            
            if (IsCreate)
                return true;

            if (_traceFile.CreateFile("Buoy1", TraceType.SingleBinary, "Buoy1", "bin", @"\UDP") == false)
            {
                return false;
            }

            if (_traceFile.CreateFile("Buoy2", TraceType.SingleBinary, "Buoy2", "bin", @"\UDP") == false)
            {
                return false;
            }
            if (_traceFile.CreateFile("Buoy3", TraceType.SingleBinary, "Buoy3", "bin", @"\UDP") == false)
            {
                return false;
            }
            if (_traceFile.CreateFile("Buoy4", TraceType.SingleBinary, "Buoy4", "bin", @"\UDP") == false)
            {
                return false;
            }
            if (_traceFile.CreateFile("Position", TraceType.String, "Pos", "bin", @"\Result") == false)
            {

                return false;
            }
            if (_traceFile.CreateFile("ALL", TraceType.SingleBinary, "All", "bin", @"\UDP") == false)
            {

                return false;
            }

            IsCreate = true;
            return true;
        }

        public bool TearDownService()
        {
            IsCreate = false;
            return _traceFile.Close();
        }

        public long Save(string sType, object bTraceBytes)
        {
            long ret = 0;
            try
            {
                switch (_traceFile.GeTraceType(sType))
                {
                    case TraceType.Binary:
                        ret = _traceFile.WriteData(sType, (byte[])bTraceBytes);
                        break;
                    case TraceType.SingleBinary:
                        ret = _traceFile.WriteSingleData(sType, (byte[])bTraceBytes);
                        break;
                    case TraceType.String:
                        ret = _traceFile.WriteString(sType, (string)bTraceBytes);
                        break;
                    default://none
                        ret = 0;
                        break;
                }
            }
            catch (Exception)
            {
                ret = 0;
            }
            
            return ret;
           
        }

    }
}
