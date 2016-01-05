﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSpec.VsAdapter.NSpecModding
{
    [Serializable]
    public class CrossDomainCollector : 
        NspecDomainRunner<ICollectorInvocation, IEnumerable<NSpecSpecification>>, 
        ICrossDomainCollector
    {
        public CrossDomainCollector(IAppDomainFactory appDomainFactory) : base(appDomainFactory) {}
    }
}