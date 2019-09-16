using UnityEngine;
using System.Collections.Generic;
using FullSerializer;

namespace Suki
{

    internal class SukiSchemaInfo
    {

        [fsProperty]
        internal string _id;
        [fsProperty]
        internal string SchemaName;
        [fsProperty]
        internal InputResolution SchemaResolution;
        [fsProperty]
        internal Device SchemaDevice;
        [fsProperty]
        internal NodeMetric SchemaMetric;
        [fsProperty]
        internal List<BodyNode> BodyNodes;
        [fsProperty]
        internal List<BodyAngle> BodyAngles;

        internal class CalculationJSON
        {
            [fsProperty]
            internal Calculation calculationOperator;
            [fsProperty]
            internal Vector3 vectorOperand;
        }
        [fsProperty]
        internal CalculationJSON CalculationInfo;

        internal class ReductionJSON
        {
            [fsProperty]
            internal Reduction reductionOperator;
            [fsProperty]
            internal Vector3 vectorOperand;
        }
        [fsProperty]
        internal ReductionJSON ReductionInfo;

        internal class ConditionJSON
        {
            [fsProperty]
            internal Condition conditionOperator;
            [fsProperty]
            internal float scalarOperand;
            [fsProperty]
            internal float extentPercentage;
        }
        [fsProperty]
        internal ConditionJSON ConditionInfo;

        internal class FloatBoundsJSON
        {
            [fsProperty]
            internal float lowBounds;
            [fsProperty]
            internal float highBounds;
            [fsProperty]
            internal bool useExtents;
            [fsProperty]
            internal bool reverse;
        }
        [fsProperty]
        internal FloatBoundsJSON FloatBoundsInfo;

        internal class Vector2BoundsJSON
        {
            [fsProperty]
            internal Vector2 lowBounds;
            [fsProperty]
            internal Vector2 highBounds;
            [fsProperty]
            internal bool useExtents;
            [fsProperty]
            internal bool reverse;
        }
        [fsProperty]
        internal Vector2BoundsJSON Vector2BoundsInfo;

        internal class Vector3BoundsJSON
        {
            [fsProperty]
            internal Vector3 lowBounds;
            [fsProperty]
            internal Vector3 highBounds;
        }
        [fsProperty]
        internal Vector3BoundsJSON Vector3BoundsInfo;

        public SukiSchemaInfo()
        {
            BodyNodes = new List<BodyNode>();
        }

        public string Serialize()
        {
            return JSONSerializer.Serialize(typeof(SukiSchemaInfo), this);
        }

        public static SukiSchemaInfo Deserialize(string json)
        {
            return JSONSerializer.Deserialize(typeof(SukiSchemaInfo), json) as SukiSchemaInfo;
        }
    }
}
