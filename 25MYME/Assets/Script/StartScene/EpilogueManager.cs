using UnityEngine;
using UnityEngine.UI; // Image ������Ʈ�� ����ϱ� ���� �ʿ��մϴ�.
using UnityEngine.SceneManagement; // �� ��ȯ�� ���� �ʿ��մϴ�.

public class EpilogueManager : MonoBehaviour
{
    // �ν����Ϳ��� �Ϸ���Ʈ�� ������ Image ������Ʈ
    public Image illustrationImage;

    // �ν����Ϳ��� �Ϸ���Ʈ ��������Ʈ���� ������� �����մϴ�.
    public Sprite[] illustrations;

    // �Ϸ���Ʈ�� ������ �ð� (��)
    public float displayTime = 5f;

    // �������� �Ѿ ���� �̸�
    public string nextSceneName = "MainMenu";

    private int currentIllustrationIndex = 0;
    private float timer;

    void Start()
    {
        // ù ��° �Ϸ���Ʈ�� �����ݴϴ�.
        if (illustrations.Length > 0)
        {
            illustrationImage.sprite = illustrations[currentIllustrationIndex];
            timer = displayTime;
        }
    }

    void Update()
    {
        // 5�ʰ� �����ų� ���콺 Ŭ���� �����Ǹ� ���� �Ϸ���Ʈ�� �Ѿ�ϴ�.
        if (timer <= 0 || Input.GetMouseButtonDown(0))
        {
            ShowNextIllustration();
        }

        // Ÿ�̸Ӹ� ���ҽ�ŵ�ϴ�.
        timer -= Time.deltaTime;
    }

    private void ShowNextIllustration()
    {
        currentIllustrationIndex++;

        // ���� ������ �Ϸ���Ʈ�� �����ִٸ�
        if (currentIllustrationIndex < illustrations.Length)
        {
            illustrationImage.sprite = illustrations[currentIllustrationIndex];
            timer = displayTime;
        }
        else // ��� �Ϸ���Ʈ�� �� ������ٸ�
        {
            // ���� ������ �̵��մϴ�.
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
