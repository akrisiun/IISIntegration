// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.IIS
{
    public class IISDeploymentParameters : DeploymentParameters
    {
        public class Params {
            static Params() {
                Default = new Params { applicationPath = "", serverType = ServerType.IIS,
                    runtimeArchitecture = RuntimeArchitecture.x64, runtimeFlavor = RuntimeFlavor.CoreClr };
            }
            public static Params Default { get; set; }

            public string applicationPath { get; set; }
            public  ServerType serverType { get; set; }
            public RuntimeFlavor runtimeFlavor { get; set; }
            public RuntimeArchitecture runtimeArchitecture { get; set; }

            public static Params TestVariant(TestVariant variant)
                => new Params {
                    applicationPath = variant.applicationPath,
                    serverType = variant.serverType,
                    runtimeArchitecture = variant.runtimeArchitecture,
                    runtimeFlavor = variant.runtimeFlavor
                };

            public static Params DeploymentParameters(DeploymentParameters parameters)
                 => new Params {
                     applicationPath = parameters.ApplicationPath,
                     serverType = parameters.ServerType,
                     runtimeArchitecture = parameters.RuntimeArchitecture,
                     runtimeFlavor = parameters.RuntimeFlavor 
                };
        }

        public IISDeploymentParameters() : this(Params.Default)
        { }

        public IISDeploymentParameters(TestVariant variant) : this(Params.TestVariant(variant))
        { }

        public IISDeploymentParameters(Params param) : this(param.applicationPath, param.serverType, param.runtimeFlavor, param.runtimeArchitecture)
        { }

        public IISDeploymentParameters(DeploymentParameters parameters)
            : this(Params.DeploymentParameters(parameters))
        {
            if (parameters is IISDeploymentParameters)
            {
                var tempParameters = (IISDeploymentParameters)parameters;
                WebConfigActionList = tempParameters.WebConfigActionList;
                ServerConfigActionList = tempParameters.ServerConfigActionList;
                WebConfigBasedEnvironmentVariables = tempParameters.WebConfigBasedEnvironmentVariables;
                HandlerSettings = tempParameters.HandlerSettings;
            }
        }

        public IISDeploymentParameters(
               string applicationPath,
               ServerType serverType,
               RuntimeFlavor runtimeFlavor,
               RuntimeArchitecture runtimeArchitecture)
             : base(applicationPath, serverType, runtimeFlavor, runtimeArchitecture)
        {
        }

        public IList<Action<XElement, string>> WebConfigActionList { get; } = new List<Action<XElement, string>>();

        public IList<Action<XElement, string>> ServerConfigActionList { get; } = new List<Action<XElement, string>>();

        public IDictionary<string, string> WebConfigBasedEnvironmentVariables { get; set; } = new Dictionary<string, string>();

        public IDictionary<string, string> HandlerSettings { get; set; } = new Dictionary<string, string>();

    }
}
