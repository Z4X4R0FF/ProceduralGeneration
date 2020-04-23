using UnityEngine;

public static class EvenNumberGenerator
{
    public static int GenerateEvenNumber(int start, int end)
    {
        var result = Random.Range(start, end);
        if (result % 2 != 0) result += 1;
        return result;
    }

    public static int GenerateOddNumber(int start, int end)
    {
        var result = Random.Range(start, end);
        if (result % 2 == 0)
            result += 1;
        return result;
    }
}