using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml.Linq;

namespace StringResourceAssemblyWriter
{

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    public class CommentAttribute : Attribute {
        public string Comment { get; set; }
        public CommentAttribute(string comment) {
            Comment = comment;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var doc = XDocument.Load("../../StringResources.xml");
            var xitems = doc.Element("resources").Elements("item");

            var items = xitems.Select(i => new {
                Key = i.Attribute("key").Value,
                Value = i.Attribute("value").Value,
                Comment = i.Attribute("comment").Value,
            });

            var srName = new AssemblyName("StringResources");
            var srAssemBuilder = AssemblyBuilder.DefineDynamicAssembly(srName, AssemblyBuilderAccess.RunAndCollect);
            var srModBuilder = srAssemBuilder.DefineDynamicModule($"{srName.Name}.dll");

            var commentAttrCons = typeof(CommentAttribute).GetConstructors()[0];

            var srType = srModBuilder.DefineType("StringResources.Resource", TypeAttributes.Public);

            foreach (var item in items) {
                var srProp = srType.DefineProperty(item.Key, PropertyAttributes.HasDefault, typeof(string), Type.EmptyTypes);
                var srCommentAttr = new CustomAttributeBuilder(commentAttrCons, new object[] { item.Comment });
                srProp.SetCustomAttribute(srCommentAttr);

                var srPropGetter = srType.DefineMethod($"get_{item.Key}", MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Public | MethodAttributes.Static, typeof(string), Type.EmptyTypes);
                var getterIl = srPropGetter.GetILGenerator();
                getterIl.Emit(OpCodes.Ldstr, item.Value);
                getterIl.Emit(OpCodes.Ret);
                srProp.SetGetMethod(srPropGetter);
                srPropGetter.SetCustomAttribute(srCommentAttr);
            }
            var type = srType.CreateType();

            const string outputAssembly = "../../out/StringResources.dll";
            if (File.Exists(outputAssembly)) {
                File.Delete(outputAssembly);
            }

            var assemblyGenerator = new Lokad.ILPack.AssemblyGenerator();
            assemblyGenerator.GenerateAssembly(srAssemBuilder, outputAssembly);
        }
    }
}
