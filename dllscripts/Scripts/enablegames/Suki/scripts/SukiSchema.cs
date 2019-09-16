using System;
using System.Collections.Generic;
using UnityEngine;

namespace Suki
{

    internal abstract class SukiSchema
    {
        private string _id;
        internal string _ID
        {
            get
            {
                return _id;
            }
        }

        private string name;
        internal string Name
        {
            get
            {
                return name;
            }
        }

        private InputResolution resolution;
        internal InputResolution Resolution
        {
            get
            {
                return resolution;
            }
        }

        private Device device;
        internal Device Device
        {
            get
            {
                return device;
            }
        }

        private NodeMetric metric;
        internal NodeMetric Metric
        {
            get
            {
                return metric;
            }
        }

        private SukiData data;
        protected SukiData Data
        {
            get
            {
                return data;
            }
        }

        private delegate Vector3 CalculationFunctionDelegate(Vector3 value);
        private delegate float ReductionFunctionDelegate(Vector3 value);
        private delegate bool ConditionFunctionDelegate(float value);
        private delegate T BoundsFunctionDelegate<T>(T value);

        private CalculationFunctionDelegate CalculationFunction;
        private ReductionFunctionDelegate ReductionFunction;
        private ConditionFunctionDelegate ConditionFunction;
        private BoundsFunctionDelegate<float> FloatBoundsFunction;
        private BoundsFunctionDelegate<Vector2> Vector2BoundsFunction;
        private BoundsFunctionDelegate<Vector3> Vector3BoundsFunction;

        private bool extentsQueried = false;
        private float minExtent = float.MaxValue;
        public float MinExtent
        {
            get
            {
                return minExtent;
            }
        }
        private float maxExtent = float.MinValue;
        public float MaxExtent
        {
            get
            {
                return maxExtent;
            }
        }
        private Vector2 min2DExtent = new Vector2(float.MaxValue, float.MaxValue);
        public Vector2 Min2DExtent
        {
            get
            {
                return min2DExtent;
            }
        }
        private Vector2 max2DExtent = new Vector2(float.MinValue, float.MinValue);
        public Vector2 Max2DExtent
        {
            get
            {
                return min2DExtent;
            }
        }
        enableGame.egBool enableExtents = null;
        void Awake()
        {
            if (enableExtents != null)
                return;
            enableExtents = new enableGame.egBool();
            enableGame.VariableHandler.Instance.Register(egParameterStrings.SUKI_EXTENTS, enableExtents);
            Debug.Log("enableExtends: " + (bool)enableExtents);
        }

        protected virtual void SetCalculationFunction(Calculation calculationOperation, Vector3 calculationOperand)
        {
            Calculation op = calculationOperation;
            Vector3 operand = calculationOperand;
            CalculationFunction = (Vector3 value) => {
                Vector3 ret = Vector3.zero;

                switch (op)
                {
                    case Calculation.CrossProduct:
                        ret = Vector3.Cross(value, operand);
                        break;
                    case Calculation.VectorAdd:
                        ret = new Vector3(value.x + operand.x, value.y + operand.y, value.z + operand.z);
                        break;
                    case Calculation.VectorMultiply:
                        ret = Vector3.Scale(value, operand);
                        break;
                    default:
                        throw new System.Exception("Calculation not recognized");
                }

                return ret;
            };
        }

        protected virtual void SetReductionFunction(Reduction reductionOperation, Vector3 reductionOperand = default(Vector3))
        {
            if (resolution != InputResolution.Trigger && resolution != InputResolution.Signal && resolution != InputResolution.Range)
            {
                throw new System.Exception("Only Trigger, Signal, and Range resolution schemas can have Reductions.");
            }
            Reduction op = reductionOperation;
            Vector3 operand = reductionOperand;
            ReductionFunction = (Vector3 value) => {
                float ret = 0f;

                switch (op)
                {
                    case Reduction.Magnitude:
                        ret = value.magnitude;
                        break;
                    case Reduction.DotProduct:
                        ret = Vector3.Dot(value, operand);
                        break;
                    case Reduction.XValue:
                        ret = value.x;
                        break;
                    case Reduction.YValue:
                        ret = value.y;
                        break;
                    case Reduction.ZValue:
                        ret = value.z;
                        break;
                    default:
                        throw new System.Exception("Reduction not recognized");
                }

                return ret;
            };
        }

