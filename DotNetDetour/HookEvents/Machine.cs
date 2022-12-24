using Mcl.Core.Network;
using Mcl.Core.Network.Interface;
using System;
using System.Linq;
using System.Net;
using WPFLauncher.Network.Protocol;

namespace DotNetDetour.HookEvents
{
    internal class Machine : IMethodHook
    {
        public static string rand(int len)
        {
            Random random = new Random();
            return new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRST123456789", len)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [HookMethod("WPFLauncher.Network.Protocol.abh")]
        public static NetRequestAsyncHandle f(string gqq, string gqr, Action<INetResponse> gqs = null, abe gqt = abe.a, string gqu = null) => null;

        [HookMethod("WPFLauncher.Manager.Log.Util.aoi")]
        public static string b() => rand(12);

        [HookMethod("WPFLauncher.Manager.alp")]
        private string d(string jou) => rand(16) + jou;

        [HookMethod("WPFLauncher.Manager.alp")]
        public string f() => rand(8);

        [HookMethod("WPFLauncher.Manager.alp")]
        public string g()
        {
            var data = new byte[4];
            new Random().NextBytes(data);
            return new IPAddress(data).ToString();
        }

        [HookMethod("WPFLauncher.bw")]
        public static string ao() => "netdns=127.0.0.1 gw=127.0.0.1 gwdns=correct";
    }
}