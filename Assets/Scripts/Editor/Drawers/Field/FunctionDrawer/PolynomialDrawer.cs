using Unity.Mathematics;
using UnityEngine;

namespace UnityEditor.Extensions
{
    [CustomPropertyDrawer(typeof(IFunction),true)]
    public class PolynomialDrawer : AFunctionDrawer
    {
        private const int kResolution = 64;
        private float XRangePerResolution = 400;
        protected override void OnFunctionDraw(SerializedProperty _property, FTextureDrawer _helper)
        {
            var info = _property.GetFieldInfo(out var parentObject);
            var function = (IFunction) info.GetValue(parentObject);

            var isPolynomial = function is IPolynomial ;
            var center = isPolynomial ? new float2(.5f) : new float2(.2f);
            var functionXRange = isPolynomial ? 20f : _helper.SizeX / XRangePerResolution;
            var functionYRange = 2f;
            
            _helper.LineWidth(new float2(0, center.y), new float2(1f,center.y),5,Color.red.SetA(.5f));
            _helper.LineWidth(new float2(center.x,0),new float2(center.x,1),5,Color.green.SetA(.5f));
            for (var i = -10; i < 10; i++)
            {
                var uv = center + new float2(i * XRangePerResolution / _helper.SizeX, 0);
                _helper.Digit(i.ToString() ,uv + kfloat2.right * 0.003f,Color.white);
                _helper.LineWidth(uv,uv + kfloat2.up * 0.05f,10f,Color.yellow);
            }
            
            _helper.PixelContinuousStart(new float2(0f,0f));
            for (var i = 0; i < kResolution; i++)
            {
                var value = (float)i / kResolution - center.x;
                var evaluate = function.Evaluate( value*functionXRange);
                _helper.PixelContinuous(new float2(value,evaluate / functionYRange) + center,Color.cyan);
            }
            
            if (function is IPolynomial polynomial)
            {
                var rootCount = polynomial.GetRoots(out var roots);
                for (var i = 0; i < rootCount; i++)
                {
                    if(roots[i] < -functionXRange/2 || roots[i] > functionXRange/2) 
                        continue;
                    
                    var rootValue = roots[i]/functionXRange;
                    _helper.Circle(new float2(rootValue,0) + center ,10,Color.yellow);
                }
            } 
        }
    }

}