﻿using Neo.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vmtool
{

    public class FuncExport
    {
        static string ConvType(string _type)
        {
            switch (_type)
            {
                case "__Signature":
                    return "Signature";

                case "System.Boolean":
                    return "Boolean";

                case "System.Byte":
                case "System.SByte":
                case "System.Int16":
                case "System.UInt16":
                case "System.Int32":
                case "System.UInt32":
                case "System.Int64":
                case "System.UInt64":
                case "System.Numerics.BigInteger":
                    return "Integer";

                case "__Hash160":
                    return "Hash160";

                case "__Hash256":
                    return "Hash256";

                case "System.Byte[]":
                    return "ByteArray";

                case "__PublicKey":
                    return "PublicKey";

                case "System.String":
                    return "String";

                case "System.Object[]":
                    return "Array";

                case "__InteropInterface":
                    return "InteropInterface";

                case "System.Void":
                    return "Void";

                case "System.Object":
                    return "Any";
            }
            if (_type.Contains("[]"))
                return "Array";

            return "Unknown:" + _type;
        }
        public static MyJson.JsonNode_Object Export(AntsModule module,byte[] script)
        {
            var sha256 = System.Security.Cryptography.SHA256.Create();
            byte[] hash256 = sha256.ComputeHash(script);
            var ripemd160 = new Neo.Cryptography.RIPEMD160Managed();
            var hash = ripemd160.ComputeHash(hash256);

            var outjson = new MyJson.JsonNode_Object();

            //hash
            StringBuilder sb = new StringBuilder();
            foreach (var b in hash)
            {
                sb.Append(b.ToString("X02"));
            }
            outjson.SetDictValue("hash", sb.ToString());

            //entrypoint
            outjson.SetDictValue("entrypoint", "Main");
            if (module.mapMethods.ContainsKey("Main"))
            {
                var name = module.mapMethods["Main"].displayName;
                outjson.SetDictValue("entrypoint", name);
            }
            //functions
            var funcsigns = new MyJson.JsonNode_Array();
            outjson["functions"] = funcsigns;
         
            foreach (var function in module.mapMethods)
            {
                var mm = function.Value;
                if (mm.isPublic == false)
                    continue;
                var ps = mm.name.Split(new char[] { ' ', '(' }, StringSplitOptions.RemoveEmptyEntries);
                var funcsign = new MyJson.JsonNode_Object();

                funcsigns.Add(funcsign);
                var funcname = ps[1];
                if (funcname.IndexOf("::") > 0)
                {
                    var sps = funcname.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
                    funcname = sps.Last();
                }
                funcsign.SetDictValue("name", function.Value.displayName);
                var rtype = ConvType(mm.returntype);
                funcsign.SetDictValue("returntype", rtype);
                MyJson.JsonNode_Array funcparams = new MyJson.JsonNode_Array();
                funcsign["paramaters"] = funcparams;
                if (mm.paramtypes != null)
                {
                    foreach (var v in mm.paramtypes)
                    {
                        var ptype = ConvType(v.type);
                        var item = new MyJson.JsonNode_Object();
                        funcparams.Add(item);

                        item.SetDictValue("name", v.name);
                        item.SetDictValue("type", ptype);
                    }
                }
            }

            //events
            var eventsigns = new MyJson.JsonNode_Array();
            outjson["events"] = eventsigns;
            foreach (var events in module.mapEvents)
            {
                var mm = events.Value;

                var ps = mm.name.Split(new char[] { ' ', '(' }, StringSplitOptions.RemoveEmptyEntries);
                var funcsign = new MyJson.JsonNode_Object();

                eventsigns.Add(funcsign);

                funcsign.SetDictValue("name", events.Value.displayName);
                var rtype = ConvType(mm.returntype);
                funcsign.SetDictValue("returntype", rtype);
                MyJson.JsonNode_Array funcparams = new MyJson.JsonNode_Array();
                funcsign["paramaters"] = funcparams;
                if (mm.paramtypes != null)
                {
                    foreach (var v in mm.paramtypes)
                    {
                        var ptype = ConvType(v.type);
                        var item = new MyJson.JsonNode_Object();
                        funcparams.Add(item);

                        item.SetDictValue("name", v.name);
                        item.SetDictValue("type", ptype);
                    }
                }
            }

            return outjson;
        }
    }
}
