using System.Collections.Generic;
using SaintsField.Condition;

namespace SaintsField
{
    public interface IVisibilityAttribute: IConditions
    {
        bool IsShow { get; }
    }
}