        private void SetConditionFunction(Condition conditionOperation, float conditionOperand, float extentPercentage = 1f)
        {
            if (resolution != InputResolution.Trigger && resolution != InputResolution.Signal)
            {
                throw new System.Exception("Only Trigger and Signal resolution schemas can have Conditions.");
            }
            Condition op = conditionOperation;
            float operand = conditionOperand;
            float percentage = Mathf.Max(Mathf.Min(extentPercentage, 1f), 0f);
            ConditionFunction = (float value) => {
                bool ret = false;

                if (percentage != 1f)
                {
                    UpdateExtents(value);
                    // if the schema uses a percentage of extents, adjust the operand value
                    operand = percentage * maxExtent;
                    //Debug.Log(_id + " | Max Extent = " + maxExtent + " | operand = " + operand + " | value = " + value);
                }

                switch (op)
                {
                    case Condition.GreaterThan:
                        ret = value > operand;
                        break;
                    case Condition.GreatherThanEqual:
                        ret = value >= operand;
                        break;
                    case Condition.Equal:
                        ret = value == operand;
                        break;
                    case Condition.LessThanEqual:
                        ret = value <= operand;
                        break;
                    case Condition.LessThan:
                        ret = value < operand;
                        break;
                    default:
                        throw new System.Exception("Condition not recognized");
                }

                return ret;
            };
        }

        private void SetFloatBoundsFunction(float lowBounds, float highBounds, bool useExtents, bool shouldReverse)
        {
            if (resolution != InputResolution.Range)
            {
                throw new System.Exception("Only Range resolution schemas can have Float Bounds.");
            }
            float low = lowBounds;
            float high = highBounds;
            Awake();
            bool extents = ((bool)enableExtents ? useExtents : false);// useExtents;
            bool reverse = shouldReverse;
            FloatBoundsFunction = (float value) => {
                if (extents)
                {
                    UpdateExtents(value);
                    // if schema specifies that we should use extents, override the argument values
                    low = useExtents ? minExtent : low;
                    high = useExtents ? maxExtent : high;
                }
                return NormalizeRange(value, low, high, reverse);
            };
        }

        private void SetVector2BoundsFunction(Vector2 lowBounds, Vector2 highBounds, bool useExtents, bool shouldReverse)
        {
            if (resolution != InputResolution.Location2D)
            {
                throw new System.Exception("Only Location2D resolution schemas can have Vector2 Bounds.");
            }
            Vector2 low = lowBounds;
            Vector2 high = highBounds;
            Awake();
            bool extents = ((bool)enableExtents ? useExtents : false);// useExtents;
            bool reverese = shouldReverse;
            Vector2BoundsFunction = (Vector2 value) => {
                if (extents)
                {
                    UpdateExtents(value);
                    // if schema specifies that we should use extents, override the argument values
                    low = useExtents ? min2DExtent : low;
                    high = useExtents ? max2DExtent : high;
                }
                // only reverse x for now (most expected behavior) - can extend the schemas later if needed
                float x = NormalizeRange(value.x, low.x, high.x, shouldReverse);
                float y = NormalizeRange(value.y, low.y, high.y);
                return new Vector2(x, y);
            };
        }

        private void SetVector3BoundsFunction(Vector3 lowBounds, Vector3 highBounds)
        {
            if (resolution != InputResolution.Location3D)
            {
                throw new System.Exception("Only Location3D resolution schemas can have Vector3 Bounds.");
            }
            Vector3 low = lowBounds;
            Vector3 high = highBounds;
            Vector3BoundsFunction = (Vector3 value) => {
                float x = NormalizeRange(value.x, low.x, high.x);
                float y = NormalizeRange(value.y, low.y, high.y);
                float z = NormalizeRange(value.z, low.z, high.z);
                return new Vector3(x, y, z);
            };
        }

        protected SukiSchema()
        {
            name = "";
            resolution = InputResolution.Trigger;
            data = SukiData.Instance;

            CalculationFunction = null;
            ReductionFunction = null;
            ConditionFunction = null;
            FloatBoundsFunction = null;
            Vector2BoundsFunction = null;
            Vector3BoundsFunction = null;
        }

