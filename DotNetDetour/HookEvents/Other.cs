using WPFLauncher.Model;

namespace DotNetDetour.HookEvents
{
    internal class Other : IMethodHook
    {
        [HookMethod("WPFLauncher.Manager.Configuration.ary")]
        public bool get_AllowGameInvite() => false;

        [HookMethod("WPFLauncher.Manager.Configuration.ary")]
        public bool get_PlayCG() => false;

        [HookMethod("WPFLauncher.Manager.aly")]
        private void e(aef ktu)
        {
            e_Original(ktu);
            d_Original(ktu);
        }

        [HookMethod("WPFLauncher.Manager.aly")]
        private void d(aef ktt)
        {
        }

        [OriginalMethod]
        private void e_Original(aef ktu)
        {
        }

        [OriginalMethod]
        private void d_Original(aef ktt)
        {

        }
    }
}
