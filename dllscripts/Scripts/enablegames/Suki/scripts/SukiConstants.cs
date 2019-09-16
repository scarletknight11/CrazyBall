namespace Suki
{

    internal enum InputResolution
    {
        Trigger,
        Signal,
        Range,
        Location2D,
        Location3D
    }

    internal enum NodeMetric
    {
        SingleNodePosition,
        SingleJointAngle,
        MultiNodeVectorBetween,
        MultiNodeAngleBetween,
    }

    internal enum Device
    {
        Kinect
    }

    public enum BodyNode
    {
        Hips,
        Spine,
        Chest,
        Neck,
        Head,
        LeftUpperArm,
        LeftLowerArm,
        LeftHand,
        LeftIndexProximal,
        LeftIndexIntermediate,
        LeftThumbProximal,
        RightUpperArm,
        RightLowerArm,
        RightHand,
        RightIndexProximal,
        RightIndexIntermediate,
        RightThumbProximal,
        LeftUpperLeg,
        LeftLowerLeg,
        LeftFoot,
        LeftToes,
        RightUpperLeg,
        RightLowerLeg,
        RightFoot,
        RightToes,
        LeftShoulder,
        RightShoulder
    }

    public enum BodyAngle
    {
        Neck_Flexion,
        Neck_LateralFlexion,
        L_Shoulder_Flexion,
        L_Shoulder_Abduction,
        L_Shoulder_HorizontalAbduction,
        L_Elbow_Flexion,
        R_Shoulder_Flexion,
        R_Shoulder_Abduction,
        R_Shoulder_HorizontalAbduction,
        R_Elbow_Flexion,
        L_Hip_Flexion,
        L_Hip_Abduction,
        L_Hip_Rotation,
        R_Hip_Flexion,
        R_Hip_Abduction,
        R_Hip_Rotation,
        R_Knee_Flexion,
        L_Knee_Flexion,
        Spine_Flexion,
        Spine_LateralFlexion,
        Spine_Rotation
    }

    internal enum Calculation
    {
        CrossProduct,
        VectorAdd,
        VectorMultiply
    }

    internal enum Reduction
    {
        Magnitude,
        DotProduct,
        XValue,
        YValue,
        ZValue,
    }

    internal enum Condition
    {
        GreaterThan,
        GreatherThanEqual,
        Equal,
        LessThanEqual,
        LessThan
    }

}