        internal SukiSchema(SukiSchemaInfo info)
            : this()
        {
            Setup(info);
        }

        internal static SukiSchema CreateSchema(SukiSchemaInfo info)
        {
            SukiSchema ret = null;
            switch (info.SchemaMetric)
            {
                case NodeMetric.SingleNodePosition:
                    ret = new SingleNodePositionSchema(info);
                    break;
                case NodeMetric.SingleJointAngle:
                    ret = new SingleJointAngleSchema(info);
                    break;
                case NodeMetric.MultiNodeVectorBetween:
                    ret = new MultiNodeVectorBetweenSchema(info);
                    break;
                case NodeMetric.MultiNodeAngleBetween:
                    ret = new MultiNodeAngleBetweenSchema(info);
                    break;
                default:
                    throw new System.Exception("Invalid Schem Node Metric");
            }
            return ret;
        }

        protected virtual void Setup(SukiSchemaInfo info)
        {
            _id = info._id;
            name = info.SchemaName;
            resolution = info.SchemaResolution;
            device = info.SchemaDevice;
            metric = info.SchemaMetric;

            if (null != info.CalculationInfo)
            {
                SetCalculationFunction(info.CalculationInfo.calculationOperator, info.CalculationInfo.vectorOperand);
            }
            if (null != info.ReductionInfo)
            {
                SetReductionFunction(info.ReductionInfo.reductionOperator, info.ReductionInfo.vectorOperand);
            }
            if (null != info.ConditionInfo)
            {
                SetConditionFunction(info.ConditionInfo.conditionOperator, info.ConditionInfo.scalarOperand, info.ConditionInfo.extentPercentage);
            }
            if (null != info.FloatBoundsInfo)
            {
                SetFloatBoundsFunction(info.FloatBoundsInfo.lowBounds, info.FloatBoundsInfo.highBounds, info.FloatBoundsInfo.useExtents, info.FloatBoundsInfo.reverse);
            }
            if (null != info.Vector2BoundsInfo)
            {
                SetVector2BoundsFunction(info.Vector2BoundsInfo.lowBounds, info.Vector2BoundsInfo.highBounds, info.Vector2BoundsInfo.useExtents, info.Vector2BoundsInfo.reverse);
            }
            if (null != info.Vector3BoundsInfo)
            {
                SetVector3BoundsFunction(info.Vector3BoundsInfo.lowBounds, info.Vector3BoundsInfo.highBounds);
            }

            switch (info.SchemaResolution)
            {
                case InputResolution.Trigger:
                    data.CreateTrigger(name, false);
                    break;
                case InputResolution.Signal:
                    data.CreateSignal(name, false);
                    break;
                case InputResolution.Range:
                    data.CreateRange(name, 0.5f);
                    break;
                case InputResolution.Location2D:
                    data.CreateLocation2D(name, Vector2.zero);
                    break;
                case InputResolution.Location3D:
                    data.CreateLocation3D(name, Vector3.zero);
                    break;
                default:
                    throw new System.Exception("Invalid Schema Resolution");
            }

            GetSchemaExtentsFromServer();
        }

        internal abstract void Execute(SkeletonData skeleton);

        protected void RunScalarMetric(float metric)
        {
            if (resolution == InputResolution.Trigger || resolution == InputResolution.Signal)
            {
                // Run Conditions if resolution requires a boolean
                if (null == ConditionFunction)
                {
                    throw new System.Exception("Triggers and Signals must have a Condition");
                }
                bool shouldFire = ConditionFunction(metric);
                // execute
                if (resolution == InputResolution.Trigger)
                {
                    if (shouldFire)
                    {
                        Data.SetTrigger(name);
                    }
                }
                else
                {
                    Data.SetSignal(name, shouldFire);
                }
            }
            else if (resolution == InputResolution.Range)
            {
                // Run Float Bounds if resolution requires a float
                if (null == FloatBoundsFunction)
                {
                    throw new System.Exception("Ranges must have a float Bounds");
                }
                float rangeValue = FloatBoundsFunction(metric);
                Data.SetRange(name, rangeValue);
            }
            else
            {
                throw new System.Exception("Only Triggers, Signals, and Ranges can RunMetric with scalar value");
            }
        }

