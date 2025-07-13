using UnityEngine;
using UnityEngine.UI; // Image 컴포넌트를 사용하기 위해 필요합니다.
using UnityEngine.SceneManagement; // 씬 전환을 위해 필요합니다.

public class EpilogueManager : MonoBehaviour
{
    // 인스펙터에서 일러스트를 연결할 Image 컴포넌트
    public Image illustrationImage;

    // 인스펙터에서 일러스트 스프라이트들을 순서대로 연결합니다.
    public Sprite[] illustrations;

    // 일러스트당 보여줄 시간 (초)
    public float displayTime = 5f;

    // 다음으로 넘어갈 씬의 이름
    public string nextSceneName = "MainMenu";

    private int currentIllustrationIndex = 0;
    private float timer;

    void Start()
    {
        // 첫 번째 일러스트를 보여줍니다.
        if (illustrations.Length > 0)
        {
            illustrationImage.sprite = illustrations[currentIllustrationIndex];
            timer = displayTime;
        }
    }

    void Update()
    {
        // 5초가 지났거나 마우스 클릭이 감지되면 다음 일러스트로 넘어갑니다.
        if (timer <= 0 || Input.GetMouseButtonDown(0))
        {
            ShowNextIllustration();
        }

        // 타이머를 감소시킵니다.
        timer -= Time.deltaTime;
    }

    private void ShowNextIllustration()
    {
        currentIllustrationIndex++;

        // 아직 보여줄 일러스트가 남아있다면
        if (currentIllustrationIndex < illustrations.Length)
        {
            illustrationImage.sprite = illustrations[currentIllustrationIndex];
            timer = displayTime;
        }
        else // 모든 일러스트를 다 보여줬다면
        {
            // 다음 씬으로 이동합니다.
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
