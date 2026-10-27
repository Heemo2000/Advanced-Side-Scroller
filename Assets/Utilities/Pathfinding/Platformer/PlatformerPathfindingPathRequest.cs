using System;
using System.Collections.Generic;

using UnityEngine;

namespace Utilities.Pathfinding.Platformer
{
    public class PlatformerPathfindingPathRequest
    {
        #region Properties
        public Vector2 positionA { get; private set; }
        public Vector2 positionB { get; private set; }
        public Action<PlatformerPathfindingPathResult> OnPathFound { get; private set; }
        public Action OnPathNotFound { get; private set; }
        #endregion

        #region Constructor
        
        public PlatformerPathfindingPathRequest(Vector2 positionA, Vector2 positionB,
                                                Action<PlatformerPathfindingPathResult> onPathFoundCallback,
                                                Action onPathNotFoundCallback)
        {
            this.positionA = positionA;
            this.positionB = positionB;
            this.OnPathFound = onPathFoundCallback;
            this.OnPathNotFound = onPathNotFoundCallback;
        }

        #endregion
    }
}
