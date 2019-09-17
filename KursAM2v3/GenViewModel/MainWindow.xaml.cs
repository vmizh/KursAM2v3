using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using Data;

namespace GenViewModel
{
    public class RSType
    {
        public string Name { set; get; }
        public Type Type { set; get; }
    }
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public List<RSType> Types { set; get; } 
        public MainWindow()
        {
            InitializeComponent();
            Types = new List<RSType>();
            var tt = Assembly.LoadFrom("Data.dll").GetTypes();
            //Type[] typelist = Assembly.().GetTypes().Where(t => t.Namespace == "Data").ToArray();
            foreach (Type type in tt)
            {
                Types.Add(new RSType
                {
                    Name = type.Name,
                    Type = type
                });
                //создание объекта
                //object targetObject = Activator.CreateInstance(Type.GetType(type.Name));

                ////что бы получить public методы без базовых(наследованных от object)
                //var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                //foreach (var methodInfo in methods)
                //{
                //    //вызов
                //    methodInfo.Invoke(targetObject, new object[] { });
                //}
            }
            typeComboBox.ItemsSource = Types;
            //GetTextViewModel();
        }

        public void GetTextViewModel()
        {
            var s = GetViewModel(typeof(NOM_SKLAD_CURRENCY_PRICE));
            richEdit.Text = s;
        }

        public void GetTextViewModel(Type type)
        {
            var s = GetViewModel(type);
            richEdit.Text = s;

        }

        private string GetViewModel(Type tbl)
        {
            var res = new StringBuilder();
            res.Append(string.Format("public class {0}ViewModel : RSViewModelBase, IEntity<{0}> \n {{ \n",
                tbl.Name));
            res.Append(string.Format("public {0}ViewModel() {{ Entity = new {0}{{ DOC_CODE = -1 }}; }}\n", tbl.Name));
            res.Append(string.Format("public {0}ViewModel({0} entity) {{ Entity = entity ?? new {0}{{ DOC_CODE = -1 }}; }}\n", tbl.Name));
            res.Append($"private {tbl.Name} myEntity; \n");
            res.Append($"public {tbl.Name} Entity \n ");
            res.Append("{ get { return myEntity; }");
            res.Append(
                "set { if(myEntity == value) return; \n myEntity = value; \n RaisePropertyChanged();\n }\n");
            res.Append("} \n");
            res.Append($"public virtual {tbl.Name} Load(decimal dc) {{ throw new NotImplementedException(); }}\n");
            res.Append($"public virtual {tbl.Name} Load(Guid id) {{ throw new NotImplementedException(); }}\n");
            res.Append($"public virtual void Save({tbl.Name} doc) {{ throw new NotImplementedException(); }}\n");
        
            foreach (var prop in tbl.GetProperties())
            {
                res.Append(PropertyGenerate(prop));
                if (prop.Name == "DOC_CODE")
                {
                    res.Append(
                        "public override decimal DocCode \n {{ \n get => Entity.DOC_CODE; \n set \n {{ \n " +
                        "if (Entity.DOC_CODE == value) return; \n Entity.DOC_CODE = value; \n RaisePropertyChanged(); \n }} \n");
                }
   
            }
            res.Append($"public void UpdateFrom({tbl.Name} ent) {{\n");
            foreach (var prop in tbl.GetProperties().Where(prop => prop.Name != "DOC_CODE"))
            {
                res.Append(string.Format("{0} = ent.{0};\n",prop.Name));
            }
            res.Append("} \n");
            res.Append($"public void UpdateTo({tbl.Name} ent) {{\n");
            foreach (var prop in tbl.GetProperties().Where(prop => prop.Name != "DOC_CODE"))
            {
                res.Append(string.Format("ent.{0} = {0};\n", prop.Name));
            }
            res.Append("} \n");
            res.Append("} \n");
            return res.ToString();
        }

        private string PropertyGenerate(PropertyInfo propInfo)
        {
            var res = new StringBuilder();
            if (propInfo.PropertyType.Name == "ICollection`1") return null;
            res.Append(
                $"public {(propInfo.PropertyType.IsGenericType && propInfo.PropertyType.GetGenericTypeDefinition() == typeof (Nullable<>) ? propInfo.PropertyType.GetGenericArguments()[0] + "?" : propInfo.PropertyType.Name)} {propInfo.Name}  \n "
                    .Replace("System.", ""));
            res.Append("{ \n");
            res.Append($"get {{ return Entity.{propInfo.Name}; }}");
            res.Append(
                string.Format(
                    "set {{ if(Entity.{0} == value) return; \n Entity.{0} = value; \n RaisePropertyChanged();\n }}",
                    propInfo.Name));
            res.Append("} \n");
            return res.ToString();
        }

        private void typeComboBox_SelectedIndexChanged(object sender, RoutedEventArgs e)
        {
            if (!(typeComboBox.SelectedItem is RSType d)) return;
            GetTextViewModel(d.Type);
        }

        private string GenerateNotAutogenerateOne(string typeName)
        {
            StringBuilder ret = new StringBuilder();
            ret.Append(
                $"var {typeName}_s = typeof(T).GetProperties().Where(c => c.PropertyType == typeof({typeName}));\n");
            ret.Append($"foreach (var prop in {typeName}_s)\n");
            ret.Append("{\n");
            ret.Append($"builder.Property(Lambda<T, {typeName}>(prop.Name)).NotAutoGenerated();\n");
            ret.Append("}\n\n");

            return ret.ToString();
        }

        private void GenerateNotAutogenerate()
        {
            var datatext = new StringBuilder();
            foreach (var t in Types )
            {
                datatext.Append(GenerateNotAutogenerateOne(t.Name));
            }
            richEdit.Text = datatext.ToString();

        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            GenerateNotAutogenerate();
        }
    }
}