using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GenerateDataContract.SqlTable;
using JetBrains.Annotations;

namespace GenerateDataContract
{
    public class GenerateWrapper
    {
        private readonly List<Table> myTables;
        private readonly List<string> myExcludeTableName; 

        public GenerateWrapper([NotNull]List<Table> tables)
        {
            myTables = tables;
        }

        public GenerateWrapper([NotNull]List<Table> tables, List<string> excludeNames )
        {
            myTables = tables;
            myExcludeTableName = excludeNames;
        }

        public string Generate()
        {
            var sb = new StringBuilder();
            sb.Append(GenerateWrapperHeader());
            foreach (var t in myTables)
            {
                if (myExcludeTableName.All(_ => _.ToUpper() != t.Name.ToUpper()))
                    sb.Append(GenerateOneWrapper(t.Name));
            }
            sb.Append(GenerateWrapperFooter());
            return sb.ToString();
        }

        private string GenerateWrapperHeader()
        {
            Console.WriteLine("Wrapper: Заголовок");
            var sb = new StringBuilder();
            sb.Append("using AutoMapper;\n");
            sb.Append("using AutoMapper.Mappers;\n"); 
            sb.Append("namespace ContractEntityWrapper\n {\n");
            sb.Append("public partial class BaseWrapper\n {\n");
            sb.Append("public MappingEngine BaseMappingEngine { set; get; }\n");
            sb.Append("public MappingEngine FullMappingEngine { set; get; }\n");
            sb.Append("public BaseWrapper()\n {\n");
            sb.Append(
                "ConfigurationStore baseConfig = new ConfigurationStore(new TypeMapFactory(),MapperRegistry.Mappers);");
            sb.Append("BaseMappingEngine = new MappingEngine(baseConfig);");
            sb.Append(
                "ConfigurationStore fullConfig = new ConfigurationStore(new TypeMapFactory(),MapperRegistry.Mappers);");
            sb.Append("FullMappingEngine = new MappingEngine(fullConfig);");


            return sb.ToString();
        }


        private string GenerateOneWrapper(string name)
        {
            string cond = string.Empty;
            Console.WriteLine("Wrapper: {0}",name);
            var sb = new StringBuilder();
            sb.Append($"baseConfig.CreateMap<DataEntity.{name}, DataContract.{name}>(){cond};\n");
            sb.Append($"fullConfig.CreateMap<DataEntity.{name}, DataContract.{name}>();\n");
            return sb.ToString();
        }

        private string GenerateWrapperFooter()
        {
            Console.WriteLine("Wrapper: Конец");
            var sb = new StringBuilder();
            sb.Append("}\n}\n}\n");
            return sb.ToString();
        }
    }
}