using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    public interface IFlowCallback
    {
        void Execute(Action callback);
    }
}
