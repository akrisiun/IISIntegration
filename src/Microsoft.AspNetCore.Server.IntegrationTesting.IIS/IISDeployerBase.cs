// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Server
{
    // workarounds for MicrosoftAspNetCoreServerIntegrationTestingPackageVersion 0.6.0

    public class DeploymentParameters051
    {
        public static AncmVersion AncmVersion { get => AncmVersion.AspNetCoreModuleV2; }
    }

    public class AncmVersion
    {
        public AncmVersion()
        {
            AspNetCoreModuleV1 = new AncmVersion { Version = "V1" };
            AspNetCoreModuleV2 = new AncmVersion { Version = "V2" };
        }

        public static AncmVersion AspNetCoreModuleV1 { get; private set; }
        public static AncmVersion AspNetCoreModuleV2 { get; private set; }

        public string Version { get; set; }
    }

    public class TestVariant
    {
        public string applicationPath { get; set; }
        public ServerType serverType { get; set; }
        public RuntimeFlavor runtimeFlavor { get; set; }
        public RuntimeArchitecture runtimeArchitecture { get; set; }
    }

    public class DotNetCommands
    {
        public static bool IsRunningX86OnX64(RuntimeArchitecture architecture)
            => architecture == RuntimeArchitecture.x64;  // ???

        public static string GetDotNetExecutable(RuntimeArchitecture architecture)
            => architecture == RuntimeArchitecture.x64 ?
               @"c:\Program Files\dotnet\dotnet.exe" :
               @"c:\Program Files (x86)\dotnet\dotnet.exe"; 

    }
}

namespace Microsoft.AspNetCore.Server.IntegrationTesting.IIS
{
    public abstract class IISDeployerBase : ApplicationDeployer
    {
        public IISDeploymentParameters IISDeploymentParameters { get; }

        public IISDeployerBase(IISDeploymentParameters deploymentParameters, ILoggerFactory loggerFactory)
            : base(deploymentParameters, loggerFactory)
        {
            IISDeploymentParameters = deploymentParameters;
        }

        protected void RunWebConfigActions(string contentRoot)
        {
            var actions = GetWebConfigActions();
            if (!actions.Any())
            {
                return;
            }

            if (!DeploymentParameters.PublishApplicationBeforeDeployment)
            {
                throw new InvalidOperationException("Cannot modify web.config file if no published output.");
            }

            var path = Path.Combine(DeploymentParameters.PublishedApplicationRootPath, "web.config");
            var webconfig = XDocument.Load(path);

            foreach (var action in actions)
            {
                action.Invoke(webconfig.Root, contentRoot);
            }

            webconfig.Save(path);
        }

        protected virtual IEnumerable<Action<XElement, string>> GetWebConfigActions()
        {
            if (IISDeploymentParameters.HandlerSettings.Any())
            {
                yield return AddHandlerSettings;
            }

            if (IISDeploymentParameters.WebConfigBasedEnvironmentVariables.Any())
            {
                yield return AddWebConfigEnvironmentVariables;
            }

            foreach (var action in IISDeploymentParameters.WebConfigActionList)
            {
                yield return action;
            }
        }

        protected virtual IEnumerable<Action<XElement, string>> GetServerConfigActions()
        {
            foreach (var action in IISDeploymentParameters.ServerConfigActionList)
            {
                yield return action;
            }
        }

        public void RunServerConfigActions(XElement config, string contentRoot)
        {
            foreach (var action in GetServerConfigActions())
            {
                action.Invoke(config, contentRoot);
            }
        }

        protected string GetAncmLocation(AncmVersion version)
        {
            var ancmDllName = version == AncmVersion.AspNetCoreModuleV2 ? "aspnetcorev2.dll" : "aspnetcore.dll";
            var arch = DeploymentParameters.RuntimeArchitecture == RuntimeArchitecture.x64 ? $@"x64\{ancmDllName}" : $@"x86\{ancmDllName}";
            var ancmFile = Path.Combine(AppContext.BaseDirectory, arch);
            if (!File.Exists(Environment.ExpandEnvironmentVariables(ancmFile)))
            {
                ancmFile = Path.Combine(AppContext.BaseDirectory, ancmDllName);
                if (!File.Exists(Environment.ExpandEnvironmentVariables(ancmFile)))
                {
                    throw new FileNotFoundException("AspNetCoreModule could not be found.", ancmFile);
                }
            }

            return ancmFile;
        }

        private void AddWebConfigEnvironmentVariables(XElement element, string contentRoot)
        {
            var environmentVariables = element
                .Descendants("system.webServer")
                .Single()
                .RequiredElement("aspNetCore")
                .GetOrAdd("environmentVariables");

            foreach (var envVar in IISDeploymentParameters.WebConfigBasedEnvironmentVariables)
            {
                environmentVariables.GetOrAdd("environmentVariable", "name", envVar.Key)
                    .SetAttributeValue("value", envVar.Value);
            }
        }

        private void AddHandlerSettings(XElement element, string contentRoot)
        {
            var handlerSettings = element
                .Descendants("system.webServer")
                .Single()
                .RequiredElement("aspNetCore")
                .GetOrAdd("handlerSettings");

            foreach (var handlerSetting in IISDeploymentParameters.HandlerSettings)
            {
                handlerSettings.GetOrAdd("handlerSetting", "name", handlerSetting.Key)
                    .SetAttributeValue("value", handlerSetting.Value);
            }
        }

        protected void ConfigureModuleAndBinding(XElement config, string contentRoot, int port)
        {
            var siteElement = config
                .RequiredElement("system.applicationHost")
                .RequiredElement("sites")
                .RequiredElement("site");

            siteElement
                .RequiredElement("application")
                .RequiredElement("virtualDirectory")
                .SetAttributeValue("physicalPath", contentRoot);

            siteElement
                .RequiredElement("bindings")
                .RequiredElement("binding")
                .SetAttributeValue("bindingInformation", $":{port}:localhost");

            var ancmVersion = AncmVersion.AspNetCoreModuleV2; // .ToString();
            config
                .RequiredElement("system.webServer")
                .RequiredElement("globalModules")
                .GetOrAdd("add", "name", ancmVersion.ToString())
                .SetAttributeValue("image", GetAncmLocation(ancmVersion)); // DeploymentParameters.AncmVersion));
        }

        public abstract void Dispose(bool gracefulShutdown);
    }
}
