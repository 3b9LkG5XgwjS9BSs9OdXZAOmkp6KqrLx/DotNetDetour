using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using WPFLauncher.Network.Protocol;

namespace DotNetDetour
{
    class DestAndOri
    {
        public MethodBase HookMethod { get; set; }

        public MethodBase OriginalMethod { get; set; }

        public IMethodHook Obj;
    }

    public class MethodHook
    {
        static public BindingFlags AllFlag = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
        static bool installed = false;
        static List<DestAndOri> destAndOris = new List<DestAndOri>();

        static MethodHook()
        {
            Application.EnableVisualStyles();
            MessageBox.Show("这仅仅是一个测试版本,请勿用来游玩大号.\r\n作者以及运营团队不承担任何责任!", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static int Install(string args)
        {
            if (installed)
                return -1;
            installed = true;

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            IEnumerable<IMethodHook> monitors = assemblies.SelectMany(t => t.GetImplementedObjectsByInterface<IMethodHook>());

            foreach (var monitor in monitors)
            {
                var all = monitor.GetType().GetMethods(AllFlag);
                var hookMethods = all.Where(t => t.CustomAttributes.Any(a => typeof(HookMethodAttribute).IsAssignableFrom(a.AttributeType)));
                var originalMethods = all.Where(t => t.CustomAttributes.Any(a => typeof(OriginalMethodAttribute).IsAssignableFrom(a.AttributeType))).ToArray();

                var destCount = hookMethods.Count();
                foreach (var hookMethod in hookMethods)
                {
                    DestAndOri destAndOri = new DestAndOri();
                    destAndOri.Obj = monitor;
                    destAndOri.HookMethod = hookMethod;
                    if (destCount == 1)
                    {
                        destAndOri.OriginalMethod = originalMethods.FirstOrDefault();
                    }
                    else
                    {
                        var originalMethodName = hookMethod.GetCustomAttribute<HookMethodAttribute>().GetOriginalMethodName(hookMethod);

                        destAndOri.OriginalMethod = FindMethod(originalMethods, originalMethodName, hookMethod, assemblies);
                    }

                    destAndOris.Add(destAndOri);
                }
            }

            InstallInternal(true, assemblies);
            return 0;
        }

        private static void InstallInternal(bool isInstall, Assembly[] assemblies)
        {
            foreach (var detour in destAndOris)
            {
                var hookMethod = detour.HookMethod;
                var hookMethodAttribute = hookMethod.GetCustomAttribute<HookMethodAttribute>();

                var typeName = hookMethodAttribute.TargetTypeFullName;
                if (hookMethodAttribute.TargetType != null)
                {
                    typeName = hookMethodAttribute.TargetType.FullName;
                }
                var type = TypeResolver(typeName, assemblies);
                if (type != null && !assemblies.Contains(type.Assembly))
                {
                    type = null;
                }

                var methodName = hookMethodAttribute.GetTargetMethodName(hookMethod);
                MethodBase rawMethod = null;
                if (type != null)
                {
                    MethodBase[] methods;

                    if (methodName == type.Name || methodName == ".ctor")
                    {//构造方法
                        methods = type.GetConstructors(AllFlag);
                        methodName = ".ctor";
                    }
                    else
                    {
                        methods = type.GetMethods(AllFlag);
                    }

                    rawMethod = FindMethod(methods, methodName, hookMethod, assemblies);
                }
                if (rawMethod != null && rawMethod.IsGenericMethod)
                {
                    //泛型方法转成实际方法
                    rawMethod = ((MethodInfo)rawMethod).MakeGenericMethod(hookMethod.GetParameters().Select(o =>
                    {
                        var rt = o.ParameterType;
                        var attr = o.GetCustomAttribute<RememberTypeAttribute>();
                        if (attr != null && attr.TypeFullNameOrNull != null)
                        {
                            rt = TypeResolver(attr.TypeFullNameOrNull, assemblies);
                        }
                        return rt;
                    }).ToArray());
                }

                if (rawMethod == null)
                {
                    if (isInstall)
                    {
#if DEBUG
                        MessageBox.Show(string.Format("没有找到与试图Hook的方法\"{0}, {1}\"匹配的目标方法.", new object[] { hookMethod.ReflectedType.FullName, hookMethod }));
#else
                        Debug.WriteLine("没有找到与试图Hook的方法\"{0}, {1}\"匹配的目标方法.", new object[] { hookMethod.ReflectedType.FullName, hookMethod });
#endif
                    }
                    continue;
                }
                if (detour.Obj is IMethodHookWithSet)
                {
                    ((IMethodHookWithSet)detour.Obj).HookMethod(rawMethod);
                }

                var originalMethod = detour.OriginalMethod;
                var engine = DetourFactory.CreateDetourEngine();
                engine.Patch(rawMethod, hookMethod, originalMethod);

                Debug.WriteLine("已将目标方法 \"{0}, {1}\" 的调用指向 \"{2}, {3}\" Ori: \"{4}\".", rawMethod.ReflectedType.FullName, rawMethod
                    , hookMethod.ReflectedType.FullName, hookMethod
                    , originalMethod == null ? " (无)" : originalMethod.ToString());
            }
        }

        private static Type TypeResolver(string typeName, Assembly[] assemblies)
        {
            return Type.GetType(typeName, null, (a, b, c) =>
            {
                Type rt;
                if (a != null)
                {
                    rt = a.GetType(b);
                    if (rt != null)
                    {
                        return rt;
                    }
                }
                rt = Type.GetType(b);
                if (rt != null)
                {
                    return rt;
                }
                foreach (var asm in assemblies)
                {
                    rt = asm.GetType(b);
                    if (rt != null)
                    {
                        return rt;
                    }
                }
                return null;
            });
        }

        private static MethodBase FindMethod(MethodBase[] methods, string name, MethodBase like, Assembly[] assemblies)
        {
            var likeParams = like.GetParameters();
            foreach (var item in methods)
            {
                if (item.Name != name)
                {
                    continue;
                }

                var paramArr = item.GetParameters();
                var len = paramArr.Count();
                if (len != likeParams.Count())
                {
                    continue;
                }

                for (var i = 0; i < len; i++)
                {
                    var t1 = likeParams[i];
                    var t2 = paramArr[i];
                    //类型相同 或者 fullname都为null的泛型参数
                    if (t1.ParameterType.FullName == t2.ParameterType.FullName)
                    {
                        continue;
                    }

                    //手动保持的类型
                    var rmtype = t1.GetCustomAttribute<RememberTypeAttribute>();
                    if (rmtype != null)
                    {
                        //泛型参数
                        if (rmtype.IsGeneric && t2.ParameterType.FullName == null)
                        {
                            continue;
                        }
                        //查找实际类型
                        if (rmtype.TypeFullNameOrNull != null)
                        {
                            if (rmtype.TypeFullNameOrNull == t2.ParameterType.FullName)
                            {
                                continue;
                            }

                            var type = TypeResolver(rmtype.TypeFullNameOrNull, assemblies);
                            if (type == t2.ParameterType)
                            {
                                continue;
                            }
                        }
                    }
                    goto next;
                }
                return item;
            next:
                continue;
            }
            return null;
        }
    }
}
