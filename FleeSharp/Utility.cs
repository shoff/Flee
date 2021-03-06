// ' This library is free software; you can redistribute it and/or
// ' modify it under the terms of the GNU Lesser General Public License
// ' as published by the Free Software Foundation; either version 2.1
// ' of the License, or (at your option) any later version.
// ' 
// ' This library is distributed in the hope that it will be useful,
// ' but WITHOUT ANY WARRANTY; without even the implied warranty of
// ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// ' Lesser General Public License for more details.
// ' 
// ' You should have received a copy of the GNU Lesser General Public
// ' License along with this library; if not, write to the Free
// ' Software Foundation, Inc., 59 Temple Place, Suite 330, Boston,
// ' MA 02111-1307, USA.
// ' 
// ' Flee - Fast Lightweight Expression Evaluator
// ' Copyright � 2007 Eugene Ciloci
// ' Updated to .net 4.6 Copyright 2017 Steven Hoff

namespace FleeSharp
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Reflection.Emit;

    internal class Utility
    {
        private Utility()
        {
        }

        public static void AssertNotNull(object o, string paramName)
        {
            var flag = o == null;
            if (flag)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        public static void EmitStoreLocal(FleeIlGenerator ilg, int index)
        {
            var flag = (index >= 0) & (index <= 3);
            if (flag)
            {
                switch (index)
                {
                    case 0:
                        ilg.Emit(OpCodes.Stloc_0);
                        break;
                    case 1:
                        ilg.Emit(OpCodes.Stloc_1);
                        break;
                    case 2:
                        ilg.Emit(OpCodes.Stloc_2);
                        break;
                    case 3:
                        ilg.Emit(OpCodes.Stloc_3);
                        break;
                }
            }
            else
            {
                Debug.Assert(index < 256, "local index too large");
                ilg.Emit(OpCodes.Stloc_S, (byte) index);
            }
        }

        public static void EmitLoadLocal(FleeIlGenerator ilg, int index)
        {
            Debug.Assert(index >= 0, "Invalid index");
            var flag = (index >= 0) & (index <= 3);
            if (flag)
            {
                switch (index)
                {
                    case 0:
                        ilg.Emit(OpCodes.Ldloc_0);
                        break;
                    case 1:
                        ilg.Emit(OpCodes.Ldloc_1);
                        break;
                    case 2:
                        ilg.Emit(OpCodes.Ldloc_2);
                        break;
                    case 3:
                        ilg.Emit(OpCodes.Ldloc_3);
                        break;
                }
            }
            else
            {
                Debug.Assert(index < 256, "local index too large");
                ilg.Emit(OpCodes.Ldloc_S, (byte) index);
            }
        }

        public static void EmitLoadLocalAddress(FleeIlGenerator ilg, int index)
        {
            Debug.Assert(index >= 0, "Invalid index");
            var flag = index <= 255;
            if (flag)
            {
                ilg.Emit(OpCodes.Ldloca_S, (byte) index);
            }
            else
            {
                ilg.Emit(OpCodes.Ldloca, index);
            }
        }

        public static void EmitArrayLoad(FleeIlGenerator ilg, Type elementType)
        {
            switch (Type.GetTypeCode(elementType))
            {
                case TypeCode.Object:
                case TypeCode.String:
                    ilg.Emit(OpCodes.Ldelem_Ref);
                    return;
                case TypeCode.Boolean:
                case TypeCode.SByte:
                    ilg.Emit(OpCodes.Ldelem_I1);
                    return;
                case TypeCode.Byte:
                    ilg.Emit(OpCodes.Ldelem_U1);
                    return;
                case TypeCode.Int16:
                    ilg.Emit(OpCodes.Ldelem_I2);
                    return;
                case TypeCode.UInt16:
                    ilg.Emit(OpCodes.Ldelem_U2);
                    return;
                case TypeCode.Int32:
                    ilg.Emit(OpCodes.Ldelem_I4);
                    return;
                case TypeCode.UInt32:
                    ilg.Emit(OpCodes.Ldelem_U4);
                    return;
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    ilg.Emit(OpCodes.Ldelem_I8);
                    return;
                case TypeCode.Single:
                    ilg.Emit(OpCodes.Ldelem_R4);
                    return;
                case TypeCode.Double:
                    ilg.Emit(OpCodes.Ldelem_R8);
                    return;
            }
            ilg.Emit(OpCodes.Ldelema, elementType);
            ilg.Emit(OpCodes.Ldobj, elementType);
        }

        public static void EmitArrayStore(FleeIlGenerator ilg, Type elementType)
        {
            switch (Type.GetTypeCode(elementType))
            {
                case TypeCode.Object:
                case TypeCode.String:
                    ilg.Emit(OpCodes.Stelem_Ref);
                    return;
                case TypeCode.Boolean:
                case TypeCode.SByte:
                case TypeCode.Byte:
                    ilg.Emit(OpCodes.Stelem_I1);
                    return;
                case TypeCode.Int16:
                case TypeCode.UInt16:
                    ilg.Emit(OpCodes.Stelem_I2);
                    return;
                case TypeCode.Int32:
                case TypeCode.UInt32:
                    ilg.Emit(OpCodes.Stelem_I4);
                    return;
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    ilg.Emit(OpCodes.Stelem_I8);
                    return;
                case TypeCode.Single:
                    ilg.Emit(OpCodes.Stelem_R4);
                    return;
                case TypeCode.Double:
                    ilg.Emit(OpCodes.Stelem_R8);
                    return;
            }
            ilg.Emit(OpCodes.Stelem, elementType);
        }

        public static void SyncFleeILGeneratorLabels(FleeIlGenerator source, FleeIlGenerator target)
        {
            while (source.LabelCount != target.LabelCount)
            {
                target.DefineLabel();
            }
        }

        public static bool IsIntegralType(Type t)
        {
            var tc = Type.GetTypeCode(t);
            return tc - TypeCode.SByte <= 7;
        }

        public static Type GetBitwiseOpType(Type leftType, Type rightType)
        {
            var flag = !IsIntegralType(leftType) || !IsIntegralType(rightType);
            var getBitwiseOpType = flag ? null : ImplicitConverter.GetBinaryResultType(leftType, rightType);
            return getBitwiseOpType;
        }

        public static MethodInfo GetSimpleOverloadedOperator(string name, Type sourceType, Type destType)
        {
            var data = new Hashtable {{"Name", "op_" + name}, {"sourceType", sourceType}, {"destType", destType}};
            var members = sourceType.FindMembers(MemberTypes.Method, BindingFlags.Static | BindingFlags.Public,
                SimpleOverloadedOperatorFilter, data);
            var flag = members.Length == 0;
            if (flag)
            {
                members = destType.FindMembers(MemberTypes.Method, BindingFlags.Static | BindingFlags.Public,
                    SimpleOverloadedOperatorFilter, data);
            }
            Debug.Assert(members.Length < 2, "Multiple overloaded operators found");
            var flag2 = members.Length == 0;
            MethodInfo getSimpleOverloadedOperator;
            if (flag2)
            {
                getSimpleOverloadedOperator = null;
            }
            else
            {
                getSimpleOverloadedOperator = (MethodInfo) members[0];
            }
            return getSimpleOverloadedOperator;
        }

        private static bool SimpleOverloadedOperatorFilter(MemberInfo member, object value)
        {
            var data = (IDictionary) value;
            var method = (MethodInfo) member;
            var nameMatch = method.IsSpecialName && method.Name.Equals((string) data["Name"], StringComparison.OrdinalIgnoreCase);
            var flag = !nameMatch;
            bool simpleOverloadedOperatorFilter;
            if (flag)
            {
                simpleOverloadedOperatorFilter = false;
            }
            else
            {
                var returnTypeMatch = method.ReturnType == (Type) data["destType"];
                var flag2 = !returnTypeMatch;
                if (flag2)
                {
                    simpleOverloadedOperatorFilter = false;
                }
                else
                {
                    var parameters = method.GetParameters();
                    var argumentMatch = parameters.Length > 0 && parameters[0].ParameterType == (Type) data["sourceType"];
                    simpleOverloadedOperatorFilter = argumentMatch;
                }
            }
            return simpleOverloadedOperatorFilter;
        }

        public static MethodInfo GetOverloadedOperator(string name, Type sourceType, Binder binder, params Type[] argumentTypes)
        {
            name = "op_" + name;
            var mi = sourceType.GetMethod(name, BindingFlags.Static | BindingFlags.Public, binder, CallingConventions.Any,
                argumentTypes, null);
            var flag = mi == null || !mi.IsSpecialName;
            var getOverloadedOperator = flag ? null : mi;
            return getOverloadedOperator;
        }

        public static int GetILGeneratorLength(ILGenerator ilg)
        {
            var fi = typeof(ILGenerator).GetField("m_length", BindingFlags.Instance | BindingFlags.NonPublic);
            return (int) fi.GetValue(ilg);
        }

        public static bool IsLongBranch(int startPosition, int endPosition)
        {
            return endPosition - startPosition > 127;
        }

        public static string FormatList(string[] items)
        {
            var separator = CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ";
            return string.Join(separator, items);
        }

        public static string GetGeneralErrorMessage(string key, params object[] args)
        {
            var msg = FleeResourceManager.Instance.GetGeneralErrorString(key);
            return string.Format(msg, args);
        }

        public static string GetCompileErrorMessage(string key, params object[] args)
        {
            var msg = FleeResourceManager.Instance.GetCompileErrorString(key);
            return string.Format(msg, args);
        }
    }
}