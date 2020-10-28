using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomizeCharacterMenu : MonoBehaviour
{
    public Renderer Obj;
    public Image YelB, RedB, GreB, PurB, MagB, BlkB, BluB, OrgB, AquB, MarB, WhtB;
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
    private Color yelCol, redCol, greCol, purCol, magCol, blkCol, bluCol, orgCol, aquCol, marCol, whtCol;
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
    }
    private void TurnWhite()
    {
        Obj.material.color = whtCol;
    }
    public void UpdateColor(int color)
    {
        switch (color)
        {
            case 1:
                YelB.color = new Color32(100, 140, 160, 200);
                RemoveOtherColors(1);
                TurnWhite();
                Obj.material.color = yelCol;
                break;
            case 2:
                RedB.color = new Color32(100, 140, 160, 200);
                TurnWhite();
                Obj.material.color = redCol;
                RemoveOtherColors(2);
                break;
            case 3:
                GreB.color = new Color32(100, 140, 160, 200);
                TurnWhite();
                Obj.material.color = greCol;
                RemoveOtherColors(3);
                break;
            case 4:
                PurB.color = new Color32(100, 140, 160, 200);
                TurnWhite();
                Obj.material.color = purCol;
                RemoveOtherColors(4);
                break;
            case 5:
                MagB.color = new Color32(100, 140, 160, 200);
                TurnWhite();
                Obj.material.color = magCol;
                RemoveOtherColors(5);
                break;
            case 6:
                BlkB.color = new Color32(100, 140, 160, 200);
                TurnWhite();
                Obj.material.color = blkCol;
                RemoveOtherColors(6);
                break;
            case 7:
                BluB.color = new Color32(100, 140, 160, 200);
                TurnWhite();
                Obj.material.color = bluCol;
                RemoveOtherColors(7);
                break;
            case 8:
                OrgB.color = new Color32(100, 140, 160, 200);
                TurnWhite();
                Obj.material.color = orgCol;
                RemoveOtherColors(8);
                break;
            case 9:
                AquB.color = new Color32(100, 140, 160, 200);
                TurnWhite();
                Obj.material.color = aquCol;
                RemoveOtherColors(9);
                break;
            case 10:
                MarB.color = new Color32(100, 140, 160, 200);
                TurnWhite();
                Obj.material.color = marCol;
                RemoveOtherColors(10);
                break;
            case 11:
                WhtB.color = new Color32(100, 140, 160, 200);
                TurnWhite();
                Obj.material.color = whtCol;
                RemoveOtherColors(11);
                break;
            default:
                TurnWhite();
                break;
        };
    }
    public void RemoveOtherColors(int color)
    {
        switch (color)
        {
            case 1:
                RedB.color = new Color32(255, 255, 255, 255);
                GreB.color = new Color32(255, 255, 255, 255);
                PurB.color = new Color32(255, 255, 255, 255);
                MagB.color = new Color32(255, 255, 255, 255);
                BlkB.color = new Color32(255, 255, 255, 255);
                BluB.color = new Color32(255, 255, 255, 255);
                OrgB.color = new Color32(255, 255, 255, 255);
                AquB.color = new Color32(255, 255, 255, 255);
                MarB.color = new Color32(255, 255, 255, 255);
                WhtB.color = new Color32(255, 255, 255, 255);
                break;
            case 2:
                YelB.color = new Color32(255, 255, 255, 255);
                GreB.color = new Color32(255, 255, 255, 255);
                PurB.color = new Color32(255, 255, 255, 255);
                MagB.color = new Color32(255, 255, 255, 255);
                BlkB.color = new Color32(255, 255, 255, 255);
                BluB.color = new Color32(255, 255, 255, 255);
                OrgB.color = new Color32(255, 255, 255, 255);
                AquB.color = new Color32(255, 255, 255, 255);
                MarB.color = new Color32(255, 255, 255, 255);
                WhtB.color = new Color32(255, 255, 255, 255);
                break;
            case 3:
                RedB.color = new Color32(255, 255, 255, 255);
                YelB.color = new Color32(255, 255, 255, 255);
                PurB.color = new Color32(255, 255, 255, 255);
                MagB.color = new Color32(255, 255, 255, 255);
                BlkB.color = new Color32(255, 255, 255, 255);
                BluB.color = new Color32(255, 255, 255, 255);
                OrgB.color = new Color32(255, 255, 255, 255);
                AquB.color = new Color32(255, 255, 255, 255);
                MarB.color = new Color32(255, 255, 255, 255);
                WhtB.color = new Color32(255, 255, 255, 255);
                break;
            case 4:
                RedB.color = new Color32(255, 255, 255, 255);
                GreB.color = new Color32(255, 255, 255, 255);
                YelB.color = new Color32(255, 255, 255, 255);
                MagB.color = new Color32(255, 255, 255, 255);
                BlkB.color = new Color32(255, 255, 255, 255);
                BluB.color = new Color32(255, 255, 255, 255);
                OrgB.color = new Color32(255, 255, 255, 255);
                AquB.color = new Color32(255, 255, 255, 255);
                MarB.color = new Color32(255, 255, 255, 255);
                WhtB.color = new Color32(255, 255, 255, 255);
                break;
            case 5:
                RedB.color = new Color32(255, 255, 255, 255);
                GreB.color = new Color32(255, 255, 255, 255);
                PurB.color = new Color32(255, 255, 255, 255);
                YelB.color = new Color32(255, 255, 255, 255);
                BlkB.color = new Color32(255, 255, 255, 255);
                BluB.color = new Color32(255, 255, 255, 255);
                OrgB.color = new Color32(255, 255, 255, 255);
                AquB.color = new Color32(255, 255, 255, 255);
                MarB.color = new Color32(255, 255, 255, 255);
                WhtB.color = new Color32(255, 255, 255, 255);
                break;
            case 6:
                RedB.color = new Color32(255, 255, 255, 255);
                GreB.color = new Color32(255, 255, 255, 255);
                PurB.color = new Color32(255, 255, 255, 255);
                MagB.color = new Color32(255, 255, 255, 255);
                YelB.color = new Color32(255, 255, 255, 255);
                BluB.color = new Color32(255, 255, 255, 255);
                OrgB.color = new Color32(255, 255, 255, 255);
                AquB.color = new Color32(255, 255, 255, 255);
                MarB.color = new Color32(255, 255, 255, 255);
                WhtB.color = new Color32(255, 255, 255, 255);
                break;
            case 7:
                RedB.color = new Color32(255, 255, 255, 255);
                GreB.color = new Color32(255, 255, 255, 255);
                PurB.color = new Color32(255, 255, 255, 255);
                MagB.color = new Color32(255, 255, 255, 255);
                BlkB.color = new Color32(255, 255, 255, 255);
                YelB.color = new Color32(255, 255, 255, 255);
                OrgB.color = new Color32(255, 255, 255, 255);
                AquB.color = new Color32(255, 255, 255, 255);
                MarB.color = new Color32(255, 255, 255, 255);
                WhtB.color = new Color32(255, 255, 255, 255);
                break;
            case 8:
                RedB.color = new Color32(255, 255, 255, 255);
                GreB.color = new Color32(255, 255, 255, 255);
                PurB.color = new Color32(255, 255, 255, 255);
                MagB.color = new Color32(255, 255, 255, 255);
                BlkB.color = new Color32(255, 255, 255, 255);
                BluB.color = new Color32(255, 255, 255, 255);
                YelB.color = new Color32(255, 255, 255, 255);
                AquB.color = new Color32(255, 255, 255, 255);
                MarB.color = new Color32(255, 255, 255, 255);
                WhtB.color = new Color32(255, 255, 255, 255);
                break;
            case 9:
                RedB.color = new Color32(255, 255, 255, 255);
                GreB.color = new Color32(255, 255, 255, 255);
                PurB.color = new Color32(255, 255, 255, 255);
                MagB.color = new Color32(255, 255, 255, 255);
                BlkB.color = new Color32(255, 255, 255, 255);
                BluB.color = new Color32(255, 255, 255, 255);
                OrgB.color = new Color32(255, 255, 255, 255);
                YelB.color = new Color32(255, 255, 255, 255);
                MarB.color = new Color32(255, 255, 255, 255);
                WhtB.color = new Color32(255, 255, 255, 255);
                break;
            case 10:
                RedB.color = new Color32(255, 255, 255, 255);
                GreB.color = new Color32(255, 255, 255, 255);
                PurB.color = new Color32(255, 255, 255, 255);
                MagB.color = new Color32(255, 255, 255, 255);
                BlkB.color = new Color32(255, 255, 255, 255);
                BluB.color = new Color32(255, 255, 255, 255);
                OrgB.color = new Color32(255, 255, 255, 255);
                AquB.color = new Color32(255, 255, 255, 255);
                YelB.color = new Color32(255, 255, 255, 255);
                WhtB.color = new Color32(255, 255, 255, 255);
                break;
            case 11:
                RedB.color = new Color32(255, 255, 255, 255);
                GreB.color = new Color32(255, 255, 255, 255);
                PurB.color = new Color32(255, 255, 255, 255);
                MagB.color = new Color32(255, 255, 255, 255);
                BlkB.color = new Color32(255, 255, 255, 255);
                BluB.color = new Color32(255, 255, 255, 255);
                OrgB.color = new Color32(255, 255, 255, 255);
                AquB.color = new Color32(255, 255, 255, 255);
                MarB.color = new Color32(255, 255, 255, 255);
                YelB.color = new Color32(255, 255, 255, 255);
                break;
            default:
                RedB.color = new Color32(255, 255, 255, 255);
                GreB.color = new Color32(255, 255, 255, 255);
                PurB.color = new Color32(255, 255, 255, 255);
                MagB.color = new Color32(255, 255, 255, 255);
                BlkB.color = new Color32(255, 255, 255, 255);
                BluB.color = new Color32(255, 255, 255, 255);
                OrgB.color = new Color32(255, 255, 255, 255);
                AquB.color = new Color32(255, 255, 255, 255);
                MarB.color = new Color32(255, 255, 255, 255);
                WhtB.color = new Color32(255, 255, 255, 255);
                break;
        };
    }
}
