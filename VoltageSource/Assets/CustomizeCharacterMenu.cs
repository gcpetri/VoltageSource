using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomizeCharacterMenu : MonoBehaviour
{
    /*
     * Yellow = 1;
     * Red = 2;
     * Green = 3;
     * Purple = 4;
     * Magenta = 5;
     * Black = 6;
     * Blue = 7;
     * Orange = 8;
     * Aqua = 9;
     * Maroon = 10;
     * White = 11;
    */
    // Materials
    public Renderer Obj;
    public Image YelB, RedB, GreB, PurB, MagB, BlkB, BluB, OrgB, AquB, MarB, WhtB;
    Image[] Images = new Image[11];
    private Color yelCol, redCol, greCol, purCol, magCol, blkCol, bluCol, orgCol, aquCol, marCol, whtCol;
    Color[] Colors = new Color[11];
    public int ActiveIndex = 0; // index of the current color and image selected in the arrays (+ 1)
    private void Start()
    {
        yelCol = new Color(0.849746f, 0.9056604f, 0.39644f, 1);
        redCol = new Color(1, 0, 0, 1);
        greCol = new Color(0, 0.8679245f, 0.119f, 1);
        purCol = new Color(0.8362575f, 0, 1, 1);
        magCol = new Color(1, 0, 0.7459249f, 1);
        blkCol = new Color(0, 0, 0, 1);
        bluCol = new Color(0.0194375f, 0.3716098f, 0.8584906f, 1);
        orgCol = new Color(1, 0.4027772f, 0, 1);
        aquCol = new Color(0, 1, 0.7527807f, 1);
        marCol = new Color(0.5283019f, 0.0966f, 0.0966f, 1);
        whtCol = new Color(1, 1, 1, 1);
        Colors[0] = yelCol;
        Colors[1] = redCol;
        Colors[2] = greCol;
        Colors[3] = purCol;
        Colors[4] = magCol;
        Colors[5] = blkCol;
        Colors[6] = bluCol;
        Colors[7] = orgCol;
        Colors[8] = aquCol;
        Colors[9] = marCol;
        Colors[10] = whtCol;
        Images[0] = YelB;
        Images[1] = RedB;
        Images[2] = GreB;
        Images[3] = PurB;
        Images[4] = MagB;
        Images[5] = BlkB;
        Images[6] = BluB;
        Images[7] = OrgB;
        Images[8] = AquB;
        Images[9] = MarB;
        Images[10] = WhtB;
    }
  
    public void UpdateColor(int color)
    {
        switch (color)
        {
            case 1:
                Images[0].color = new Color32(100, 140, 160, 200);
                Obj.material.color = Colors[0];
                ActiveIndex = 1;
                for (int i = 0; i < 11; i++)
                {
                    if (i != (ActiveIndex - 1))
                        Images[i].color = new Color32(255, 255, 255, 255);
                }
                break;
            case 2:
                Images[1].color = new Color32(100, 140, 160, 200);
                Obj.material.color = Colors[1];
                ActiveIndex = 2;
                for (int i = 0; i < 11; i++)
                {
                    if (i != (ActiveIndex - 1))
                        Images[i].color = new Color32(255, 255, 255, 255);
                }
                break;
            case 3:
                Images[2].color = new Color32(100, 140, 160, 200);
                Obj.material.color = Colors[2];
                ActiveIndex = 3;
                for (int i = 0; i < 11; i++)
                {
                    if (i != (ActiveIndex - 1))
                        Images[i].color = new Color32(255, 255, 255, 255);
                }
                break;
            case 4:
                Images[3].color = new Color32(100, 140, 160, 200);
                Obj.material.color = Colors[3];
                ActiveIndex = 4;
                for (int i = 0; i < 11; i++)
                {
                    if (i != (ActiveIndex - 1))
                        Images[i].color = new Color32(255, 255, 255, 255);
                }
                break;
            case 5:
                Images[4].color = new Color32(100, 140, 160, 200);
                Obj.material.color = Colors[4];
                ActiveIndex = 5;
                for (int i = 0; i < 11; i++)
                {
                    if (i != (ActiveIndex - 1))
                        Images[i].color = new Color32(255, 255, 255, 255);
                }
                break;
            case 6:
                Images[5].color = new Color32(100, 140, 160, 200);
                Obj.material.color = Colors[5];
                ActiveIndex = 6;
                for (int i = 0; i < 11; i++)
                {
                    if (i != (ActiveIndex - 1))
                        Images[i].color = new Color32(255, 255, 255, 255);
                }
                break;
            case 7:
                Images[6].color = new Color32(100, 140, 160, 200);
                Obj.material.color = Colors[6];
                ActiveIndex = 7;
                for (int i = 0; i < 11; i++)
                {
                    if (i != (ActiveIndex - 1))
                        Images[i].color = new Color32(255, 255, 255, 255);
                }
                break;
            case 8:
                Images[7].color = new Color32(100, 140, 160, 200);
                Obj.material.color = Colors[7];
                ActiveIndex = 8;
                for (int i = 0; i < 11; i++)
                {
                    if (i != (ActiveIndex - 1))
                        Images[i].color = new Color32(255, 255, 255, 255);
                }
                break;
            case 9:
                Images[8].color = new Color32(100, 140, 160, 200);
                Obj.material.color = Colors[8];
                ActiveIndex = 9;
                for (int i = 0; i < 11; i++)
                {
                    if (i != (ActiveIndex - 1))
                        Images[i].color = new Color32(255, 255, 255, 255);
                }
                break;
            case 10:
                Images[9].color = new Color32(100, 140, 160, 200);
                Obj.material.color = Colors[9];
                ActiveIndex = 10;
                for (int i = 0; i < 11; i++)
                {
                    if (i != (ActiveIndex - 1))
                        Images[i].color = new Color32(255, 255, 255, 255);
                }
                break;
            case 11:
                Images[10].color = new Color32(100, 140, 160, 200);
                Obj.material.color = Colors[10];
                ActiveIndex = 11;
                for (int i = 0; i < 11; i++)
                {
                    if (i != (ActiveIndex - 1))
                        Images[i].color = new Color32(255, 255, 255, 255);
                }
                break;
            default:
                Images[11].color = new Color32(100, 140, 160, 200);
                Obj.material.color = Colors[11];
                ActiveIndex = 11;
                for (int i = 0; i < 11; i++)
                {
                    if (i != (ActiveIndex-1)) 
                        Images[i].color = new Color32(255, 255, 255, 255);
                }
                break;
        };
    }
}
