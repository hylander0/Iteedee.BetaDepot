using Iteedee.BetaDepot.Messaging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;

namespace Iteedee.BetaDepot.Common
{
    public static class Messaging
    {
        private const string EMBEDDED_RESOURCE_BUILDNOTIFICATION_HTML = @"BuildNotification.html";
        private const string TEMPLATE_KEY_UPLOADED_BY = "UPLOADED_BY";
        private const string TEMPLATE_KEY_APP_NAME = "APP_NAME";
        private const string TEMPLATE_KEY_APP_IDENTIFIER = "APP_IDENTIFIER";
        private const string TEMPLATE_KEY_VERSION = "VERSION";
        private const string TEMPLATE_KEY_TARGET_ENVIRONMENT = "TARGET_ENVIRONMENT";
        private const string TEMPLATE_KEY_NOTES = "NOTES";
        private const string TEMPLATE_KEY_DATE_UPLOADED = "DATE_UPLOADED";
        private const string TEMPLATE_KEY_BETA_DEPOT_HOME_URL = "BETA_DEPOT_HOME_URL";
        private const string TEMPLATE_KEY_BETA_DEPOT_APP_DOWNLOAD_URL = "BETA_DEPOT_APP_DOWNLOAD_URL";

        public static void NotifyTeamOfNewBuild(int BuildId)
        {
            Dictionary<string,string> NotificationData = new Dictionary<string,string>();
            List<String> TeamMemberEmails = new List<string>();
            using(var context = new Repository.BetaDepotContext())
            {
                var build = context.Builds.Where(w => w.Id == BuildId).FirstOrDefault();
                NotificationData.Add(TEMPLATE_KEY_UPLOADED_BY, string.Format("{0} {1}", build.AddedBy.FirstName, build.AddedBy.LastName));
                NotificationData.Add(TEMPLATE_KEY_APP_NAME, build.Application.Name);
                NotificationData.Add(TEMPLATE_KEY_APP_IDENTIFIER, build.Application.ApplicationIdentifier);
                NotificationData.Add(TEMPLATE_KEY_VERSION, build.versionNumber);
                NotificationData.Add(TEMPLATE_KEY_TARGET_ENVIRONMENT, build.Environment.EnvironmentName);
                NotificationData.Add(TEMPLATE_KEY_NOTES, build.Notes);
                NotificationData.Add(TEMPLATE_KEY_DATE_UPLOADED, String.Format("{0:g}", build.AddedDtm));
                NotificationData.Add(TEMPLATE_KEY_BETA_DEPOT_HOME_URL, System.Configuration.ConfigurationManager.AppSettings["FullyQualifiedBaseUrl"]);
                NotificationData.Add(TEMPLATE_KEY_BETA_DEPOT_APP_DOWNLOAD_URL, Platforms.Common.GeneratePackageInstallUrl("App", "Download", build.Platform, build.UniqueIdentifier.ToString()));
                TeamMemberEmails = context.ApplicationTeamMembers.Where(w => w.ApplicationId == build.Application.Id)
                                                                    .Select(s => s.TeamMember.EmailAddress)
                                                                    .ToList();
            }

            string template = GetEmbeddedResource(EMBEDDED_RESOURCE_BUILDNOTIFICATION_HTML);
            string emailBody = Iteedee.TextTemplater.Templify.PopulateTemplate(template, NotificationData);
            TeamMemberEmails.ForEach(f => {
                SendEmail(f, "Beta Depot - Build Notification", emailBody);
            });
            
        }
        private static string GetEmbeddedResource(string resourceName)
        {
            String retval = string.Empty;
            var assembly = Assembly.GetExecutingAssembly();
            var fullResourceName = string.Format("Iteedee.BetaDepot.Common.MessageTemplates.{0}", resourceName);
            try
            {
                using (Stream stream = assembly.GetManifestResourceStream(fullResourceName))
                using (StreamReader reader = new StreamReader(stream))
                {
                    retval = reader.ReadToEnd();
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.EventLog.WriteEntry("Application", String.Format(@"##Iteedee.BetaDepot.Common.MessageTemplates## Failed to get Email Template Resource: {0} {1}", ex.Message, ex.StackTrace));
            }
            return retval;
        }
        private static void SendEmail(string emailAddress, string subject, string body)
        {
            Task.Run(() => {
                if (MessageProviderManager.Default != null)
                    MessageProviderManager.Default.SendMessage(MessageType.Email, emailAddress, subject, body); 
            });
           
        }
    }

    public class MessageProviderManager
    {
        static MessageProviderManager()
        {
            Initialize();
        }

        private static MessageProviderBase _default;
        /// <summary>
        /// Returns the default configured data provider
        /// </summary>
        public static MessageProviderBase Default
        {
            get 
            { 
                return _default; 
            }
        }

        private static MessageProviderCollection _providerCollection;
        /// <summary>
        /// .Returns the provider collection
        /// </summary>
        public static MessageProviderCollection Providers
        {
            get { return _providerCollection; }
        }

        private static ProviderSettingsCollection _providerSettings;
        public static ProviderSettingsCollection ProviderSettings
        {
            get { return _providerSettings; }
        }

        /// <summary>
        /// Reads the configuration related to the set of configured 
        /// providers and sets the default and collection of providers and settings.
        /// </summary>
        private static void Initialize()
        {
            MessageProviderConfiguration configSection = (MessageProviderConfiguration)ConfigurationManager.GetSection("MessageProviders");
            if (configSection == null)
                throw new ConfigurationErrorsException("Data provider section is not set.");

            _providerCollection = new MessageProviderCollection();
            ProvidersHelper.InstantiateProviders(configSection.Providers, _providerCollection, typeof(MessageProviderBase));

            _providerSettings = configSection.Providers;

            if (_providerCollection.Count > 0)
            {
                if (_providerCollection[configSection.DefaultProviderName] == null)
                    throw new ConfigurationErrorsException("Default data provider is not set.");
                _default = _providerCollection[configSection.DefaultProviderName];
                var defaultSettings = _providerSettings[configSection.DefaultProviderName];

                _default.SetParameters(defaultSettings.Parameters);
            }

        }
    }
}