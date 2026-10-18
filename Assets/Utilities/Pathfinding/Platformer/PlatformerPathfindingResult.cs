using System;
using System.Collections.Generic;


namespace Utilities.Pathfinding
{
    public class PlatformerPathfindingResult
    {
        #region Enums
        public enum Status
        {
            None = -1,
            Success,
            Failed
        }
        #endregion

        #region Properties
        public Status CompletionStatus { get; private set; }
        public Action<List<IAstarNode>> OnPathFindSuccess { get; private set; }
        public Action OnPathFindFailed { get; private set; }
        #endregion

        #region Constructor

        public PlatformerPathfindingResult(Status completionStatus, Action<List<IAstarNode>> pathFindSuccess,
                                           Action pathFindFailed)
        {
            this.CompletionStatus = completionStatus;
            this.OnPathFindSuccess = pathFindSuccess;
            this.OnPathFindFailed = pathFindFailed;
        }

        #endregion
    }
}
