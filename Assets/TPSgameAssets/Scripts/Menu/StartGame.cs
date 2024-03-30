using UnityEngine;
using UnityEngine.SceneManagement;

namespace TPSgameAssets.Scripts.Menu
{
    public class StartGame : MonoBehaviour
    {
        public void StartGameBtn()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}
