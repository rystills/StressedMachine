using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class StatsController : MonoBehaviour
{
    public static StatsController instance;
    [SerializeField] private Text text;

    private void Awake()
    {
        instance = this;
        transform.parent.gameObject.SetActive(false);
    }

    public static void FlushStats()
    {
        if (!instance.gameObject.activeInHierarchy)
        {
            int i = -1;
            instance.text.text = Regex.Replace(instance.text.text, @"0",
                m => GameState.deathByCounts[++i].ToString() + new string(' ', 20 - 3 * GameState.deathByCounts[i].ToString().Length));

        instance.transform.parent.gameObject.SetActive(true);
        }
    }
}
