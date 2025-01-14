namespace HourDataProcessor.utils;

public class CustomStringUtils
{
    /// <summary>
    /// 문자열의 유사도를 비교합니다.
    /// </summary>
    /// <returns>
    /// 0~1 사이의 값을 리턴합니다.
    /// </returns>
    public static double CalculateSimilarity(string? s1, string? s2)
    {
        var jaroDistance = CalculateJaroDistance(s1, s2);
    
        var prefixLength = 0;
        var maxPrefixLength = 4;
        for (var i = 0; i < Math.Min(Math.Min(s1.Length, s2.Length), maxPrefixLength); i++)
        {
            if (s1[i] == s2[i])
                prefixLength++;
            else
                break;
        }

        var p = 0.1;
        return jaroDistance + (prefixLength * p * (1 - jaroDistance));
    }

    private static double CalculateJaroDistance(string s1, string s2)
    {
        if (s1.Length == 0) return s2.Length == 0 ? 1.0 : 0.0;
        
        var matchDistance = Math.Max(s1.Length, s2.Length) / 2 - 1;

        var s1Matches = new bool[s1.Length];
        var s2Matches = new bool[s2.Length];

        var matches = 0;
        var transpositions = 0;

        for (int i = 0; i < s1.Length; i++)
        {
            var start = Math.Max(0, i - (int)matchDistance);
            var end = Math.Min(i + (int)matchDistance + 1, s2.Length);

            for (int j = start; j < end; j++)
            {
                if (!s2Matches[j] && s1[i] == s2[j])
                {
                    s1Matches[i] = true;
                    s2Matches[j] = true;
                    matches++;
                    break;
                }
            }
        }
        
        if (matches == 0) return 0.0;

        var k = 0;
        for (var i = 0; i < s1.Length; i++)
        {
            if (!s1Matches[i]) continue;
        
            while (!s2Matches[k]) k++;
        
            if (s1[i] != s2[k]) transpositions++;
            k++;
        }

        return (matches / (double)s1.Length + 
                matches / (double)s2.Length +
                (matches - transpositions/2.0) / matches) / 3.0;
    }
}