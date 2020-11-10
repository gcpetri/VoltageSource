using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterColorChoices : MonoBehaviour
{
    private 
         static Color Yellow = new Color(0.849746f, 0.9056604f, 0.39644f, 1),
        Red = new Color(1, 0, 0, 1),
        Green = new Color(0, 0.8679245f, 0.119f, 1),
        Purple = new Color(0.8362575f, 0, 1, 1),
        Magenta = new Color(1, 0, 0.7459249f, 1),
        Black = new Color(0, 0, 0, 1),
        Orange = new Color(1, 0.4027772f, 0, 1),
        Aqua = new Color(0, 1, 0.7527807f, 1),
        Maroon = new Color(0.5283019f, 0.0966f, 0.0966f, 1),
        White = new Color(1,1,1,1),
        Blue = new Color(0.0194375f, 0.3716098f, 0.8584906f, 1);

    public static Color[] ColorChoices =
    {
        Yellow,
        Red,
        Green,
        Purple,
        Magenta,
        Black,
        Orange,
        Aqua,
        Maroon,
        White,
        Blue
    };
}
