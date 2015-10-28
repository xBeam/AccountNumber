using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

namespace CreateAccountNumber
{
    /// <summary>
    /// При создании Организации автоматически заполняется поле "Код организации"
    /// 
    /// Register: PostCreate of Account
    /// </summary>
    public class CreateAccountNumber : IPlugin
    {
        private crmContext xrmContext;
        private IOrganizationService service;
        private ITracingService t;

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context;
            InitializeServices(serviceProvider, out context);
            var isValidated = ValidatePlugin(context);
            if (!isValidated) return;

            var postImageEntity = context.PostEntityImages["Image"].ToEntity<Account>();

            var numberOfAccounts = xrmContext.AccountSet.Count();

            var myNewAccount = xrmContext.AccountSet.Single(c => c.AccountId == postImageEntity.AccountId);

            var newUpdatedAccount = new Account()
            {
                AccountId = postImageEntity.AccountId,
                AccountNumber = string.Format("ACC" + numberOfAccounts.ToString("D6") + "-SL")
            };

            xrmContext.Detach(myNewAccount);

            xrmContext.Attach(newUpdatedAccount);

            xrmContext.UpdateObject(newUpdatedAccount);

            xrmContext.SaveChanges();
        }

        private void InitializeServices(IServiceProvider serviceProvider, out IPluginExecutionContext context)
        {
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            ((IProxyTypesAssemblyProvider)context).ProxyTypesAssembly = typeof(Contact).Assembly;

            t = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            service = serviceFactory.CreateOrganizationService(context.UserId);

            xrmContext = new crmContext(service);
        }

        private static bool ValidatePlugin(IPluginExecutionContext context)
        {
            if (context.PrimaryEntityName != Account.EntityLogicalName)
            {
                return false;
            }
            return true;
        }
    }
}

      