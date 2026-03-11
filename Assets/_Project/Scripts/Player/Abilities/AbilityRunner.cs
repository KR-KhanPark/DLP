using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace DLP.Player.Abilities
{
    /// <summary>
    /// Player에 붙어있는 IAbility(능력) 컴포넌트들을 모아서
    /// Update/FixedUpdate/Input 이벤트를 전달해주는 "허브" 역할.
    /// </summary>
    public class AbilityRunner : MonoBehaviour
    {
        // IAbility를 구현한 모든 능력을 담아두는 리스트
        private readonly List<IAbility> _abilities = new();

        // 모든 능력이 공유해서 쓰는 플레이어 관련 정보 묶음(Context)
        private AbilityContext _ctx;

        /// <summary>
        /// PlayerController(Awake 등)에서 1번 호출해줘야 함.
        /// 여기서 능력들을 수집하고 초기화한다.
        /// </summary>
        public void Initialize(AbilityContext ctx)
        {
            _ctx = ctx;
            _abilities.Clear();

            // 같은 GameObject(Player)에 붙어 있는 컴포넌트 중,
            // IAbility를 구현한 컴포넌트만 골라서 수집한다.
            //
            // 왜 MonoBehaviour를 대상으로 GetComponents 하냐?
            // Unity 컴포넌트는 MonoBehaviour 기반이기 때문.
            // (IAbility는 컴포넌트 타입이 아니라 "규약"이라 직접 검색이 애매할 수 있음)
            var components = GetComponents<MonoBehaviour>();
            foreach (var c in components)
            {
                if (c is IAbility ability)
                {
                    ability.Initialize(_ctx);
                    _abilities.Add(ability);
                }
            }

            // Priority가 높은 Ability부터 먼저 실행되도록 정렬
            _abilities.Sort((a, b) => b.Priority.CompareTo(a.Priority));

            for (int i = 0; i < _abilities.Count; i++)
            {
                _abilities[i].Initialize(_ctx);
            }
        }

        // ===== Input Forwarding (PlayerController → Runner → Abilities) =====

        public void ForwardMoveInput(Vector2 move)
        {
            for (int i = 0; i < _abilities.Count; i++)
                _abilities[i].OnMoveInput(move);
        }

        public void ForwardJumpPressed()
        {
            for (int i = 0; i < _abilities.Count; i++)
            {
                bool handled = _abilities[i].OnJumpPressed();

                if (handled)
                    break;
            }
        }

        public void ForwardJumpHeld(bool held)
        {
            for (int i = 0; i < _abilities.Count; i++)
                _abilities[i].OnJumpHeld(held);
        }

        // ===== Unity Lifecycle (Runner가 능력들을 "자동 실행") =====

        private void Update()
        {
            if (_ctx == null) return; // Initialize 안 됐으면 아무것도 안 함

            for (int i = 0; i < _abilities.Count; i++)
                _abilities[i].OnUpdate();
        }

        private void FixedUpdate()
        {
            if (_ctx == null) return;

            for (int i = 0; i < _abilities.Count; i++)
                _abilities[i].OnFixedUpdate();
        }
    }
}