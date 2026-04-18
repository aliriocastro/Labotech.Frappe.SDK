using System.Collections.Generic;

namespace Labotech.Frappe.Connector.Core
{
    public enum FrappeFilterOperator
    {
        EqualTo,
        NotEqualTo,
        In,
        NotIn,
        LessThan,
        LessThanOrEqualTo,
        GreaterThan,
        GreaterThanOrEqualTo,
        Like,
        NotLike,
        Between,
        AncestorsOf,
        DescendantsOf,
        NotAncestorsOf,
        NotDescendantsOf
    }

    public static class FrappeFilterOperandsExtension {

        public static string ToStringOperand(this FrappeFilterOperator op)
        {
            var operands = new Dictionary<FrappeFilterOperator, string>()
            {
                {FrappeFilterOperator.EqualTo, "="},
                {FrappeFilterOperator.NotEqualTo, "!="},
                {FrappeFilterOperator.In, "in"},
                {FrappeFilterOperator.NotIn, "not in"},
                {FrappeFilterOperator.LessThan, "<"},
                {FrappeFilterOperator.LessThanOrEqualTo, "<="},
                {FrappeFilterOperator.GreaterThan, ">"},
                {FrappeFilterOperator.GreaterThanOrEqualTo, ">="},
                {FrappeFilterOperator.Like, "like"},
                {FrappeFilterOperator.NotLike, "not like"},
                {FrappeFilterOperator.Between, "between"},
                {FrappeFilterOperator.AncestorsOf, "ancestors of"},
                {FrappeFilterOperator.DescendantsOf, "descendants of"},
                {FrappeFilterOperator.NotAncestorsOf, "not ancestors of"},
                {FrappeFilterOperator.NotDescendantsOf, "not descendants of"},
            };

            return operands[op];
        }
        
    }

}
