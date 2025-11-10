using MoreMountains.Feedbacks;
using UnityEngine;

namespace GEM
{
    [DisallowMultipleComponent]
    public class PlayerAnimationController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator animator;

        [Header("Feedbacks")]
        [SerializeField] private MMF_Player footstepFeedbacks;
        [SerializeField] private MMF_Player jumpFeedbacks;
        [SerializeField] private MMF_Player landFeedbacks;
        [SerializeField] private MMF_Player dashFeedbacks;
        [SerializeField] private MMF_Player meleeFeedbacks;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDDash;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;
        private int _animIDAttack;
        private int _animIDAttackIndex;

        private bool _hasAnimator;

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }
            _hasAnimator = animator != null;
            AssignAnimationIDs();
        }

        private void AssignAnimationIDs()
        {
            if (!_hasAnimator) return;
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDDash = Animator.StringToHash("Dash");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            _animIDAttack = Animator.StringToHash("Attack");
            _animIDAttackIndex = Animator.StringToHash("AttackIndex");
        }

        public void SetGrounded(bool grounded)
        {
            if (_hasAnimator) animator.SetBool(_animIDGrounded, grounded);
        }

        public void SetSpeed(float blend, float motionMagnitude)
        {
            if (!_hasAnimator) return;
            animator.SetFloat(_animIDSpeed, blend);
            animator.SetFloat(_animIDMotionSpeed, motionMagnitude);
        }

        public void SetJump(bool jumping)
        {
            if (_hasAnimator) animator.SetBool(_animIDJump, jumping);
        }

        public void SetFreeFall(bool freeFall)
        {
            if (_hasAnimator) animator.SetBool(_animIDFreeFall, freeFall);
        }

        public void SetDash(bool dashing)
        {
            if (_hasAnimator) animator.SetBool(_animIDDash, dashing);
            // Feedbacks tied to dash start/stop can be handled here (optional)
            if (dashFeedbacks != null)
            {
                if (dashing && !dashFeedbacks.IsPlaying) dashFeedbacks.PlayFeedbacks();
                else if (!dashing && dashFeedbacks.IsPlaying) dashFeedbacks.StopFeedbacks();
            }
        }

        public void PlayMelee(int stage)
        {
            if (!_hasAnimator) return;
            animator.SetInteger(_animIDAttackIndex, stage);
            animator.SetTrigger(_animIDAttack);
            meleeFeedbacks?.PlayFeedbacks();
        }

        public void TriggerJumpFeedback()
        {
            jumpFeedbacks?.PlayFeedbacks();
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            footstepFeedbacks?.PlayFeedbacks();
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            landFeedbacks?.PlayFeedbacks();
        }
    }
}