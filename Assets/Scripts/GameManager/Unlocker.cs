using UnityEngine;

namespace GameManager
{
    public class Unlocker : MonoBehaviour
    {
        public static void CompleteFirstBranch() => ProgressPersistence.FirstBranchDone = true;
        public static void CompleteSecondBranch() => ProgressPersistence.SecondBranchDone = true;
    }
}