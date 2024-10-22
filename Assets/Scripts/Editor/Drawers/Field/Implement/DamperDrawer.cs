using UnityEngine;

namespace UnityEditor.Extensions
{
 
    [CustomPropertyDrawer(typeof(Damper))]
    public class DamperDrawer : AFunctionDrawer
    {
        private const float kDeltaTime = .05f;
        private const float kEstimateSizeX = 600;
        protected override void OnFunctionDraw(SerializedProperty _property, FFunctionDrawerColors _helper)
        {
            float sizeAspect =  _helper.sizeX / kEstimateSizeX;
            float deltaTime = kDeltaTime / sizeAspect;
            int division1 =(int)( 10f/kDeltaTime * sizeAspect);
            int division2 = (int)( 20f/kDeltaTime * sizeAspect);
            
            Damper damper = (Damper)_property.GetFieldInfo(out var parentObject).GetValue(parentObject);
            
            for (int i = 0; i < _helper.sizeX; i++)
            {
                Vector3 point = i>=division1? i>=division2?Vector3.one*.8f:Vector3.one*.2f:Vector3.one * .5f;
                var value = damper.Tick(deltaTime,point);
                int x = i;
                int y = (int) (value.x * _helper.sizeY);
                _helper.PixelContinuous(x,y,Color.cyan);
                _helper.Pixel(x , (int)(point.x*_helper.sizeY) , Color.red);
            }
            
            for (int i = 0; i < 60; i++)
            {
                var xDelta =(int) (i / deltaTime);
                if (xDelta > _helper.sizeX)
                    break;
                
                for(int j=0;j<_helper.sizeY;j++)
                    _helper.Pixel(xDelta , j , Color.green.SetA(.3f));
            }
        }
    }
}