        protected void RunVectorMetric(Vector3 metric)
        {
            Vector3 vectorValue = metric;

            // Run Calculations first, transforming Vectors into other vectors
            if (null != CalculationFunction)
            {
                vectorValue = CalculationFunction(vectorValue);
            }

            // Run Reductions, Conditions, and Bounds based on the resolution
            switch (resolution)
            {
                case InputResolution.Trigger:
                case InputResolution.Signal:
                case InputResolution.Range:
                    // Run Reductions if resolution requires a scalar and not a vector
                    if (null == ReductionFunction)
                    {
                        throw new System.Exception("Triggers, Signals, and Ranges must have a Reduction");
                    }
                    float scalarValue = ReductionFunction(vectorValue);
                    // once reduced to a scalar, run the scalar metric
                    RunScalarMetric(scalarValue);
                    break;
                case InputResolution.Location2D:
                    // Run Vector2 Bounds if resolution requires a Vector2
                    if (null == Vector2BoundsFunction)
                    {
                        throw new System.Exception("Location2D must have a Vector2 Bounds");
                    }
                    Vector2 v2Value = Vector2BoundsFunction(new Vector2(vectorValue.x, vectorValue.y));
                    Data.SetLocation2D(name, v2Value);
                    break;
                case InputResolution.Location3D:
                    // Run Vector3 Bounds if resolution requires a Vector3
                    if (null == Vector3BoundsFunction)
                    {
                        throw new System.Exception("Ranges must have a Vector3 Bounds");
                    }
                    Vector3 v3Value = Vector3BoundsFunction(vectorValue);
                    Data.SetLocation3D(name, v3Value);
                    break;
                default:
                    throw new System.Exception("Unrecognized resolution");
            }

        }
        private bool startMoving = false;

        protected Vector3 JointPosition(SkeletonData skeleton, BodyNode node)
        {
            //            int boneIndex = (int)node;
            //            return skeleton.bonePos[boneIndex];
            startMoving = false;


            int boneIndex = (int)node;
            if (skeleton.Moving && node != null && skeleton.bonePos != null && boneIndex<skeleton.bonePos.Length)
            {
                //Debug.Log ("boneIndex=" + boneIndex);
                startMoving = true;//
                                   //		if (boneIndex < skeleton.bonePos.GetLength())
                return skeleton.bonePos[boneIndex];
                //				else
                //					return new Vector3 ();
            }
            else
            {
                return Vector3.zero;
            }
        }

        protected float JointAngle(SkeletonData skeleton, BodyAngle joint)
        {
            int jointAngleIndex = (int)joint;
            return skeleton.jointAng[jointAngleIndex];
        }

        // retrieve extents data from server (if it exists)
        private void GetSchemaExtentsFromServer()
        {
			extentsQueried = true;
			return; // not yet using server extents 
            // backward compatibility - don't deal with the server if we don't have an id defined
            if (String.IsNullOrEmpty(_id))
            {
                return;
            }
            Action<bool, Dictionary<string, float>> SaveExtents = (exists, data) => {
				if (exists)
                {
                    if (data.ContainsKey("min"))
                    {
                        minExtent = data["min"];
                        maxExtent = data["max"];
                    }
                    else if (data.ContainsKey("xMin"))
                    {
                        min2DExtent = new Vector2(data["xMin"], data["yMin"]);
                        max2DExtent = new Vector2(data["xMax"], data["yMax"]);
                    }
                }
                extentsQueried = true;
            };
            EnableAPI.Instance.GetSchemaExtents(this._id, SaveExtents);
        }

