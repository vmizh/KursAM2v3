using System.Collections.Generic;
using System.Data.Entity;
using Core;
using Core.ViewModel.Common;

namespace KursAM2.Managers
{
    public class ProjectManager
    {
        public List<Project> LoadProjects()
        {
            var ret = new List<Project>();
            using (var ctx = GlobalOptions.GetEntities())
            {
                foreach (var p in ctx.Projects.Include(_ => _.ProjectsDocs))
                {
                    ret.Add(new Project(p));
                }
            }
            return ret;
        }
    }
}