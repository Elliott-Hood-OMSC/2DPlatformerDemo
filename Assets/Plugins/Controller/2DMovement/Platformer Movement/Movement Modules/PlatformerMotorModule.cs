using System;

namespace NetControllerSystem.Platformer2D
{
    public abstract class PlatformerMotorModule : MotorModule
    {
        protected PlatformerMotor motor;
        protected EntityController Controller => motor.Controller;
        
        public override void Initialize(Motor newMotor)
        {
            base.Initialize(newMotor);
            
            if (newMotor is not PlatformerMotor platformerMotor)
                throw new Exception($"{nameof(Motor)} is not type of {nameof(PlatformerMotor)}");
            
            motor = platformerMotor;
            OnInitialize();
        }

        protected virtual void OnInitialize() { }
    }
}