        // updates the min/max extents if the value lies outside the previously observed extents
        private void UpdateExtents(float value)
        {
            // backward compatibility - don't deal with the server if we don't have an id defined
            if (String.IsNullOrEmpty(_id))
            {
                return;
            }
            // do nothing if we have not yet received a response from server
            if (!extentsQueried)
            {
                //return;
            }

            bool updated = false;
            if (value < minExtent)
            {
				if (minExtent < float.MaxValue)
					minExtent = (minExtent * (minmaxHistory-1)  + value) / minmaxHistory;
				else
                minExtent = value;
                updated = true;
            }
            if (value > maxExtent)
            {
				if (maxExtent > float.MinValue)
					maxExtent = (maxExtent * (minmaxHistory-1)  + value)/minmaxHistory;
				else
                maxExtent = value;
                updated = true;
            }
			/*
            if (value < minExtent)
            {
                minExtent = value;
                updated = true;
            }
            if (value > maxExtent)
            {
                maxExtent = value;
                updated = true;
            }*/
            if (updated)
            {
                Dictionary<string, object> values = new Dictionary<string, object>();
                values.Add("min", (object)minExtent);
                values.Add("max", (object)maxExtent);
                EnableAPI.Instance.SetSchemaExtents(_id, values);
            }
        }

		float minmaxHistory = 100f;
        // updates the min/max extents if the value lies outside the previously observed extents
        private void UpdateExtents(Vector2 value)
        {
            // backward compatibility - don't deal with the server if we don't have an id defined
            if (String.IsNullOrEmpty(_id))
            {
                return;
            }
            // do nothing if we have not yet received a response from server
            if (!extentsQueried)
            {
                return;
            }

            bool updated = false;
            if (value.x < min2DExtent.x)
            {
				if (min2DExtent.x < float.MaxValue)
					min2DExtent.x = (min2DExtent.x * (minmaxHistory-1)  + value.x) / minmaxHistory;
				else
                min2DExtent.x = value.x;
                updated = true;
            }
            if (value.x > max2DExtent.x)
            {
				//Debug.Log ("maxx=" + max2DExtent + ", floatmin=" + float.MinValue + " greater than? " + (max2DExtent.x > float.MinValue));
				if (max2DExtent.x > float.MinValue)
					max2DExtent.x = (max2DExtent.x*(minmaxHistory-1)  + value.x)/minmaxHistory;
				else
                max2DExtent.x = value.x;
                updated = true;
            }
            if (value.y < min2DExtent.y)
            {
				if (min2DExtent.y < float.MaxValue)
					min2DExtent.y = (min2DExtent.y * (minmaxHistory-1)  + value.y) / minmaxHistory;
				else
                min2DExtent.y = value.y;
                updated = true;
            }
            if (value.y > max2DExtent.y)
            {
				if (max2DExtent.y > float.MinValue)
					max2DExtent.y = (max2DExtent.y * (minmaxHistory-1)  + value.y) / minmaxHistory;
				else
                max2DExtent.y = value.y;
                updated = true;
            }
            if (updated)
            {
                Dictionary<string, object> values = new Dictionary<string, object>();
                values.Add("xMin", (object)min2DExtent.x);
                values.Add("xMax", (object)max2DExtent.x);
                values.Add("yMin", (object)min2DExtent.y);
                values.Add("yMax", (object)max2DExtent.y);
                EnableAPI.Instance.SetSchemaExtents(_id, values);
            }
        }

        // converts a value into a number between 0-1, based on its location between low and high
        private float NormalizeRange(float value, float low, float high, bool reverse = false)
        {

            // if we should reverse, flip the high and low
            if (reverse)
            {
                float temp = low;
                low = high;
                high = temp;
            }

            if (high <= low)
            {
                //	throw new System.Exception("Cannot normalize range if high bounds is less than or equal to low bounds");
                if (value >= low)
                {
                    return 0f;
                }
                else if (value <= high)
                {
                    return 1f;
                }
            }
            else
            {
                // handle out of bounds
                if (value <= low)
                {
                    return 0f;
                }
                else if (value >= high)
                {
                    return 1f;
                }
            }

            float fullRange = high - low;
            float valueInRange = (value - low);

            return (valueInRange / fullRange);
        }
    }

    internal class SingleNodePositionSchema : SukiSchema
    {
        private BodyNode target;

        internal SingleNodePositionSchema(SukiSchemaInfo info) : base(info) { }

