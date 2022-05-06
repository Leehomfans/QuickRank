using _51Sole.DJG.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace FastRankTool
{
    public class IpHelper
    {
        public static List<string> GetAvilableIpList(out string errmsg)
        {

            List<string> ipList = new List<string>();
            int code = 200;
            errmsg = "";
            string apiUrl = "http://webapi.http.zhimacangku.com/getip?num=50&type=2&pro=0&city=0&yys=100017&port=1&time=1&ts=0&ys=0&cs=0&lb=1&sb=0&pb=4&mr=1&regions=440000";
            string res= HttpHelper.GetWebRequest(apiUrl, Encoding.UTF8, out code);
            RetModel ret= JsonConvert.DeserializeObject<RetModel>(res);
            if(ret!=null&&ret.data.Count()>0)
            {
                foreach(var item in ret.data)
                {
                    if(PingIp(item.ip))
                    {
                        ipList.Add(item.ip + ":" + item.port);
                    }
                }
            }
            else
            {
                errmsg = ret.msg;
            }
            return ipList;
        }
        private static bool PingIp(string strIP)
        {
            bool bRet = false;
            try
            {
                Ping pingSend = new Ping();
                PingReply reply = pingSend.Send(strIP, 30);
                if (reply.Status == IPStatus.Success)
                    bRet = true;
            }
            catch (Exception)
            {
                bRet = false;
            }
            return bRet;
        }
        public class RetModel
        {
            public int code { get; set; }
            public string msg { get; set; }
            public List<IpModel> data { get; set; }
        }
        public class IpModel
        {
            public string ip { get; set; }
            public int port { get; set; }
        }
    }
}

