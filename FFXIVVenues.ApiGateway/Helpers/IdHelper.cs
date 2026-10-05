using System;

namespace FFXIVVenues.ApiGateway.Helpers;

internal static class IdHelper
{
    public static string GenerateId(int length = 12)
    {
        string text = "BCDFGHJKLMNPQRSTVWXYZbcdfghjklmnpqrstvwxyz0123456789";
        char[] array = new char[length];
        Random random = new Random();
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = text[random.Next(text.Length)];
        }

        return new string(array);
    }
}