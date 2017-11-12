using System;
using Microsoft.Xrm.Sdk;

namespace Dynamics.Plugin.Helper
{
    /// <summary>
    /// Base class with multiple properties for the plugins
    /// </summary>
    public abstract class PluginBase : IPlugin
    {
        protected readonly bool TracingEnabled;
        protected IPluginExecutionContext Context;
        protected IOrganizationService OrgService;
        protected ITracingService TracingService;
        protected PluginConfiguration PluginConfiguration;

        protected Entity PreImage;
        protected Entity PostImage;
        protected Entity InputParameter;
        protected EntityReference InputReference;


        protected PluginBase(string unsecureString, string secureString)
        {
            if (!string.IsNullOrWhiteSpace(unsecureString))
            {
                try
                {
                    PluginConfiguration = new PluginConfiguration(unsecureString);
                    TracingEnabled = PluginConfiguration.GetConfigDataBool("TracingEnabled");
                }
                catch (Exception)
                {
                    throw new InvalidOperationException("Error during parsing the tracing setting of unsecure setting.");
                }
            }

            if (string.IsNullOrWhiteSpace(secureString)) return;

            try
            {
                PluginConfiguration = new PluginConfiguration(secureString);
                TracingEnabled = PluginConfiguration.GetConfigDataBool("TracingEnabled");
            }
            catch (Exception)
            {
                throw new InvalidOperationException("Error during parsing the tracing setting of secure setting.");
            }
        }

        /// <summary>
        /// Method called by crm. The method sets multiple propterties befor calling the actual custom plugin code
        /// </summary>
        /// <param name="serviceProvider"></param>
        public void Execute(IServiceProvider serviceProvider)
        {
            // Obtain the execution context from the service provider.
            Context =
                (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            // Get a reference to the Organization service.
            var factory =
                (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            OrgService = factory.CreateOrganizationService(Context.UserId);

            if (TracingEnabled)
                //Tracing service for logging
                TracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            if (Context.PreEntityImages.Contains("Target"))
                PreImage = Context.PreEntityImages["Target"];

            if (Context.PostEntityImages.Contains("Target"))
                PostImage = Context.PostEntityImages["Target"];

            if (Context.InputParameters.Contains("Target") && Context.InputParameters["Target"] is Entity)
                InputParameter = (Entity)Context.InputParameters["Target"];

            // if it's a delete request, it's an EntityReference
            if (Context.InputParameters.Contains("Target") && Context.InputParameters["Target"] is EntityReference)
                InputReference = (EntityReference)Context.InputParameters["Target"];

            CustomExecute(serviceProvider);
        }

        /// <summary>
        /// Method the be overridden in the custom plugin code
        /// </summary>
        /// <param name="serviceProvider"></param>
        public abstract void CustomExecute(IServiceProvider serviceProvider);

        /// <summary>
        /// List of common message types
        /// </summary>
        public enum Messages
        {
            Assign,
            Create, // Triggered when the record is created.
            Delete, // Triggered when the record is deleted.
            GrantAccess,
            ModifyAccess,
            Retrieve, // Triggered when record is retrieved, for example when user opens a record on a form.
            RetrieveMultiple, // Triggered when a record set is retrieved using RetrieveMultiple, for example showing a view.
            RetrievePrincipalAccess,
            RetrieveSharedPrincipalsAndAccess,
            RevokeAccess,
            SetState,
            SetStateDynamicEntity,
            Update // Triggered when the record is updated.
        }
    }
}