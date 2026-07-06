using FieldDay.SharedState;
using FieldDay.UI;
using UnityEngine;
using UnityEngine.UI;

public class Background: SharedPanel
{
    [SerializeField] public Sprite backgroundImage;
    public void SetBackground(Sprite newBackground)
    {
        if (backgroundImage != null)
        {
            backgroundImage = newBackground;
        }
    }
}