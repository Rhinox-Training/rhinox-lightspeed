using UnityEngine;

namespace Rhinox.Lightspeed
{
    public static class AnimatorExtensions
    {
        public static bool IsPlaying(this Animator animator, string name = null)
        {
            if (animator == null) return false;

            var state = animator.GetCurrentAnimatorStateInfo(0);
            var isPlaying = state.normalizedTime <= 1 || animator.IsInTransition(0);
            if (isPlaying && !string.IsNullOrWhiteSpace(name))
                return state.IsName(name);
            return isPlaying;
        }
    }
}