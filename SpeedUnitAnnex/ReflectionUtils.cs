﻿using System;
using System.Reflection;
using static SpeedUnitAnnex.Logging;

namespace SpeedUnitAnnex
{
    public static class ReflectionUtils
    {
        public static bool IsAssemblyLoaded(string assemblyName)
        {
            foreach (AssemblyLoader.LoadedAssembly assembly in AssemblyLoader.loadedAssemblies)
            {
                try
                {
                    if (assembly.assembly.GetName().Name == assemblyName)
                    {
                        return true;
                    }
                }
                catch (InvalidOperationException)
                {
                    // Silently drop exception generated by users who manage to put assembly that
                    // can't load for reasons (missing deps most of the time)
                }
            }
            return false;
        }

        public static FieldInfo GetFieldByReflection(String assemblyString, String className, String fieldName, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
        {
            string assemblyName = "";

            foreach (AssemblyLoader.LoadedAssembly loaded in AssemblyLoader.loadedAssemblies)
            {
                if (loaded.assembly.GetName().Name == assemblyString)
                {
                    assemblyName = loaded.assembly.FullName;
                }
            }

            if (assemblyName == "")
            {
                Log("ReflectionUtils: could not find assembly " + assemblyString);
                return null;
            }

            Type type = Type.GetType(className + ", " + assemblyName);

            if (type == null)
            {
                Log("ReflectionUtils: could not find type  " + className + ", " + assemblyName);
                return null;
            }

            return type.GetField(fieldName, flags);
        }

        public static MethodInfo GetMethodByReflection(String assemblyString, String className, String methodName, BindingFlags flags)
        {
            string assemblyName = "";

            foreach (AssemblyLoader.LoadedAssembly loaded in AssemblyLoader.loadedAssemblies)
            {
                if (loaded.assembly.GetName().Name == assemblyString)
                {
                    assemblyName = loaded.assembly.FullName;
                }
            }

            if (assemblyName == "")
            {
                Log("ReflectionUtils: could not find assembly " + assemblyString);
                return null;
            }

            Type type = Type.GetType(className + ", " + assemblyName);

            if (type == null)
            {
                Log("ReflectionUtils: could not find type  " + className + ", " + assemblyName);
                return null;
            }
            return type.GetMethod(methodName, flags);
        }

        public static MethodInfo GetMethodByReflection(String assemblyString, String className, String methodName, BindingFlags flags, Type[] args)
        {
            string assemblyName = "";

            foreach (AssemblyLoader.LoadedAssembly loaded in AssemblyLoader.loadedAssemblies)
            {
                if (loaded.assembly.GetName().Name == assemblyString)
                {
                    assemblyName = loaded.assembly.FullName;
                }
            }

            if (assemblyName == "")
            {
                Log("ReflectionUtils: could not find assembly " + assemblyString);
                return null;
            }

            Type type = Type.GetType(className + ", " + assemblyName);

            if (type == null)
            {
                Log("ReflectionUtils: could not find type  " + className + ", " + assemblyName);
                return null;
            }
            return type.GetMethod(methodName, flags, null, args, null);
        }
    }
}