        protected override void Setup(SukiSchemaInfo info)
        {
            if (info.SchemaMetric != NodeMetric.SingleNodePosition)
            {
                throw new System.Exception("Schema does not match node metric.");
            }
            if (null == info.BodyNodes || info.BodyNodes.Count != 1)
            {
                throw new System.Exception("SingleNodePositionSchema requires one node.");
            }
            target = info.BodyNodes[0];

            base.Setup(info);
        }

        internal override void Execute(SkeletonData skeleton)
        {
            Vector3 targetPosition = JointPosition(skeleton, target);

            RunVectorMetric(targetPosition);
        }
    }

    internal class SingleJointAngleSchema : SukiSchema
    {
        private BodyAngle target;

        internal SingleJointAngleSchema(SukiSchemaInfo info) : base(info) { }

        protected override void Setup(SukiSchemaInfo info)
        {
            if (info.SchemaMetric != NodeMetric.SingleJointAngle)
            {
                throw new System.Exception("Schema does not match node metric.");
            }
            if (null == info.BodyAngles || info.BodyAngles.Count != 1)
            {
                throw new System.Exception("SingleJointAngleSchema requires one angle.");
            }
            target = info.BodyAngles[0];

            base.Setup(info);
        }

        internal override void Execute(SkeletonData skeleton)
        {
            float targetAngle = JointAngle(skeleton, target);

            RunScalarMetric(targetAngle);
        }
    }

    internal class MultiNodeVectorBetweenSchema : SukiSchema
    {
        private BodyNode first;
        private BodyNode second;

        internal MultiNodeVectorBetweenSchema(SukiSchemaInfo info) : base(info) { }

        protected override void Setup(SukiSchemaInfo info)
        {
            if (info.SchemaMetric != NodeMetric.MultiNodeVectorBetween)
            {
                throw new System.Exception("Schema does not match node metric.");
            }
            if (null == info.BodyNodes || info.BodyNodes.Count != 2)
            {
                throw new System.Exception("MultiNodeVectorBetweenSchema requires two nodes.");
            }
            first = info.BodyNodes[0];
            second = info.BodyNodes[1];

            base.Setup(info);
        }

        internal override void Execute(SkeletonData skeleton)
        {
            Vector3 firstJointPosition = JointPosition(skeleton, first);
            Vector3 secondJointPosition = JointPosition(skeleton, second);

            Vector3 vectorBetween = secondJointPosition - firstJointPosition;

            RunVectorMetric(vectorBetween);
        }
    }

    internal class MultiNodeAngleBetweenSchema : SukiSchema
    {
        private BodyNode first;
        private BodyNode middle;
        private BodyNode second;

        internal MultiNodeAngleBetweenSchema(SukiSchemaInfo info) : base(info) { }

        protected override void Setup(SukiSchemaInfo info)
        {
            if (info.SchemaMetric != NodeMetric.MultiNodeAngleBetween)
            {
                throw new System.Exception("Schema does not match node metric.");
            }
            if (null == info.BodyNodes || info.BodyNodes.Count != 3)
            {
                throw new System.Exception("MultiNodeAngleBetweenSchema requires two nodes.");
            }
            first = info.BodyNodes[0];
            middle = info.BodyNodes[1];
            second = info.BodyNodes[2];

            base.Setup(info);
        }

        internal override void Execute(SkeletonData skeleton)
        {
            Vector3 firstJointPosition = JointPosition(skeleton, first);
            Vector3 secondJointPosition = JointPosition(skeleton, second);
            Vector3 middleJointPosition = JointPosition(skeleton, middle);

            Vector3 firstJointVector = firstJointPosition - middleJointPosition;
            Vector3 secondJointVector = secondJointPosition - middleJointPosition;

            float angleBetween = Vector3.Angle(firstJointVector, secondJointVector);

            RunScalarMetric(angleBetween);
            //Debug.Log("SukiSchema:Angle: " + angleBetween);
        }

        protected override void SetCalculationFunction(Calculation calculationOperation, Vector3 calculationOperand)
        {
            throw new System.Exception("Multi-Node Angle-Between Schemas cannot run Calculations");
        }
        protected override void SetReductionFunction(Reduction reductionOperation, Vector3 reductionOperand)
        {
            throw new System.Exception("Multi-Node Angle-Between Schemas cannot run Reductions");
        }

    }
}
