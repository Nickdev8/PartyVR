using System;
using System.Diagnostics;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]

    // ReSharper disable once InconsistentNaming
    public class DOTweenPlayGroupAttribute: DOTweenPlayStartAttribute
    {
        public DOTweenPlayGroupAttribute(string label = null, ETweenStop stopAction = ETweenStop.Rewind, string groupBy="")
            : base(label, stopAction, groupBy)
        {
        }
    }
}
