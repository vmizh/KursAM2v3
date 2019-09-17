using System.Data.Entity;

namespace Core.WindowsManager
{
    public class StandartErrorManager : ErrorMessageBase
    {
        public StandartErrorManager(DbContext dbContext, string formName, bool isUIWindow)
        {
            EntityFrameworkContext = dbContext;
            FormName = formName;
            if (isUIWindow)
            {
                ShowUIWindow = true;
                ShowWindow = false;
            }
            else
            {
                ShowUIWindow = false;
                ShowWindow = true;
            }
        }
        public StandartErrorManager(DbContext dbContext, string formName)
        {
            EntityFrameworkContext = dbContext;
            FormName = formName;
            ShowUIWindow = false;
            ShowWindow = false;
        }
    }
}