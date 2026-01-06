using System;
public static class LetterData
{
    static readonly string[] letters = {
        "A", "B", "C", "Ç", "D", "E", "F", "G", "H",
        "Ý","K", "L", "M", "N", "O","P",
        "R", "S", "Þ", "T","Y", "Z"
    };

    public static string GetLetter(int index)
    {
        return letters[index];
    }

    public static int GetIndex(string letter)
    {
        return Array.IndexOf(letters, letter);
    }

    public static int Count => letters.Length;
}
