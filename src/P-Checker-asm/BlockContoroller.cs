using Modding;
using Modding.Blocks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PCheckerSpace
{
    namespace BlockController
    {
        public class BlockSelector : SingleInstance<BlockSelector>
        {
            public override string Name { get { return "P Checker BlockSelector"; } }
            public Dictionary<int, Type> BlockDict = new Dictionary<int, Type>
            {
                {
                    (int)BlockType.StartingBlock, typeof(StartingBlockScript)
                },
                {
                    (int)BlockType.Altimeter, typeof(AltimeterScript)
                },
                {
                    (int)BlockType.Ballast, typeof(BallastScript)
                },
                {
                    (int)BlockType.Balloon, typeof(BalloonScript)
                },
                {
                    (int)BlockType.CameraBlock, typeof(CameraScript)
                },
                {
                    (int)BlockType.FlyingBlock, typeof(FlyingBlockScript)
                },
                {
                    (int)BlockType.Piston, typeof(PistonScript)
                },
                {
                    (int)BlockType.Rocket, typeof(RocketScript)
                },
                {
                    (int)BlockType.Sensor, typeof(SensorScript)
                },
                {
                    (int)BlockType.Spring, typeof(SpringScript)
                },
                {
                    (int)BlockType.RopeWinch, typeof(RopeScript)
                },
                {
                    (int)BlockType.SteeringBlock, typeof(SteeringBlockScript)
                },
                {
                    (int)BlockType.SteeringHinge, typeof(SteeringBlockScript)
                },
                {
                    (int)BlockType.Timer, typeof(TimerScript)
                },
                {
                    (int)BlockType.WaterCannon, typeof(WaterCannonScript)
                },
                {
                    (int)BlockType.Wheel, typeof(WheelScript)
                },
                {
                    (int)BlockType.CircularSaw, typeof(WheelScript)
                },
                {
                    (int)BlockType.SpinningBlock, typeof(SpinningBlockScript)
                },
                {
                    (int)BlockType.CogMediumPowered, typeof(WheelScript)
                },
                {
                    (int)BlockType.LargeWheel, typeof(WheelScript)
                },
                {
                    (int)BlockType.Drill, typeof(WheelScript)
                },
                {
                    (int)BlockType.Suspension, typeof(SuspensionScript)
                },
                {
                    (int)BlockType.LogicGate, typeof(LogicGateScript)
                },
                {
                    (int)BlockType.Speedometer, typeof(SpeedometerScript)
                }
            };

            public void Awake()
            {
                Events.OnBlockInit += new Action<Block>(AddScript);
            }
            public void AddScript(Block block)
            {
                BlockBehaviour internalObject = block.BuildingBlock.InternalObject;
                if (BlockDict.ContainsKey(internalObject.BlockID))
                {
                    Type type = BlockDict[internalObject.BlockID];
                    try
                    {
                        if (internalObject.GetComponent(type) is null)
                        {
                            internalObject.gameObject.AddComponent(type);
                        }
                    }
                    catch
                    {
                        Mod.Error("AddScript Error");
                    }
                    return;
                }
                if (internalObject.GetComponent(typeof(CustomBlockBehaviour)) is null)
                {
                    internalObject.gameObject.AddComponent<CustomBlockBehaviour>();
                }
            }
        }
        public class CustomBlockBehaviour : MonoBehaviour
        {
            BlockBehaviour internalObject;
            public bool IsOverpowered = false;
            public bool IsScaled = false;
            public bool isFirstFrame = true;
            protected Vector3 defaultSize = Vector3.one;
            public bool IsNaN { get; protected set; }
            public void Awake()
            {
                internalObject = GetComponent<BlockBehaviour>();
                SafeAwake();
            }
            public void Update()
            {
                if (internalObject.isSimulating)
                {
                    if (isFirstFrame)
                    {
                        isFirstFrame = false;
                        OnSimulationStart();
                    }
                }
                else
                {
                    BuildingUpdate();
                    IsScaled = internalObject.transform.localScale != defaultSize;
                    isFirstFrame = true;
                }
            }
            public void OnMouseOver()
            {
                PCGUI.PickedBlockBehaviour = internalObject;
            }
            public virtual void SafeAwake() { }
            public virtual void BuildingUpdate() { }
            public virtual void OnSimulationStart() { }
        }
        public class NormalBlock : CustomBlockBehaviour
        {

        }
        public class StartingBlockScript : CustomBlockBehaviour
        {

        }
        public class AltimeterScript : CustomBlockBehaviour
        {
            private AltimeterBlock AB; //BlockBehaviourより継承
            private float minValue = 0.5f;
            private float maxValue = 250f;
            public override void SafeAwake()
            {
                AB = GetComponent<AltimeterBlock>();
            }
            public override void BuildingUpdate()
            {
                IsOverpowered = AB.HeightSlider.Value > maxValue || AB.HeightSlider.Value < minValue;
                IsNaN = float.IsNaN(AB.HeightSlider.Value);
            }
        }
        public class BallastScript : CustomBlockBehaviour
        {
            private BallastWeightController BWC;
            private float minValue = 0.2f;
            private float maxValue = 2f;
            public override void SafeAwake()
            {
                BWC = GetComponent<BallastWeightController>();
            }
            public override void BuildingUpdate()
            {
                IsOverpowered = BWC.MassSlider.Value > maxValue || BWC.MassSlider.Value < minValue;
                IsNaN = float.IsNaN(BWC.MassSlider.Value);
            }
        }
        public class BalloonScript : CustomBlockBehaviour
        {
            private BalloonController BC;
            public override void SafeAwake()
            {
                BC = GetComponent<BalloonController>();
            }
            public override void BuildingUpdate()
            {
                IsOverpowered =
                    (BC.BuoyancySlider.Value > 1.5f || BC.BuoyancySlider.Value < 0.2f) ||
                    (BC.StringLengthSlider.Value > 6f || BC.StringLengthSlider.Value < 0f);
                IsNaN =
                    float.IsNaN(BC.BuoyancySlider.Value) ||
                    float.IsNaN(BC.StringLengthSlider.Value);
            }
        }
        public class CameraScript : CustomBlockBehaviour
        {
            private FixedCameraBlock FCB;
            private float[] Distance = new float[2] { 1f, 80f };
            private float[] Height = new float[2] { -90f, 90f };
            private float[] Rotation = new float[2] { -1800f, 180f };
            private float[] Tilt = new float[2] { -1800f, 180f };
            private float[] Roll = new float[2] { -1800f, 180f };
            private float[] Yaw = new float[2] { -1800f, 180f };
            private float[] FoV = new float[2] { 30f, 70f };
            private float[] Smooth = new float[2] { 0f, 1f };
            private float[] Pred = new float[2] { 0f, 10f };
            public override void SafeAwake()
            {
                FCB = GetComponent<FixedCameraBlock>();
            }
            public override void BuildingUpdate()
            {
                IsOverpowered =
                    (FCB.DistanceSlider.Value < Distance[0] || Distance[1] < FCB.DistanceSlider.Value) ||
                    (FCB.HeightSlider.Value < Height[0] || Height[1] < FCB.HeightSlider.Value) ||
                    (FCB.RotationSlider.Value < Rotation[0] || Rotation[1] < FCB.RotationSlider.Value) ||
                    (FCB.TiltSlider.Value < Tilt[0] || Tilt[1] < FCB.TiltSlider.Value) ||
                    (FCB.RollSlider.Value < Roll[0] || Roll[1] < FCB.RollSlider.Value) ||
                    (FCB.YawSlider.Value < Yaw[0] || Yaw[1] < FCB.YawSlider.Value) ||
                    (FCB.fovSlider.Value < FoV[0] || FoV[1] < FCB.fovSlider.Value) ||
                    (FCB.SmoothSlider.Value < Smooth[0] || Smooth[1] < FCB.SmoothSlider.Value) ||
                    (FCB.PredictSlider.Value < Pred[0] || Pred[1] < FCB.PredictSlider.Value);
                IsNaN =
                    float.IsNaN(FCB.DistanceSlider.Value) ||
                    float.IsNaN(FCB.HeightSlider.Value) ||
                    float.IsNaN(FCB.RotationSlider.Value) ||
                    float.IsNaN(FCB.TiltSlider.Value) ||
                    float.IsNaN(FCB.RollSlider.Value) ||
                    float.IsNaN(FCB.YawSlider.Value) ||
                    float.IsNaN(FCB.fovSlider.Value) ||
                    float.IsNaN(FCB.SmoothSlider.Value) ||
                    float.IsNaN(FCB.PredictSlider.Value);
            }
        }
        public class FlyingBlockScript : CustomBlockBehaviour
        {
            private FlyingController FC;
            private float minValue = 0f;
            private float maxValue = 1.25f;
            public override void SafeAwake()
            {
                FC = GetComponent<FlyingController>();
            }
            public override void BuildingUpdate()
            {
                IsOverpowered = FC.SpeedSlider.Value < minValue || maxValue < FC.SpeedSlider.Value;
                IsNaN = float.IsNaN(FC.SpeedSlider.Value);
            }
        }
        public class PistonScript : CustomBlockBehaviour
        {
            private SliderCompress SC;
            private float minValue = 0.1f;
            private float maxValue = 2f;
            public override void SafeAwake()
            {
                SC = GetComponent<SliderCompress>();
            }
            public override void BuildingUpdate()
            {
                IsOverpowered = SC.SpeedSlider.Value < minValue || maxValue < SC.SpeedSlider.Value;
                IsNaN = float.IsNaN(SC.SpeedSlider.Value);
            }
        }
        public class RocketScript : CustomBlockBehaviour
        {
            private TimedRocket TR;
            private float[] Duration = new float[2] { 0.5f, 10f };
            private float[] Thrust = new float[2] { 0.1f, 1.5f };
            private float[] ThrustExceed = new float[2] { 0.1f, 4f };
            private float[] Charge = new float[2] { 0f, 1.5f };
            public override void SafeAwake()
            {
                TR = GetComponent<TimedRocket>();
            }
            public override void BuildingUpdate()
            {
                IsOverpowered =
                    PCGUI.Instance.AllowRocketExceed ?

                    // ロケットの限凸可能
                    (TR.DelaySlider.Value < Duration[0] || Duration[1] < TR.DelaySlider.Value) ||
                    (TR.PowerSlider.Value < ThrustExceed[0] || ThrustExceed[1] < TR.PowerSlider.Value) ||
                    (TR.ChargeSlider.Value < Charge[0] || Charge[1] < TR.ChargeSlider.Value) :

                    // ロケットの限凸不可能
                    (TR.DelaySlider.Value < Duration[0] || Duration[1] < TR.DelaySlider.Value) ||
                    (TR.PowerSlider.Value < Thrust[0] || Thrust[1] < TR.PowerSlider.Value) ||
                    (TR.ChargeSlider.Value < Charge[0] || Charge[1] < TR.ChargeSlider.Value);

                IsNaN =
                    float.IsNaN(TR.DelaySlider.Value) ||
                    float.IsNaN(TR.PowerSlider.Value) ||
                    float.IsNaN(TR.ChargeSlider.Value);
            }
        }
        public class SensorScript : CustomBlockBehaviour
        {
            private SensorBlock SB;
            private float[] Distance = new float[2] { 0.5f, 50f };
            private float[] Radius = new float[2] { 0.25f, 2f };
            public override void SafeAwake()
            {
                SB = GetComponent<SensorBlock>();
            }
            public override void BuildingUpdate()
            {
                IsOverpowered =
                    (SB.DistanceSlider.Value < Distance[0] || Distance[1] < SB.DistanceSlider.Value) ||
                    (SB.RadiusSllider.Value < Radius[0] || Radius[1] < SB.RadiusSllider.Value);
                IsNaN =
                    float.IsNaN(SB.DistanceSlider.Value) ||
                    float.IsNaN(SB.RadiusSllider.Value);
            }
        }
        public class SpringScript : CustomBlockBehaviour
        {
            private SpringCode SC;
            protected float minValue = 0.3f;
            protected float maxValue = 10f;
            public override void SafeAwake()
            {
                SC = GetComponent<SpringCode>();
            }
            public override void BuildingUpdate()
            {
                IsOverpowered = SC.SpeedSlider.Value < minValue || maxValue < SC.SpeedSlider.Value;
                IsNaN = float.IsNaN(SC.SpeedSlider.Value);
            }
        }
        public class RopeScript : CustomBlockBehaviour
        {
            private SpringCode SC;
            protected float minValue = 0.3f;
            protected float maxValue = 2f;
            public override void SafeAwake()
            {
                SC = GetComponent<SpringCode>();
            }
            public override void BuildingUpdate()
            {
                IsOverpowered = SC.SpeedSlider.Value < minValue || maxValue < SC.SpeedSlider.Value;

                // 重さ（どうしよう）
                IsOverpowered |= SC.MassSlider.Value < SC.MassSlider.Min || SC.MassSlider.Max < SC.MassSlider.Value;

                IsNaN = float.IsNaN(SC.SpeedSlider.Value);
            }
        }
        public class SteeringBlockScript : CustomBlockBehaviour
        {
            private SteeringWheel SW;
            private float minValue = 0f;
            private float maxValue = 2f;
            public override void SafeAwake()
            {
                SW = GetComponent<SteeringWheel>();
            }
            public override void BuildingUpdate()
            {
                IsOverpowered = SW.SpeedSlider.Value < minValue || maxValue < SW.SpeedSlider.Value;
                IsNaN = float.IsNaN(SW.SpeedSlider.Value);
            }
        }
        public class SuspensionScript : CustomBlockBehaviour
        {
            private SuspensionController SC;
            private float minValue = 0.25f;
            private float minValueDamper = 0.1f;
            private float maxValue = 3f;
            public override void SafeAwake()
            {
                SC = GetComponent<SuspensionController>();
            }
            public override void BuildingUpdate()
            {
                IsOverpowered =
                    SC.SpringSlider.Value < minValue || maxValue < SC.SpringSlider.Value ||
                    SC.DamperSlider.Value < minValueDamper || maxValue < SC.DamperSlider.Value;
                IsNaN =
                    float.IsNaN(SC.SpringSlider.Value) ||
                    float.IsNaN(SC.DamperSlider.Value);
            }
        }
        public class TimerScript : CustomBlockBehaviour
        {
            private TimerBlock TB;
            private float[] Wait = new float[2] { 0f, 999999.99f };
            private float[] Duration = new float[2] { 0f, 999999.99f };
            public override void SafeAwake()
            {
                TB = GetComponent<TimerBlock>();
            }
            public override void BuildingUpdate()
            {
                IsOverpowered =
                    (TB.WaitSlider.Value < Wait[0] || Wait[1] < TB.WaitSlider.Value) ||
                    (TB.EmulationSlider.Value < Duration[0] || Duration[1] < TB.EmulationSlider.Value);
                IsNaN =
                    float.IsNaN(TB.WaitSlider.Value) ||
                    float.IsNaN(TB.EmulationSlider.Value);
            }
        }
        public class WaterCannonScript : CustomBlockBehaviour
        {
            private WaterCannonController WCC;
            private float minValue = 0.1f;
            private float maxValue = 2f;
            public override void SafeAwake()
            {
                WCC = GetComponent<WaterCannonController>();
            }
            public override void BuildingUpdate()
            {
                IsOverpowered = WCC.StrengthSlider.Value < minValue || maxValue < WCC.StrengthSlider.Value;
                IsNaN = float.IsNaN(WCC.StrengthSlider.Value);
            }
        }
        public class WheelScript : CustomBlockBehaviour
        {
            private CogMotorControllerHinge CMCH;
            private float[] Speed = new float[2] { 0f, 2f };
            private float[] Acceleration = new float[2] { 0.1f, 50f };
            public override void SafeAwake()
            {
                CMCH = GetComponent<CogMotorControllerHinge>();
            }
            public override void BuildingUpdate()
            {
                IsOverpowered =
                    // speed < min
                    (CMCH.SpeedSlider.Value < CMCH.speedSlider.Min ||

                    // max < speed
                    CMCH.speedSlider.Max < CMCH.SpeedSlider.Value) //||

                    // acc < min
                    //(CMCH.AccelerationSlider.Value < CMCH.AccelerationSlider.Min ||

                    // max < acc < +inf
                    //(CMCH.AccelerationSlider.Max < CMCH.AccelerationSlider.Value && CMCH.AccelerationSlider.Value < float.PositiveInfinity))

                    ;
                IsNaN = float.IsNaN(CMCH.SpeedSlider.Value);
            }
        }
        public class SpinningBlockScript : CustomBlockBehaviour
        {
            private CogMotorControllerHinge CMCH;
            private float[] Speed = new float[2] { 0f, 2f };
            private float[] Acceleration = new float[2] { 0.1f, 50f };
            public override void SafeAwake()
            {
                CMCH = GetComponent<CogMotorControllerHinge>();
            }
            public override void BuildingUpdate()
            {
                IsOverpowered =
                    // speed < min
                    (CMCH.SpeedSlider.Value < CMCH.speedSlider.Min ||

                    // max < speed
                    CMCH.speedSlider.Max < CMCH.SpeedSlider.Value) ||

                    // acc < min
                    (CMCH.AccelerationSlider.Value < CMCH.AccelerationSlider.Min ||

                    // max < acc < +inf
                    (CMCH.AccelerationSlider.Max < CMCH.AccelerationSlider.Value && CMCH.AccelerationSlider.Value < float.PositiveInfinity)) ||
                    // スピニングブロックの自動ブレーキ = LimitSpeedToMotor
                    CMCH.AutoBreakToggle.IsActive == false;

                IsNaN = float.IsNaN(CMCH.SpeedSlider.Value) || float.IsNaN(CMCH.AccelerationSlider.Value);
            }
        }
        public class LogicGateScript : CustomBlockBehaviour
        {
            private LogicGate LG;
            private int EdgeDetector = 11; //エッジ検出のモード番号
            public override void SafeAwake()
            {
                LG = GetComponent<LogicGate>();
            }
            public override void BuildingUpdate()
            {
                IsOverpowered = LG.ModeMenu.Value == EdgeDetector;
            }
        }
        public class SpeedometerScript : CustomBlockBehaviour
        {
            private SpeedometerBlock SB; //BlockBehaviourより継承
            private float minValue = -99999f;
            private float maxValue = 999999.99f;
            public override void SafeAwake()
            {
                SB = GetComponent<SpeedometerBlock>();
            }
            public override void BuildingUpdate()
            {
                IsOverpowered = SB.HeightSlider.Value > maxValue || SB.HeightSlider.Value < minValue;
                IsNaN = float.IsNaN(SB.HeightSlider.Value);
            }
        }

        // 以下v1.50.0で追加されたブロック
        public class HotAirBalloonScript : CustomBlockBehaviour
        {
            private SqrBalloonController _balloon;
            public override void SafeAwake()
            {
                _balloon = GetComponent<SqrBalloonController>();
            }
            public override void BuildingUpdate()
            {
                IsOverpowered =
                    (_balloon.FixedHeight.Value < 0f || 250f < _balloon.FixedHeight.Value) ||
                (_balloon.PowerSlider.Value < 0f || 1.5 < _balloon.PowerSlider.Value);
                IsNaN =
                    float.IsNaN(_balloon.FixedHeight.Value) ||
                    float.IsNaN(_balloon.PowerSlider.Value);
            }
        }

        // 以下海アプデで追加されたブロック
        public abstract class BarrelScript : CustomBlockBehaviour
        {
            protected BuoyancyDensityController _buoyancy;
            public override void SafeAwake()
            {
                _buoyancy = GetComponent<BuoyancyDensityController>();
            }
        }
        public class BarrelSmallScript : BarrelScript
        {
            public override void BuildingUpdate()
            {
                IsOverpowered =
                    _buoyancy.BuoyancySlider.Value < 0.1f || 10f < _buoyancy.BuoyancySlider.Value;
                // 他のスライダはprivateだったので取得できなかった
                var sliders = _buoyancy.Sliders;
                foreach (var slider in sliders)
                {
                    if (slider == _buoyancy.BuoyancySlider) continue;

                    IsOverpowered |= slider.Value < 0.01f || 1f < slider.Value;
                }
                IsNaN = float.IsNaN(_buoyancy.BuoyancySlider.Value);
            }
        }
        public class BarrelLargeScript : BarrelScript
        {
            public override void BuildingUpdate()
            {
                IsOverpowered =
                    _buoyancy.BuoyancySlider.Value < 0.1f || 12f < _buoyancy.BuoyancySlider.Value;
                // 他のスライダはprivateだったので取得できなかった
                var sliders = _buoyancy.Sliders;
                foreach (var slider in sliders)
                {
                    if (slider == _buoyancy.BuoyancySlider) continue;

                    IsOverpowered |= slider.Value < 0.01f || 1f < slider.Value;
                }
                IsNaN = float.IsNaN(_buoyancy.BuoyancySlider.Value);
            }
        }
        public class ScrewScript : BarrelScript
        {
            private NauticalScrew _screw;
            public override void SafeAwake()
            {
                _screw = GetComponent<NauticalScrew>();
            }
            public override void BuildingUpdate()
            {
                IsOverpowered =
                    _screw.SpeedSlider.Value < 0f || 3f < _screw.SpeedSlider.Value ||
                    _screw.AccelerationSlider.Value < 0.1f;// || 2f < _screw.AccelerationSlider.Value;
                IsNaN =
                    float.IsNaN(_screw.SpeedSlider.Value) ||
                    float.IsNaN(_screw.AccelerationSlider.Value);
            }
        }
        public class RudderScript : CustomBlockBehaviour
        {
            private RudderController _rudder;
            public override void SafeAwake()
            {
                _rudder = GetComponent<RudderController>();
            }
            public override void BuildingUpdate()
            {
                IsOverpowered =
                    _rudder.SpeedSlider.Value < 0f || 1.5f < _rudder.SpeedSlider.Value;
                IsNaN = float.IsNaN(_rudder.SpeedSlider.Value);
            }
        }
        public class SailPanelScript : CustomBlockBehaviour
        {
            private SailBlock _sail;
            public override void SafeAwake()
            {
                _sail = GetComponent<SailBlock>();
            }
            public override void BuildingUpdate()
            {
                IsOverpowered =
                    _sail.SizeSlider.Value < 0 || 1f < _sail.SizeSlider.Value ||
                    _sail.SpeedSlider.Value < 0f || 1.5f < _sail.SpeedSlider.Value;
                IsNaN =
                    float.IsNaN(_sail.SizeSlider.Value) ||
                    float.IsNaN(_sail.SpeedSlider.Value);
            }
        }
        public class HarpoonScript : CustomBlockBehaviour
        {
            private HarpoonController _harpoon;
            public override void SafeAwake()
            {
                _harpoon = GetComponent<HarpoonController>();
            }
            public override void BuildingUpdate()
            {
                IsOverpowered =
                    _harpoon.PowerSlider.Value < 0f || 2f < _harpoon.PowerSlider.Value ||
                    _harpoon.SpeedSlider.Value < 0f || 1.5f < _harpoon.SpeedSlider.Value;
                IsNaN =
                    float.IsNaN(_harpoon.PowerSlider.Value) ||
                    float.IsNaN(_harpoon.SpeedSlider.Value);
            }
        }

        // v1.65.0で追加されたブロック
        public class FlyWheelScript : CustomBlockBehaviour
        {
            private FlyWheelBlock _flyWheel;

            public override void SafeAwake()
            {
                _flyWheel = GetComponent<FlyWheelBlock>();
            }
            public override void BuildingUpdate()
            {
                IsOverpowered =
                    _flyWheel.InertiaSlider.Value < 1f || 10f < _flyWheel.InertiaSlider.Value ||
                    _flyWheel.SpeedSlider.Value < 1f || //2f < _flyWheel.SpeedSlider.Value ||
                    _flyWheel.ClutchSlider.Value < 0f || 2f < _flyWheel.ClutchSlider.Value;
                IsNaN =
                    float.IsNaN(_flyWheel.InertiaSlider.Value) ||
                    float.IsNaN(_flyWheel.SpeedSlider.Value) ||
                    float.IsNaN(_flyWheel.ClutchSlider.Value);
            }
        }
        public class RollerWheelScript : CustomBlockBehaviour
        {
            private SkateboardWheel _skateboardWheel;
            public override void SafeAwake()
            {
                _skateboardWheel = GetComponent<SkateboardWheel>();
            }
            public override void BuildingUpdate()
            {
                IsOverpowered = _skateboardWheel.TensionSlider.Value < 0.1f || 1f < _skateboardWheel.TensionSlider.Value;
                IsNaN = float.IsNaN(_skateboardWheel.TensionSlider.Value);
            }
        }
        public class BouncyPadScript : CustomBlockBehaviour
        {
            private BouncyPadBlock _bouncyPad;
            public override void SafeAwake()
            {
                _bouncyPad = GetComponent<BouncyPadBlock>();
            }
            public override void BuildingUpdate()
            {
                IsOverpowered =
                    _bouncyPad.SpringSlider.Value < 0.5f || 2f < _bouncyPad.SpringSlider.Value ||
                    _bouncyPad.FloppySlider.Value < 0.0f || 1f < _bouncyPad.FloppySlider.Value;
                IsNaN =
                    float.IsNaN(_bouncyPad.SpringSlider.Value) ||
                    float.IsNaN(_bouncyPad.FloppySlider.Value);
            }
        }
        public class DragBlockScript : CustomBlockBehaviour
        {
            private DragBlock _dragBlock;
            public override void SafeAwake()
            {
                _dragBlock = GetComponent<DragBlock>();
            }
            public override void BuildingUpdate()
            {
                IsOverpowered =
                    _dragBlock.MagSlider.Value < 0.1f || 3f < _dragBlock.MagSlider.Value;
                IsNaN = float.IsNaN(_dragBlock.MagSlider.Value);
            }
        }
    }
}