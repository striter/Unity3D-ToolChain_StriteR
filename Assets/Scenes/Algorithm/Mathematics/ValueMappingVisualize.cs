using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Examples.Mathmatics
{
    public class ValueMappingVisualize : MonoBehaviour
    {
        public LinearPolynomial m_LinearPolynomial = LinearPolynomial.kDefault;
        public QuadraticPolynomial m_QuadraticPolynomial = QuadraticPolynomial.kDefault;
        public CubicPolynomial m_CubicPolynomial = CubicPolynomial.kDefault;
        public Damper m_Damper = Damper.kDefault;
        public Function.Identity m_Identity = Function.Identity.kDefault;
        public Function.Impulse m_Impulse = Function.Impulse.kDefault;
        public Function.UnitaryRemapping m_UnitaryRemapping = Function.UnitaryRemapping.kDefault;
    }

}