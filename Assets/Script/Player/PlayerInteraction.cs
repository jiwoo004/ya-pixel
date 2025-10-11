using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 1. 모든 플레이어에게 항상 보이는 npc (예: 상점 npc)
///     photonview를 안붙이고 프리팹을 만들어서 프리팹 폴더에 넣는다.
///         각자 고유한 npc가 보여 각자 상호작용 가능
/// 2. 각 플레이어마다 고유하게 보이는 npc (예: 퀘스트 npc, 몬스터)
///     photonview를 붙여서 프리팹을 만들어서 리소스 폴더에 넣는다.
///         네트워크로 동기화해서 모든 플레이어가 동일한 npc를 보고 상호작용 가능
/// <summary>



/// 플레이어의 상호작용을 관리합니다. 주변의 IInteractable을 감지하고 상호작용을 실행합니다.
/// </summary>
public class PlayerInteraction : MonoBehaviourPunCallbacks
{
    // 감지 범위 내에 있는 상호작용 가능한 오브젝트들
    private List<IInteractable> nearbyInteractables = new List<IInteractable>();

    void Update()
    {
        // 내 캐릭터가 아니면 입력을 처리하지 않음
        if (photonView != null && !photonView.IsMine) return;

        // 상호작용 키를 눌렀고, 주변에 상호작용할 오브젝트가 있다면
        if (Input.GetKeyDown(KeyCode.E) && nearbyInteractables.Count > 0)
        {
            // 가장 가까운 오브젝트를 찾아서 상호작용 실행
            IInteractable closest = FindClosestInteractable();
            if (closest != null)
            {
                closest.Interact(gameObject); // '나(플레이어)'를 상호작용의 주체로 전달
            }
        }
    }

    /// <summary>
    /// 주변에 있는 상호작용 가능한 오브젝트 중 가장 가까운 것을 찾습니다.
    /// </summary>
    private IInteractable FindClosestInteractable()
    {
        // 리스트에서 비활성화된(null) 오브젝트들을 먼저 정리
        nearbyInteractables.RemoveAll(item => item == null || (item as MonoBehaviour) == null);

        IInteractable closest = null;
        float minDistance = float.MaxValue;

        foreach (var interactable in nearbyInteractables)
        {
            // IInteractable을 구현한 MonoBehaviour의 transform을 가져옴
            Transform interactableTransform = (interactable as MonoBehaviour).transform;
            float distance = Vector2.Distance(transform.position, interactableTransform.position);

            if (distance < minDistance)
            {
                minDistance = distance;
                closest = interactable;
            }
        }
        return closest;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 내 캐릭터가 아니면 감지하지 않음
        if (photonView != null && !photonView.IsMine) return;

        // 충돌한 오브젝트에서 IInteractable 컴포넌트를 찾음
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null && !nearbyInteractables.Contains(interactable))
        {
            nearbyInteractables.Add(interactable);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // 내 캐릭터가 아니면 감지하지 않음
        if (photonView != null && !photonView.IsMine) return;

        // 충돌이 끝난 오브젝트에서 IInteractable 컴포넌트를 찾음
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null && nearbyInteractables.Contains(interactable))
        {
            nearbyInteractables.Remove(interactable);
        }
    }
}
