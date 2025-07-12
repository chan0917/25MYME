using UnityEngine;
using UnityEngine.SceneManagement; // �� ��ȯ�� ���� �ʿ��մϴ�.

public class MainMenuManager : MonoBehaviour
{
    // "���� ����" ��ư Ŭ�� �� ȣ��� �Լ�
    public void StartGame()
    {
        // "Sample Scene"���� ���� ��ȯ�մϴ�.
        // SceneManager.LoadScene("Sample Scene");
        // �Ǵ� ���� ������ ��ϵ� ���� �ε����� ����� ���� �ֽ��ϴ�.
        SceneManager.LoadScene(1); 
    }

    // "���� ����" ��ư Ŭ�� �� ȣ��� �Լ�
    public void OpenSettings()
    {
        // ���� ���� UI �г��� Ȱ��ȭ�ϴ� ���� ������ ���⿡ �߰��մϴ�.
        // ���� ���, settingsPanel.SetActive(true);
        Debug.Log("���� ���� ��ư�� Ŭ���Ǿ����ϴ�.");
    }

    // "���� ����" ��ư Ŭ�� �� ȣ��� �Լ�
    public void QuitGame()
    {
        Debug.Log("���� ���� ��ư�� Ŭ���Ǿ����ϴ�.");
        // ���ø����̼��� �����մϴ�. ����Ƽ �����Ϳ����� �������� �ʰ�, ����� ���ӿ����� �۵��մϴ�.
        Application.Quit();
    }
}

