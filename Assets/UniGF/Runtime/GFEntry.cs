using System;
using System.Collections.Generic;

namespace UniGF
{
    /// <summary>
    /// 游戏框架入口。
    /// </summary>
    public static class GFEntry
    {
        private static readonly List<GFModule> s_GFModules = new List<GFModule>();

        /// <summary>
        /// 所有游戏框架模块轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public static void Update(float elapseSeconds, float realElapseSeconds)
        {
            foreach (GFModule module in s_GFModules)
            {
                module.Update(elapseSeconds, realElapseSeconds);
            }
        }

        /// <summary>
        /// 关闭并清理所有游戏框架模块。
        /// </summary>
        public static void Shutdown()
        {
            for (int i = s_GFModules.Count - 1; i >= 0; i--)
            {
                s_GFModules[i].Shutdown();
            }

            s_GFModules.Clear();
            ReferencePool.ClearAll();
        }

        /// <summary>
        /// 获取游戏框架模块。
        /// </summary>
        /// <typeparam name="T">要获取的游戏框架模块类型。</typeparam>
        /// <returns>要获取的游戏框架模块。</returns>
        /// <remarks>如果要获取的游戏框架模块不存在，则自动创建该游戏框架模块。</remarks>
        public static T GetModule<T>() where T : class
        {
            Type interfaceType = typeof(T);
            if (!interfaceType.IsInterface)
            {
                throw new Exception($"You must get module by interface, but '{interfaceType.FullName}' is not.");
            }

            if (!interfaceType.FullName.StartsWith("GameFramework.", StringComparison.Ordinal))
            {
                throw new Exception($"You must get a Game Framework module, but '{interfaceType.FullName}' is not.");
            }

            string moduleName = $"{interfaceType.Namespace}.{interfaceType.Name.Substring(1)}";
            Type moduleType = Type.GetType(moduleName);
            if (moduleType == null)
            {
                throw new Exception($"Can not find Game Framework module type '{moduleName}'.");
            }

            return GetModule(moduleType) as T;
        }

        /// <summary>
        /// 获取游戏框架模块。
        /// </summary>
        /// <param name="moduleType">要获取的游戏框架模块类型。</param>
        /// <returns>要获取的游戏框架模块。</returns>
        /// <remarks>如果要获取的游戏框架模块不存在，则自动创建该游戏框架模块。</remarks>
        private static GFModule GetModule(Type moduleType)
        {
            foreach (GFModule module in s_GFModules)
            {
                if (module.GetType() == moduleType)
                {
                    return module;
                }
            }

            return CreateModule(moduleType);
        }

        /// <summary>
        /// 创建游戏框架模块。
        /// </summary>
        /// <param name="moduleType">要创建的游戏框架模块类型。</param>
        /// <returns>要创建的游戏框架模块。</returns>
        private static GFModule CreateModule(Type moduleType)
        {
            GFModule module = (GFModule)Activator.CreateInstance(moduleType);
            if (module == null)
            {
                throw new Exception($"Can not create module '{moduleType.FullName}'.");
            }

            int insertIndex = s_GFModules.Count;
            for (int i = 0; i < s_GFModules.Count; i++)
            {
                if (module.Priority > s_GFModules[i].Priority)
                {
                    insertIndex = i;
                    break;
                }
            }
            s_GFModules.Insert(insertIndex, module);
            return module;
        }
    }
}