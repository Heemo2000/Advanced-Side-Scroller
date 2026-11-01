using UnityEngine;

namespace Utilities.Generic
{
    public class HelperFunctions
    {
        public static bool IsDetectable(BoxCollider2D colliderA, BoxCollider2D colliderB, float checkDistance, int obstacleLayerToCheck = 0)
        {
            Vector2 positionA = new Vector2(colliderA.transform.position.x, colliderA.transform.position.y) + colliderA.offset;
            Vector2 positionB = new Vector2(colliderB.transform.position.x, colliderB.transform.position.y) + colliderB.offset;

            bool isUnderDistance = Vector2.SqrMagnitude(positionB - positionA) <= checkDistance * checkDistance;
            bool canSee = true;

            if(obstacleLayerToCheck != 0)
            {
                RaycastHit2D hit = Physics2D.Linecast(positionA, positionB, obstacleLayerToCheck);
                if(hit.collider != null)
                {
                    Debug.Log($"Detected collider while seeing: {hit.collider.transform.name}");
                }
                canSee = hit.collider == null;
            }

            return isUnderDistance && canSee;
        }
    }
}
