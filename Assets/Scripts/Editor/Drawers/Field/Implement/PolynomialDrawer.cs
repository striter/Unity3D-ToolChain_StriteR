using Unity.Mathematics;
using UnityEngine;

namespace UnityEditor.Extensions
{
    [CustomPropertyDrawer(typeof(IPolynomial),true)]
    public class PolynomialDrawer : AFunctionDrawer
    {
        private const float kXRange = 20f;
        public override float2 GetOrigin() => kfloat2.one * .5f;

        protected override void OnFunctionDraw(SerializedProperty _property, FTextureDrawer _helper)
        {
            var info = _property.GetFieldInfo(out var parentObject);
            var polynomial = (IPolynomial) info.GetValue(parentObject);
            
            // _helper.DrawPixelContinuousStart(_helper.sizeX/2,_helper.sizeY/2);
            for (int i = 0; i < _helper.SizeX; i++)
            {
                var value = polynomial.Evaluate( ((float)i / _helper.SizeX -.5f)*kXRange) + .5f;
                int x = i;
                int y = (int) (value * _helper.SizeY);
                _helper.PixelContinuous(x,y,Color.cyan);
            }


            var rootCount = polynomial.GetRoots(out var roots);
            for (int i = 0; i < rootCount; i++)
            {
                var rootValue = roots[i]/kXRange;
                rootValue += .5f;
                _helper.Circle(new int2((int)(rootValue * _helper.SizeX),_helper.SizeY/2) ,10,Color.yellow);
            }
        }
    }

}