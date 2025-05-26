using Unity.Mathematics;
using UnityEngine;

namespace UnityEditor.Extensions
{
    [CustomPropertyDrawer(typeof(Damper))]
    public class DamperDrawer : AFunctionDrawer
    {
        private const float kDeltaTime = .05f;
        private const float kEstimateSizeX = 600;
        protected override void OnFunctionDraw(SerializedProperty _property, FTextureDrawer _helper)
        {
            var sizeAspect =  _helper.SizeX / kEstimateSizeX;
            var deltaTime = kDeltaTime / sizeAspect;
            var division1 =(int)( 10f/kDeltaTime * sizeAspect);
            var division2 = (int)( 20f/kDeltaTime * sizeAspect);
            
            var damper = (Damper)_property.GetFieldInfo(out var parentObject).GetValue(parentObject);
            damper.Initialize(0f);
            for (var i = 0; i < _helper.SizeX; i++)
            {
                var point = i>=division1? i>=division2?kfloat3.one*.8f:kfloat3.one*.2f:kfloat3.one * .5f;
                var value = damper.Tick(deltaTime,point);
                var uv = new float2((float)i / _helper.SizeX, value.x);
                _helper.PixelContinuous(uv,Color.cyan);
                _helper.Pixel(new float2(uv.x,point.x) , Color.red);
            }
            // for (var i = 0; i < 60; i++)
            // {
            //     var xDelta =(int) (i / deltaTime);
            //     if (xDelta > _helper.SizeX)
            //         break;
            //     
            //     for(var j=0;j<_helper.SizeY;j++)
            //         _helper.Pixel(xDelta , j , Color.green.SetA(.3f));
            // }
        }
    }
}