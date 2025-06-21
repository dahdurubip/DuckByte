//using System.Collections;
//using UnityEngine;

//public class SpiderEgg : MonoBehaviour
//{
//    private bool isCounting = false;
//    private bool isPlayerInRange = false;
//    private Coroutine countdownCoroutine;
//    private EJPlayer currentPlayer;
//    private Rigidbody rb;
//    [SerializeField] GameObject fireSpider;

//    private void Awake()
//    {
//        rb = GetComponent<Rigidbody>();
//    }

//    void OnTriggerEnter(Collider other)
//    {
//        if (other.CompareTag("Player"))
//        {
//            Debug.Log($"{gameObject.name} 거미알이 플레이어를 감지!");

//            currentPlayer = other.GetComponent<EJPlayer>();
//            if (currentPlayer != null)
//            {
//                currentPlayer.SetEscapeCallback(OnPlayerEscape); // X 버튼 탈출 기능 활성화
//                currentPlayer.SetEggOneCallback(EggOne); // 첫 번째 X 입력 시 EggOne() 호출
//                currentPlayer.ResetXPressCount(); // X 입력 횟수 초기화
//            }

//            isPlayerInRange = true;

//            if (!isCounting)
//            {
//                isCounting = true;
//                countdownCoroutine = StartCoroutine(Countdown());
//            }
//        }
//    }

//    void OnTriggerExit(Collider other)
//    {
//        if (other.CompareTag("Player") && currentPlayer != null)
//        {
//            Debug.Log($"{gameObject.name} 플레이어가 범위를 벗어남! X 버튼이 비활성화됩니다.");
//            currentPlayer.SetEscapeCallback(null);
//            currentPlayer.SetEggOneCallback(null); // 첫 번째 X 입력 기능도 비활성화
//            currentPlayer = null;
//            isPlayerInRange = false;
//        }
//    }

//    IEnumerator Countdown()
//    {
//        float timer = 2f;
//        while (timer > 0)
//        {
//            Debug.Log($"{gameObject.name} 남은 시간: {timer:F1}초");
//            timer -= 1f;
//            yield return new WaitForSeconds(1f);
//        }

//        Debug.Log($"{gameObject.name} 시간 초과! 플레이어가 실패했습니다.");
//        fireSpider.SetActive(true);
//        isCounting = false;
//    }

//    private void OnPlayerEscape()
//    {
//        Debug.Log($"{gameObject.name} 플레이어가 탈출을 성공했습니다!");

//        if (countdownCoroutine != null)
//        {
//            StopCoroutine(countdownCoroutine);
//            countdownCoroutine = null;
//        }

//        Destroy(gameObject);
//    }

//    private void EggOne()
//    {
//        Debug.Log("EggOne() 실행됨!");

//        Transform meeple = this.gameObject.transform.GetChild(1);

//        foreach (Transform grandChild in meeple)
//        {
//            GameObject grandChildObject = grandChild.gameObject;
//            Debug.Log("손자 오브젝트: " + grandChildObject.name);

//            // Rigidbody 가져오기
//            Rigidbody rb = grandChildObject.GetComponent<Rigidbody>();
//            if (rb != null)
//            {
//                // 현재 isKinematic 값 출력
//                Debug.Log($"{grandChildObject.name}의 isKinematic 상태: {rb.isKinematic}");

//                // isKinematic 값 변경 (예: false로 설정하여 물리 적용)
//                rb.isKinematic = false;
//                Debug.Log($"{grandChildObject.name}의 isKinematic을 false로 설정!");
//            }
//            else
//            {
//                Debug.Log($"{grandChildObject.name}에는 Rigidbody가 없음!");
//            }
//        }
//    }
//}
