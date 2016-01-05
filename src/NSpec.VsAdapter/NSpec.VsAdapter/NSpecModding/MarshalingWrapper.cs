﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSpec.VsAdapter.NSpecModding
{
    public class MarshalingWrapper<TInvocation, TResult> : MarshalByRefObject
    {
        public override object InitializeLifetimeService()
        {
            return null;
        }

        public TResult Execute(TInvocation invocation, Func<TInvocation, TResult> outputSelector)
        {
            return outputSelector(invocation);
        }
    }
}