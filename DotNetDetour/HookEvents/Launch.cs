using DotNetDetour.Properties;
using Mcl.Core.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WPFLauncher.Code;
using WPFLauncher.Manager;
using WPFLauncher.Model;
using WPFLauncher.Util;

namespace DotNetDetour.HookEvents
{
    internal class Launch : IMethodHook
    {
        [HookMethod("WPFLauncher.Manager.alu")]
        public static List<string> n() => new List<string> { "Minecraft" };

        [HookMethod("WPFLauncher.Manager.amg")]
        public Tuple<string, IntPtr> o(Process lcu, GameM lcv, string lcw = null) => Tuple.Create(string.Empty, IntPtr.Zero);

        [HookMethod("WPFLauncher.Util.tf")]
        public static alt a(string fqe, string fqf, EventHandler fqg, alr fqh, string fqi = null, bool fqj = false, Action<string> fqk = null)
        {
            switch (Path.GetFileName(fqe))
            {
                case "java.exe":
                    return null;
                case "javaw.exe":
                    break;
                default:
                    return a_Original(fqe, fqf, fqg, fqh, fqi, fqj, fqk);
            }

            if (MessageBox.Show("是否加入模组?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) 
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog()) 
                {
                    openFileDialog.Title = "请选择模组文件,选中后确定即可.";
                    openFileDialog.Multiselect = true;
                    openFileDialog.RestoreDirectory = true;
                    openFileDialog.Filter = "模组文件(*.jar)|*.jar";
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        foreach (var filePath in openFileDialog.FileNames)
                        {
                            File.Copy(filePath, RegistryHelper.GetValue("DownloadPath") + $"\\Game\\.minecraft\\mods\\{Guid.NewGuid().ToString("X")}⁫.jar", true);
                        }
                    }
                }
            }

            var gameProcess = a_Original(fqe, fqf, fqg, fqh, fqi, fqj, fqk);

            return gameProcess;
        }

        [OriginalMethod]
        public static alt a_Original(string fqe, string fqf, EventHandler fqg, alr fqh, string fqi = null, bool fqj = false, Action<string> fqk = null) => null;
    }
}
