using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace Main.Model
{
    /// <summary>
    /// 死亡判定のコライダー
    /// </summary>
    [RequireComponent(typeof(CapsuleCollider2D))]
    public class DangerCollider : AbstractCollider
    {
        /// <summary>接触するか</summary>
        private readonly BoolReactiveProperty _isHit = new BoolReactiveProperty();
        /// <summary>接触するか</summary>
        public IReactiveProperty<bool> IsHit => _isHit;

        protected override void OnTriggerEnter2D(Collider2D collision)
        {
            if (!_isHit.Value &&
                IsCollisionToTags(collision, tags))
            {
                _isHit.Value = true;
            }
        }
    }
}
