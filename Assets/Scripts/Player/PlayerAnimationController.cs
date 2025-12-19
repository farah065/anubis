using System.Collections;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace GEM
{
    [DisallowMultipleComponent]
    public class PlayerAnimationController : Singleton<PlayerAnimationController>
    {
        [Header("References")]
        [SerializeField] private Animator playerAnimator;
        //[SerializeField] private Animator meleeAttackHitboxAnimator; attempted to hace a custom hitbox animator but abandoned due to too much complexity
        public Animator PlayerAnimator => playerAnimator;

        [Header("Feedbacks")]
        [SerializeField] private MMF_Player footstepFeedbacks;
        [SerializeField] private MMF_Player landFeedbacks;
        [SerializeField] private MMF_Player dashFeedbacks;
        [SerializeField] private MMF_Player meleeFeedbacks;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDDash;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;
        private int _animIDAttackIndex;
        private int _animIDBlock;

        private bool _hasAnimator;

        private void Awake()
        {
            if (playerAnimator == null)
            {
                playerAnimator = GetComponent<Animator>();
            }
            _hasAnimator = playerAnimator != null;
            AssignAnimationIDs();
        }

        private void AssignAnimationIDs()
        {
            if (!_hasAnimator) return;
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDDash = Animator.StringToHash("Dash");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            _animIDAttackIndex = Animator.StringToHash("AttackIndex");
            _animIDBlock = Animator.StringToHash("Block");
        }

        public void SetGrounded(bool grounded)
        {
            if (_hasAnimator) playerAnimator.SetBool(_animIDGrounded, grounded);
        }

        public void SetSpeed(float blend, float motionMagnitude)
        {
            if (!_hasAnimator) return;
            playerAnimator.SetFloat(_animIDSpeed, blend);
            playerAnimator.SetFloat(_animIDMotionSpeed, motionMagnitude);
        }

        public void SetBlock(bool blocking)
        {
            if (_hasAnimator) playerAnimator.SetBool(_animIDBlock, blocking);
        }

        public void SetFreeFall(bool freeFall)
        {
            if (_hasAnimator) playerAnimator.SetBool(_animIDFreeFall, freeFall);
        }

        public void SetDash(bool dashing)
        {
            if (_hasAnimator) playerAnimator.SetBool(_animIDDash, dashing);
            // Feedbacks tied to dash start/stop can be handled here (optional)
            if (dashFeedbacks != null)
            {
                if (dashing && !dashFeedbacks.IsPlaying) dashFeedbacks.PlayFeedbacks();
                else if (!dashing && dashFeedbacks.IsPlaying) dashFeedbacks.StopFeedbacks();
            }
        }

        public void SetMelee(int stage)
        {
            if (!_hasAnimator) return;
            playerAnimator.SetInteger(_animIDAttackIndex, stage);
            //meleeAttackHitboxAnimator.SetInteger(_animIDAttackIndex, stage);
            meleeFeedbacks?.PlayFeedbacks();
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