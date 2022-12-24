using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFLauncher.Model;

namespace DotNetDetour.HookEvents
{
    internal class Guard : IMethodHook
    {
        [HookMethod("WPFLauncher.Manager.amg")]
        public void e(GameM kbs)
        {
            if (kbs != null)
            {
                if (kbs.Type == GType.NET_GAME || kbs.Type == GType.SERVER_GAME || kbs.Type == GType.ONLINE_LOBBY_GAME)
                {
                    kbs.Status = GStatus.PLAYING;
                }
                else
                {
                    e_Original(kbs);
                }
            }
        }

        [HookMethod("WPFLauncher.Manager.amg")]
        private void p()
        { }

        [HookMethod("WPFLauncher.Network.Protocol.abc")]
        public static void d(string gfr, object gfs, uint gft = 0U)
        { }

        [HookMethod("WPFLauncher.Model.CompGameM")]
        public void f(byte[] hmr)
        { }

        [HookMethod("WPFLauncher.Model.CompGameM")]
        public void i(byte[] hmu)
        { }

        [OriginalMethod]
        public void e_Original(GameM kbs)
        { }
    }
}
