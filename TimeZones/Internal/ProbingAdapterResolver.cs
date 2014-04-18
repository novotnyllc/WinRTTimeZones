// -----------------------------------------------------------------------
// Copyright (c) David Kean. All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;
using System.IO;

namespace TimeZones.Internal
{
    // An implementation IAdapterResolver that probes for platforms-specific adapters by dynamically
    // looking for concrete types in platform-specific assemblies, such as Portable.Silverlight.
    internal class ProbingAdapterResolver : IAdapterResolver
    {
        private readonly Func<AssemblyName, Assembly> _assemblyLoader;
        private readonly object _lock = new object();
        private readonly Dictionary<Type, object> _adapters = new Dictionary<Type, object>();
        private Assembly _assembly;

        public ProbingAdapterResolver()
            : this(Assembly.Load)
        {
        }

        public ProbingAdapterResolver(Func<AssemblyName, Assembly> assemblyLoader)
        {
            Debug.Assert(assemblyLoader != null);

            _assemblyLoader = assemblyLoader;
        }

        public object Resolve(Type type)
        {
            Debug.Assert(type != null);

            lock (_lock)
            {
                object instance;
                if (!_adapters.TryGetValue(type, out instance))
                {
                    Assembly assembly = GetPlatformSpecificAssembly();
                    instance = ResolveAdapter(assembly, type);
                    _adapters.Add(type, instance);
                }

                return instance;
            }
        }

        private static object ResolveAdapter(Assembly assembly, Type interfaceType)
        {
            string typeName = MakeAdapterTypeName(interfaceType);

            try
            {
                Type type = assembly.GetType(typeName);
                if (type != null)
                    return Activator.CreateInstance(type);
                
                // Fallback to looking in this assembly for a default
                type = typeof (ProbingAdapterResolver).GetTypeInfo().Assembly.GetType(typeName);
                if (type != null)
                    return Activator.CreateInstance(type);
                
                return type;
            }
            catch
            {
                return null;
            }
        }

        private static string MakeAdapterTypeName(Type interfaceType)
        {
            Debug.Assert(interfaceType.GetTypeInfo().IsInterface);
            Debug.Assert(interfaceType.DeclaringType == null);
            Debug.Assert(interfaceType.Name.StartsWith("I", StringComparison.Ordinal));

            // For example, if we're looking for an implementation of System.Security.Cryptography.ICryptographyFactory, 
            // then we'll look for System.Security.Cryptography.CryptographyFactory
            return interfaceType.Namespace + "." + interfaceType.Name.Substring(1);
        }

        private Assembly GetPlatformSpecificAssembly()
        {   // We should be under a lock

            if (_assembly == null)
            {
                lock(_lock)
                {
                    _assembly = ProbeForPlatformSpecificAssembly();
                    if (_assembly == null)
                        throw new InvalidOperationException(Strings.AssemblyNotSupported);
                }
            }

            return _assembly;
        }


        private Assembly ProbeForPlatformSpecificAssembly()
        {
            AssemblyName assemblyName = new AssemblyName(GetType().GetTypeInfo().Assembly.FullName);
            assemblyName.Name = "TimeZones.Platform";    

            try
            {
                return _assemblyLoader(assemblyName);
            }
            catch (FileNotFoundException)
            {
            }

            return null;
        }
    }
}
