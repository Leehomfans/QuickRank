using HtmlAgilityPack;
using PuppeteerExtraSharp;
using PuppeteerExtraSharp.Plugins.ExtraStealth;
using PuppeteerSharp;
using PuppeteerSharp.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace FastRankTool
{
    public class Program
    {
        //Random UA List
        public static List<string> UAList = new List<string> { "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2226.0 Safari/537.36", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_8_3) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/27.0.1453.93 Safari/537.36", "Mozilla/5.0 (Windows NT 4.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/37.0.2049.0 Safari/537.36", "Mozilla/5.0 (Windows NT 5.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/35.0.3319.102 Safari/537.36", "Mozilla/5.0 (Windows NT 6.2) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/28.0.1467.0 Safari/537.36", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_8_3) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/27.0.1453.93 Safari/537.36", "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/29.0.1547.2 Safari/537.36", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/27.0.1453.93 Safari/537.36", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.17 (KHTML, like Gecko) Chrome/24.0.1312.60 Safari/537.17", "Mozilla/5.0 (X11; Linux x86_64; rv:28.0) Gecko/20100101  Firefox/28.0", "Mozilla/5.0 (Windows NT 10.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/40.0.2214.93 Safari/537.36", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/27.0.1453.93 Safari/537.36", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/27.0.1453.93 Safari/537.36", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.8; rv:24.0) Gecko/20100101 Firefox/24.0", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:21.0) Gecko/20130514 Firefox/21.0", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_8_2) AppleWebKit/537.17 (KHTML, like Gecko) Chrome/24.0.1309.0 Safari/537.17", "Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:21.0) Gecko/20100101 Firefox/21.0", "Mozilla/5.0 (Macintosh; U; Intel Mac OS X 10_6_6; sv-se) AppleWebKit/533.20.25 (KHTML, like Gecko) Version/5.0.4 Safari/533.20.27", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.17 (KHTML, like Gecko) Chrome/24.0.1312.60 Safari/537.17", "Mozilla/5.0 (Windows NT 4.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/37.0.2049.0 Safari/537.36" };
        public static List<string> IpList = new List<string> {};
        public const int maxPage= 3;//最大页码深度
        public static string domain = "cnblogs.com";//要优化的站点域名
        public static void Main(string[] args)
        {
            Console.WriteLine("====开始执行====");
            handler = new ConsoleEventDelegate(ConsoleEventCallback);
            SetConsoleCtrlHandler(handler, true);
            List<string> keyList = new List<string> { "Python", "Java", "Php", "GO", "C#" };//要优化的关键词
           
            for (int i = 0; i < 3; i++)
            {
                ParallelLoopResult result = Parallel.For(0, 5, x =>
                {
                    string keyword = HttpUtility.UrlEncode(keyList[x]);
                    var d = Task.Run(() => TaskRun("https://www.baidu.com/s?wd=" + keyword + "&pn=1&tn=monline_4_dg&ie=utf-8&si="+domain+"&ct=2097152"));
                    d.Wait();
                });
            }
            Console.WriteLine("ok");
            Console.Read();
        }
        async static Task TaskRun(string link)
        {
            string ip = string.Empty;
            string errmsg = "";
            while (string.IsNullOrEmpty(ip))
            {
                IpList = IpHelper.GetAvilableIpList(out errmsg);
                if (!string.IsNullOrEmpty(errmsg))
                {
                    Console.WriteLine(errmsg);
                }
                else
                {
                    int index = new Random().Next(0, IpList.Count());
                    ip = IpList[index];
                    Console.WriteLine("当前代理IP:" + ip);
                }
                Thread.Sleep(new Random().Next(1000, 3000));
            }

            //await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);//下载浏览器驱动
            LaunchOptions options = new LaunchOptions
            {
                Headless = false,
                Args = new[] {
                    //string.Format("--proxy-server={0}",ip),//代理设置
                    "--start-maximized",//最大窗口
                    "--disable-infobars",//--隐藏自动化标题
                    "--no-sandbox",
                    "--disable-setuid-sandbox",
                    "--ignore-certificate-errors",
                    "--app=https://www.baidu.com/"
                },
                IgnoreHTTPSErrors = true
            };
            var extra = new PuppeteerExtra();
            extra.Use(new StealthPlugin());
            using (var browser = await extra.LaunchAsync(options))
            {
                using (var page = await browser.NewPageAsync())
                {
                    string userAgent = UAList[new Random().Next(0, UAList.Count())];
                    await page.SetUserAgentAsync(userAgent);
                    ViewPortOptions vOptions = new ViewPortOptions
                    {
                        Width = 1920,
                        Height = 1080
                    };
                    await page.SetViewportAsync(vOptions);
                    Dictionary<string, string> dicHeader = new Dictionary<string, string>();
                    dicHeader.Add("referer", "https://www.baidu.com/s?ie=utf-8&f=3&rsv_bp=1&tn=baidu&wd=c%23%20htmlagility&oq=%25E5%25BE%25AE%25E8%25B0%25B1%25E6%25A3%2580%25E6%25B5%258B%25E6%2590%259C%25E4%25BA%2586%25E7%25BD%2591&rsv_pq=eb9ff0ce00008fdb&rsv_t=5794Qmog%2FW4kfXpoYcJXzzRk4iN0Dx7vYa8xiv%2Fhej8i69AmoTGkqlME680&rqlang=cn&rsv_dl=ts_2&rsv_enter=1&rsv_sug3=10&rsv_sug1=3&rsv_sug7=100&rsv_sug2=1&rsv_btype=t&prefixsug=%2526lt%253B%2523%2520htmla&rsp=2&inputT=6556&rsv_sug4=8091");
                    await page.SetExtraHttpHeadersAsync(dicHeader);
                    try
                    {
                        //隐藏webdriver特征
                        //await page.EvaluateExpressionOnNewDocumentAsync("delete navigator.__proto__.webdriver;");
                        try
                        {
                            await page.GoToAsync(link, WaitUntilNavigation.Networkidle2);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("启动浏览器异常:" + ex.Message);
                            await browser.CloseAsync();//关闭浏览器
                            ip = string.Empty;
                            //重新运行任务
                            var d = Task.Run(() => TaskRun(link));
                            d.Wait();
                            return;
                        }
         
                        string eqid = await Geteqid(page);
                        if (string.IsNullOrEmpty(eqid))
                        {
                            await browser.CloseAsync();//关闭浏览器
                            ip = string.Empty;
                            ////重新运行任务
                            var d = Task.Run(() => TaskRun(link));
                            d.Wait();
                            return;
                        }
                        List<LinkModel> linkList = new List<LinkModel>();
                        List<Page> pages = new List<Page>();

                        string pagesource = await page.GetContentAsync();
                        linkList = GetAllHrefs(pagesource, eqid);//得到页面所有需要点击的链接
                        linkList = GetListRandomItems(linkList, linkList.Count()/2);
                        ElementHandle[] handlers = await page.XPathAsync("//a[@class='siteLink_9TPP3']");
                        //遍历访问搜索结果页面
                        foreach (var href in linkList)
                        {
                            using (var newPage = await browser.NewPageAsync())
                            {
                                await newPage.SetUserAgentAsync(userAgent);
                                await newPage.SetViewportAsync(vOptions);
                                try
                                {
                                    await newPage.GoToAsync(href.link, WaitUntilNavigation.DOMContentLoaded);
                                    await newPage.WaitForNavigationAsync(new NavigationOptions { Timeout = 15000 });
                                    await ScrollPage(newPage, 300, 700, 6, 500, 800);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("当前打开页面链接异常:" + ex.Message);
                                }
                            }
                        }
                        if (linkList.Count() > 1)
                        {
                            Console.WriteLine("休息6秒钟...");
                            Thread.Sleep(6000);
                        }
                        else
                        {
                            Console.WriteLine("当前结果页面暂无目标链接...");
                        }
                        //关闭打开的子页面
                        foreach (var p in pages)
                        {
                            await p.CloseAsync();
                        }

                        await page.ClickAsync(".page-inner_2jZi2>a:last-child");//点击下一页
                        await page.ReloadAsync();
                        if(page.Url.Contains("pass.baidu.com"))
                        {
                            throw new Exception("安全验证");
                        }
                        var n = Task.Run(() => ChildTaskRun(browser, page, userAgent, vOptions));
                        n.Wait();
                        await browser.CloseAsync();//关闭浏览器
                        Console.WriteLine("全部任务已完成....");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("global:" + ex.Message);
                        if (browser != null && !browser.IsClosed)
                        {
                            await browser.CloseAsync();
                            ip = string.Empty;
                            var d = Task.Run(() => TaskRun(link));//递归执行
                            d.Wait();
                        }
                    }
                }
            }
        }
        public async static Task ChildTaskRun(Browser browser, Page page, string uA, ViewPortOptions options)
        {
            int currentPage = 1;
            while (currentPage <= maxPage)
            {
                for (int i = 0; i < 5; i++)
                {
                    Thread.Sleep(new Random().Next(700, 1300));
                    await page.Mouse.WheelAsync(110 + new Random().Next(8, 15) * i, 8 + new Random().Next(69, 123) * i);
                }
                Thread.Sleep(1000);
                string eqid = await Geteqid(page);
                if (string.IsNullOrEmpty(eqid))
                {
                    await browser.CloseAsync();//关闭浏览器
                    string link = page.Url;
                    if (!link.Contains("www.baidu.com"))
                    {
                        link = "https://www.baidu.com/";
                    }
                    var d = Task.Run(() => TaskRun(link));
                    d.Wait();
                    return;
                }
                List<LinkModel> linkList = new List<LinkModel>();
                string pagesource = await page.GetContentAsync();
                linkList = GetAllHrefs(pagesource, eqid);//得到页面所有需要点击的链接
                ElementHandle[] handlers = await page.XPathAsync("//a[@class='siteLink_9TPP3']");
                if (linkList != null && linkList.Count() > 0)
                {
                    foreach (var url in linkList)
                    {
                        using (var newPage = await browser.NewPageAsync())
                        {
                            await newPage.SetUserAgentAsync(uA);
                            await newPage.SetViewportAsync(options);
                            try
                            {
                                await newPage.GoToAsync(url.link, WaitUntilNavigation.DOMContentLoaded);
                                await newPage.WaitForNavigationAsync(new NavigationOptions { Timeout = 15000 });
                                await ScrollPage(newPage, 300, 700, 6, 500, 800);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("当前打开页面链接异常:" + ex.Message);
                            }
                        }
                    }
                }
               
                await ScrollPage(page, 200, 500, 6);
                Thread.Sleep(new Random().Next(1000, 2000));
                await page.ClickAsync(".page-inner_2jZi2>a:last-child");//点击下一页
                currentPage++;
            }
        }
        /// <summary>
        /// 模拟滚动页面
        /// </summary>
        /// <param name="page">当前页面</param>
        /// <param name="times">次数</param>
        /// <returns></returns>
        async static Task ScrollPage(Page page, int minPix, int maxPix, int times, int minSleep = 1000, int maxSleep = 2000)
        {
            Console.WriteLine("Preparing to scroll");
            Func<Task> scroll = null;
            int i = 0;
            scroll = new Func<Task>(async () =>
            {
                Console.WriteLine("Scrolling");
                int top = new Random().Next(minPix, maxPix);
                await page.EvaluateExpressionAsync("window.scrollBy({top:" + top + ",behavior:'smooth'})");
                Thread.Sleep(new Random().Next(minSleep, maxSleep));
                i++;
                if (i < times)
                {
                    await scroll();
                }
            });
            await scroll();
            //再向上随机滚动
            await page.EvaluateExpressionAsync("window.scrollBy({top:" + (0 - new Random().Next(500, 1000)) + ",behavior:'smooth'})");
        }
        /// <summary>
        /// 获取百度加密链接参数变量值
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        async static Task<string> Geteqid(Page page)
        {
            string eqids = string.Empty;
            try
            {
                var handle = await page.EvaluateExpressionHandleAsync("({bds})");
                var properties = await handle.GetPropertiesAsync();
                var windowHandle = properties["bds"];
                var comm = await windowHandle.GetPropertyAsync("comm");
                var eqid = await comm.GetPropertyAsync("eqid");
                eqids = eqid.ToString().Replace("JSHandle:", "");
            }
            catch (Exception ex)
            {
                Console.WriteLine("被拉进小黑屋了..." + ex.Message);
            }
            return eqids;
        }
        /// <summary>
        /// 获取当前搜索结果页面全部需要访问的链接
        /// </summary>
        /// <param name="pagesource"></param>
        /// <param name="eqid"></param>
        /// <returns></returns>
        static List<LinkModel> GetAllHrefs(string pagesource, string eqid)
        {
            List<LinkModel> linkList = new List<LinkModel>();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(pagesource);
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//a[@class='siteLink_9TPP3']");
            if (nodes == null)
            {
                nodes = doc.DocumentNode.SelectNodes("//*/h3/a");   //*/h3/a
            }
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    string baiduUrl = string.Empty;
                    LinkModel link = new LinkModel
                    {
                        index = nodes.IndexOf(node)
                    };
                    if (node.InnerHtml.Contains("博客园") || node.InnerHtml.Contains(domain))
                    {
                        baiduUrl = node.Attributes["href"].Value + "&wd=&eqid=" + eqid.ToString().Replace("JSHandle:", "");
                        link.isSole = true;
                        Console.WriteLine(baiduUrl);
                    }
                    else
                    {
                        baiduUrl = node.Attributes["href"].Value + "&wd=&eqid=" + eqid.ToString().Replace("JSHandle:", "");
                        link.isSole = false;
                        Console.WriteLine(baiduUrl);
                    }

                    if (!string.IsNullOrEmpty(baiduUrl))
                    {
                        link.link = baiduUrl;
                        linkList.Add(link);
                    }
                }
            }
            return linkList;
        }
        /// <summary>
        /// 随机从List取出指定个数的元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public static List<T> GetListRandomItems<T>(IList<T> list, int num)
        {
            //新建一个泛型列表,将传入的列表复制过来,用于运算,而不要直接操作传入的列表;    

            //这样写是引用复制,不对啦,谢谢Osamede指出.
            //IList<string> temp_list = list;

            //另外这样写也要注意,也不是深度复制喽,关于深度复制可以做为一个新话题来说,这儿就不说啦;
            IList<T> temp_list = new List<T>(list);

            //取出的项,保存在此列表
            List<T> return_list = new List<T>();

            //Random random = new Random(unchecked((int)DateTime.Now.Ticks));
            Random random = new Random();

            for (int i = 0; i < num; i++)
            {
                //判断如果列表还有可以取出的项,以防下标越界
                if (temp_list.Count > 0)
                {
                    //在列表中产生一个随机索引
                    int arrIndex = random.Next(0, temp_list.Count);
                    //将此随机索引的对应的列表元素值复制出来
                    return_list.Add(temp_list[arrIndex]);
                    //然后删掉此索引的列表项
                    temp_list.RemoveAt(arrIndex);
                }
                else
                {
                    //列表项取完后,退出循环,比如列表本来只有10项,但要求取出20项.
                    break;
                }
            }
            return return_list;
        }
        private static bool PingIp(string strIP)
        {
            bool bRet = false;
            try
            {
                Ping pingSend = new Ping();
                PingReply reply = pingSend.Send(strIP, 50);
                if (reply.Status == IPStatus.Success)
                    bRet = true;
            }
            catch (Exception)
            {
                bRet = false;
            }
            return bRet;
        }
        public class IpModel
        {
            public string ip { get; set; }
            public int port { get; set; }
        }
        public class LinkModel
        {
            public string link { get; set; }
            public int index { get; set; }
            public bool isSole { get; set; }//标识是否为我们公司的链接
        }
        static ConsoleEventDelegate handler;   // Keeps it from getting garbage collected
        static bool ConsoleEventCallback(int eventType)
        {
            if (eventType == 2)
            {
                List<Process> pList = Process.GetProcessesByName("chrome.exe").ToList();
                pList.ForEach(i => i.Kill());
            }
            return false;
        }
        private delegate bool ConsoleEventDelegate(int eventType);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);
    }

}
