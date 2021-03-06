﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ValueUtils
{
    static class ReflectionHelper
    {
        public static IEnumerable<Type> WalkMeaningfulInheritanceChain(Type type)
        {
            if (type.IsClass)
            {
                while (type != typeof (object))
                {
                    yield return type;
                    type = type.BaseType;
                }
            }
            else if (type.IsValueType)
            {
                yield return type;
            }
        }

        const BindingFlags OnlyDeclaredInstanceMembers =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        public static IEnumerable<FieldInfo> GetAllFields(Type type)
            => WalkMeaningfulInheritanceChain(type).Reverse()
                .SelectMany(t => t.GetFields(OnlyDeclaredInstanceMembers));
    }
}