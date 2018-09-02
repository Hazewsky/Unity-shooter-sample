using System.Collections;
using System.Collections.Generic;


public static class Utility{
    //Fisher Yates Shuffle
    public static T[] ShuffleArray<T>(T[] array, int seed)
    {
        System.Random prng = new System.Random(seed);
        //no need for last element iteration - it is never swap
        for( int i = 0; i < array.Length - 1; i++)
        {
            int randomIndex = prng.Next(i, array.Length);
            T tempItem = array[randomIndex];
            array[randomIndex] = array[i];
            array[i] = tempItem;
        }
        return array;
    }
}
