using System;
using System.Linq;
using System.Web.Security;
using Core;

namespace DevExpress.Providers
{
    public class CustomeRoleProvider : RoleProvider
    {
        public override string ApplicationName { get; set; }

        public override bool IsUserInRole(string username, string roleName)
        {
            var outputResult = false;
            using (var ctx = GlobalOptions.GetEntities())
            {
                var user = ctx.EXT_USERS.FirstOrDefault(_ =>
                    string.Equals(_.USR_NICKNAME, username, StringComparison.CurrentCultureIgnoreCase));
                if (user != null)
                    outputResult = user.EXT_GROUPS?.FirstOrDefault(_ =>
                                       string.Equals(_.GR_NAME, roleName, StringComparison.CurrentCultureIgnoreCase)) !=
                                   null;
            }
            return outputResult;
        }

        public override string[] GetRolesForUser(string username)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var user = ctx.EXT_USERS.FirstOrDefault(_ => _.USR_NICKNAME.ToUpper() == username.ToUpper());
                if (user == null) return new string[] { };
                var roles = new string[user.EXT_GROUPS.Count];
                var i = 0;
                foreach (var grp in user.EXT_GROUPS)
                {
                    roles[i] = grp.GR_NAME;
                    ++i;
                }
                return roles;
            }
        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }
    }
}