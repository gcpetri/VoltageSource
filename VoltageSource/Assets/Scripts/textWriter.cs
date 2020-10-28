using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class textWriter : MonoBehaviour
{
    private Text messageText;
    private string textToWrite;
    private int characterIndex;
    private float timePerCharacter;
    private float timer;

    public void AddWriter(Text messageText, string textToWrite, float timePerCharacter)
    {
        this.messageText = messageText;
        this.textToWrite = textToWrite;
        this.timePerCharacter = timePerCharacter;
        characterIndex = 0;
    }

    private void Update()
    {
        if (messageText != null)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                // Display next character
                timer += timePerCharacter;
                characterIndex++;
                messageText.text = textToWrite.Substring(0, characterIndex);

                if (characterIndex >= textToWrite.Length)
                {
                    // entire string displayed
                    messageText = null;
                    return;
                }
            }
        }
    }
}
