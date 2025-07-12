using UnityEngine;
using UnityEngine.SceneManagement; // 씬 전환을 위해 필요합니다.

public class MainMenuManager : MonoBehaviour
{
    // "게임 시작" 버튼 클릭 시 호출될 함수
    public void StartGame()
    {
        // "Sample Scene"으로 씬을 전환합니다.
        // SceneManager.LoadScene("Sample Scene");
        // 또는 빌드 설정에 등록된 씬의 인덱스를 사용할 수도 있습니다.
        SceneManager.LoadScene(1); 
    }

    // "게임 설정" 버튼 클릭 시 호출될 함수
    public void OpenSettings()
    {
        // 게임 설정 UI 패널을 활성화하는 등의 동작을 여기에 추가합니다.
        // 예를 들어, settingsPanel.SetActive(true);
        Debug.Log("게임 설정 버튼이 클릭되었습니다.");
    }

    // "게임 종료" 버튼 클릭 시 호출될 함수
    public void QuitGame()
    {
        Debug.Log("게임 종료 버튼이 클릭되었습니다.");
        // 애플리케이션을 종료합니다. 유니티 에디터에서는 동작하지 않고, 빌드된 게임에서만 작동합니다.
        Application.Quit();
    }
}

