using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Boids.Fish
{
    public class FFishFlock : ABoidsFlock
    {
        protected override ABoidsBehaviour GetController() => default;

        protected override ABoidsTarget GetTarget() => default;
        protected override IBoidsAnimation GetAnimation() => default;

    }
}